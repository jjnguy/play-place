

var imageSetString = File.ReadAllText("../image-downloader/images_index.json");
var imageSet = System.Text.Json.JsonSerializer.Deserialize<ImageSet>(imageSetString);
Console.WriteLine(imageSet.count);




record FileDescriptor(string name, string date);
record ImageSet(int count, List<FileDescriptor> files);