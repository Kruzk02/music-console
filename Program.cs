
using NAudio.Wave;
using Spectre.Console;

var musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
var musicFiles = Directory.EnumerateFiles(musicFolder, "*.mp3",  SearchOption.AllDirectories)
    .Select(Path.GetFileName)
    .Where(name => name is not null)
    .Select(name => name!)
    .ToList();

var file = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("What music you want to play?")
        .PageSize(10)
        .MoreChoicesText("[grey](Move up and down to reveal more music)[/]")
        .AddChoices(musicFiles));

using var audioFile = new AudioFileReader(musicFolder + "/" + file);
using var outputDevice = new WaveOutEvent();

outputDevice.Init(audioFile);
outputDevice.Volume = 1.0f;
outputDevice.Play();

while (true)
{
    var key = Console.ReadKey(true).Key;
    
    HandleKey(key);
}

void HandleKey(ConsoleKey key)
{
    switch (key)
    {
        case ConsoleKey.Spacebar:
            TogglePlayback();
            break;

        case ConsoleKey.UpArrow:
            AdjustVolume(+0.1f);
            break;

        case ConsoleKey.DownArrow:
            AdjustVolume(-0.1f);
            break;
    }
}

void TogglePlayback()
{           
    if (outputDevice.PlaybackState == PlaybackState.Playing)
        outputDevice.Stop();
    else
        outputDevice.Play();
}

void AdjustVolume(float delta)
{
    outputDevice.Volume = Math.Clamp(outputDevice.Volume + delta, 0.0f, 1.0f);   
}