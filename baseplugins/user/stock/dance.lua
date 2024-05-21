function Query(localeName, localizationTokens)
    local localizedOption1 = localizationTokens["Addons:StockDanceOption1"] or "Dance"
    local localizedOption2 = localizationTokens["Addons:StockDanceOption2"] or "Repeating forward-reverse with music."
    local localizedOption3 = localizationTokens["Addons:StockDanceOption3"] or "Out of 100, where the video will have no music."
    local localizedOption4 = localizationTokens["Addons:StockDanceOption4"] or "The minimum amount of time for each segment."
    local localizedOption5 = localizationTokens["Addons:StockDanceOption5"] or "The maximum amount of time for each segment."
    local localizedOption6 = localizationTokens["Addons:StockDanceOption6"] or "The minimum amount of time to seek music."
    local localizedOption7 = localizationTokens["Addons:StockDanceOption7"] or "The maximum amount of time to seek music."
    return {
        ["settings"] = {
            {
                ["name"] = "Display Name",
                ["value"] = localizedOption1,
                ["type"] = "label",
            },
            {
                ["name"] = "Description",
                ["value"] = localizedOption2,
                ["type"] = "label",
            },
            {
                ["name"] = "No Music Chance",
                ["tooltip"] = localizedOption3,
                ["value"] = "25",
                ["type"] = "int",
            },
            {
                ["name"] = "Segment Min",
                ["tooltip"] = localizedOption4,
                ["value"] = "0.15",
                ["type"] = "float",
            },
            {
                ["name"] = "Segment Max",
                ["tooltip"] = localizedOption5,
                ["value"] = "0.2",
                ["type"] = "float",
            },
            {
                ["name"] = "Music Seek Start",
                ["tooltip"] = localizedOption6,
                ["value"] = "1",
                ["type"] = "float",
            },
            {
                ["name"] = "Music Seek End",
                ["tooltip"] = localizedOption7,
                ["value"] = "5",
                ["type"] = "float",
            },
        }
    }
end

-- Default settings
local randomTimeMin = 0.15
local randomTimeMax = 0.2
local noMusicChance = 25
local musicSeekStart = 1
local musicSeekEnd = 5

-- Temp files
local temp2 = "temp2.mp4"
local temp3 = "temp3.mp4"
local music = "music.wav"

-- Variables
local segmentLength = 0.2
local seek = 1;
local randomSound = ""
local useOriginalAudio = false
local runCount = 0

function StartGeneration(options, pluginSettings, functions)
    -- Set settings
    if pluginSettings["Segment Min"] != nil then
        randomTimeMin = tonumber(pluginSettings["Segment Min"])
    end
    if pluginSettings["Segment Max"] != nil then
        randomTimeMax = tonumber(pluginSettings["Segment Max"])
    end
    if pluginSettings["No Music Chance"] != nil then
        noMusicChance = tonumber(pluginSettings["No Music Chance"])
    end
    if pluginSettings["Music Seek Start"] != nil then
        musicSeekStart = tonumber(pluginSettings["Music Seek Start"])
    end
    if pluginSettings["Music Seek End"] != nil then
        musicSeekEnd = tonumber(pluginSettings["Music Seek End"])
    end
    -- Set variables
    runCount = 0
    segmentLength = functions.randomDouble(randomTimeMin, randomTimeMax)
    seek = functions.randomInt(musicSeekStart, musicSeekEnd)
    randomSound = functions.getRandomLibraryFile("audio", "music")
    useOriginalAudio = (randomSound == "" or functions.randomInt(1, 100) <= tonumber(pluginSettings["No Music Chance"]))
    -- Apply effect
    if useOriginalAudio then
        -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -t $randomTime -filter_complex "[0:v]setpts=.5*PTS[v];[0:a]atempo=2.0[a]" -map "[v]" -map "[a]" -preset veryfast -y "$temp2"}
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -t " .. tostring(segmentLength) .. " -filter_complex \"[0:v]setpts=.5*PTS[v];[0:a]atempo=2.0[a]\" -map \"[v]\" -map \"[a]\" -preset veryfast -y \"" .. temp2 .. "\"")
    else
        -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -an -t $randomTime -vf setpts=.5*PTS  -preset veryfast -y "$temp2"}
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -an -t " .. tostring(segmentLength) .. " -vf setpts=.5*PTS  -preset veryfast -y \"" .. temp2 .. "\"")
    end
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
    if useOriginalAudio then
        if commandindex == 1 then
            -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -vf reverse -af areverse -preset veryfast -y "$temp3"}
            functions.runFFmpeg("-i \"" .. temp2 .. "\" -vf reverse -af areverse -preset veryfast -y \"" .. temp3 .. "\"")
        elseif commandindex == 2 then
            -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp3" -i "$temp2" -filter_complex "[0:v][1:v][0:v][1:v][0:v]concat=n=5:v=1[out];[0:a][1:a][0:a][1:a][0:a][1:a][0:a][1:a]concat=n=8:v=0:a=1[out2]" -map "[out]" -map "[out2]" -preset veryfast -shortest -y "$output"}
            functions.runFFmpeg("-i \"" .. temp3 .. "\" -i \"" .. temp2 .. "\" -filter_complex \"[0:v][1:v][0:v][1:v][0:v]concat=n=5:v=1[out];[0:a][1:a][0:a][1:a][0:a][1:a][0:a][1:a]concat=n=8:v=0:a=1[out2]\" -map \"[out]\" -map \"[out2]\" -preset veryfast -shortest -y \"" .. options.outputVideo .. "\"")
        end
    else
        if commandindex == 1 then
            -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -vf reverse -preset veryfast -y "$temp3"}
            functions.runFFmpeg("-i \"" .. temp2 .. "\" -vf reverse -preset veryfast -y \"" .. temp3 .. "\"")
        elseif commandindex == 2 then
            -- convert mp3 to wav
            functions.runFFmpeg("-i \"" .. randomSound .. "\" -acodec pcm_s16le -ac 2 -ar 44100 \"" .. music .. "\"")
        elseif commandindex == 3 then
            -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp3" -i "$temp2" -ss $seek -i "$randomSound" -filter_complex "[0:v][1:v][0:v][1:v][0:v]concat=n=5:v=1[out]" -map "[out]" -map 2:a -preset veryfast -shortest -preset veryfast -y "$output"}
            functions.runFFmpeg("-i \"" .. temp3 .. "\" -i \"" .. temp2 .. "\" -ss " .. tostring(seek) .. " -i \"" .. music .. "\" -filter_complex \"[0:v][1:v][0:v][1:v][0:v]concat=n=5:v=1[out]\" -map \"[out]\" -map 2:a -preset veryfast -shortest -preset veryfast -y \"" .. options.outputVideo .. "\"")
        end
    end
end

function StopGeneration(options, pluginSettings, functions)
    return true
end
