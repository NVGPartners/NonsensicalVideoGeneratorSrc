function Query()
    return {
        ["settings"] = {
            {
                ["name"] = "Display Name",
                ["value"] = "Random Sound",
                ["type"] = "label",
            },
            {
                ["name"] = "Description",
                ["value"] = "Applies a random sound effect",
                ["type"] = "label",
            },
            {
                ["name"] = "Mute Original Chance",
                ["tooltip"] = "Out of 100, chance to mute the original audio",
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
    functions.runFFmpeg("-i " .. randomSound .. " -y " .. soundEffect)
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
    if commandindex == 1 then
        if muteOriginalAudio then
            -- Override original audio with sfx (shortest)
            functions.runFFmpeg("-i " .. options.inputVideo .. " -i " .. soundEffect .. " -filter_complex \"[0:a]aformat=sample_fmts=fltp:sample_rates=44100:channel_layouts=stereo,volume=0.0[a0];[1:a]aformat=sample_fmts=fltp:sample_rates=44100:channel_layouts=stereo,volume=1.0[a1];[a0][a1]amerge=inputs=2[a]\" -map 0:v -map \"[a]\" -c:v copy -shortest -preset ultrafast -y " .. options.outputVideo)
        else
            -- Add sfx to original audio
            functions.runFFmpeg("-i " .. options.inputVideo .. " -i " .. soundEffect .. " -filter_complex \"[0:a]aformat=sample_fmts=fltp:sample_rates=44100:channel_layouts=stereo,volume=1.0[a0];[1:a]aformat=sample_fmts=fltp:sample_rates=44100:channel_layouts=stereo,volume=1.0[a1];[a0][a1]amerge=inputs=2[a]\" -map 0:v -map \"[a]\" -c:v copy -preset ultrafast -y " .. options.outputVideo)
        end
    end
end

function StopGeneration(options, pluginSettings, functions)
    return true
end
