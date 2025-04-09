
using NAudio.Wave;

var musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
var enumerable = Directory.EnumerateFiles(musicFolder, "*.mp3",  SearchOption.AllDirectories);

var files = enumerable.ToList();

var filePath = "";
foreach (var file in files)
{
    filePath = file;
}

using var audioFile = new AudioFileReader(filePath);
using var outputDevice = new WaveOutEvent();
outputDevice.Init(audioFile);
outputDevice.Volume = 0.25f;
outputDevice.Play();
while (outputDevice.PlaybackState == PlaybackState.Playing)
{
    Thread.Sleep(1000);
}