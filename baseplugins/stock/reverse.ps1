# Reverse plugin

# Query
if ($args.Length -eq 1 -and $args[0] -eq "query") {
    # No query support
    exit 0
}

# Check command line args
if ($args.Length -lt 13) {
    Write-Host "This is a Nonsensical Video Generator plugin."
    Write-Host "Usage: reverse.ps1 <video> <width> <height> <temp> <ffmpeg> <ffprobe> <magick> <resources> <sounds> <sources> <music> <library> <options> <settingcount> [<settingname> <settingvalue> ... ...]"
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

# Pick whether or not to perform "forward-reverse"
$forwardReverse = Get-Random -Minimum 0 -Maximum 2

# Apply reverse filter
if ($forwardReverse -eq 0) {
    # Normal reverse
    Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -vf reverse -af areverse -y "$output"}
} else {
    # Half forward, half reverse
    # Get length of video
    $length = $(ffprobe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 "$video")
    # Get half of length
    $halfLength = $length / 2
    # Split video into two parts
    Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -t $halfLength -y "$temp2"}
    # Reverse second part
    Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -vf reverse -af areverse -y "$temp3"}
    # Concatenate two parts
    Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -i "$temp3" -filter_complex "[0:v:0][0:a:0][1:v:0][1:a:0]concat=n=2:v=1:a=1[v][a]" -map "[v]" -map "[a]" -y "$output"}
}
