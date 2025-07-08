# Music Console

a simple interactive MP3 Player built with .NET, leveraging [NAudio](https://github.com/naudio/NAudio) for audio playback and [Spectre.Console](https://github.com/spectreconsole/spectre.console) for a command-line UI.

# Feature
- Play ```.mp3``` files from system's ```Music``` folder
- Control playback (Play/Pause, Volume, Next/Previous, Mute)
- Loop mode and Shuffle mode
- Live progress bar for playback status

# Requirements
- .NET 9.0 SDK
- NAudio NuGet package
- Spectre.Console NuGet package
- Your Music folder should contain ```.mp3``` files

# Installation

1. Clone the repository: ` https://github.com/Kruzk02/music-console.git `.
2. Navigate to the project directory: `cd music-console `.
3. Install dependencies: 
    ```
    dotnet add package NAudio
    dotnet add package Spectre.Console
    ```
4. Build and run: `dotnet run`
