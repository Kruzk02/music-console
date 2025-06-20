
using NAudio.Wave;
using Spectre.Console;
using Spectre.Console.Rendering;

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

var i = musicFiles.FindIndex(0, mFile => mFile == file);

var audioFile = new AudioFileReader(musicFolder + "/" + file);
using var outputDevice = new WaveOutEvent();

outputDevice.Stop();
outputDevice.Init(audioFile);
outputDevice.Play();

var columns = new List<IRenderable>();
var spaceColumn = new Panel("Hit spacebar to Play or Pause")
{
    Header = new PanelHeader("Play/Pause"),
    Border = BoxBorder.Square,
    Expand = true
};

var volumeColumn = new Panel("Arrow up/down to increase/decrease volume")
{
    Header = new PanelHeader("Volume"),
    Border = BoxBorder.Square,
    Expand = true
};

var switchMusicColumn = new Panel("Arrow left/right to change left/right music")
{
    Header = new PanelHeader("Switch Music"),
    Border = BoxBorder.Square,
    Expand = true
};

var muteVolumeColumn = new Panel("Press M key to mute or unmute")
{
    Header = new PanelHeader("Mute/Unmute volume"),
    Border = BoxBorder.Square,
    Expand = true
};

columns.Add(spaceColumn);
columns.Add(volumeColumn);
columns.Add(switchMusicColumn);
columns.Add(muteVolumeColumn);

AnsiConsole.Write(new Columns(columns){Expand = true});

var oldVolume = 0.0f;

while (true)
{
    _ = Task.Run(async () =>
    {
        try
        {
            while (true)
            {
                BeginTimer();
                await Task.Delay(200);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    });
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
        
        case ConsoleKey.LeftArrow:
            SwitchMusic(--i);
            break;
        case ConsoleKey.RightArrow:
            SwitchMusic(++i);
            break;
        case ConsoleKey.M:
            if (outputDevice.Volume > 0)
            {
                Mute();
            }
            else
            {
                UnMute();
            }
            break;
    }

}

void SwitchMusic(int index)
{
    audioFile.Dispose();
    
    audioFile = new AudioFileReader(musicFolder + "/" + musicFiles[index]);
    outputDevice.Stop();
    outputDevice.Init(audioFile);
    outputDevice.Play();
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

void Mute()
{
    oldVolume = outputDevice.Volume;
    outputDevice.Volume = 0;
}

void UnMute()
{
    outputDevice.Volume = oldVolume;
}

void BeginTimer()
{
    var current = audioFile.CurrentTime;
    var end = audioFile.TotalTime;
    
    var ratio = current.TotalSeconds / end.TotalSeconds;
    
    Console.WriteLine(ratio);
    if (Math.Abs(ratio - 1.0) < 1e-6) {
        outputDevice.Stop();
    }
}