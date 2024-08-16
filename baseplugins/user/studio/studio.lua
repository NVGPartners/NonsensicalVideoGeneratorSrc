local customLocalization = {
    ["en_us"] = {
        ["Addons:CustomStudioOption1"] = "Studio",
        ["Addons:CustomStudioOption2"] = "No rounded corners, little color.",
        ["Addons:CustomStudioOption3"] = "Also replaces SFX.",
    },
}

function Query(localeName, localizationTokens)
    local localization = customLocalization[localeName] or customLocalization["en_us"]
    for k, v in pairs(localizationTokens) do
        localization[k] = v
    end
    return {
        ["settings"] = {
            {
                ["name"] = "Addon Type",
                ["value"] = "theme",
                ["type"] = "label"
            },
            {
                ["name"] = "Display Name",
                ["value"] = localization["Addons:CustomStudioOption1"],
                ["type"] = "label"
            },
            {
                ["name"] = "Description",
                ["value"] = localization["Addons:CustomStudioOption2"],
                ["type"] = "label"
            },
            {
                ["name"] = "Label1",
                ["value"] = localization["Addons:CustomStudioOption3"],
                ["type"] = "label"
            },
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
            ["VideoPlayerProgressBar"] = {64, 64, 64, 255},
            ["VideoPlayerProgressBarBackground"] = {0, 0, 0, 0},
        }
    }
end
