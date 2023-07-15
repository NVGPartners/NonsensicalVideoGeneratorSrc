# Speed plugin

# Query
if ($args.Length -eq 1 -and $args[0] -eq "query") {
    # No query support
    exit 0
}

# Check command line args
if ($args.Length -lt 13) {
    Write-Host "This is a Nonsensical Video Generator plugin."
    Write-Host "Usage: speed.ps1 <video> <width> <height> <temp> <ffmpeg> <ffprobe> <magick> <resources> <sounds> <sources> <music> <library> <options> <settingcount> [<settingname> <settingvalue> ... ...]"
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


# Pick slow down or speed up
$speed = Get-Random -Minimum 0 -Maximum 2

# Apply speed filter
if ($speed -eq 0) {
    # Speed up
    Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -filter:v setpts=0.5*PTS -filter:a atempo=2.0 -y "$output"}
} else {
    # Slow down
    Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -filter:v setpts=2.0*PTS -filter:a atempo=0.5 -y "$output"}
}
