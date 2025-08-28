-- [[ Boot Movie Template ]]
    -- These are the parameters that will be displayed in the addons tab.

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
                ["value"] = "bootmovie",
                ["type"] = "label"
            },
            {
                ["name"] = "Display Name",
                ["value"] = "%prettyname%",
                ["type"] = "label"
            },
            {
                ["name"] = "Description",
                ["value"] = "Boot movie (%filename%)",
                ["type"] = "label"
            }
        },
    }
end
