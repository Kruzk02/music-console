
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

var oldVolume = 0.0f;
var isLoop = false;

var columns = new List<IRenderable>();

var spaceColumn = SetPanel("Hit space bar to Play or Pause", "Play/Pause");
columns.Add(spaceColumn);

var volumeColumn = SetPanel("Arrow up/down to increase/decrease volume", "Volume");
columns.Add(volumeColumn);

var switchMusicColumn = SetPanel("Arrow left/right to change left/right music", "Switch Music");
columns.Add(switchMusicColumn);

var loopMusicColumn = SetPanel("Hit R key to loop music", "Loop Music");
columns.Add(loopMusicColumn);

var muteVolumeColumn = SetPanel("Press M key to mute or unmute", "Mute/Unmute volume");
columns.Add(muteVolumeColumn);

var quiteColumn = SetPanel("Press Q key to Exit", "Exit");
columns.Add(quiteColumn);

AnsiConsole.Write(new Columns(columns){Expand = true});

// ReSharper disable FunctionNeverReturns
await AnsiConsole.Progress()
    .AutoClear(false)
    .StartAsync(async ctx =>
    {
        var task = ctx.AddTask(file);
        var isRunning = true;
        while (isRunning)
        {
            var progress = BeginTimer();
            task.Value = Math.Round(progress * 100, 2);
            
            if (task.Value >= 99.99)
            {
                if (isLoop)
                {
                    SwitchMusic(i);
                }
                else
                {
                    NextMusic();
                }
                task = ctx.AddTask(musicFiles[i]);   
            }
            
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
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
                        PrevMusic();
                        task = ctx.AddTask(musicFiles[i]);
                        break;
                    case ConsoleKey.RightArrow:
                        NextMusic();
                        task = ctx.AddTask(musicFiles[i]);
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
                    case ConsoleKey.R:
                        isLoop = !isLoop;
                        break;
                    case ConsoleKey.Q:
                        isRunning = false;
                        break;
                }
            }
            
            await Task.Delay(200);
        }
    });
// ReSharper restore FunctionNeverReturns

void SwitchMusic(int index)
{
    audioFile.Dispose();
    
    audioFile = new AudioFileReader(musicFolder + "/" + musicFiles[index]);
    outputDevice.Stop();
    outputDevice.Init(audioFile);
    outputDevice.Play();
}

void NextMusic()
{
    if (i + 1 < musicFiles.Count) SwitchMusic(++i);
}

void PrevMusic()
{
    if (i > 0) SwitchMusic(--i);
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

double BeginTimer()
{
    var current = audioFile.CurrentTime;
    var end = audioFile.TotalTime;
    
    var ratio = current.TotalSeconds / end.TotalSeconds;
    
    return ratio;
}

Panel SetPanel(string text, string header)
{
    return new Panel(text)
    {
        Header = new PanelHeader(header),
        Border = BoxBorder.Square,
        Expand = true
    };
}