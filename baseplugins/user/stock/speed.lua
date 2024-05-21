function Query(localeName, localizationTokens)
    local localizedOption1 = localizationTokens["Addons:StockSpeedOption1"] or "Speed"
    local localizedOption2 = localizationTokens["Addons:StockSpeedOption1"] or "Speeds up or down the clip."
    local localizedOption3 = localizationTokens["Addons:StockSpeedOption2"] or "Out of 100, chance for which type to apply."
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
local speedUpOrDown = false

function StartGeneration(options, pluginSettings, functions)
    -- Set settings
    if pluginSettings["Chance Roll"] != nil then
        chance = tonumber(pluginSettings["Chance Roll"])
    end
    -- Set variables
    speedUpOrDown = functions.randomInt(1, 100) <= chance and true or false
    -- Apply effect
    if speedUpOrDown then
        -- Speed up effect
        -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -filter:v setpts=0.5*PTS -filter:a atempo=2.0 -preset veryfast -y "$output"}
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -filter:v setpts=0.5*PTS -filter:a atempo=2.0 -preset veryfast -y \"" .. options.outputVideo .. "\"")
    else
        -- Slow down effect
        -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -filter:v setpts=2.0*PTS -filter:a atempo=0.5 -preset veryfast -y "$output"}
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -filter:v setpts=2.0*PTS -filter:a atempo=0.5 -preset veryfast -y \"" .. options.outputVideo .. "\"")
    end
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
end

function StopGeneration(options, pluginSettings, functions)
    return true
end
