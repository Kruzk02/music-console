
var musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
var enumerable = Directory.EnumerateFiles(musicFolder, "*.mp3",  SearchOption.AllDirectories);

var files = enumerable.ToList();

foreach (var file in files)
{
    Console.WriteLine(file);
}