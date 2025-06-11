function Query(localeName, localizationTokens)
    local localizedOption1 = localizationTokens["Addons:StockRaveOption1"] or "Rave"
    local localizedOption2 = localizationTokens["Addons:StockRaveOption2"] or "Random frame order with color cycle."
    local localizedOption3 = localizationTokens["Addons:StockRaveOption3"] or "The minimum amount of time to seek music."
    local localizedOption4 = localizationTokens["Addons:StockRaveOption4"] or "The maximum amount of time to seek music."
    local localizedOption5 = localizationTokens["Addons:StockRaveOption5"] or "Revert to how the old Rave effect worked."
    local localizedOption6 = localizationTokens["Addons:StockRaveOption6"] or "Use the Music library instead."
    local localizedOption7 = localizationTokens["Addons:StockRaveOption7"] or "High-intensity music for the Rave effect."
    return {
        ["settings"] = {
            {
                ["name"] = "Display Name",
                ["value"] = localizedOption1,
                ["type"] = "label",
            },
            {
                ["name"] = "Description",
                ["value"] = localizedOption2,
                ["type"] = "label",
            },
            {
                ["name"] = "Music Seek Start",
                ["tooltip"] = localizedOption3,
                ["value"] = "1",
                ["type"] = "float",
            },
            {
                ["name"] = "Music Seek End",
                ["tooltip"] = localizedOption4,
                ["value"] = "5",
                ["type"] = "float",
            },
            {
                ["name"] = "Use Classic Rave",
                ["tooltip"] = localizedOption5,
                ["value"] = "0",
                ["type"] = "bool",
            },
            {
                ["name"] = "Use Music Library",
                ["tooltip"] = localizedOption6,
                ["value"] = "0",
                ["type"] = "bool",
            },
        },
        ["libraries"] = {
            {
                ["name"] = localizedOption1,
                ["tooltip"] = localizedOption7,
                ["path"] = "rave",
                ["type"] = "audio",
            }
        }
    }
end

-- Default settings
local musicSeekStart = 1
local musicSeekEnd = 5
local classicRave = false
local useMusicLibrary = false

-- Temp files
local temp2 = "temp2.mp4"
local frames = ".\\frames"

-- Variables
local randomSound = ""
local success = false
local music = "music.wav"

-- Old rave temp files
local temp3 = "temp3.mp4"
local temp4 = "temp4.mp4"
local temp5 = "temp5.mp4"
local temp6 = "temp6.mp4"
local temp7 = "temp7.mp4"
local temp8 = "temp8.mp4"
local temp9 = "temp9.mp4"
local temp10 = "temp10.mp4"
local temp11 = "temp11.mp4"
local temp12 = "temp12.mp4"
local temp13 = "temp13.mp4"
local temp14 = "temp14.mp4"

