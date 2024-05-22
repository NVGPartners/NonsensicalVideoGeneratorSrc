-- [[ Theme Template ]]
    -- These are the parameters that will be displayed in the addons tab.
    -- When creating a theme, the entire default theme is copied over and you can edit it from there.

    -- You should delete any graphics or SFX that have not been changed, as they will be loaded from the default theme.

    -- For music, you can have up to 9 songs as "theme1.wma" through "theme9.wma". Only the music included in your theme will play.
    -- All assets are stored inside of your addon's folder under "layer".

    -- For more information on how to modify this file, please visit:
    -- https://github.com/KiwifruitDev/NonsensicalVideoGenerator/wiki

    -- If you need help, feel free to join the NVG Discord server:
    -- https://discord.gg/8ppmspR6Wh

-- [[ Localization ]]
    -- This table is used to store the localization for your addon.
    -- The key is the locale name, and the value is a table containing the localization tokens.
    -- If a locale is not found, it will default to "en_us".

    -- Supporting multiple languages is not required, but the option is available if you wish to do so.

    -- Please keep this format consistent to ensure community contributions are possible.
    -- "Addons:Custom%filenameraw%Option1" - Replace "%filenameraw%" if you decide to change the filename.
local customLocalization = {
    ["en_us"] = {
        ["Addons:Custom%filenameraw%Option1"] = "%prettyname%",
        ["Addons:Custom%filenameraw%Option2"] = "This is %filename%.",
    },
}

function Query(localeName, localizationTokens)
    -- Get the localization table for the current locale.
    -- Otherwise, default to en_us.
    local localization = customLocalization[localeName] or addonLocalization["en_us"]

    -- Community-contributed localization tokens are added, if available.
    -- Please preserve localization in order to support multiple languages,
    -- even if you don't plan on translating the addon yourself.
    for k, v in pairs(localizationTokens) do
        localization[k] = v
    end

    -- Return the queried settings.
    return {
        ["settings"] = {
            {
                ["name"] = "Addon Type",
                ["value"] = "theme",
                ["type"] = "label"
            },
            {
                ["name"] = "Display Name",
                ["value"] = localization["Addons:Custom%filenameraw%Option1"],
                ["type"] = "label"
            },
            {
                ["name"] = "Description",
                ["value"] = localization["Addons:Custom%filenameraw%Option2"],
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
            ["PluginEntryEffectPluginsPage"] = {192, 192, 255, 255},
            ["PluginEntryEffectAltPluginsPage"] = {128, 128, 192, 255},
            ["PluginEntryPostRenderEffectPluginsPage"] = {128, 255, 128, 255},
            ["PluginEntryPostRenderEffectAltPluginsPage"] = {64, 192, 64, 255},
            ["PluginEntryThemePluginsPage"] = {255, 128, 128, 255},
            ["PluginEntryThemeAltPluginsPage"] = {192, 64, 64, 255},
            ["BackgroundOverlayScreen"] = {128, 128, 128, 255},
            ["BackgroundLibraryPage"] = {128, 128, 128, 255},
            ["ObstaclePastimeGameScreen"] = {0, 0, 0, 255},
            ["BackgroundScreen"] = {64, 64, 64, 255},
            ["TileBackgroundScreen"] = {102, 102, 102, 255},
            ["VideoPlayerProgressBar"] = {255, 0, 0, 255},
            ["VideoPlayerProgressBarBackground"] = {0, 0, 0, 0},
        }
    }
end
