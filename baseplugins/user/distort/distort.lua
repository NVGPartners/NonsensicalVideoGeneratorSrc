function Query()
    return {
        ["settings"] = {
            {
                ["name"] = "Display Name",
                ["value"] = "Distort",
                ["type"] = "label",
            },
            {
                ["name"] = "Description",
                ["value"] = "Distorts still frames to the beat.",
                ["type"] = "label",
            },
            {
                ["name"] = "Label1",
                ["value"] = "Uses the Distort library.",
                ["type"] = "label",
            }
        },
        ["libraries"] = {
            {
                ["name"] = "Distort",
                ["tooltip"] = "Audio clips in rhythm.",
                ["path"] = "distort",
                ["type"] = "audio",
            }
        }
    }
end

-- Temp files
local black = "black.png"
local distort0 = "distort0.png"
local distort1 = "distort1.png"
local distort2 = "distort2.png"
local distort3 = "distort3.png"
local distort4 = "distort4.png"
local distort5 = "distort5.png"
local distorts = {
    distort1,
    distort2,
    distort3,
    distort4,
    distort5
}
local music = "music.wav"
local isnvgupdated = false

-- Variables
local distortmusic = ""
local indexoffset = 0

function StartGeneration(options, pluginSettings, functions)
    isnvgupdated = options["saveData"] ~= nil
    -- Download default media
    if not functions.libraryHasFile("audio", "distort", "distort.mp3") then
        functions.downloadFile("https://github.com/NVGPartners/NonsensicalVideoGenerator/raw/refs/heads/main/addonlibraries/distort.mp3", "audio", "distort")
        indexoffset = 1
    else
        -- Set variables
        distortmusic = functions.getRandomLibraryFile("audio", "distort")
        -- Apply effect
        if functions.magickInstalled() then
            functions.runMagick("-size " .. options.width .. "x" .. options.height .. " canvas:black " .. black)
        else
            -- one frame
            if isnvgupdated then
                functions.setStatusText("Distort: Creating black frame...")
            end
            print("Distort: Creating black frame...")
            functions.runFFmpeg("-f lavfi -i color=c=black:s=" .. options.width .. "x" .. options.height .. ":d=1 -update 1 -preset veryfast -y \"" .. black .. "\"")
        end
    end
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
    if commandindex == 1 and indexoffset == 1 then
        -- Set variables
        distortmusic = functions.getRandomLibraryFile("audio", "distort")
        -- Apply effect
        if functions.magickInstalled() then
            functions.runMagick("-size " .. options.width .. "x" .. options.height .. " canvas:black " .. black)
        else
            -- one frame
            if isnvgupdated then
                functions.setStatusText("Distort: Creating black frame...")
            end
            print("Distort: Creating black frame...")
            functions.runFFmpeg("-f lavfi -i color=c=black:s=" .. options.width .. "x" .. options.height .. ":d=1 -update 1 -preset veryfast -y \"" .. black .. "\"")
        end
    elseif commandindex == 1+indexoffset then
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -ss 0 -frames:v 1 -update 1 -q:v 1 -preset veryfast -y \"" .. distort0 .. "\"")
    elseif commandindex <= 6+indexoffset then
        local action = ""
        local actionreadable = ""
        local rng = functions.randomInt(1, 7)
        if functions.magickInstalled() then
            if rng == 1 then
                action = "-flop"
                actionreadable = "Flopp"
            elseif rng == 2 then
                action = "-flip"
                actionreadable = "Flipp"
            elseif rng == 3 then
                action = "-implode " .. functions.randomInt(-3, -1)
                actionreadable = "Shrink"
            elseif rng == 4 then
                action = "-implode " .. functions.randomInt(1, 3)
                actionreadable = "Implod"
            elseif rng == 5 then
                action = "-swirl " .. functions.randomInt(-180, 180)
                actionreadable = "Swirl"
            elseif rng == 6 then
                action = "-channel RGB -negate"
                actionreadable = "Negat"
            end
            if isnvgupdated then
                functions.setStatusText("Distort: " .. actionreadable .. "ing... (" .. (commandindex - indexoffset) .. "/6)")
            end
            print("Distort: " .. actionreadable .. "ing... (" .. (commandindex - indexoffset) .. "/6)")
            functions.runMagick("\"" .. distort0 .. "\" " .. action .. " \"" .. distorts[commandindex - 1 - indexoffset] .. "\"")
        else
            if rng <= 2 then
                action = "-vf hflip"
                actionreadable = "H-Flipp"
            elseif rng <= 4 then
                action = "-vf vflip"
                actionreadable = "V-Flipp"
            elseif rng <= 6 then
                action = "-vf hflip,vflip"
                actionreadable = "HV-Flipp"
            elseif rng == 7 then
                action = "-vf negate"
                actionreadable = "Negat"
            end
            if isnvgupdated then
                functions.setStatusText("Distort: " .. actionreadable .. "ing... (" .. (commandindex - indexoffset) .. "/6)")
            end
            print("Distort: " .. actionreadable .. "ing... (" .. (commandindex - indexoffset) .. "/6)")
            functions.runFFmpeg("-i \"" .. distort0 .. "\" " .. action .. " -frames:v 1 -update 1 -preset veryfast -y \"" .. distorts[commandindex - 1 - indexoffset] .. "\"")
        end
    elseif commandindex == 7+indexoffset then
        -- mp3 to wav
        functions.runFFmpeg("-i \"" .. distortmusic .. "\" -acodec pcm_s16le -ac 2 -ar 44100 -af \"aresample=async=1000\" -y \"" .. music .. "\"")
    elseif commandindex == 8+indexoffset then
        -- 0 duration = 0.533 seconds
        -- 1 duration = 0.4 seconds
        -- 2 duration = 0.4 seconds
        -- black duration = 0.8 seconds
        -- 3 duration = 0.533 seconds
        -- 4 duration = 0.4 seconds
        -- 5 duration = 0.433 seconds
        if isnvgupdated then
            functions.setStatusText("Distort: Combining frames...")
        end
        print("Distort: Combining frames...")
        functions.runFFmpeg("-loop 1 -t 0.533 -i \"" .. distort0 .. "\" -loop 1 -t 0.4 -i \"" .. distort1 .. "\" -loop 1 -t 0.4 -i \"" .. distort2 .. "\" -loop 1 -t 0.8 -i \"" .. black .. "\" -loop 1 -t 0.533 -i \"" .. distort3 .. "\" -loop 1 -t 0.4 -i \"" .. distort4 .. "\" -loop 1 -t 0.433 -i \"" .. distort5 .. "\" -i \"" .. music .. "\" -filter_complex \"[0:v][1:v][2:v][3:v][4:v][5:v][6:v]concat=n=7:v=1:a=0[outv];[7:a]aresample=async=1000[outa]\" -map \"[outv]\" -map \"[outa]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -fflags +genpts -y \"" .. options.outputVideo .. "\"")
    end
end

function StopGeneration(options, pluginSettings, functions)
    return true
end
