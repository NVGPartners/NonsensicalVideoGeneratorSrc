# Mirror Symmetry plugin

# Query
if ($args.Length -eq 1 -and $args[0] -eq "query") {
    # No query support
    exit 0
}

# Check command line args
if ($args.Length -lt 13) {
    Write-Host "This is a Nonsensical Video Generator plugin."
    Write-Host "Usage: mirrorsymmetry.ps1 <video> <width> <height> <temp> <ffmpeg> <ffprobe> <magick> <resources> <sounds> <sources> <music> <library> <options> <settingcount> [<settingname> <settingvalue> ... ...]"
    exit 1
}

# Get command line args
$video = $args[0]
$width = $args[1]
$height = $args[2]
$temp = $args[3]
$ffmpeg = $args[4]
$ffprobe = $args[5]
$magick = $args[6]
$resources = $args[7]
$sounds = $args[8]
$sources = $args[9]
$music = $args[10]
$library = $args[11]
$options = $args[12]
$output = $args[13]
$settingcount = $args[14]

# Temp files

$temp2 = Join-Path $temp "temp2.mp4"
$temp3 = Join-Path $temp "temp3.mp4"

# Pick which sides to mirror
$mirrorVerticalOrHorizontal = Get-Random -Minimum 0 -Maximum 2
$mirrorSide = Get-Random -Minimum 0 -Maximum 2

# Apply effect
if ($mirrorVerticalOrHorizontal -eq 0) {
    # hflip
    if ($mirrorSide -eq 0) {
        # Crop video to half width (temp2)
        Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -filter:v "crop=in_w/2:in_h:0:0" -c:a copy -preset veryfast -y "$temp2"}
        # Mirror video (temp3)
        Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -vf "hflip" -c:a copy -preset veryfast -y "$temp3"}
        # Combine videos
        Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -i "$temp3" -filter_complex "[0:v][1:v]hstack=inputs=2[v];[0:a][1:a]amerge[a]" -map "[v]" -map "[a]" -ac 2 -c:v libx264 -c:a aac -preset veryfast -y "$output"}
    }
    else {
        # Crop video to half width panned to right side (temp2)
        Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -filter:v "crop=in_w/2:in_h:in_w/2:0" -c:a copy -preset veryfast -y "$temp2"}
        # Mirror video (temp3)
        Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -vf "hflip" -c:a copy -preset veryfast -y "$temp3"}
        # Combine videos
        Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -i "$temp3" -filter_complex "[0:v][1:v]hstack=inputs=2[v];[0:a][1:a]amerge[a]" -map "[v]" -map "[a]" -ac 2 -c:v libx264 -c:a aac -preset veryfast -y "$output"}
    }
}
else {
    # vflip
    if ($mirrorSide -eq 0) {
        # Crop video to half height (temp2)
        Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -filter:v "crop=in_w:in_h/2:0:0" -c:a copy -preset veryfast -y "$temp2"}
        # Mirror video (temp3)
        Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -vf "vflip" -c:a copy -preset veryfast -y "$temp3"}
        # Combine videos
        Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -i "$temp3" -filter_complex "[0:v][1:v]vstack=inputs=2[v];[0:a][1:a]amerge[a]" -map "[v]" -map "[a]" -ac 2 -c:v libx264 -c:a aac -preset veryfast -y "$output"}
    }
    else {
        # Crop video to half height panned to bottom side (temp2)
        Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -filter:v "crop=in_w:in_h/2:0:in_h/2" -c:a copy -preset veryfast -y "$temp2"}
        # Mirror video (temp3)
        Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -vf "vflip" -c:a copy -preset veryfast -y "$temp3"}
        # Combine videos
        Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -i "$temp3" -filter_complex "[0:v][1:v]vstack=inputs=2[v];[0:a][1:a]amerge[a]" -map "[v]" -map "[a]" -ac 2 -c:v libx264 -c:a aac -preset veryfast -y "$output"}
    }
}