function StartGeneration(options, pluginSettings, functions)
    -- Set settings
    success = false
    if pluginSettings["Music Seek Start"] != nil then
        musicSeekStart = tonumber(pluginSettings["Music Seek Start"])
    end
    if pluginSettings["Music Seek End"] != nil then
        musicSeekEnd = tonumber(pluginSettings["Music Seek End"])
    end
    if pluginSettings["Use Classic Rave"] != nil then
        classicRave = tonumber(pluginSettings["Use Classic Rave"]) == 1
    end
    if pluginSettings["Use Music Library"] != nil then
        useMusicLibrary = tonumber(pluginSettings["Use Music Library"]) == 1
    end
    -- Set variables
    local libraryPath = "rave"
    if useMusicLibrary then
        libraryPath = "music"
    end
    randomSound = functions.getRandomLibraryFile("audio", libraryPath)
    -- Apply effect
    if not classicRave then
        functions.folderCreate(frames)
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -vf fps=" .. options.fps .. " \"" .. frames .. "\\frame%0d.png\"")
    else
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -filter_complex \"[0:v]frei0r=filter_name=nervous,frei0r=filter_name=colorize:filter_params=0[outv]\" -map \"[outv]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. temp2 .. "\"")
    end
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
    if not classicRave then
        if commandindex == 1 then
            -- Get frame count
            local frameCount = 0
            for fl in functions.enumerateFiles(frames) do
                frameCount = frameCount + 1
            end
            local frametable = {}
            for i = 1, frameCount do
                frametable[i] = i
            end
            -- Rename frames in random order with prefix _frame
            if frameCount > 0 then
                for fl in functions.enumerateFiles(frames) do
                    -- Pop frame from table
                    local mathrandom = math.random(1, #frametable)
                    print("#frametable: " .. #frametable)
                    print("mathrandom: " .. mathrandom)
                    local randomFrame = table.remove(frametable, mathrandom)
                    if randomFrame == 0 or randomFrame == nil then
                        break
                    end
                    -- Rename frame
                    local flname = ".\\frames\\" .. fl
                    local randomFrameName = ".\\frames\\" .. "_frame" .. tostring(randomFrame-1) .. ".png"
                    functions.fileMove(flname, randomFrameName)
                end
                functions.runFFmpeg("-i \"" .. frames .. "\\_frame%0d.png\" -vf \"hue='H=PI*t: s=sin(PI*t)+1.5: enable=between(t,0,10)'\" -preset veryfast -y \"" .. frames .. "\\_frame%0d.png\"")
            end
        elseif commandindex == 2 then
            -- Invoke-Command -ScriptBlock {&$ffmpeg -i $frames\_%0d.png -i $video -map 0:v -map 1:a -preset veryfast -preset veryfast -y "$temp2"}
            functions.runFFmpeg("-i \"" .. frames .. "\\_frame%0d.png\" -i \"" .. options.inputVideo .. "\" -map 0:v -map 1:a -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. temp2 .. "\"")
        elseif commandindex == 3 then
            if randomSound == "" then
                -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -filter_complex "[0:v]setpts=0.75*PTS[f];[0:v]setpts=0.5*PTS,reverse[fr];[f][fr]concat=n=2:v=1:a=0,format=yuv420p[v];[0:a]atempo=2.0[a1];[0:a]atempo=0.75,areverse[a2];[a1][a2]concat=n=2:v=0:a=1[a]" -map "[v]" -map "[a]" -shortest -preset veryfast -y "$output"}
                functions.runFFmpeg("-i \"" .. temp2 .. "\" -filter_complex \"[0:v]setpts=0.75*PTS[f];[0:v]setpts=0.5*PTS,reverse[fr];[f][fr]concat=n=2:v=1:a=0,format=yuv420p[v];[0:a]atempo=2.0[a1];[0:a]atempo=0.75,areverse[a2];[a1][a2]concat=n=2:v=0:a=1,aresample=async=1000[a]\" -map \"[v]\" -map \"[a]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. options.outputVideo .. "\"")
            else
                -- convert mp3 to wav
                functions.runFFmpeg("-i \"" .. randomSound .. "\" -acodec pcm_s16le -ac 2 -ar 44100 \"" .. music .. "\"")
            end
            success = true
        elseif commandindex == 4 and randomSound != "" then
            -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -i "$randomSound" -filter_complex "[0:v]setpts=0.75*PTS[f];[0:v]setpts=0.5*PTS,reverse[fr];[f][fr]concat=n=2:v=1:a=0,format=yuv420p[v]" -map "[v]" -map "1:a" -shortest -preset veryfast -y "$output"}
            functions.runFFmpeg("-i \"" .. temp2 .. "\" -i \"" .. music .. "\" -filter_complex \"[0:v]setpts=0.75*PTS[f];[0:v]setpts=0.5*PTS,reverse[fr];[f][fr]concat=n=2:v=1:a=0,format=yuv420p[v];[1:a]aresample=async=1000[outa]\" -map \"[v]\" -map \"[outa]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. options.outputVideo .. "\"")
        end
    else
        if commandindex == 1 then
            functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -i \"" .. temp2 .. "\" -filter_complex \"[0:v]frei0r=filter_name=nervous,frei0r=filter_name=colorize:filter_params=0.1[vid2];[1:v][vid2]concat=n=2:v=1[outv]\" -map \"[outv]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. temp3 .. "\"")
        elseif commandindex == 2 then
            functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -i \"" .. temp3 .. "\" -filter_complex \"[1:v]reverse[vid];[0:v]frei0r=filter_name=nervous,frei0r=filter_name=colorize:filter_params=0.25[vid2];[vid][vid2]concat=n=2:v=1[outv]\" -map \"[outv]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. temp4 .. "\"")
        elseif commandindex == 3 then
            functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -i \"" .. temp4 .. "\" -filter_complex \"[0:v]frei0r=filter_name=nervous,frei0r=filter_name=colorize:filter_params=0.6[vid2];[1:v][vid2]concat=n=2:v=1[outv]\" -map \"[outv]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. temp5 .. "\"")
        elseif commandindex == 4 then
            functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -i \"" .. temp5 .. "\" -filter_complex \"[1:v]reverse[vid];[0:v]frei0r=filter_name=nervous,frei0r=filter_name=colorize:filter_params=0.75[vid2];[vid][vid2]concat=n=2:v=1[outv]\" -map \"[outv]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. temp6 .. "\"")
        elseif commandindex == 5 then
            functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -i \"" .. temp6 .. "\" -filter_complex \"[0:v]frei0r=filter_name=nervous,frei0r=filter_name=colorize:filter_params=1[vid2];[1:v][vid2]concat=n=2:v=1[outv]\" -map \"[outv]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. temp7 .. "\"")
        elseif commandindex == 6 then
            functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -i \"" .. temp7 .. "\" -filter_complex \"[1:v]reverse[vid];[0:v]frei0r=filter_name=nervous,frei0r=filter_name=colorize:filter_params=0.75[vid2];[vid][vid2]concat=n=2:v=1[outv]\" -map \"[outv]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. temp8 .. "\"")
        elseif commandindex == 7 then
            functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -i \"" .. temp8 .. "\" -filter_complex \"[0:v]frei0r=filter_name=nervous,frei0r=filter_name=colorize:filter_params=1[vid2];[1:v][vid2]concat=n=2:v=1[outv]\" -map \"[outv]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. temp9 .. "\"")
        elseif commandindex == 8 then
            functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -i \"" .. temp9 .. "\" -filter_complex \"[1:v]reverse[vid];[0:v]frei0r=filter_name=nervous,frei0r=filter_name=colorize:filter_params=0.75[vid2];[vid][vid2]concat=n=2:v=1[outv]\" -map \"[outv]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. temp10 .. "\"")
        elseif commandindex == 9 then
            functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -i \"" .. temp10 .. "\" -filter_complex \"[0:v]frei0r=filter_name=nervous,frei0r=filter_name=colorize:filter_params=1[vid2];[1:v][vid2]concat=n=2:v=1[outv]\" -map \"[outv]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. temp11 .. "\"")
        elseif commandindex == 10 then
            functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -i \"" .. temp11 .. "\" -filter_complex \"[1:v]reverse[vid];[0:v]frei0r=filter_name=nervous,frei0r=filter_name=colorize:filter_params=0.75[vid2];[vid][vid2]concat=n=2:v=1[outv]\" -map \"[outv]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. temp12 .. "\"")
        elseif commandindex == 11 then
            functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -i \"" .. temp12 .. "\" -filter_complex \"[0:v]frei0r=filter_name=nervous,frei0r=filter_name=colorize:filter_params=1[vid2];[1:v][vid2]concat=n=2:v=1[outv]\" -map \"[outv]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. temp13 .. "\"")
        elseif commandindex == 12 then
            functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -i \"" .. temp13 .. "\" -filter_complex \"[1:v]reverse[vid];[0:v]frei0r=filter_name=nervous,frei0r=filter_name=colorize:filter_params=0.75[vid2];[vid][vid2]concat=n=2:v=1[outv]\" -map \"[outv]\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. temp14 .. "\"")
        elseif commandindex == 13 then
            functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -i \"" .. temp14 .. "\" -i \"" .. randomSound .. "\" -filter_complex \"[0:v]frei0r=filter_name=nervous,frei0r=filter_name=colorize:filter_params=0.1[vid2];[1:v][vid2]concat=n=2:v=1[outv];[2:a]volume=1[aud]\" -ac 2 -ar 44100 -map [aud] -map [outv] -t 3.5 -y \"" .. options.outputVideo .. "\"")
            success = true
        end
    end
end

function StopGeneration(options, pluginSettings, functions)
    return success
end
