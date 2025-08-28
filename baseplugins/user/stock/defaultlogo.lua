function Query(localeName, localizationTokens)
    local localizedOption1 = localizationTokens["Addons:StockDefaultLogoOption1"] or "Default Logo"
    local localizedOption2 = localizationTokens["Addons:StockDefaultLogoOption2"] or "The default logo animation."
    return {
        ["settings"] = {
            {
                ["name"] = "Addon Type",
                ["value"] = "bootmovie",
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
