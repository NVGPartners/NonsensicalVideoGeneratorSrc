-- These are the parameters that will be displayed in the addons tab.
-- When creating a theme, the entire default theme is copied over and you can edit it from there.
-- You should delete any graphics or SFX that have not been changed, as they will be loaded from the default theme.
-- For music, you can have up to 9 songs as "theme1.wma" through "theme9.wma". Only the music included in your theme will play.
-- All assets are stored inside of your addon's folder under "layer".

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

-- Here is where you can set the colors for your theme.
-- All colors are in RGBA format, with each value being from 0 to 255.
-- Do not set colors to pure transparent (0, 0, 0, 0) as they will not work.
-- If you want to use the default color for a specific element, just remove it from the table.
-- This function is entirely optional and can be deleted if you don't want to use it.
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
