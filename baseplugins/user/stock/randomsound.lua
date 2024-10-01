function Query(localeName, localizationTokens)
    local localizedOption1 = localizationTokens["Addons:StockRandomSoundOption1"] or "Random Sound"
    local localizedOption2 = localizationTokens["Addons:StockRandomSoundOption2"] or "Applies a random sound effect"
    local localizedOption3 = localizationTokens["Addons:StockRandomSoundOption3"] or "Out of 100, chance to mute the original audio"
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
                ["name"] = "Mute Original Chance",
                ["tooltip"] = localizedOption3,
                ["value"] = "50",
                ["type"] = "int",
            },
        }
    }
end

-- Default settings
local chance = 50

-- Variables
local muteOriginalAudio = false
local randomSound = ""
local soundEffect = "sound.wav"

function StartGeneration(options, pluginSettings, functions)
    -- Set settings
    if pluginSettings["Mute Original Chance"] != nil then
        chance = tonumber(pluginSettings["Mute Original Chance"])
    end
    -- Set variables
    muteOriginalAudio = functions.randomInt(1, 100) <= chance and true or false
    randomSound = functions.getRandomLibraryFile("audio", "sfx")
    -- Apply effect
    -- Convert randomSound to soundEffect
    functions.runFFmpeg("-i \"" .. randomSound .. "\" -y \"" .. soundEffect .. "\"")
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
    if commandindex == 1 then
        -- ffprobe get length of soundEffect
        functions.runFFprobe("-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"" .. soundEffect .. "\"")
    elseif commandindex == 2 then
        local length = tonumber(outputresult)
        if muteOriginalAudio then
            -- Override original audio with sfx with video cut to length
            functions.runFFmpeg("-i \"" .. soundEffect .. "\" -i " .. options.inputVideo .. " -filter_complex \"[0:a]apad[0a];[1:a]volume=0.0[a1];[0a][a1]amix=inputs=2:duration=first:dropout_transition=0,aresample=async=1000[outa]\" -c:v copy -map 1:v -map \"[outa]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. options.outputVideo .. "\"")
        else
            -- Add sfx to original audio to length
            functions.runFFmpeg("-i \"" .. soundEffect .. "\" -i " .. options.inputVideo .. " -filter_complex \"[0:a]apad[0a];[1:a]volume=1.0[a1];[0a][a1]amix=inputs=2:duration=first:dropout_transition=0,aresample=async=1000[outa]\" -c:v copy -map 1:v -map \"[outa]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. options.outputVideo .. "\"")
        end
    end
end

function StopGeneration(options, pluginSettings, functions)
    return true
end
