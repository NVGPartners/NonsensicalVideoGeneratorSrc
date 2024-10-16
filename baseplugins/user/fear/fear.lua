local fear_library_baseurl = "https://files.catbox.moe/"
local fear_library_defaults = {
    "3wtjqk.mp4",
    "eknoz9.mp4",
    "ior05v.mp4",
    "yvch3h.mp4",
    "lofbuv.mp4",
    "qr3c0e.mp4",
    "gfhcy8.mp4",
    "9eu7fr.mp4"
}

function Query(localeName, localizationTokens)
    local queryparams = {
        ["settings"] = {
            {
                ["name"] = "Addon Type",
                ["value"] = "effect",
                ["type"] = "label"
            },
            {
                ["name"] = "Display Name",
                ["value"] = "Fearful",
                ["type"] = "label"
            },
            {
                ["name"] = "Description",
                ["value"] = "This is your fear factor.",
                ["type"] = "label"
            },
            {
                ["name"] = "Label1",
                ["value"] = "Trims the clip and adds a fearful",
                ["type"] = "label"
            },
            {
                ["name"] = "Label2",
                ["value"] = "expression onto it.",
                ["type"] = "label"
            },
            {
                ["name"] = "Chance Roll",
                ["tooltip"] = "Out of 100, chance to add another fear clip\nwith the start of a random material.",
                ["value"] = "50",
                ["type"] = "int",
            },
        },
        ["libraries"] = {
            {
                ["name"] = "Fear",
                ["tooltip"] = "Fearful clips.",
                ["path"] = "fear",
                ["type"] = "video"
            }
        },
        ["userconsent"] = {
            ["consents"] = {
                {
                    ["flag"] = "DownloadFiles",
                    ["params"] = {}
                },
                {
                    ["flag"] = "AddToLibrary",
                    ["params"] = {}
                },
            }
        }
    }
    for i, v in ipairs(fear_library_defaults) do
        table.insert(queryparams.userconsent.consents[1].params, fear_library_baseurl .. v)
        table.insert(queryparams.userconsent.consents[2].params, v)
    end
    return queryparams
end

local chance = 50

local vidTypes = {
    [[file 'clip1.mp4'
file 'fear1.mp4'
]],
    [[file 'clip1.mp4'
file 'fear1.mp4'
file 'clip2.mp4'
file 'fear2.mp4'
]]
}

local fear1 = ""
local fear2 = ""
local material = ""
local doTwoFears = false
local cioffset = 0
local hadToDownload = false
local mediaToDownload = {}

function StartGeneration(options, pluginSettings, functions)
    chance = 50
    if pluginSettings["Chance Roll"] != nil then
        chance = tonumber(pluginSettings["Chance Roll"])
    end
    fear1 = ""
    fear2 = ""
    material = ""
    doTwoFears = functions.randomInt(1, 100) <= chance and true or false
    functions.fileWrite("vids.txt", vidTypes[doTwoFears and 2 or 1])
    cioffset = 0
    hadToDownload = false
    mediaToDownload = {}
    for i, v in ipairs(fear_library_defaults) do
        if not functions.libraryHasFile("video", "fear", v) then
            table.insert(mediaToDownload, v)
            cioffset = cioffset + 1
        end
    end
    if cioffset > 0 then
        functions.downloadFile(fear_library_baseurl .. mediaToDownload[1], "video", "fear")
        mediaToDownload = {unpack(mediaToDownload, 2)}
        cioffset = cioffset - 1
        hadToDownload = true
    else
        fear1 = functions.getRandomLibraryFile("video", "fear")
        functions.runFFmpeg("-i \"" .. fear1 .. "\" -r 30 -filter_complex \"[0:v]scale='" .. options.width .. ":" .. options.height .. "',setsar=1[vid]\" -map \"[vid]\" -map \"0:a\" -y \"fear1.mp4\"")
    end
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
    if commandindex <= cioffset then
        functions.downloadFile(fear_library_baseurl .. mediaToDownload[commandindex], "video", "fear")
        return
    end
    local ci = commandindex - cioffset + (hadToDownload and 0 or 1)
    if ci == 1 then
        fear1 = functions.getRandomLibraryFile("video", "fear")
        functions.runFFmpeg("-i \"" .. fear1 .. "\" -r 30 -filter_complex \"[0:v]scale='" .. options.width .. ":" .. options.height .. "',setsar=1[vid]\" -map \"[vid]\" -map \"0:a\" -y \"fear1.mp4\"")
    elseif ci == 2 then
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -ss 00:00:00.000 -to 00:00:03.750 -y \"clip1.mp4\"")
    elseif ci == 3 then
        if doTwoFears then
            functions.getRandomLibraryFile("video", "materials")
            functions.runFFmpeg("-i \"" .. material .. "\" -r 30 -ss 00:00:00.000 -to 00:00:03.750 -y \"clip2.mp4\"")
        else
            functions.runFFmpeg("-f concat -i \"vids.txt\" -y \"" .. options.outputVideo .. "\"")
        end
    elseif ci == 4 then
        if doTwoFears then
            fear2 = functions.getRandomLibraryFile("video", "fear")
            functions.runFFmpeg("-i \"" .. fear2 .. "\" -r 30 -filter_complex \"[0:v]scale='" .. options.width .. ":" .. options.height .. "',setsar=1[vid]\" -map \"[vid]\" -map \"0:a\" -y \"fear2.mp4\"")
        end
    elseif ci == 5 then
        if doTwoFears then
            functions.runFFmpeg("-f concat -i \"vids.txt\" -y \"" .. options.outputVideo .. "\"")
        end
    end
end

function StopGeneration(options, pluginSettings, functions)
    return true
end
