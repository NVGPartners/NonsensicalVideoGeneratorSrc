function Query(localeName, localizationTokens)
    local localizedOption1 = localizationTokens["Addons:StockDSPOption1"] or "DSP"
    local localizedOption2 = localizationTokens["Addons:StockDSPOption2"] or "Applies vibrato or chorus audio effects."
    local localizedOption3 = localizationTokens["Addons:StockDSPOption3"] or "Out of 100, chance for which type to apply."
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
                ["name"] = "Chance Roll",
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
local vibratoOrChorus = false

function StartGeneration(options, pluginSettings, functions)
    -- Set settings
    if pluginSettings["Chance Roll"] != nil then
        chance = tonumber(pluginSettings["Chance Roll"])
    end
    -- Set variables
    vibratoOrChorus = functions.randomInt(1, 100) <= chance and true or false
    -- Apply effect
    if vibratoOrChorus then
        -- Vibrato effect
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -af chorus=\"0.7:0.9:55:0.4:0.25:2\" -preset veryfast -y \"" .. options.outputVideo .. "\"")
    else
        -- Chorus effect
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -af vibrato=f=6.5:d=0.5 -preset veryfast -y \"" .. options.outputVideo .. "\"")
    end
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
end

function StopGeneration(options, pluginSettings, functions)
    return true
end
