<p align="center">
  <img width=50% alt="Nonsensical Video Generator logo" src="https://i.imgur.com/4o4bXj5.png">
</p>

# Nonsensical Video Generator (Source)

This repository provides the source code and build system for [Nonsensical Video Generator](https://store.steampowered.com/app/2516360/Nonsensical_Video_Generator/).

Public issue tracker can be found [here](https://github.com/NVGPartners/NonsensicalVideoGenerator).

## Building
The following instructions are for building **Nonsensical Video Generator** from source.

- Prerequisites:
    - [Visual Studio Code](https://code.visualstudio.com/download)
        - [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
        - [C# Dev extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
        - [MonoGame Content Builder (Editor) extension](https://marketplace.visualstudio.com/items?itemName=mangrimen.mgcb-editor)
    - [.NET SDK 8.0.411](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-8.0.411-windows-x64-installer)
    - [ImageMagick-*-portable-Q16-HDRI-x64.7z](https://imagemagick.org/script/download.php#windows)
    - [ffmpeg-release-full.7z (gyan.dev)](https://www.gyan.dev/ffmpeg/builds/)
    - [frei0r-*-win64.7z](https://github.com/dyne/frei0r/releases)
    - [yt-dlp.exe](https://github.com/yt-dlp/yt-dlp/releases)
    - [vocoder-*-x86-win32.zip](https://borsboom.io/vocoder/)
- Setup:
    - Clone this repository to `C:\NVGDev\NVGMonoGame` using these commands:
      ```
      mkdir C:\NVGDev
      git clone --recurse-submodules https://github.com/NVGPartners/NonsensicalVideoGeneratorSrc.git C:\NVGDev\NVGMonoGame
      ```
    - Extract `ImageMagick-*-portable-Q16-HDRI-x64.7z` and copy only these files into `baseroot/bin/`:
      - All `*.xml` files
      - `magick.exe`
      - All `*.dll` files
      - `sRGB.icc`
    - Extract `ffmpeg.exe` and `ffprobe.exe` from `ffmpeg-release-full.7z` into `baseroot/bin/`
    - Extract all `.dll` files from `filter/` in `frei0r-*-win64.7z` into `baseroot/bin/frei0r-1` (create the directory if it doesn't exist)
    - Copy `yt-dlp.exe` into `baseroot/bin/`
    - Extract `vocoder.exe` from `vocoder-*-x86-win32.zip` into `baseroot/bin/`
    - Run `dotnet restore` inside of the root directory to install dependencies
    - Copy `cred.template.bat` in the root directory
        - Paste as a new file named `cred.bat`
        - Fill in the placeholders with your details
- Debugging:
    - Open the **Run and Debug** tab in VS Code
    - Select "Build and Launch (Debug)"
- Publishing:
    - Make sure that `NonsensicalVideoGenerator.csproj` has an incremented version number from live release
    - Edit `BlogData.cs`, `update.txt`, and `update.md` with your changelog
    - Create a new Steam news post for the update and paste the contents of `update.txt` inside
    - Add the post ID to `BlogData.cs`
    - Close Steam to prevent login conflicts
    - Open the command pallette in VS Code (Ctrl+Shift+P)
    - Press backspace to remove the prepending `>`
    - Type `task publish` to build the release version
    - After release build is finished, type `task upload` to upload it to steam
    - Provide your password and Steam guard authentication if it asks for them
    - Once the build is uploaded, visit the [SteamPipe partner page](https://partner.steamgames.com/apps/builds/2516360)
    - Set current `default` branch build as live on `previous` branch
        - Preview the change and accept it
    - Set latest build to `default` branch
        - Preview the change and accept it
        - Confirm the build in your Steam mobile app
        - Proceed to the next page
        - Link the Steam news post to the build and then make it public by publishing it

DLL files available in `packages` are built beforehand to prevent .NET conflicts:
- [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET)
- [MonoGame.Extended](https://github.com/craftworkgames/MonoGame.Extended)
- [MonoGame.Extended2](https://github.com/NVGPartners/MonoGame.Extended2)
- [MoonSharp](https://github.com/NVGPartners/moonsharp-PR-327)

## Links
<a href="https://store.steampowered.com/app/2516360/Nonsensical_Video_Generator/" target="_blank" alt="Nonsensical Video Generator Store Page" title="Steam Store Page">
  <img width="50%" src="https://i.imgur.com/Dc34oSC.png">
</a>
<br>
<a href="https://steamcommunity.com/app/2516360/workshop/" target="_blank" alt="Nonsensical Video Generator Steam Workshop" title="Steam Workshop">
  <img width="50%" src="https://i.imgur.com/Bz3Nf6O.png">
</a>
<br>
<a href="https://discord.gg/8ppmspR6Wh" target="_blank" alt="Nonsensical Video Generator Discord" title="Discord">
  <img width="50%" src="https://i.imgur.com/X5CC4vv.png">
</a>

## License
Nonsensical Video Generator is licensed under the GNU General Public License v3.0. See [LICENSE](LICENSE) for more information.
