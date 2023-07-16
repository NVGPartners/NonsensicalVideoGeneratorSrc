function Query()
    return {
        ["settings"] = {
            {
                ["name"] = "Display Name",
                ["value"] = "DSP",
                ["type"] = "label",
            },
            {
                ["name"] = "Description",
                ["value"] = "Applies vibrato or chorus audio effects.",
                ["type"] = "label",
            },
            {
                ["name"] = "Polarity",
                ["tooltip"] = "Vibrato is >50% chance when on, otherwise <50% chance.",
                ["value"] = "1",
                ["type"] = "bool",
            },
            {
                ["name"] = "Chance Roll",
                ["tooltip"] = "Out of 100, chance for which effect to apply.",
                ["value"] = "50",
                ["type"] = "int",
            },
        }
    }
end

-- Default settings
local vibratoGreaterThan50 = true
local chance = 50
local vibratoOrChorus = false

function StartGeneration(options, pluginSettings, functions)
    -- Set settings
    if pluginSettings["Polarity"] != nil then
        vibratoGreaterThan50 = tonumber(pluginSettings["Polarity"]) > 0
    end
    if pluginSettings["Chance Roll"] != nil then
        chance = tonumber(pluginSettings["Chance Roll"])
    end
    -- Set variables
    vibratoOrChorus = functions.randomInt(1, 100) <= chance and true or false
    -- Apply effect
    if vibratoOrChorus then
        -- Vibrato effect
        -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -af chorus="0.5:0.9:50|60|40:0.4|0.32|0.3:0.25|0.4|0.3:2|2.3|1.3" -preset veryfast -y "$output"}
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -af chorus=\"0.5:0.9:50|60|40:0.4|0.32|0.3:0.25|0.4|0.3:2|2.3|1.3\" -preset veryfast -y \"" .. options.outputVideo .. "\"")
    else
        -- Chorus effect
        -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -af vibrato=f=7.0:d=0.5 -preset veryfast -y "$output"}
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -af vibrato=f=7.0:d=0.5 -preset veryfast -y \"" .. options.outputVideo .. "\"")
    end
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
end

function StopGeneration(options, pluginSettings, functions)
    return true
end
