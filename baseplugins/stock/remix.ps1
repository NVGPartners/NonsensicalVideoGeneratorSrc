#Remix plugin

# Query
if ($args.Length -eq 1 -and $args[0] -eq "query") {
    Write-Host "1:Remix:remix:140_BPM,_remixes_the_video_in_tune_with_the_music"
    exit 0
}

# Check command line args
if ($args.Length -lt 13) {
    Write-Host "This is a Nonsensical Video Generator plugin."
    Write-Host "Usage: remix.ps1 <video> <width> <height> <temp> <ffmpeg> <ffprobe> <magick> <resources> <sounds> <sources> <music> <library> <options> <settingcount> [<settingname> <settingvalue> ... ...]"
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
$temp4 = Join-Path $temp "temp4.mp4"

# Pick random sound from $library *.wav, *.mp3, *.ogg, *.m4a, *.flac
$librarypath = Join-Path $library audio
$librarypath = Join-Path $librarypath remix
$librarypath = Join-Path $librarypath *
$randomSound = Get-ChildItem -Path $librarypath -File -Include *.wav, *.mp3, *.ogg, *.m4a, *.flac | Get-Random

# Make randomsound "" if it doesn't exist
if ($null -ne $randomSound) {
    $randomSound = $randomSound.FullName.Trim('"')
}

Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -ss 0 -t 0.100 -preset veryfast -y "$temp2"}
Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -ss 0.140 -t 0.050 -preset veryfast -y "$temp3"}
Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -ss 0 -t 0.050 -preset veryfast -y "$temp4"}

# Random sound not found?
if ($null -eq $randomSound) {
    Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -i "$temp3" -i "$temp4" -filter_complex "[0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a]concat=n=42:v=1:a=1[remixv][remixa];[remixa]volume=volume=1[ogaudio]" -map "[remixv]" -map "[remixa]" -preset veryfast -y "$output"}
} else {
    Invoke-Command -ScriptBlock {&$ffmpeg -i "$temp2" -i "$temp3" -i "$temp4" -i "$randomSound" -filter_complex "[0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][2:v][2:a][0:v][0:a][0:v][0:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][1:v][1:a][2:v][2:a][1:v][1:a][1:v][1:a]concat=n=42:v=1:a=1[remixv][remixa];[remixa]volume=volume=1[ogaudio];[3:a]volume=volume=1[sfx];[sfx][ogaudio]amix=inputs=2:duration=shortest:dropout_transition=0:weights='0.3 1':normalize=0[music]" -map "[remixv]" -map "[music]" -preset veryfast -y "$output"}
}
