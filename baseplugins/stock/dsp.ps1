# DSP plugin

# Query
if ($args.Length -eq 1 -and $args[0] -eq "query") {
    Write-Host ";|Chorus_Enabled:1:Use_chorus_audio_effect.;Vibrato_Enabled:1:Use_vibrato_audio_effect.;G_Major_Enabled:1:Use_G_major_audio_effect.;G_Major_Negate:1:Negate_video_colors_when_using_G_major."
    exit 0
}

# Check command line args
if ($args.Length -lt 13) {
    Write-Host "This is a Nonsensical Video Generator plugin."
    Write-Host "Usage: dsp.ps1 <video> <width> <height> <temp> <ffmpeg> <ffprobe> <magick> <resources> <sounds> <sources> <music> <library> <options> <settingcount> [<settingname> <settingvalue> ... ...]"
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

$offset = 0
for ($i = 0; $i -lt $settingcount; $i++) {
    $settingname = $args[15 + $i + $offset]
    $settingvalue = $args[16 + $i + $offset]
    if ($settingname -eq "Chorus_Enabled") {
        $useChorus = [int]$settingvalue
    }
    if ($settingname -eq "Vibrato_Enabled") {
        $useVibrato = [int]$settingvalue
    }
    if ($settingname -eq "G_Major_Enabled") {
        $useGMajor = [int]$settingvalue
    }
    if ($settingname -eq "G_Major_Negate") {
        $useGMajorNegate = [int]$settingvalue
    }
    $offset++
}

# Temp files


# Pick which DSP effect to apply
$listOfEffects = @()
if ($useChorus -eq 1) {
    $listOfEffects += "chorus"
}
if ($useVibrato -eq 1) {
    $listOfEffects += "vibrato"
}
if ($useGMajor -eq 1) {
    $listOfEffects += "gmajor"
}
$dspEffect = Get-Random -InputObject $listOfEffects

# Apply DSP filter
switch ($dspEffect) {
    # Chorus
    "chorus" {
        Write-Host "Applying chorus effect..."
        $command = {&$ffmpeg -i "$video" -af chorus="0.5:0.9:50|60|40:0.4|0.32|0.3:0.25|0.4|0.3:2|2.3|1.3" -y "$output"}
    }
    # Vibrato
    "vibrato" {
        Write-Host "Applying vibrato effect..."
        $command = {&$ffmpeg -i "$video" -af vibrato=f=7.0:d=0.5 -y "$output"}
    }
    # G Major
    "gmajor" {
        Write-Host "Applying G major effect..."
        if($useGMajorNegate -eq 1) {
            $command = {&$ffmpeg -i "$video" -filter_complex "[0:a]asetrate=22050,aresample=44100,atempo=2[lowc];[0:a]asetrate=33037.671045130824,aresample=44100,atempo=1.3348398541700344[lowg];[0:a]asetrate=55562.51830036391,aresample=44100,atempo=.7937005259840998[e];[0:a]asetrate=66075.34209026165,aresample=44100,atempo=.6674199270850172[g];[0:a]asetrate=88200,aresample=44100,atempo=.5[highc];[0:a][lowc][lowg][e][g][highc]amix=inputs=6[aud]" -map 0:v -map "[aud]" -vf negate -y "$output"}
        } else {
            $command = {&$ffmpeg -i "$video" -filter_complex "[0:a]asetrate=22050,aresample=44100,atempo=2[lowc];[0:a]asetrate=33037.671045130824,aresample=44100,atempo=1.3348398541700344[lowg];[0:a]asetrate=55562.51830036391,aresample=44100,atempo=.7937005259840998[e];[0:a]asetrate=66075.34209026165,aresample=44100,atempo=.6674199270850172[g];[0:a]asetrate=88200,aresample=44100,atempo=.5[highc];[0:a][lowc][lowg][e][g][highc]amix=inputs=6[aud]" -map 0:v -map "[aud]" -y "$output"}
        }
    }
}

# Invoke effect
Invoke-Command -ScriptBlock $command

