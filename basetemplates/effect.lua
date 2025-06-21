-- [[ Effect Template ]]
    -- Welcome to your very own %filename% custom addon!

    -- For more information on how to modify this file, please visit:
    -- https://github.com/KiwifruitDev/NonsensicalVideoGenerator/wiki

    -- If you need help, feel free to join the NVG Discord server:
    -- https://discord.gg/8ppmspR6Wh

    -- Please edit this file in a code editor such as VS Code for syntax highlighting.

-- This function tells the program what to display in the menu, alongside which user-customizable options to display.
function Query(localeName, localizationTokens)
    -- Return the queried settings.
    return {
        -- Set up settings for this addon.
        ["settings"] = {
            -- This determines the type of addon this is.
            -- Only these values are allowed:
            --   effect             An effect that gets applied at random.
            --   postrendereffect   An effect that gets applied after rendering.
            --   theme              A theme for the software. Only Query() is called.
            --   bootmovie          A boot movie that plays when the software starts. Must include %filenamenoext%.mp4 in the addon directory.
            {
                ["name"] = "Addon Type",
                ["value"] = "effect",
                ["type"] = "label"
            },
            -- This is what the user will see in the addons tab.
            -- Do not append "Effect", "Theme", or "Post-Render Effect" as this is done automatically and would support localization.
            -- "Display Name" and "Addon Type" are not shown on the settings menu.
            {
                ["name"] = "Display Name",
                ["value"] = "%prettyname%",
                ["type"] = "label"
            },
            -- Description is a short blurb about the addon, use extra labels for more information.
            {
                ["name"] = "Description",
                ["value"] = "This is %filename%.",
                ["type"] = "label"
            },
            -- Labels can be used to display text to the user in the settings menu.
            -- They cannot have tooltips.
            -- All labels are added to the Steam Workshop description, separated by newlines.
            {
                ["name"] = "Label1",
                ["value"] = "This effect will perform a",
                ["type"] = "label"
            },
            {
                ["name"] = "Label2",
                ["value"] = "speed up/down on the clip.",
                ["type"] = "label"
            },
            -- Here is where we can add user-customizable options.
            -- These are the types of options that can be added:
            --   int            Only accept integers.
            --   float          Only accept decimals.
            --   alphabetic     Only accept letters.
            --   alphanumeric   Only accept letters and numbers.
            --   string         Only accept letters, numbers, and spaces.
            --   bool           Only accept 1 or 0. (displayed as a switch)
            --   label          Read-only text for the user. (see labels above)
            --   any            Accept any text.
            --   button         Runs the function named after the value when clicked. (displayed as a button)
            -- This example option will be a number between 1 and 100.
            -- Tooltips will be displayed once the user hovers over the option.
            {
                ["name"] = "Chance Roll",
                ["tooltip"] = "Out of 100, chance for which type to apply.",
                ["value"] = "50",
                ["type"] = "int"
            }
            -- To create a button, use the "button" type and this format:
            -- {
            --     ["name"] = "Test Button",
            --     ["tooltip"] = "Print a message to the console.",
            --     ["type"] = "button"
            -- }
        },
        -- If your addon would like to use a custom media directory, you can set it up here.
        -- Libraries are displayed in the Library tab of the program and store a specific type of media.
        -- When uploading an addon using this feature, make sure to tick the "Custom Library" tag.
        -- Use functions.getRandomLibraryFile("video", "example") to pick from this library.
        -- Remove the --[[ and ]] below to enable this feature.
        --[[
        ["libraries"] = {
            -- This sets up the "example" library to accept video files.
            -- The "name" is what the user will see in the Library tab.
            -- The "tooltip" is what the user will see once they hover over the library.
            -- The "path" is the internal directory name of the library.
            -- The "type" is the type of media that the library will accept.
            -- This can be "video", "audio", or "image".
            -- The "preventToggle" option will prevent the user from toggling media on and off.
            -- The "preventImport" option will prevent the user from importing media into the library.
            -- The "preventDownload" option will prevent the user from downloading media into the library.
            -- Set these to true if you want to limit how the user can interact with the library.
            {
                ["name"] = "Example",
                ["tooltip"] = "This is an example library.",
                ["path"] = "example",
                ["type"] = "video",
                ["preventToggle"] = false,
                ["preventImport"] = false,
                ["preventDownload"] = false
            }
        }
        ]]
    }
end

-- These are where our default settings are stored.
-- If the user has not changed these settings, they will be used.
local chance = 50

-- Temporary files
local temp = "temp.mp4"

-- Variables for the effect to use and modify.
local speedUpOrDown = false
local finished = false

