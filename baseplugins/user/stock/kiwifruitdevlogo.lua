function Query(localeName, localizationTokens)
    local localizedOption1 = localizationTokens["Addons:StockKiwifruitDevLogoOption1"] or "KiwifruitDev Logo"
    local localizedOption2 = localizationTokens["Addons:StockKiwifruitDevLogoOption2"] or "The KiwifruitDev logo animation."
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
