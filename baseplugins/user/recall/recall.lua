function Query(localeName, localizationTokens)
    return {
        ["settings"] = {
            {
                ["name"] = "Addon Type",
                ["value"] = "postrendereffect",
                ["type"] = "label"
            },
            {
                ["name"] = "Display Name",
                ["value"] = "Recall",
                ["type"] = "label"
            },
            {
                ["name"] = "Description",
                ["value"] = "Advanced form of overlaying videos.",
                ["type"] = "label"
            },
            {
                ["name"] = "Increment Clip Count",
                ["value"] = "20",
                ["tooltip"] = "For every X clips, one recall effect will be added.\nThis means that 20 clips will have 1 recall effect,\n40 clips will have 2 recall effects, etc.",
                ["type"] = "int"
            },
            {
                ["name"] = "Creation: Delay",
                ["tooltip"] = "(Recall Video Creation)\nHow long it take before playing the inner video.\nIn seconds.",
                ["value"] = "0",
                ["type"] = "float"
            },
            {
                ["name"] = "Creation: Length",
                ["tooltip"] = "(Recall Video Creation)\nHow long the inner video should play for.\nIn seconds.",
                ["value"] = "5",
                ["type"] = "float"
            },
            {
                ["name"] = "Creation: Inner Position",
                ["tooltip"] = "(Recall Video Creation)\nThe X and Y positions of the inner video.\nValues are separated by a ','.\nExample: 0,0\nMust be in whole numbers and divisible by 2.",
                ["value"] = "0,0",
                ["type"] = "any"
            },
            {
                ["name"] = "Creation: Inner Resolution",
                ["tooltip"] = "(Recall Video Creation)\nThe width and height of the inner video.\nValues are separated by a ','.\nExample: 640,480\nMust be in whole numbers and divisible by 2.",
                ["value"] = "640,480",
                ["type"] = "any"
            },
            {
                ["name"] = "Create Recall Video",
                ["tooltip"] = "Create a recall video with the specified settings.\nThe video will be added to the Recall Library.",
                ["type"] = "button"
            }
        },
        ["libraries"] = {
            {
                ["name"] = "Recall",
                ["tooltip"] = "Advanced form of overlaying videos.\nYou must create recall videos in the addon settings.",
                ["path"] = "recall",
                ["type"] = "video",
                ["preventToggle"] = false,
                ["preventImport"] = true,
                ["preventDownload"] = false
            }
        }
    }
end

-- Recall works by using very specific video clips.
-- The file name must be in the format:
-- videoname_s0l0x0y0w0h0.mp4
-- s: delay to start the video inside of the recall
-- l: length of the video inside of the recall
-- x: x position of the video inside of the recall
-- y: y position of the video inside of the recall
-- w: width of the video inside of the recall
-- h: height of the video inside of the recall
-- A segment of the entire render will be cut using the length value.
-- That section will be replaced with the recall video, with the segment starting after the delay.
-- However long the recall video is will be how long this new section will last.

-- Variables (r refers to inner video)
local inputVideo = ""
local outputVideo = "output.mp4"
local before = "before.mp4"
local after = "after.mp4"
local r_temp = "innertemp.mp4"
local recalltempbefore = "recalltempbefore.mp4"
local recalltempduring = "recalltempduring.mp4"
local recalltempafter = "recalltempafter.mp4"
local recalltempfinal = "recalltempfinal.mp4"
local totalrecalls = 1
local increment = 0
local r_startdelay = 0
local r_length = 0
local r_x = 0
local r_y = 0
local r_width = 0
local r_height = 0
local recallduration = 0
local randomstarttime = 0
local randomrecall = ""
local finished = false
local outputwidth = 0
local outputheight = 0
local outputframerate = 0
local randompointintime = 0

