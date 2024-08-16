function Query()
    return {
        ["settings"] = {
            {
                ["name"] = "Addon Type",
                ["value"] = "effect",
                ["type"] = "label"
            },
            {
                ["name"] = "Display Name",
                ["value"] = "Spadinner",
                ["type"] = "label"
            },
            {
                ["name"] = "Description",
                ["value"] = "Applies several SFX over time.",
                ["type"] = "label"
            },
            {
                ["name"] = "Author",
                ["value"] = "Effect idea by Waltman13.",
                ["type"] = "label"
            },
            {
                ["name"] = "Min Delay",
                ["tooltip"] = "In seconds, minimum delay between SFX.",
                ["value"] = "0.1",
                ["type"] = "float"
            },
            {
                ["name"] = "Max Delay",
                ["tooltip"] = "In seconds, maximum delay between SFX.",
                ["value"] = "0.75",
                ["type"] = "float"
            },
            {
                ["name"] = "Min SFX Count",
                ["tooltip"] = "How many SFX to apply at minimum.",
                ["value"] = "1",
                ["type"] = "int"
            },
            {
                ["name"] = "Max SFX Count",
                ["tooltip"] = "How many SFX to apply at maximum.",
                ["value"] = "3",
                ["type"] = "int"
            },
            {
                ["name"] = "Use Spadinner Library",
                ["tooltip"] = "If disabled, the Sound FX library will be used instead.",
                ["value"] = "1",
                ["type"] = "bool"
            },
        },
        ["libraries"] = {
            {
                ["name"] = "Spadinner",
                ["tooltip"] = "Optional sound effects for spadinner.",
                ["path"] = "spadinner",
                ["type"] = "audio",
            }
        }
    }
end

-- Default settings
local mindelay = 1
local maxdelay = 1
local minsfxcount = 1
local maxsfxcount = 3
local useSpadinnerLibrary = true

-- Parameters
local material = ""
local spadinners = {}
local convertedspadinners = {}
local lengths = {}
local starttime = 0
local sfxcount = 0
local materialclip = "materialclip.mp4"
local combinedspadinner = "combinedspadinner.mp3"
local silentaudio = "silence.mp3"
local lastindex = 0

function StartGeneration(options, pluginSettings, functions)
    lastindex = 0
    -- Get user settings
    if pluginSettings["Min Delay"] != nil then
        mindelay = tonumber(pluginSettings["Min Delay"])
    end
    if pluginSettings["Max Delay"] != nil then
        maxdelay = tonumber(pluginSettings["Max Delay"])
    end
    if pluginSettings["Min SFX Count"] != nil then
        minsfxcount = tonumber(pluginSettings["Min SFX Count"])
    end
    if pluginSettings["Max SFX Count"] != nil then
        maxsfxcount = tonumber(pluginSettings["Max SFX Count"])
    end
    if pluginSettings["Use Spadinner Library"] != nil then
        useSpadinnerLibrary = tonumber(pluginSettings["Use Spadinner Library"]) == 1
    end

    -- Get library files
    sfxcount = functions.randomInt(minsfxcount, maxsfxcount)
    for i = 1, sfxcount do
        if useSpadinnerLibrary then
            spadinners[i] = functions.getRandomLibraryFile("audio", "spadinner")
        else
            spadinners[i] = functions.getRandomLibraryFile("audio", "sfx")
        end
    end
    material = functions.getRandomLibraryFile("video", "materials")

    -- Create silent audio file at maxdelay
    functions.runFFmpeg("-f lavfi -i anullsrc=channel_layout=stereo:sample_rate=44100 -t " .. maxdelay .. " -c:a libmp3lame -b:a 192k \"" .. silentaudio .. "\"")
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
    print(commandindex .. " " .. lastindex .. " " .. sfxcount)
    -- delay, spadinner, delay, spadinner, delay, spadinner, delay ...
    if commandindex == 1 then
        -- Get the length of the material
        functions.runFFprobe("-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"" .. material .. "\"")
    elseif commandindex == 2 then
        -- outputResult is the duration of the material
        starttime = functions.randomDouble(0, tonumber(outputresult))
        -- Trim material to start at starttime and resize to options.width x options.height at 30 fps
        functions.runFFmpeg("-ss " .. starttime .. " -i \"" .. material .. "\" -filter:v \"fps=30,scale=" .. options.width .. ":" .. options.height .. "\" -c:a copy -c:v libx264 -preset ultrafast -crf 0 -y \"" .. materialclip .. "\"")
    elseif commandindex > 2 and commandindex <= sfxcount + 2 then
        -- Convert all spadinners to 44100hz, 224k bitrate, and mp3
        local index = commandindex - 2
        local spadinner = spadinners[index]
        local convertedspadinner = index .. ".mp3"
        convertedspadinners[index] = convertedspadinner
        functions.runFFmpeg("-i \"" .. spadinner .. "\" -b:a 224k -ar 44100 -c:a libmp3lame -y \"" .. convertedspadinner .. "\"")
        lastindex = lastindex + 1
    elseif commandindex-lastindex > 2 and commandindex-lastindex <= sfxcount + 2 then
        local delay = functions.randomDouble(mindelay, maxdelay)
        local index = commandindex-lastindex - 2
        local spadinner = convertedspadinners[index]
        -- concat silence with spadinner as 1.mp3 and so on
        local startdifference = maxdelay - delay
        -- always use 41khz and 224k bitrate
        functions.runFFmpeg("-ss " .. startdifference .. " -i \"concat:" .. silentaudio .. "|" .. spadinner .. "\" -b:a 224k -ar 44100 -c:a libmp3lame -y \"" .. index .. ".mp3\"")
    elseif commandindex-lastindex == sfxcount + 3 then
        -- Concatenate all spadinner files
        local concat = ""
        for i = 1, sfxcount do
            concat = concat .. i .. ".mp3|"
        end
        concat = concat:sub(1, -2)
        functions.runFFmpeg("-i \"concat:" .. concat .. "\" -acodec copy \"" .. combinedspadinner .. "\"")
    elseif commandindex-lastindex == sfxcount + 4 then
        -- Combine material and spadinner
        functions.runFFmpeg("-async 30 -i \"" .. materialclip .. "\" -i \"" .. combinedspadinner .. "\" -filter_complex \"[0:a]volume=1[0a];[1:a]volume=1.5[1a];[0a][1a]amix=inputs=2:duration=shortest[out]\" -map \"[out]\" -map 0:v -shortest -c:v libx264 -preset ultrafast -crf 0 -y \"" .. options.outputVideo .. "\"")
    end
end

function StopGeneration(options, pluginSettings, functions)
    return true
end
