<p align="center">
  <img width=50% alt="Nonsensical Video Generator logo" src="https://i.imgur.com/4o4bXj5.png">
</p>

# Nonsensical Video Generator (Source)

This repository provides the source code and build system for [Nonsensical Video Generator](https://store.steampowered.com/app/2516360/Nonsensical_Video_Generator/).

Public issue tracker can be found [here](https://github.com/KiwifruitDev/NonsensicalVideoGenerator).

## Building
The following instructions are for building **Nonsensical Video Generator** from source.

- Prerequisites:
    - [Visual Studio Code](https://code.visualstudio.com/download)
        - [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
    - [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Setup:
    - Run `dotnet restore` inside of the root directory to install dependencies
    - Copy `cred.template.bat` in the root directory
        - Paste as a new file named `cred.bat`
        - Fill in the placeholders with your details
- Debugging:
    - Open the **Run and Debug** tab in VS Code
    - Select "Build and Launch (Debug WindowsDX)"
- Publishing:
    - Make sure that `NonsensicalVideoGenerator.csproj` has an incremented version number from live release
    - Close Steam to prevent login conflicts
    - Open the command pallette in VS Code (Ctrl+Shift+P)
    - Press backspace to remove the prepending `>`
    - Type `task publish` to build the release version
    - After release build is finished, type `task upload`
    - Inside of the new terminal window, type in your Steam Guard code
    - Once the build is uploaded, visit the [SteamPipe partner page](https://partner.steamgames.com/apps/builds/2516360)
    - Set current `default` branch build as live on `previous` branch
        - Preview the change and accept it
    - Set latest build to `default` branch
        - Preview the change and accept it
        - Confirm the build in your Steam mobile app
        - Proceed to the next page
        - Create a changelog for the new build and publish it

DLL files available in `packages` are built beforehand to prevent .NET conflicts:
- [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET)
- [MonoGame.Extended](https://github.com/craftworkgames/MonoGame.Extended)
- [MonoGame.Extended2](https://github.com/hozuki/MonoGame.Extended2)
- [MoonSharp](

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
