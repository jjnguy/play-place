using Frogger;
using Frogger.Adapters;
using Microsoft.VisualBasic.FileIO;
using PostgresNet;

const string rawFilePath = "C:\\Users\\justi\\Downloads\\2022_place_canvas_history.csv\\2022_place_canvas_history.csv";

using var rawFile = File.Open(rawFilePath, FileMode.Open);
using var reader = new StreamReader(rawFile);
using var parser = new TextFieldParser(reader);
parser.SetDelimiters(",");

var line = default(string[]?);
parser.ReadLine();

var frogger = new Frogger.FroggerClient("data-loader").WriteTo(new ConsoleAdapter(false)
{
  Level = Microsoft.Extensions.Logging.LogLevel.Critical,
});
var db = new Db(databaseConnectionString, frogger);

var knownUsers = new Dictionary<string, long>();
var knownColors = new Dictionary<string, short>();

var lineCount = 0;

await db.OpenAsync(async conn =>
{
  do
  {
    line = parser.ReadFields();

    if (line is not null)
    {
      var (rawTime, userHash, colorHex, pixelString) = line;

      var timeStamp = DateTimeOffset.Parse(rawTime.Replace(" UTC", ""));
      var userIsKnown = knownUsers.ContainsKey(userHash);
      var colorIsKnown = knownColors.ContainsKey(colorHex);

      var splitPx = pixelString.Split(",");
      var x1 = short.Parse(splitPx[0]);
      var y1 = short.Parse(splitPx[1]);
      var x2 = splitPx.Length == 2 ? default(short?) : short.Parse(splitPx[2]);
      var y2 = splitPx.Length == 2 ? default(short?) : short.Parse(splitPx[3]);

      if (!userIsKnown)
      {
        var userRow = await conn.Sql($"SELECT id FROM users WHERE hash = {userHash}").Query().ToList();
        if (userRow.Count == 0)
        {
          userRow = await conn.Sql($"INSERT INTO users (hash) VALUES ({userHash}) RETURNING id").Query().ToList();
        }
        knownUsers[userHash] = userRow.Single().Id();
      }

      if (!colorIsKnown)
      {
        var colorRow = await conn.Sql($"SELECT id FROM colors WHERE hex = {colorHex}").Query().ToList();
        if (colorRow.Count == 0)
        {
          colorRow = await conn.Sql($"INSERT INTO colors (hex) VALUES ({colorHex}) RETURNING id").Query().ToList();
        }
        var row = colorRow.Single();
        var raw = row["id"];
        knownColors[colorHex] = (short)raw;
      }

      await conn.Sql($@"INSERT INTO pixels 
        (user_id, timestamp, color_id, x1, y1, x2, y2) VALUES 
        ({knownUsers[userHash]},{(int)timeStamp.ToUnixTimeSeconds()},{knownColors[colorHex]},{x1},{y1},{x2},{y2})").Execute();
      lineCount++;

      if (lineCount % 1000 == 0)
      {
        Console.WriteLine(lineCount);
      }
    }
  } while (line is not null);

});



internal static class Ex
{
  public static void Deconstruct(this string[] source, out string one, out string two, out string three, out string four)
  {
    one = source[0];
    two = source[1];
    three = source[2];
    four = source[3];
  }
}