-- This function is where generation is started.
-- It is called once per clip and is passed a few variables.
-- If "postrendereffect" is selected as the addon type, options.inputVideo will be the rendered video.
-- options: A table containing the input and output video paths, alongside a few other things.
-- pluginSettings: A table containing the user-customizable settings for this addon.
-- functions: A table containing useful functions, such as calling FFmpeg and generating random numbers.
function StartGeneration(options, pluginSettings, functions)
    -- Let's breakdown what we're doing here.
    -- We're going to get the user's customizable settings.
    -- Then, we'll set our variables accordingly.
    -- Finally, we'll start applying the effect.

    -- After each call to an external command (FFmpeg, FFprobe, etc.),
    -- the PostCommand function is needed to get the results of such.
    -- This means that you cannot set a variable to an FFprobe call, for example.

    -- All commands are filtered too, so special characters are limited to being
    -- inside of quotes. This is to prevent malicious code from being executed.

    -- Let's start!

    -- Get our user-customizable settings.
    if pluginSettings["Chance Roll"] != nil then
        chance = tonumber(pluginSettings["Chance Roll"])
    end

    -- Set local variables.
    -- Here, we're going to generate a random number between 1 and 100.
    -- Using the chance variable, the user can select the weight of how
    -- often the addon will be applied within the settings menu.
    speedUpOrDown = functions.randomInt(1, 100) <= chance and true or false
    finished = false -- Variables may be persisted between runs, so we reset it here.

    -- Choose which effect to apply.
    -- Because this addon flips between two types of effects,
    -- we're going to use a boolean to determine which one to apply.
    if speedUpOrDown then
        -- Speed up was chosen.
        -- FFmpeg will speed up the video and audio to 2x speed.
        -- Note: setpts is not the same as tempo.
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -filter:v setpts=0.5*PTS -filter:a atempo=2.0 -preset veryfast -y \"" .. temp .. "\"")
    else
        -- Slow down was chosen.
        -- FFmpeg will slow down the video and audio to 0.5x speed.
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -filter:v setpts=2.0*PTS -filter:a atempo=0.5 -preset veryfast -y \"" .. temp .. "\"")
    end
    return true
end

-- Here, we're going to tell the program what to do once the temporary file has been generated.
-- This function is called after every single run* function.
-- commandindex: The index of the command that was run. Sequentually speaking, each run* function is plus one index.
-- outputresult: The output of the command that was run. For FFprobe and others, this is the console output.
-- errorresult: The error of the command that was run. For FFmpeg, this is the console output.
function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
    -- We can still use our previous variables, so let's apply a special case after the last run* function.

    -- Command index 1 refers to both FFmpeg calls because they only run once.
    if commandindex == 1 then
        if speedUpOrDown then
            -- We sped up earlier, so apply a brightness/contrast filter.
            -- This will make the video brighter and more contrasted.
            -- Since this is the last run* function, we can just use the output video path.
            functions.runFFmpeg("-i \"" .. temp .. "\" -vf eq=brightness=0.5:contrast=0.5 -preset veryfast -y \"" .. options.outputVideo .. "\"")
        else
            -- We slowed down earlier, so apply a negative filter.
            -- This will make the video's colors inverted.
            functions.runFFmpeg("-i \"" .. temp .. "\" -vf negate -preset veryfast -y \"" .. options.outputVideo .. "\"")
        end
    elseif commandindex == 2 then
        -- Now that we have run the last command, let's demonstrate printing to the in-app console.
        -- This is useful for debugging, but can also be used to display information to the user.
        -- The prefix "<[R,G,B]>" is optional, but it can be used to color the text.
        -- The default color is a dark cyan.
        print("<[255,0,0]>This is red text!")
        print("<[0,255,0]>This is green text!")
        print("<[0,0,255]>This is blue text!")
        print("This is default text!")
        finished = true -- Set the finished variable to true to indicate that the effect has been applied.
    end
end

-- StopGeneration is run after all run* functions and PostCommands have been run.
-- This is where you can do something after the effect has been applied.
function StopGeneration(options, pluginSettings, functions)
    -- Nothing is usually done here, but you can do something if you want.

    -- Let's print out which effect we ended up using.
    if speedUpOrDown then
        print("Speed up was chosen!")
    else
        print("Slow down was chosen!")
    end

    -- If you return false, the console will report that this addon failed to apply.
    -- We're returning the finished variable to indicate whether the effect was applied successfully.
    return finished
end

-- See the options above for how to create a button, this function will be called when the button is clicked.
-- function ButtonInteraction(options, pluginSettings, functions, mouseX, mouseY, buttonName)
--    print("Hello from " .. buttonName .. "! Mouse coordinates: (" .. mouseX .. ", " .. mouseY .. ")")
-- end
