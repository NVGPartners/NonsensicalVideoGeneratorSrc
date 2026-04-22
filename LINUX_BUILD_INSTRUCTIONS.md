# Linux Build Instructions
The following instructions are for building **Nonsensical Video Generator** from source on Linux, tested on Ubuntu 24.04.

- Prerequisites:
    - [Steam installed from .deb](https://store.steampowered.com/about/)
        - Run `sudo add-apt-repository multiverse`, `sudo dpkg --add-architecture i386` and `sudo apt update` if you're missing dependencies
    - [GE-Proton10-34](https://github.com/GloriousEggroll/proton-ge-custom/releases/tag/GE-Proton10-34)
        - Extract to `~/.local/share/Steam/compatibilitytools.d/GE-Proton10-34` and restart Steam
    - [Nonsensical Video Generator](https://store.steampowered.com/app/2516360/Nonsensical_Video_Generator/)
        - Change its properties in Steam to force **Steam Play** usage of `GE-Proton10-34`
    - [Visual Studio Code](https://code.visualstudio.com/download)
        - [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
        - [C# Dev extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
        - [MonoGame Content Builder (Editor) extension](https://marketplace.visualstudio.com/items?itemName=mangrimen.mgcb-editor)
    - [ImageMagick-*-portable-Q16-HDRI-x64.7z](https://imagemagick.org/script/download.php#windows)
    - [ffmpeg-release-full.7z (gyan.dev)](https://www.gyan.dev/ffmpeg/builds/)
    - [frei0r-*-win64.7z](https://github.com/dyne/frei0r/releases)
    - [yt-dlp.exe](https://github.com/yt-dlp/yt-dlp/releases)
    - [deno-x86_64-pc-windows-msvc.zip](https://github.com/denoland/deno/releases)
    - [vocoder-*-x86-win32.zip](https://borsboom.io/vocoder/)

- Setup:
    - Run **Nonsensical Video Generator** through **GE-Proton10-34** at least once (see above)
    - Clone this repository to `~/~NVGDev/NVGMonoGame` using these commands:
      ```
      mkdir ~/NVGDev
      git clone --recurse-submodules https://github.com/NVGPartners/NonsensicalVideoGeneratorSrc.git ~/NVGDev/NVGMonoGame
      ```
    - Install the `.ttf` fonts inside of `content/fonts/` by opening them (Ubuntu opens a font installer)
    - Open the repository directory in **VS Code** and open a terminal window inside of it
    - Install and link these dependencies (also see [SteamCMD](https://developer.valvesoftware.com/wiki/SteamCMD) if it can't be found) in the VS Code terminal:
        ```
        sudo apt install steamcmd gstreamer1.0-libav gstreamer1.0-plugins-good gstreamer1.0-plugins-bad gstreamer1.0-plugins-ugly gstreamer1.0-plugins-base:i386 gstreamer1.0-libav:i386
        sudo ln -s /lib/x86_64-linux-gnu/libjpeg.so.8 /lib/x86_64-linux-gnu/libjpeg.so.62
        sudo ln -s /lib/i386-linux-gnu/libjpeg.so.8 /lib/i386-linux-gnu/libjpeg.so.62
        ```
        - You will need to type "I AGREE" after scrolling down when installing `steamcmd`
    - Install .NET SDK 8.0.411 in the VS Code terminal:
      ```
      curl -L https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
      chmod +x ./dotnet-install.sh
      ./dotnet-install.sh --version 8.0.411
      ```
    - Open `~/.bashrc` in VS Code and add these lines to the end of the file and save it:
      ```
      export PATH="$PATH:$HOME/.dotnet:$HOME/.dotnet/tools"
      export DOTNET_ROOT="$HOME/.dotnet"
      ```
    - Log out and log back in
    - Install the MGCB Editor in the VS Code terminal:
      ```
      dotnet tool install --global dotnet-mgcb-editor
      mgcb-editor --register
      ```
    - Copy `cred.template.sh` in the root directory
        - Paste as a new file named `cred.sh`
        - Fill in the placeholders with your details
    - Run setup scripts in the VS Code terminal:
      ```
      chmod +x cred.sh
      chmod +x install-dotnet-sdk-in-proton.sh
      chmod +x proton.sh
      chmod +x publish.sh
      chmod +x start.sh
      ./install-dotnet-sdk-in-proton.sh
      ```
    - Extract `ImageMagick-*-portable-Q16-HDRI-x64.7z` and copy only these files into `baseroot/bin/`:
      - All `*.xml` files
      - `magick.exe`
      - All `*.dll` files
      - `sRGB.icc`
    - Extract `ffmpeg.exe` and `ffprobe.exe` from `ffmpeg-release-full.7z` into `baseroot/bin/`
    - Extract all `.dll` files from `filter/` in `frei0r-*-win64.7z` into `baseroot/bin/frei0r-1` (create the directory if it doesn't exist)
    - Copy `yt-dlp.exe` into `baseroot/bin/`
    - Extract `deno.exe` from `deno-x86_64-pc-windows-msvc.zip` into `baseroot/bin/`
    - Extract `vocoder.exe` from `vocoder-*-x86-win32.zip` into `baseroot/bin/`
    - Run `dotnet restore` inside of the root directory to install dependencies
- Building:
    - Open the **Run and Debug** tab in VS Code
    - Select "Build and Launch (Debug)"
    - Ignore the `No frameworks were found.` error, see below
- Testing:
    - Type `./start.sh` in a VS Code terminal
    - After closing NVG you may have to press Ctrl+C in the terminal to cancel Proton
- Publishing:
    - Make sure that `NonsensicalVideoGenerator.csproj` has an incremented version number from live release
      - `1.[Major].[Minor].[Patch]` format is used for versioning
      - Builds are prefixed with `1` because this is NVG 1 and not NVG 2
    - Edit `BlogData.cs`, `update.txt`, and `update.md` with your changelog
      - Set the `Tier` variable depending on the importance of the update
    - Create a new Steam news post for the update and paste the contents of `update.txt` inside
    - Add the post ID to `BlogData.cs`
    - Open the **Run and Debug** tab in VS Code
    - Select "Build and Launch (Release)", ignoring `No frameworks were found.`
    - Close Steam to prevent login conflicts
    - Type `publish.sh` in a VS Code terminal to upload it to steam
    - Provide your password and Steam guard authentication if it asks for them
    - Once the build is uploaded, visit the [SteamPipe partner page](https://partner.steamgames.com/apps/builds/2516360)
    - Set current `default` branch build as live on `previous` branch
        - Preview the change and accept it
    - Set latest build to `default` branch
        - Preview the change and accept it
        - Confirm the build in your Steam mobile app
        - Proceed to the next page
        - Link the Steam news post to the build and then make it public by publishing it
