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
        },
        ["userconsent"] = {
            ["consents"] = {
                {
                    ["flag"] = "DownloadFiles",
                    ["params"] = {
                        "https://github.com/KiwifruitDev/NonsensicalVideoGenerator/raw/main/addonlibraries/distort.mp3",
                    }
                },
                {
                    ["flag"] = "AddToLibrary",
                    ["params"] = {
                        "distort.mp3"
                    }
                },
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
local concatdistort = "concatdistort.txt"
local distorts = {
    distort1,
    distort2,
    distort3,
    distort4,
    distort5
}
local music = "music.wav"

-- Variables
local distortmusic = ""
local indexoffset = 0

function StartGeneration(options, pluginSettings, functions)
    -- Download default media
    if not functions.libraryHasFile("audio", "distort", "distort.mp3") then
        functions.downloadFile("https://github.com/KiwifruitDev/NonsensicalVideoGenerator/raw/main/addonlibraries/distort.mp3", "audio", "distort")
        indexoffset = 1
    else
        -- Set variables
        distortmusic = functions.getRandomLibraryFile("audio", "distort")
        -- Apply effect
        if functions.magickInstalled() then
            functions.runMagick("convert -size " .. options.width .. "x" .. options.height .. " canvas:black " .. black)
        else
            -- one frame
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
            functions.runMagick("convert -size " .. options.width .. "x" .. options.height .. " canvas:black " .. black)
        else
            -- one frame
            functions.runFFmpeg("-f lavfi -i color=c=black:s=" .. options.width .. "x" .. options.height .. ":d=1 -update 1 -preset veryfast -y \"" .. black .. "\"")
        end
    elseif commandindex == 1+indexoffset then
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -ss 0 -frames:v 1 -update 1 -q:v 1 -preset veryfast -y \"" .. distort0 .. "\"")
    elseif commandindex <= 6+indexoffset then
        local action = ""
        local rng = functions.randomInt(1, 8)
        if functions.magickInstalled() then
            if rng == 1 then
                action = "-flop"
            elseif rng == 2 then
                action = "-flip"
            elseif rng == 3 then
                action = "-implode " .. functions.randomInt(-3, -1)
            elseif rng == 4 then
                action = "-implode " .. functions.randomInt(1, 3)
            elseif rng == 5 then
                action = "-swirl " .. functions.randomInt(1, 180)
            elseif rng == 6 then
                action = "-swirl " .. functions.randomInt(-180, -1)
            elseif rng == 7 then
                action = "-channel RGB -negate"
            end
            functions.runMagick("convert \"" .. distort0 .. "\" " .. action .. " \"" .. distorts[commandindex - 1 - indexoffset] .. "\"")
        else
            if rng <= 2 then
                action = "-vf hflip"
            elseif rng <= 4 then
                action = "-vf vflip"
            elseif rng <= 6 then
                action = "-vf hflip,vflip"
            elseif rng == 7 then
                action = "-vf negate"
            end
            functions.runFFmpeg("-i \"" .. distort0 .. "\" " .. action .. " -frames:v 1 -update 1 -preset veryfast -y \"" .. distorts[commandindex - 1 - indexoffset] .. "\"")
        end
    elseif commandindex == 7+indexoffset then
        local concatstring = [[file ']] .. distort0 .. [['
duration 0.467
file ']] .. distort1 .. [['
duration 0.434
file ']] .. distort2 .. [['
duration 0.4
file ']] .. black .. [['
duration 0.834
file ']] .. distort3 .. [['
duration 0.467
file ']] .. distort4 .. [['
duration 0.4
file ']] .. distort5 .. [['
duration 0.467
]]
        functions.fileWrite(concatdistort, concatstring)
        -- convert mp3 to wav
        functions.runFFmpeg("-i \"" .. distortmusic .. "\" -acodec pcm_s16le -ac 2 -ar 44100 \"" .. music .. "\"")
    elseif commandindex == 8+indexoffset then
        functions.runFFmpeg("-f concat -i \"" .. concatdistort .. "\" -i \"" .. music .. "\" -c:v libx264 -c:a aac -pix_fmt yuv420p -preset veryfast -shortest -y \"" .. options.outputVideo .. "\"")
    end
end

function StopGeneration(options, pluginSettings, functions)
    return true
end
