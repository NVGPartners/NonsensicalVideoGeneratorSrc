function Query(localeName, localizationTokens)
    return {}
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