function StartRecall(options, pluginSettings, functions)
    print("Starting recall " .. (increment + 1) .. " of " .. totalrecalls)
    functions.setStatusText("Processing recall " .. (increment + 1) .. " of " .. totalrecalls)
    -- First, pick a video file to use as the inner video
    randomrecall = functions.getRandomLibraryFile("video", "recall")
    if randomrecall == nil or randomrecall == "" then
        print("No recall video found in the library.")
        return false
    end
    -- Parse the file name to get the recall settings
    local filename = functions.getLibraryFileName(randomrecall)
    print("Using recall video: " .. filename)
    -- Remove .mp4 at the end of the file name if it exists
    filename = filename:gsub("%.mp4$", "")
    -- Split it by underscores
    local parts = {}
    for part in filename:gmatch("([^_]+)") do
        table.insert(parts, part)
    end
    -- The last parts is the settings string
    if #parts < 2 then
        print("Invalid recall video file name format.")
        return false
    end
    local settings = parts[#parts]
    -- Split the settings string by 's', 'l', 'x', 'y', 'w', 'h'
    r_startdelay = tonumber(settings:match("s(%d+%.?%d*)")) or 0
    if r_startdelay <= 0 then
        r_startdelay = 0.1 -- Ensure we have a small delay to avoid issues with ffmpeg
    end
    r_length = tonumber(settings:match("l(%d+%.?%d*)")) or 5
    r_x = tonumber(settings:match("x(%d+%.?%d*)")) or 0
    r_y = tonumber(settings:match("y(%d+%.?%d*)")) or 0
    r_width = tonumber(settings:match("w(%d+)")) or 640
    r_height = tonumber(settings:match("h(%d+)")) or 480
    -- Check if the recall video is valid
    if r_startdelay < 0 or r_length <= 0 or r_x < 0 or r_y < 0 or r_width <= 0 or r_height <= 0 then
        print("Invalid recall video settings.")
        return false
    end
    -- Ask ffprobe to get the video duration and resolution
    functions.runFFprobe("-v error -show_entries format=duration,width,height -of default=noprint_wrappers=1 \"" .. randomrecall .. "\"")
end

function StartGeneration(options, pluginSettings, functions)
    -- Set default values for recall variables
    inputVideo = options.inputVideo
    outputVideo = "output.mp4"
    before = "before.mp4"
    after = "after.mp4"
    r_temp = "innertemp.mp4"
    recalltempbefore = "recalltempbefore.mp4"
    recalltempduring = "recalltempduring.mp4"
    recalltempafter = "recalltempafter.mp4"
    recalltempfinal = "recalltempfinal.mp4"
    totalrecalls = 1
    increment = 0
    r_startdelay = 0
    r_length = 5
    r_x = 0
    r_y = 0
    r_width = 640
    r_height = 480
    recallduration = 0
    randomstarttime = 0
    randomrecall = ""
    finished = false
    outputwidth = tonumber(options.saveData["VideoWidth"]) or 640
    outputheight = tonumber(options.saveData["VideoHeight"]) or 480
    outputframerate = tonumber(options.saveData["VideoFPS"]) or 30
    randompointintime = 0
    -- Every 20 clips, increment the recall count by 1
    local totalclips = tonumber(options.saveData["MaxClipCount"]) or 0
    local incrementor = tonumber(pluginSettings["Increment Clip Count"]) or 20
    local current = 0
    for i = incrementor, totalclips do
        if current == incrementor then
            totalrecalls = totalrecalls + 1
            current = 0
        end
        current = current + 1
    end
    -- Start the recall process
    if not functions.libraryHasFile("video", "recall", "jasonbourne_s0.067l0.968x251y212w822h616.mp4") then
        functions.downloadFileSync("https://github.com/NVGPartners/NonsensicalVideoGenerator/raw/refs/heads/main/addonlibraries/recall/jasonbourne_s0.067l0.968x251y212w822h616.mp4", "video", "recall")
    end
    StartRecall(options, pluginSettings, functions)
    return true
end

function PostCommand(commandindex, outputresult, errorresult, options, pluginSettings, functions)
    if commandindex == 1 + (10 * increment) then
        -- ffprobe output parsing
        local duration = 0
        for line in outputresult:gmatch("[^\r\n]+") do
            if line:find("duration=") then
                duration = tonumber(line:sub(10))
            end
        end
        if duration <= 0 then
            print("Invalid video file.")
            functions.setStatusText("Invalid video file.")
            return false
        end
        recallduration = duration
        -- Get the total duration of the input video
        functions.runFFprobe("-v error -show_entries format=duration -of default=noprint_wrappers=1 \"" .. inputVideo .. "\"")
    elseif commandindex == 2 + (10 * increment) then
        -- parse the total duration of the input video
        local duration = 0
        for line in outputresult:gmatch("[^\r\n]+") do
            if line:find("duration=") then
                duration = tonumber(line:sub(10))
            end
        end
        if duration <= 0 then
            print("Invalid input video file.")
            functions.setStatusText("Invalid input video file.")
            return false
        end
        print("Input video duration: " .. duration .. " seconds")
        -- Create the before and after segments
        randomstarttime = functions.randomDouble(0, duration - recallduration)
        randompointintime = functions.randomDouble(0, randomstarttime)
        functions.runFFmpeg("-to " .. randomstarttime .. " -i \"" .. inputVideo .. "\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -af \"aresample=async=1000\" -y \"" .. before .. "\"")
    elseif commandindex == 3 + (10 * increment) then
        -- Create the after segment
        --local endtime = randomstarttime + recallduration
        functions.runFFmpeg("-ss " .. randomstarttime .. " -i \"" .. inputVideo .. "\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -af \"aresample=async=1000\" -y \"" .. after .. "\"")
    elseif commandindex == 4 + (10 * increment) then
        -- Create the inner video segment
        functions.runFFmpeg("-ss " .. randompointintime .. " -t " .. r_length .. " -i \"" .. inputVideo .. "\" -vf \"scale=" .. r_width .. ":" .. r_height .. ",setsar=1\" -r " .. outputframerate .. " -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. r_temp .. "\"")
    elseif commandindex == 5 + (10 * increment) then
        -- Create the before segment of the recall video
        functions.runFFmpeg("-t " .. r_startdelay .. " -i \"" .. randomrecall .. "\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. recalltempbefore .. "\"")
    elseif commandindex == 6 + (10 * increment) then
        -- Create the during segment of the recall video, overlaying the inner video with audio overlapping
        functions.runFFmpeg("-ss " .. r_startdelay .. " -t " .. r_length .. " -i \"" .. randomrecall .. "\" -i \"" .. r_temp .. "\" -filter_complex \"[1:v]scale=" .. r_width .. ":" .. r_height .. ",setsar=1[inner];[0:v][inner]overlay=x=" .. r_x .. ":y=" .. r_y .. ":shortest=1[outv];[0:a][1:a]amix=inputs=2:duration=shortest[outa]\" -map \"[outv]\" -map \"[outa]\" -c:v libx264 -crf 28 -preset ultrafast -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. recalltempduring .. "\"")
    elseif commandindex == 7 + (10 * increment) then
        -- Create the after segment of the recall video
        functions.runFFmpeg("-ss " .. (r_startdelay + r_length) .. " -i \"" .. randomrecall .. "\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. recalltempafter .. "\"")
    elseif commandindex == 8 + (10 * increment) then
        -- Combine the before, during, and after segments of the recall video
        functions.runFFmpeg("-i \"" .. recalltempbefore .. "\" -i \"" .. recalltempduring .. "\" -i \"" .. recalltempafter .. "\" -filter_complex \"[0:v][1:v][2:v]concat=n=3:v=1:a=0[outv];[outv]scale=" .. outputwidth .. ":" .. outputheight .. ",setsar=1[final];[0:a][1:a][2:a]concat=n=3:v=0:a=1[outa]\" -r " .. outputframerate .. " -map \"[final]\" -map \"[outa]\" -c:v libx264 -crf 28 -preset ultrafast -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. recalltempfinal .. "\"")
    elseif commandindex == 9 + (10 * increment) then
        -- Combine the before, recalltemp, and after segments into the final output
        functions.runFFmpeg("-i \"" .. before .. "\" -i \"" .. recalltempfinal .. "\" -i \"" .. after .. "\" -filter_complex \"[0:v][1:v][2:v]concat=n=3:v=1:a=0[outv];[0:a][1:a][2:a]concat=n=3:v=0:a=1[outa]\" -map \"[outv]\" -map \"[outa]\" -c:v libx264 -crf 28 -preset ultrafast -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -y \"" .. outputVideo .. "\"")
    elseif commandindex == 10 + (10 * increment) then
        -- Start the next recall if there are more to do
        increment = increment + 1
        if increment < totalrecalls then
            inputVideo = outputVideo
            if increment == totalrecalls - 1 then
                outputVideo = options.outputVideo -- final output video
            else
                outputVideo = increment .. "_output.mp4"
            end
            before = increment .. "_before.mp4"
            after = increment .. "_after.mp4"
            r_temp = increment .. "_innertemp.mp4"
            recalltempbefore = increment .. "_recalltempbefore.mp4"
            recalltempduring = increment .. "_recalltempduring.mp4"
            recalltempafter = increment .. "_recalltempafter.mp4"
            recalltempfinal = increment .. "_recalltempfinal.mp4"
            r_startdelay = 0
            r_length = 5
            r_x = 0
            r_y = 0
            r_width = 640
            r_height = 480
            recallduration = 0
            randomstarttime = 0
            randomrecall = ""
            StartRecall(options, pluginSettings, functions)
        else
            -- Finished!
            finished = true
        end
    end
end

function StopGeneration(options, pluginSettings, functions)
    return finished
end

function ButtonInteraction(options, pluginSettings, functions, mouseX, mouseY, buttonName)
    if buttonName == "Create Recall Video" then
        functions.playSound("Select")
        local filePicked = functions.openFilePicker("Select Recall Video", "Recall Video (*.mp4;*.m4v;*.webm;*.mov;*.avi;*.mkv;*.wmv)|*.mp4;*.m4v;*.webm;*.mov;*.avi;*.mkv;*.wmv", false)
        if filePicked == nil or filePicked == "" then
            functions.setStatusText("No file selected.")
            functions.playSound("Error")
            return false
        end
        -- these are strings!
        local startdelay = pluginSettings["Creation: Delay"]
        local length = pluginSettings["Creation: Length"]
        local innerPosition = pluginSettings["Creation: Inner Position"]
        local innerResolution = pluginSettings["Creation: Inner Resolution"]
        -- Parse the inner position and resolution
        local innerPosParts = {}
        for part in innerPosition:gmatch("([^,]+)") do
            table.insert(innerPosParts, tonumber(part))
        end
        if #innerPosParts ~= 2 then
            functions.setStatusText("Invalid inner position format. Use 'x,y'.")
            functions.playSound("Error")
            return false
        end
        local x = innerPosParts[1]
        local y = innerPosParts[2]
        local innerResParts = {}
        for part in innerResolution:gmatch("([^,]+)") do
            table.insert(innerResParts, tonumber(part))
        end
        if #innerResParts ~= 2 then
            functions.setStatusText("Invalid inner resolution format. Use 'width,height'.")
            functions.playSound("Error")
            return false
        end
        local width = innerResParts[1]
        local height = innerResParts[2]
        -- Validate the inner position and resolution
        if x < 0 or y < 0 or width <= 0 or height <= 0 then
            functions.setStatusText("Invalid inner position or resolution values.")
            functions.playSound("Error")
            return false
        end
        -- Create the file name for the recall video
        local timestamp = os.date("%Y%m%d%H%M%S")
        local fileRenamed = timestamp .. "_s" .. startdelay .. "l" .. length .. "x" .. x .. "y" .. y .. "w" .. width .. "h" .. height .. ".mp4"
        -- Using ffmpeg to re-encode the video as mp4 (just in case)
        functions.runFFmpegSync("-i \"" .. filePicked .. "\" -vcodec libx264 -crf 28 -preset ultrafast -ac 2 -c:a aac -b:a 160k -reset_timestamps 1 -shortest -fflags +genpts -af \"aresample=async=1000\" -y \"" .. fileRenamed .. "\"")
        functions.addToLibrary("video", "recall", fileRenamed)
        functions.setStatusText("Recall video created!")
        functions.playSound("RenderComplete")
        return true
    end
end
