
using NAudio.Wave;
using Spectre.Console;

var filePath = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("What music you want to play?")
        .PageSize(10)
        .MoreChoicesText("[grey](Move up and down to reveal more music)[/]")
        .AddChoices(GetMusicFiles()));

using var audioFile = new AudioFileReader(filePath);
using var outputDevice = new WaveOutEvent();
outputDevice.Init(audioFile);
outputDevice.Volume = 0.25f;
outputDevice.Play();
while (outputDevice.PlaybackState == PlaybackState.Playing)
{
    Thread.Sleep(1000);
}

List<string> GetMusicFiles()
{
    var musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

    return Directory.EnumerateFiles(musicFolder, "*.mp3",  SearchOption.AllDirectories)
        .Select(Path.GetFileNameWithoutExtension)
        .Where(name => name is not null)
        .Select(name => name!)
        .ToList();
}