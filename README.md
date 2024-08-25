# Musik Macher

Musik Macher is your go-to solution for **organizing and tagging your music collection**, making it a breeze to use in video editing projects. 🎵🎥
![image](https://github.com/Runinho/MusikMacher/assets/3237686/a0db753e-075d-4fdc-9448-4ef009123924)

## Main features:
- Tag music
- Play music fast
- drag and drop into Adobe Premiere 🚀

## Installation
Download latest version zip from [releases](https://github.com/Runinho/MusikMacher/releases), unzip and run exe.

## Usage
Go to the settings tab and enter the location of your music library, and push "load data":
![image](https://github.com/Runinho/MusikMacher/assets/3237686/5a12ebbc-5c78-404a-a182-d085414926c2)

In the browse tab, you will find your library.
You can drag and drop single songs into your editing program:
![drag_to_premiere](https://github.com/Runinho/MusikMacher/assets/3237686/a459bf46-6710-4dcf-9435-bf9d520c4c43)

### Filter and search
You can also drag and drop songs into the tab list to add the tag to the songs.
Click the checkboxes on the right to filter, or enter a search term to find the songs you need.

## Development
The software uses .Net 7 WPF with data binding and Entity Framework as a database.
Download [Visual Studio 2022](https://visualstudio.microsoft.com/de/downloads/) and .Net 8 to compile the solution.
You also need the UWP development kit to compile NAudio. Don't forget to initalize the submodules with `git submodule init` before compiling.

Make sure to compile `CoverartHelper`, otherwise coverarts do not get loaded.

## Feedback
This is currently a prototype and alpha level software, and there is a lot to do.
If you have ideas or find annoying bugs, feel free to file an [issue](https://github.com/Runinho/MusikMacher/issues/new). Pull requests are welcome :)

## FAQ
### I importet to much what to do?
Currently there is only the option to reset the full db. This will empty all songs and tags you added.

Go to `C:\Users\<USERNAME>\AppData\Local` (replace `<USERNAME>` with your windows username) and delete `track.db`.

[Here](https://clips.twitch.tv/BombasticPleasantDelicataCopyThis-dLi2RsKZ6ylnQJTI) is a Twitch clip from a user that explains it in german.
