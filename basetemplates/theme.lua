-- These parameters are the only things that themes can use for now.
-- When creating a theme, the entire default theme is copied over and you can edit it from there.
-- You should delete any graphics or SFX that have not been changed, as they will be loaded from the default theme.
-- For music, you can have up to 9 songs as "theme1.wma" through "theme9.wma". Only the music included in your theme will play.
-- All assets are stored inside of your addon's folder under "layer".
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
