function Query(localeName, localizationTokens)
    local localizedOption1 = localizationTokens["Addons:StockRaveOption1"] or "Rave"
    local localizedOption2 = localizationTokens["Addons:StockRaveOption2"] or "Random frame order with color cycle."
    local localizedOption3 = localizationTokens["Addons:StockRaveOption3"] or "The minimum amount of time to seek music."
    local localizedOption4 = localizationTokens["Addons:StockRaveOption4"] or "The maximum amount of time to seek music."
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
                ["name"] = "Music Seek Start",
                ["tooltip"] = localizedOption3,
                ["value"] = "1",
                ["type"] = "float",
            },
            {
                ["name"] = "Music Seek End",
                ["tooltip"] = localizedOption4,
                ["value"] = "5",
                ["type"] = "float",
            },
        }
    }
end

-- Default settings
local musicSeekStart = 1
local musicSeekEnd = 5

-- Temp files
local temp2 = "temp2.mp4"
local frames = ".\\frames"

-- Variables
local randomSound = ""
local success = false
local music = "music.wav"

function StartGeneration(options, pluginSettings, functions)
    -- Set settings
    if pluginSettings["Music Seek Start"] != nil then
        musicSeekStart = tonumber(pluginSettings["Music Seek Start"])
    end
    if pluginSettings["Music Seek End"] != nil then
        musicSeekEnd = tonumber(pluginSettings["Music Seek End"])
    end
    -- Set variables
    randomSound = functions.getRandomLibraryFile("audio", "music")
    -- Apply effect
    functions.folderCreate(frames)
    functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -vf fps=30 \"" .. frames .. "\\frame%0d.png\"")
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
    if commandindex == 1 then
        -- Get frame count
        local frameCount = 0
        for fl in functions.enumerateFiles(frames) do
            frameCount = frameCount + 1
        end
        local frametable = {}
        for i = 1, frameCount do
            frametable[i] = i
        end
        -- Rename frames in random order with prefix _frame
        if frameCount > 0 then
            for fl in functions.enumerateFiles(frames) do
                -- Pop frame from table
                local randomFrame = table.remove(frametable, math.random(1, #frametable))
                -- Rename frame
                local flname = ".\\frames\\" .. fl
                local randomFrameName = ".\\frames\\" .. "_frame" .. tostring(randomFrame-1) .. ".png"
                functions.fileMove(flname, randomFrameName)
            end
            functions.runFFmpeg("-i \"" .. frames .. "\\_frame%0d.png\" -vf \"hue='H=PI*t: s=sin(PI*t)+1.5: enable=between(t,0,10)'\" -preset veryfast -y \"" .. frames .. "\\_frame%0d.png\"")
        end
    elseif commandindex == 2 then
        -- Invoke-Command -ScriptBlock {&$ffmpeg -i $frames\_%0d.png -i $video -map 0:v -map 1:a -preset veryfast -preset veryfast -y "$temp2"}
        functions.runFFmpeg("-i \"" .. frames .. "\\_frame%0d.png\" -i \"" .. options.inputVideo .. "\" -map 0:v -map 1:a -preset veryfast -y \"" .. temp2 .. "\"")
    elseif commandindex == 3 then
        if randomSound == "" then
            -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -filter_complex "[0:v]setpts=0.75*PTS[f];[0:v]setpts=0.5*PTS,reverse[fr];[f][fr]concat=n=2:v=1:a=0,format=yuv420p[v];[0:a]atempo=2.0[a1];[0:a]atempo=0.75,areverse[a2];[a1][a2]concat=n=2:v=0:a=1[a]" -map "[v]" -map "[a]" -shortest -preset veryfast -y "$output"}
            functions.runFFmpeg("-i \"" .. temp2 .. "\" -filter_complex \"[0:v]setpts=0.75*PTS[f];[0:v]setpts=0.5*PTS,reverse[fr];[f][fr]concat=n=2:v=1:a=0,format=yuv420p[v];[0:a]atempo=2.0[a1];[0:a]atempo=0.75,areverse[a2];[a1][a2]concat=n=2:v=0:a=1[a]\" -map \"[v]\" -map \"[a]\" -shortest -preset veryfast -y \"" .. options.outputVideo .. "\"")
        else
            -- convert mp3 to wav
            functions.runFFmpeg("-i \"" .. randomSound .. "\" -acodec pcm_s16le -ac 2 -ar 44100 \"" .. music .. "\"")
        end
        success = true
    elseif commandindex == 4 and randomSound != "" then
        -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -i "$randomSound" -filter_complex "[0:v]setpts=0.75*PTS[f];[0:v]setpts=0.5*PTS,reverse[fr];[f][fr]concat=n=2:v=1:a=0,format=yuv420p[v]" -map "[v]" -map "1:a" -shortest -preset veryfast -y "$output"}
        functions.runFFmpeg("-i \"" .. temp2 .. "\" -i \"" .. music .. "\" -filter_complex \"[0:v]setpts=0.75*PTS[f];[0:v]setpts=0.5*PTS,reverse[fr];[f][fr]concat=n=2:v=1:a=0,format=yuv420p[v]\" -map \"[v]\" -map \"1:a\" -shortest -preset veryfast -y \"" .. options.outputVideo .. "\"")
    end
end

function StopGeneration(options, pluginSettings, functions)
    return success
end
