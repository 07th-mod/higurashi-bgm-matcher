using Info;
using System.Text;

// This folder contains named music files which are used to build the database
string reference_folder = "reference";
bool force_rebuild = false;
int? maxToProcess = null;

// Mapping of MD5 of file to VideoInfo
Dictionary<string, VideoInfo> md5Database = new Dictionary<string, VideoInfo>();

EasyFingerPrinting[] fingerPrinterCascade = new EasyFingerPrinting[] {
    new EasyFingerPrinting(Path.Combine(reference_folder, "mario"), "database_mario", force_rebuild, maxToProcess),
    new EasyFingerPrinting(Path.Combine(reference_folder, "manual_matches"), "database_manual_matches", force_rebuild, maxToProcess),
    new EasyFingerPrinting(Path.Combine(reference_folder, "console_hou"), "database_console_hou", force_rebuild, maxToProcess),
};

Console.WriteLine($"--------------- Building Filename and MD5 List [{reference_folder}]-----------------");
Dictionary<string, VideoInfo> pathToVideoInfo = new Dictionary<string, VideoInfo>();
{
    int cnt2 = 0;
    string youtube_folder = "mario";
    string[] database_paths = Directory.GetFiles(reference_folder, "*.*", SearchOption.AllDirectories);
    foreach (string path in database_paths)
    {
        cnt2++;
        VideoInfo info = new VideoInfo(path, path.Contains(youtube_folder) ? Mode.YOUTUBE_DL : Mode.CONSOLE);
        pathToVideoInfo[path] = info;

        // Add MD5 to database
        string md5 = EasyMD5.GetMD5(path);
        md5Database[md5] = info;

        Console.WriteLine($"MD5-ing {cnt2}/{database_paths.Count()} {path}");
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

sb.AppendLine("path, name1, source1, url1,");

int cnt = 0;
foreach (string path in query_paths)
{
    cnt++;
    Console.WriteLine($"Processing [{cnt}/{query_paths.Length}]: {path}");

    // Search through each database looking for a match
    VideoInfo? match = null;

    string queryMD5 = EasyMD5.GetMD5(path);

    // Do an approixmate match using the sound fingerprinting library
    // NOTE: The audio fingerprinting library used does not handle short audio files (about less than one second?)
    // Those short files will never be matched, so I've added MD5 matching as well.
    foreach (var fingerPrinter in fingerPrinterCascade)
    {
        var queryResult = await fingerPrinter.QueryPath(pathToVideoInfo, sb, path);
        if (queryResult.BestMatch != null)
        {
            match = pathToVideoInfo[queryResult.BestMatch.TrackId];
            break;
        }
    }


    if (match == null)
    {
        // Do an exact match using the MD5 of the file
        if (md5Database.TryGetValue(queryMD5, out VideoInfo? info) && info != null)
        {
            match = info;
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



