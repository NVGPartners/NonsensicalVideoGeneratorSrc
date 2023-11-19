function Query()
    return {
        ["settings"] = {
            {
                ["name"] = "Addon Type",
                ["value"] = "theme",
                ["type"] = "label"
            },
            {
                ["name"] = "Display Name",
                ["value"] = "KiwifruitDev",
                ["type"] = "label"
            },
            {
                ["name"] = "Description",
                ["value"] = "A green theme.",
                ["type"] = "label"
            },
            {
                ["name"] = "Label1",
                ["value"] = "Settings only apply after reloading.",
                ["type"] = "label"
            },
            {
                ["name"] = "No Background",
                ["value"] = "0",
                ["type"] = "bool"
            }
        },
    }
end

function ThemeMetadata(pluginSettings)
    local colorTable = {}
    if pluginSettings["No Background"] == "0" then
        colorTable["BackgroundScreen"] = {100, 122, 58, 255}
        colorTable["TileBackgroundScreen"] = {134, 165, 79, 255}
    end
    return {
        ["colorTable"] = colorTable
    }
end
