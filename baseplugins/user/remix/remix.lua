function Query()
    return {
        ["settings"] = {
            {
                ["name"] = "Display Name",
                ["value"] = "Remix",
                ["type"] = "label",
            },
            {
                ["name"] = "Description",
                ["value"] = "Applies a stutter loop to the beat.",
                ["type"] = "label",
            },
            {
                ["name"] = "Label1",
                ["value"] = "Uses the Remix library.",
                ["type"] = "label",
            }
        },
        ["libraries"] = {
            {
                ["name"] = "Remix",
                ["tooltip"] = "Stuttering audio clips in a pattern.",
                ["path"] = "remix",
                ["type"] = "audio",
            }
        }
    }
end

-- Temp files
local temp2 = "temp2.mp4"
local temp3 = "temp3.mp4"
local temp4 = "temp4.mp4"

-- Variables
local randomSound = ""

function StartGeneration(options, pluginSettings, functions)
    -- Set variables
    randomSound = functions.getRandomLibraryFile("audio", "remix")
    -- Apply effect
    -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp1" -ss 0 -t 0.100 -y "$temp2"}
    functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -ss 0 -t 0.100 -y \"" .. temp2 .. "\"")
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
    if commandindex == 1 then
        -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp1" -ss 0.140 -t 0.050 -y "$temp3"}
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -ss 0.140 -t 0.050 -y \"" .. temp3 .. "\"")
    elseif commandindex == 2 then
        -- Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp1" -ss 0 -t 0.050 -y "$temp4"}
        functions.runFFmpeg("-i \"" .. options.inputVideo .. "\" -ss 0 -t 0.050 -y \"" .. temp4 .. "\"")
    elseif commandindex == 3 then
        if randomSound == "" then
            -- Invoke-Command -ScriptBlock {&$ffmpeg -r 30 -i "$temp2" -i "$temp3" -i "$temp4" -filter_complex "[0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a]concat=n=42:v=1:a=1[remixv][remixa];[remixa]volume=volume=1[ogaudio]" -map "[remixv]" -map "[ogaudio]" -fps_mode vfr -y "$video"}
            functions.runFFmpeg("-r 30 -i \"" .. temp2 .. "\" -i \"" .. temp3 .. "\" -i \"" .. temp4 .. "\" -filter_complex \"[0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a]concat=n=42:v=1:a=1[remixv][remixa];[remixa]volume=volume=1[ogaudio]\" -map \"[remixv]\" -map \"[ogaudio]\" -fps_mode vfr -y " .. options.outputVideo)
        else
            -- Invoke-Command -ScriptBlock {&$ffmpeg -r 30 -i "$temp2" -i "$temp3" -i "$temp4" -i "$randomSound" -filter_complex "[0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a]concat=n=42:v=1:a=1[remixv][remixa];[remixa]volume=volume=1[ogaudio];[3:a]volume=volume=1[sfx];[sfx][ogaudio]amix=inputs=2:duration=shortest:dropout_transition=0:weights='0.3 1':normalize=0[music]" -map "[remixv]" -map "[music]" -fps_mode vfr -y "$video"}
            functions.runFFmpeg("-r 30 -i \"" .. temp2 .. "\" -i \"" .. temp3 .. "\" -i \"" .. temp4 .. "\" -i \"" .. randomSound .. "\" -filter_complex \"[0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a]concat=n=42:v=1:a=1[remixv][remixa];[remixa]volume=volume=1[ogaudio];[3:a]volume=volume=1[sfx];[sfx][ogaudio]amix=inputs=2:duration=shortest:dropout_transition=0:weights='0.3 1':normalize=0[music]\" -map \"[remixv]\" -map \"[music]\" -fps_mode vfr -y " .. options.outputVideo)
        end
    end
end

function StopGeneration(options, pluginSettings, functions)
    return true
end
