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
            ["ClearColor"] = {0, 0, 0, 255},
            ["ShadowActionButtonInteractable"] = {0, 0, 0, 255},
            ["ShadowButtonInteractable"] = {0, 0, 0, 255},
            ["ShadowDialInteractable"] = {0, 0, 0, 255},
            ["ShadowSwitchInteractable"] = {0, 0, 0, 255},
            ["ShadowTextInputInteractable"] = {0, 0, 0, 255},
            ["BackgroundTooltip"] = {0, 0, 0, 255},
            ["BackgroundConsoleScreen"] = {0, 0, 0, 255},
            ["OverlayContentScreen"] = {0, 0, 0, 96},
            ["VideoHolderStaticAnimFilledLibraryPage"] = {64, 64, 64, 255},
            ["VideoHolderAddOverlayLibraryPage"] = {255, 255, 255, 128},
            ["PluginEntryGenericPluginsPage"] = {255, 255, 255, 255},
            ["PluginEntryGenericAltPluginsPage"] = {192, 192, 192, 255},
            ["PluginEntryEffectPluginsPage"] = {255, 255, 255, 255},
            ["PluginEntryEffectAltPluginsPage"] = {192, 192, 192, 255},
            ["PluginEntryPostRenderEffectPluginsPage"] = {255, 255, 255, 255},
            ["PluginEntryPostRenderEffectAltPluginsPage"] = {192, 192, 192, 255},
            ["PluginEntryThemePluginsPage"] = {255, 255, 255, 255},
            ["PluginEntryThemeAltPluginsPage"] = {255, 255, 255, 255},
            ["BackgroundOverlayScreen"] = {128, 128, 128, 255},
            ["BackgroundLibraryPage"] = {128, 128, 128, 255},
            ["ObstaclePastimeGameScreen"] = {0, 0, 0, 255},
            ["BackgroundScreen"] = {64, 64, 64, 255},
            ["TileBackgroundScreen"] = {102, 102, 102, 255},
            ["VideoPlayerProgressBar"] = {0, 128, 255, 255},
            ["VideoPlayerProgressBarBackground"] = {255, 255, 255, 255},
        }
    }
end
