function Query(localeName, localizationTokens)
    local localizedOption1 = localizationTokens["Addons:StockColorblindOption1"] or "Colorblind"
    local localizedOption2 = localizationTokens["Addons:StockColorblindOption2"] or "A colorblind-friendly theme."
    return {
        ["settings"] = {
            {
                ["name"] = "Addon Type",
                ["value"] = "theme",
                ["type"] = "label"
            },
            {
                ["name"] = "Display Name",
                ["value"] = localizedOption1,
                ["type"] = "label"
            },
            {
                ["name"] = "Description",
                ["value"] = localizedOption2,
                ["type"] = "label"
            }
        },
    }
end

function ThemeMetadata(pluginSettings)
    return {
        ["colorTable"] = {
            ["VideoPlayerProgressBar"] = {0, 128, 255, 255},
            ["VideoPlayerProgressBarBackground"] = {255, 255, 255, 255},
        }
    }
end
