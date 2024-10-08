function Query(localeName, localizationTokens)
    return {
        ["settings"] = {
            {
                ["name"] = "Addon Type",
                ["value"] = "effect",
                ["type"] = "label"
            },
            {
                ["name"] = "Display Name",
                ["value"] = "%prettyname%",
                ["type"] = "label"
            },
            {
                ["name"] = "Description",
                ["value"] = "Speeds up the video by 2x.",
                ["type"] = "label"
            }
        }
    }
end

function StartGeneration(options, pluginSettings, functions)
    functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -filter:v setpts=0.5*PTS -filter:a atempo=2.0 -preset veryfast -y \"" .. options.outputVideo .. "\"")
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
end

function StopGeneration(options, pluginSettings, functions)
    return true
end
