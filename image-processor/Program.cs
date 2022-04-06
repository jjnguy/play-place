using System.Drawing;

var imageSetString = File.ReadAllText("../image-downloader/images_index.json");
var imageSet = System.Text.Json.JsonSerializer.Deserialize<ImageSet>(imageSetString);
Console.WriteLine(imageSet.count);

var sorted = imageSet.files.OrderBy(f => f.name).ToArray();

// TODO: how do I deal with images?

var previousImage = Graphics.FromImage(Image.FromFile("../image-downloader/out/" + sorted[0].name));
foreach (var file in sorted)
{
  var img = Graphics.FromImage(Image.FromFile("../image-downloader/out/" + file.name));
  for (var x = 0; x < img.)
}


record FileDescriptor(string name, string date);
record ImageSet(int count, List<FileDescriptor> files);
record PixelDiff(short x, short y, byte newColor);