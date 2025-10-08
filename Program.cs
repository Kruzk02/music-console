
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

var index = musicFiles.FindIndex(0, mFile => mFile == file);

var audioFile = new AudioFileReader(musicFolder + "/" + file);
using var outputDevice = new WaveOutEvent();

outputDevice.Stop();
outputDevice.Init(audioFile);
outputDevice.Play();

var shuffleMusic= new List<string>(musicFiles) ;
var oldVolume = 0.0f;
var isLoop = false;
var isShuffle = false;

var columns = new List<IRenderable>();

var spaceColumn = SetPanel("Hit space bar to Play or Pause", "Play/Pause");
columns.Add(spaceColumn);

var volumeColumn = SetPanel("Arrow up/down to increase/decrease volume", "Volume");
columns.Add(volumeColumn);

var switchMusicColumn = SetPanel("Arrow left/right to change left/right music", "Switch Music");
columns.Add(switchMusicColumn);

var jumpTimeMusicColumn = SetPanel("A/D to change forward/backward current time music", "Peek Music");
columns.Add(jumpTimeMusicColumn);

var loopMusicColumn = SetPanel("Hit R key to loop music", "Loop Music");
columns.Add(loopMusicColumn);

var shuffleMusicColumn = SetPanel("Press S key to shuffle", "Shuffle Music");
columns.Add(shuffleMusicColumn);

var muteVolumeColumn = SetPanel("Press M key to mute or unmute", "Mute/Unmute volume");
columns.Add(muteVolumeColumn);

var quiteColumn = SetPanel("Press Q key to Exit", "Exit");
columns.Add(quiteColumn);

AnsiConsole.Write(new Columns(columns){Expand = true});

// ReSharper disable FunctionNeverReturns
await AnsiConsole.Progress()
    .AutoClear(true)
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
                if (isLoop) SwitchMusic(index);
                else NextMusic();
                task = SetMusicName(ctx);
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
                        task = SetMusicName(ctx);
                        break;
                    case ConsoleKey.RightArrow:
                        NextMusic();
                        task = SetMusicName(ctx);
                        break;
                    case ConsoleKey.A:
                        if (audioFile.CurrentTime > TimeSpan.Zero) 
                            ChangeCurrentTimeOfAudio(TimeSpan.FromSeconds(-10));
                        break;
                    case ConsoleKey.D:
                        if (audioFile.CurrentTime < audioFile.TotalTime)
                            ChangeCurrentTimeOfAudio(TimeSpan.FromSeconds(10));
                        break;
                    case ConsoleKey.M:
                        if (outputDevice.Volume > 0) Mute();
                        else UnMute();
                        break;
                    case ConsoleKey.R:
                        isLoop = !isLoop;
                        break;
                    case ConsoleKey.Q:
                        isRunning = false;
                        break;
                    case ConsoleKey.S:
                        isShuffle = !isShuffle;
                        if (isShuffle)
                        {
                            ShuffleMusic();
                            task = SetMusicName(ctx);
                        }
                        else
                        {
                            SwitchMusic(0);
                            task = SetMusicName(ctx);
                        }
                        break;
                }
            }
            
            await Task.Delay(200);
        }
    });
return;
// ReSharper restore FunctionNeverReturns

ProgressTask SetMusicName(ProgressContext ctx)
{
   return isShuffle ? ctx.AddTask(shuffleMusic[index]) : ctx.AddTask(musicFiles[index]);
}

void ChangeCurrentTimeOfAudio(TimeSpan timeSpan)
{
    audioFile.CurrentTime = audioFile.CurrentTime.Add(timeSpan);
}

void SwitchMusic(int i)
{
    audioFile.Dispose();
    
    audioFile = new AudioFileReader(musicFolder + "/" + (isShuffle ? shuffleMusic[i] : musicFiles[i]));
    outputDevice.Stop();
    outputDevice.Init(audioFile);
    outputDevice.Play();
}

void ShuffleMusic()
{
    var random = new Random();
    
    for (var i = musicFiles.Count - 1; i > 0; i--)
    {
        var j = random.Next(i + 1);
        
        (shuffleMusic[i], shuffleMusic[j]) = (shuffleMusic[j], shuffleMusic[i]);
    }

    index = 0;
    SwitchMusic(0);
}

void NextMusic()
{
    if (index + 1 < musicFiles.Count) SwitchMusic(++index);
}

void PrevMusic()
{
    if (index > 0) SwitchMusic(--index);
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