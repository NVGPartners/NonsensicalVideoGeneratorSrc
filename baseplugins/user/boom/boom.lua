function Query()
    return {
        ["settings"] = {
            {
                ["name"] = "Display Name",
                ["value"] = "Boom",
                ["type"] = "label",
            },
            {
                ["name"] = "Description",
                ["value"] = "\"What the\" ... \"Boom\"",
                ["type"] = "label",
            },
            {
                ["name"] = "Label1",
                ["value"] = "Uses the What The library.",
                ["type"] = "label",
            },
            {
                ["name"] = "Label2",
                ["value"] = "Uses the Boom library.",
                ["type"] = "label",
            }
        },
        ["libraries"] = {
            {
                ["name"] = "What The",
                ["tooltip"] = "Comes before a \"Boom\" in a clip.",
                ["path"] = "whatthe",
                ["type"] = "audio",
            },
            {
                ["name"] = "Boom",
                ["tooltip"] = "Comes after a \"What The\" in a clip.",
                ["path"] = "boom",
                ["type"] = "video",
            }
        }
    }
end

-- Temp files
local temp2 = "temp2.mp4"
local temp3 = "temp3.mp4"
local temp4 = "temp4.mp4"
local temp5 = "temp5.mp4"

-- Variables
local material = ""
local whatthe = ""
local boom = ""
local failed = false
local whatthelength = 0
local materiallength = 0

function StartGeneration(options, pluginSettings, functions)
    -- Set variables
    whatthe = functions.getRandomLibraryFile("audio", "whatthe")
    material = functions.getRandomLibraryFile("video", "materials")
    boom = functions.getRandomLibraryFile("video", "boom")
    -- Apply effect
    functions.runFFprobe("-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"" .. whatthe .. "\"")
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
    if commandindex == 1 then
        whatthelength = tonumber(outputresult)
        -- Get length of material
        functions.runFFprobe("-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"" .. material .. "\"")
    elseif commandindex == 2 then
        materiallength = tonumber(outputresult)
        -- Cut material to whatthe length from random start
        local multiplier = (whatthelength*0.75)
        local start = functions.randomDouble(0, materiallength - multiplier)
        functions.runFFmpeg("-ss " .. start .. " -i \"" .. material .. "\" -t " .. multiplier .. " -c:v libx264 -preset ultrafast -crf 18 -vf scale=" .. options.width .. ":" .. options.height .. ",setsar=1:1,fps=fps=30 -y \"" .. temp2 .. "\"")
    elseif commandindex == 3 then
        -- Cut material again from different random start
        local multiplier = (whatthelength*0.25)
        local start = functions.randomDouble(0, materiallength - multiplier)
        functions.runFFmpeg("-ss " .. start .. " -i \"" .. material .. "\" -t " .. multiplier .. " -c:v libx264 -preset ultrafast -crf 18 -vf scale=" .. options.width .. ":" .. options.height .. ",setsar=1:1,fps=fps=30 -y \"" .. temp3 .. "\"")
    elseif commandindex == 4 then
        -- Concatenate the two clips and use the whatthe audio on top
        functions.runFFmpeg("-i \"" .. temp2 .. "\" -i \"" .. temp3 .. "\" -i \"" .. whatthe .. "\" -filter_complex \"[0:v:0][0:a:0][1:v:0][1:a:0]concat=n=2:v=1:a=1[v][a];[a][2:a:0]amix=inputs=2[a]\" -map \"[a]\" -map \"[v]\" -c:v libx264 -preset ultrafast -crf 0 \"" .. temp4 .. "\"")
    elseif commandindex == 5 then
        -- Convert boom to use screen resolution
        functions.runFFmpeg("-i \"" .. boom .. "\" -c:v libx264 -preset ultrafast -crf 0 -vf scale=" .. options.width .. ":" .. options.height .. ",setsar=1:1,fps=fps=30 -y \"" .. temp5 .. "\"")
    elseif commandindex == 6 then
        -- Concatenate the boom video
        functions.runFFmpeg("-i \"" .. temp4 .. "\" -i \"" .. temp5 .. "\" -filter_complex \"[0:v:0][0:a:0][1:v:0][1:a:0]concat=n=2:v=1:a=1[v][a]\" -map \"[a]\" -map \"[v]\" -c:v libx264 -preset ultrafast -crf 0 \"" .. options.outputVideo .. "\"")
    end
end

function StopGeneration(options, pluginSettings, functions)
    return not failed
end
