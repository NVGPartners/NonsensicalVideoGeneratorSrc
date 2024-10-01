function Query(localeName, localizationTokens)
    local localizedOption1 = localizationTokens["Addons:StockVocoderOption1"] or "Vocoder"
    local localizedOption2 = localizationTokens["Addons:StockVocoderOption2"] or "Makes speech sound cool."
    local localizedOption3 = localizationTokens["Addons:StockVocoderOption3"] or "Carrier sounds for Vocoder effect."
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
            }
        },
        ["libraries"] = {
            {
                ["name"] = localizedOption1,
                ["tooltip"] = localizedOption3,
                ["path"] = "vocoder",
                ["type"] = "audio",
            }
        }
    }
end

-- Temp files
local modulator = "modulator.wav"
local vocoded = "vocoded.wav"

-- Variables
local carrier = ""
local success = false

function StartGeneration(options, pluginSettings, functions)
    -- Set settings
    success = false
    -- Set variables
    carrier = functions.getRandomLibraryFile("audio", "vocoder")
    -- fail if no carrier
    if carrier == "" then
        print("No carrier found")
        return false
    end
    -- Apply effect
    functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -vn -acodec pcm_s16le -ar 44100 -ac 1 -y \"" .. modulator .. "\"")
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
    if commandindex == 1 then
        functions.runVocoder("-b 1024 \"" .. modulator .. "\" \"" .. carrier .. "\" \"" .. vocoded .. "\"")
    elseif commandindex == 2 then
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -i \"" .. vocoded .. "\" -filter:v \"frei0r=filter_name=glow\" -map 0:v -map 1:a -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. options.outputVideo .. "\"")
        success = true
    end
end

function StopGeneration(options, pluginSettings, functions)
    return success
end
