using Info;
using System.Text;

// This folder contains named music files which are used to build the database
string reference_folder = "reference";
bool force_rebuild = false;
int? maxToProcess = null;

EasyFingerPrinting[] fingerPrinterCascade = new EasyFingerPrinting[] {
    new EasyFingerPrinting(Path.Combine(reference_folder, "mario"), "database_mario", force_rebuild, maxToProcess),
    new EasyFingerPrinting(Path.Combine(reference_folder, "manual_matches"), "database_manual_matches", force_rebuild, maxToProcess),
    new EasyFingerPrinting(Path.Combine(reference_folder, "console_hou"), "database_console_hou", force_rebuild, maxToProcess),
};

Console.WriteLine($"--------------- Building Filename List [{reference_folder}]-----------------");
Dictionary<string, VideoInfo> pathToVideoInfo = new Dictionary<string, VideoInfo>();
{
    string youtube_folder = "mario";
    string[] database_paths = Directory.GetFiles(reference_folder, "*.*", SearchOption.AllDirectories);
    foreach (string path in database_paths)
    {
        VideoInfo info = new VideoInfo(path, path.Contains(youtube_folder) ? Mode.YOUTUBE_DL : Mode.CONSOLE);
        // Console.WriteLine($"Parsing filename {info}");
        pathToVideoInfo[path] = info;
    }
}

Console.WriteLine($"--------------- Loading/Generating Database-----------------");
foreach (var fingerPrinter in fingerPrinterCascade)
{
    await fingerPrinter.LoadOrRegenerate();
}


string query_folder = "mod";
Console.WriteLine($"--------------- Begin querying [{query_folder}]-----------------");
string[] query_paths = Directory.GetFiles(query_folder, "*.*", SearchOption.AllDirectories); ;

StringBuilder sb = new StringBuilder();

sb.AppendLine("path, name1, source1, url1, name2, source2, url2, name3, source3, url3,");

int cnt = 0;
foreach (string path in query_paths)
{
    cnt++;
    Console.WriteLine($"Processing [{cnt}/{query_paths.Length}]: {path}");

    // Search through each database looking for a match
    VideoInfo? match = null;
    foreach (var fingerPrinter in fingerPrinterCascade)
    {
        var queryResult = await fingerPrinter.QueryPath(pathToVideoInfo, sb, path);
        // Console.WriteLine($"Best Match: {queryResult.BestMatch.TrackId}");
        if (queryResult.BestMatch != null)
        {
            //{entry.TrackCoverageWithPermittedGapsLength:0.00}/{entry.Track.Length},
            match = pathToVideoInfo[queryResult.BestMatch.TrackId];
            break;
        }
    }

    sb.Append($"\"{path}\",");
    if (match != null)
    {
        sb.Append($"\"{match.name}\",\"{match.source}\",\"{match.url}\",");
    }
    sb.AppendLine();

    if (cnt >= maxToProcess)
    {
        break;
    }
}

File.WriteAllText("query_result.csv", sb.ToString());



