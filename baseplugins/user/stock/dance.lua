function Query()
    return {
        ["settings"] = {
            {
                ["name"] = "Display Name",
                ["value"] = "Dance",
                ["type"] = "label",
            },
            {
                ["name"] = "Description",
                ["value"] = "Repeating forward-reverse with music.",
                ["type"] = "label",
            },
            {
                ["name"] = "No Music Chance",
                ["tooltip"] = "Out of 100, where the video will have no music.",
                ["value"] = "25",
                ["type"] = "int",
            },
            {
                ["name"] = "Segment Min",
                ["tooltip"] = "The minimum amount of time for each segment.",
                ["value"] = "0.15",
                ["type"] = "float",
            },
            {
                ["name"] = "Segment Max",
                ["tooltip"] = "The maximum amount of time for each segment.",
                ["value"] = "0.2",
                ["type"] = "float",
            },
            {
                ["name"] = "Music Seek Start",
                ["tooltip"] = "The minimum amount of time to seek music.",
                ["value"] = "1",
                ["type"] = "float",
            },
            {
                ["name"] = "Music Seek End",
                ["tooltip"] = "The maximum amount of time to seek music.",
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
local temp2 = ""
local temp3 = ""

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
    -- Set temp files
    temp2 = "temp2.mp4"
    temp3 = "temp3.mp4"
    -- Set variables
    runCount = 0
    segmentLength = functions.randomDouble(randomTimeMin, randomTimeMax)
    seek = functions.randomInt(musicSeekStart, musicSeekEnd)
    randomSound = functions.getRandomLibraryFile("audio", "music")
    useOriginalAudio = (randomSound != "" and functions.randomInt(1, 100) <= tonumber(pluginSettings["No Music Chance"]))
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
            -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp3" -i "$temp2" -filter_complex "[0:v][1:v][0:v][1:v][0:v][1:v][0:v][1:v]concat=n=8:v=1[out];[0:a][1:a][0:a][1:a][0:a][1:a][0:a][1:a]concat=n=8:v=0:a=1[out2]" -map "[out]" -map "[out2]" -preset veryfast -shortest -y "$output"}
            functions.runFFmpeg("-i \"" .. temp3 .. "\" -i \"" .. temp2 .. "\" -filter_complex \"[0:v][1:v][0:v][1:v][0:v][1:v][0:v][1:v]concat=n=8:v=1[out];[0:a][1:a][0:a][1:a][0:a][1:a][0:a][1:a]concat=n=8:v=0:a=1[out2]\" -map \"[out]\" -map \"[out2]\" -preset veryfast -shortest -y \"" .. options.outputVideo .. "\"")
        end
    else
        if commandindex == 1 then
            -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -vf reverse -preset veryfast -y "$temp3"}
            functions.runFFmpeg("-i \"" .. temp2 .. "\" -vf reverse -preset veryfast -y \"" .. temp3 .. "\"")
        elseif commandindex == 2 then
            -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp3" -i "$temp2" -ss $seek -i "$randomSound" -filter_complex "[0:v][1:v][0:v][1:v][0:v][1:v][0:v][1:v]concat=n=8:v=1[out]" -map "[out]" -map 2:a -preset veryfast -shortest -preset veryfast -y "$output"}
            functions.runFFmpeg("-i \"" .. temp3 .. "\" -i \"" .. temp2 .. "\" -ss " .. tostring(seek) .. " -i \"" .. randomSound .. "\" -filter_complex \"[0:v][1:v][0:v][1:v][0:v][1:v][0:v][1:v]concat=n=8:v=1[out]\" -map \"[out]\" -map 2:a -preset veryfast -shortest -preset veryfast -y \"" .. options.outputVideo .. "\"")
        end
    end
end

function StopGeneration(options, pluginSettings, functions)
    return true
end
