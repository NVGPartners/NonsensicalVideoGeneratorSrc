-- Welcome to your very own %filename% custom clip effect!

-- For more information on how to modify this file, please visit:
-- https://github.com/KiwifruitDev/NonsensicalVideoGenerator/wiki

-- This function tells the program what to display in the menu
-- alongside which user-customizable options to display.
function Query()
    return {
        -- Set up "plugin" settings for this effect.
        ["settings"] = {
            -- This is what the user will see in the effects tab.
            -- "Display Name" is always hidden from the settings menu.
            {
                ["name"] = "Display Name",
                ["value"] = "%prettyname%",
                ["type"] = "label"
            },
            -- Description is what the user will see once entering the effect settings.
            -- This is also the default description when publishing to the Workshop.
            {
                ["name"] = "Description",
                ["value"] = "This is %filename%.",
                ["type"] = "label"
            },
            -- Labels can be used to display text to the user in the settings menu.
            -- They are not customizable.
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
            -- int: Only accept integers.
            -- float: Only accept decimals.
            -- alphabetic: Only accept letters.
            -- alphanumeric: Only accept letters and numbers.
            -- string: Only accept letters, numbers, and spaces.
            -- bool: Only accept 1 or 0. (displayed as a switch)
            -- label: Display text to the user. (see labels above)
            -- any: Accept anything.
            -- This example option will be a number between 1 and 100.
            -- Tooltip will be displayed once the user hovers over the option.
            {
                ["name"] = "Chance Roll",
                ["tooltip"] = "Out of 100, chance for which type to apply.",
                ["value"] = "50",
                ["type"] = "int"
            }
        },
        -- If your effect would like to use a custom media directory, you can set it up here.
        -- Libraries are displayed in the Library tab of the program and store a specific type of media.
        -- When uploading an effect using this feature, make sure to tick the "Custom Library" tag.
        -- Remove the --[[ and ]]-- below to enable this feature.
        --[[
        ["libraries"] = {
            -- This sets up the "example" library to accept video files.
            -- The "name" is what the user will see in the Library tab.
            -- The "tooltip" is what the user will see once they hover over the library.
            -- The "path" is the internal directory name of the library.
            -- The "type" is the type of media that the library will accept.
            -- This can be "video" or "audio".
            {
                ["name"] = "Example",
                ["tooltip"] = "This is an example library.",
                ["path"] = "example",
                ["type"] = "video",
            }
        }
        ]]--
    }
end

-- These are where our default settings are stored.
-- If the user has not changed these settings, they will be used.
local chance = 50

-- Temporary files
local temp = ""

-- Variables for the effect to use and modify.
local speedUpOrDown = false

-- This function is where generation is started.
-- It is called once per clip and is passed a few variables.
-- options: A table containing the input and output video paths, alongside a few other things.
-- pluginSettings: A table containing the user-customizable settings for this effect.
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
    -- often the effect will be applied within the settings menu.
    speedUpOrDown = functions.randomInt(1, 100) <= chance and true or false
    
    -- Also, the temporary directory will be set.
    temp = "temp.mp4"

    -- Choose which effect to apply.
    -- Because this effect flips between two types of effects,
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

    -- If you return false, the console will report that this effect failed to apply.
    -- Because we successfully applied the effect, we're going to return true.
    return true
end
