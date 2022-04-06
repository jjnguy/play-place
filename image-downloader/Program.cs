

var rootUrl = "https://rplace.space/combined/";

var http = new HttpClient
{
  BaseAddress = new Uri(rootUrl)
};

var outDir = "./out/";
Directory.CreateDirectory(outDir);

var htmlResponse = await http.GetAsync("");
var rootHtml = await htmlResponse.Content.ReadAsStringAsync();
await File.WriteAllTextAsync("images_index.html", rootHtml);
var htmlLines = rootHtml.Split("\n");
var links = htmlLines
  .Where(line => line.StartsWith("<a href"))
  .Select(line =>
  {
    var name = line.Split("\"")[1];
    var dateString = line.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1];
    return (name, dateString);
  })
  .ToList();

var fileList = new List<FileDescriptor>();

foreach (var link in links)
{
  fileList.Add(new FileDescriptor(link.name));
  var response = await http.GetAsync(link.name);
  using var output = File.OpenWrite(outDir + link.name);
  using var input = response.Content.ReadAsStream();
  input.CopyTo(output);
}

File.WriteAllText("images_index.json", System.Text.Json.JsonSerializer.Serialize(new
{
  count = fileList.Count,
  files = fileList,
}));

record FileDescriptor(string name)
{
  public DateTime date
  {
    get
    {
      return new DateTime(1970, 1, 1).AddSeconds(double.Parse(name.Split(".")[0]));
    }
  }
}