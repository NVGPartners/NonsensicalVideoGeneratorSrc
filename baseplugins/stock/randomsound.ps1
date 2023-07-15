# Random sound plugin

# Query
if ($args.Length -eq 1 -and $args[0] -eq "query") {
    # No query support
    exit 0
}

# Check command line args
if ($args.Length -lt 13) {
    Write-Host "This is a Nonsensical Video Generator plugin."
    Write-Host "Usage: randomsound.ps1 <video> <width> <height> <temp> <ffmpeg> <ffprobe> <magick> <resources> <sounds> <sources> <music> <library> <options> <settingcount> [<settingname> <settingvalue> ... ...]"
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


# Pick random sound from $library *.wav, *.mp3, *.ogg, *.m4a, *.flac
$librarypath = Join-Path $library audio
$librarypath = Join-Path $librarypath sfx
$librarypath = Join-Path $librarypath *
$randomSound = Get-ChildItem -Path $librarypath -File -Include *.wav, *.mp3, *.ogg, *.m4a, *.flac | Get-Random

# Apply random sound
if ($null -ne $randomSound) {
    $randomSound = $randomSound.FullName.Trim('"')

    # Pick whether or not to mute original audio
    $muteOriginalAudio = Get-Random -Minimum 0 -Maximum 1

    # Use original audio if equal to 0
    if ($muteOriginalAudio -eq 0) {
        Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -i "$randomSound" -filter_complex "[0:a]volume=1[a0];[1:a]volume=1[a1];[a0][a1]amix=inputs=2[a]" -map 0:v -map "[a]" -c:v copy -c:a aac -shortest -y "$output"}
    } else {
        Invoke-Command -ScriptBlock {&$ffmpeg -i "$video" -i "$randomSound" -filter_complex "[0:a]volume=0[a0];[1:a]volume=1[a1];[a0][a1]amix=inputs=2[a]" -map 0:v -map "[a]" -c:v copy -c:a aac -shortest -y "$output"}
    }
}
