function Query(localeName, localizationTokens)
    local localizedOption1 = localizationTokens["Addons:StockReverseOption1"] or "Reverse"
    local localizedOption2 = localizationTokens["Addons:StockReverseOption2"] or "Reverses or forward-reverses a clip."
    local localizedOption3 = localizationTokens["Addons:StockReverseOption3"] or "Out of 100, chance for which type to apply."
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

-- Temp files
local temp2 = "temp2.mp4"
local temp3 = "temp3.mp4"

-- Variables
local forwardNormal = false
local length = 0
local halfLength = 0

function StartGeneration(options, pluginSettings, functions)
    -- Set settings
    if pluginSettings["Chance Roll"] != nil then
        chance = tonumber(pluginSettings["Chance Roll"])
    end
    -- Set temp files
    -- Set variables
    forwardNormal = functions.randomInt(1, 100) <= chance and true or false
    length = 0
    halfLength = 0
    -- Apply effect
    if forwardNormal then
        -- Reverse effect
        -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -vf reverse -af areverse -preset veryfast -y "$output"}
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -vf reverse -af areverse -preset veryfast -y \"" .. options.outputVideo .. "\"")
    else
        -- Forward-reverse effect
        -- Invoke-Command -ScriptBlock {&$ffprobe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 "$video"}
        functions.runFFprobe("-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"" .. options.inputVideo .. "\"")
    end
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
    if not forwardNormal then
        if commandindex == 1 then
            length = tonumber(outputresult)
            halfLength = length / 2
            -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -t $halfLength -preset veryfast -y "$temp2"}
            functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -t " .. halfLength .. " -preset veryfast -y \"" .. temp2 .. "\"")
        elseif commandindex == 2 then
            -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -vf reverse -af areverse -preset veryfast -y "$temp3"}
            functions.runFFmpeg("-i \"" .. temp2 .. "\" -vf reverse -af areverse -preset veryfast -y \"" .. temp3 .. "\"")
        elseif commandindex == 3 then
            -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -i "$temp3" -filter_complex "[0:v:0][0:a:0][1:v:0][1:a:0]concat=n=2:v=1:a=1[v][a]" -map "[v]" -map "[a]" -preset veryfast -y "$output"}
            functions.runFFmpeg("-i \"" .. temp2 .. "\" -i \"" .. temp3 .. "\" -filter_complex \"[0:v:0][0:a:0][1:v:0][1:a:0]concat=n=2:v=1:a=1[v][a]\" -map \"[v]\" -map \"[a]\" -preset veryfast -y \"" .. options.outputVideo .. "\"")
        end
    end
end

function StopGeneration(options, pluginSettings, functions)
    return true
end
