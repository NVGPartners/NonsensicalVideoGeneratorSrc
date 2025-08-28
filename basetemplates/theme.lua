-- [[ Theme Template ]]
    -- These are the parameters that will be displayed in the addons tab.
    -- When creating a theme, the entire default theme is copied over and you can edit it from there.

    -- You should delete any graphics or SFX that have not been changed, as they will be loaded from the default theme.
    
    -- All assets are stored inside of your addon's folder under "layer".

    -- For music, you can have up to 9 songs as "theme1.wma" through "theme9.wma". These files may be in .mp3 or .ogg format. (.wma is deprecated)
    -- Only the music included in your theme will play, otherwise the default theme music will play.
    -- This also applies to the misclick graphics in the "misclick" folder, you can have from 1-9 misclick graphics.
    -- If any music and/or misclick graphics are found, the default music and/or misclick graphics will not be loaded.

    -- For more information on how to modify this file, please visit:
    -- https://github.com/NVGPartners/NonsensicalVideoGenerator/wiki

    -- If you need help, feel free to join the NVG Discord server:
    -- https://discord.gg/8ppmspR6Wh

function Query(localeName, localizationTokens)
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
                ["value"] = "%prettyname%",
                ["type"] = "label"
            },
            {
                ["name"] = "Description",
                ["value"] = "This is %filename%.",
                ["type"] = "label"
            }
        },
    }
end

-- When this theme gets loaded, these settings will be applied.
-- This function is entirely optional and can be deleted if you don't want to use it.
function ThemeMetadata(pluginSettings)
    return {
        -- Here is where you can set the colors for your theme.
        -- All colors are in RGBA format, with each value being from 0 to 255.
        -- Do not set colors to pure transparent (0, 0, 0, 0) as they will not work.
        -- If you want to use the default color for a specific element, just remove it from the table.
        ["colorTable"] = {
            ["ClearColor"] = {0, 0, 0, 255},
            ["ShadowActionButtonInteractable"] = {0, 0, 0, 255},
            ["ShadowButtonInteractable"] = {0, 0, 0, 255},
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
            ["PluginEntryBootMoviePluginsPage"] = {255, 255, 0, 255},
            ["PluginEntryBootMovieAltPluginsPage"] = {192, 192, 0, 255},
            ["BackgroundOverlayScreen"] = {128, 128, 128, 255},
            ["BackgroundLibraryPage"] = {128, 128, 128, 255},
            ["ObstaclePastimeGameScreen"] = {0, 0, 0, 255},
            ["BackgroundScreen"] = {64, 64, 64, 255},
            ["TileBackgroundScreen"] = {102, 102, 102, 255},
            ["VideoPlayerProgressBar"] = {255, 0, 0, 255},
            ["VideoPlayerProgressBarBackground"] = {0, 0, 0, 0},
        },
        -- Set proper names for each of the music tracks.
        -- These names will be displayed in the top center of the screen when the music is playing.
        ["musicattributions"] = {
            "Bobby I Guess - A Nonsensical Song",
            "Bobby I Guess - A Nonsensical Song Yet Again",
            "Bobby I Guess - Creation",
            "Bobby I Guess - Where We Began",
            "Bobby I Guess - Jazzy",
        }
    }
end
