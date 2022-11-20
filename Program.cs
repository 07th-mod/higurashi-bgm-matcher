using CsvHelper;
using Info;
using Matcher.Override;
using MODJSONClasses;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;

void WriteResultsAsCSV(Dictionary<string, VideoInfo?> results, string outputPath)
{
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("path, name1, source1, url1,");

    foreach ((string path, VideoInfo? maybeInfo) in results)
    {
        sb.Append($"\"{path}\",");
        if (maybeInfo != null)
        {
            sb.Append($"\"{maybeInfo.name}\",\"{maybeInfo.source}\",\"{maybeInfo.url}\",");
        }
        sb.AppendLine();
    }

    File.WriteAllText(outputPath, sb.ToString());
}

void WriteResultsAsJSONFiles(Dictionary<string, VideoInfo?> results, string pathSuffix)
{
    Dictionary<string, BGMInfoDict> jsonToBeWritten = new Dictionary<string, BGMInfoDict>();

    // First, sort files based on their folder
    foreach ((string path, VideoInfo? maybeInfo) in results)
    {
        string[] pathParts = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });

        // Determine which JSON file this BGM should be put in
        string jsonPrefix = "unknown";

        if (pathParts.Length > 3)
        {
            jsonPrefix = $"{pathParts[1]}-{pathParts[2]}";
        }

        if (pathParts.Length < 4)
        {
            throw new Exception("Invalid path");
        }

        // Determine what the relative path of the BGM is
        string relativePath = "";
        for (int i = 3; i < pathParts.Length; i++)
        {
            relativePath = Path.Combine(relativePath, pathParts[i]);
        }

        // Console.WriteLine($"JsonPrefix: {jsonPrefix} RelativePath: {relativePath}");

        if (!jsonToBeWritten.TryGetValue(jsonPrefix, out BGMInfoDict _))
        {
            jsonToBeWritten[jsonPrefix] = new BGMInfoDict();
        }

        // Store the BGM in a way it can be written out later
        jsonToBeWritten[jsonPrefix].bgmList[relativePath] = maybeInfo == null ? new BGMInfo() : new BGMInfo(
            comment: "",
            name: maybeInfo.name,
            source: maybeInfo.source,
            url: maybeInfo.url
        );
    }

    // Write out all the saved BGM info
    foreach ((string jsonPath, BGMInfoDict maybeInfo) in jsonToBeWritten)
    {
        File.WriteAllText($"{jsonPath}-{pathSuffix}", JsonConvert.SerializeObject(maybeInfo, Formatting.Indented));
    }
}

string GetPathWithoutExtension(string path)
{
    string filenameNoExt = Path.GetFileNameWithoutExtension(path);
    string containingFolder = Path.GetDirectoryName(path);

    return Path.Combine(containingFolder, filenameNoExt);
}

Dictionary<string, Override> pathToOverrideDictionary = new Dictionary<string, Override>();

using (var reader = new StreamReader("overrides.csv"))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    var records = csv.GetRecords<Override>();

    foreach (var r in records)
    {
        pathToOverrideDictionary[r.path] = r;
    }
}

// This folder contains named music files which are used to build the database
string reference_folder = "reference";
bool force_rebuild = false;
int? maxToProcess = null;
bool skipmd5 = false;

// Mapping of MD5 of file to VideoInfo
Dictionary<string, VideoInfo> md5Database = new Dictionary<string, VideoInfo>();

EasyFingerPrinting[] fingerPrinterCascade = new EasyFingerPrinting[] {
    new EasyFingerPrinting(Path.Combine(reference_folder, "youtube_preferred"), "db_youtube_preferred", force_rebuild, maxToProcess),
    new EasyFingerPrinting(Path.Combine(reference_folder, "manual_matches"), "db_manual_matches", force_rebuild, maxToProcess),
    new EasyFingerPrinting(Path.Combine(reference_folder, "console_hou"), "db_console_hou", force_rebuild, maxToProcess),
    new EasyFingerPrinting(Path.Combine(reference_folder, "youtube_extra"), "db_youtube_extra", force_rebuild, maxToProcess),
};

Console.WriteLine($"--------------- Building Filename and MD5 List [{reference_folder}]-----------------");
Dictionary<string, VideoInfo> pathToVideoInfo = new Dictionary<string, VideoInfo>();
{
    int cnt2 = 0;
    string[] database_paths = Directory.GetFiles(reference_folder, "*.*", SearchOption.AllDirectories);
    foreach (string path in database_paths)
    {
        cnt2++;

        string no_top_level = Path.GetRelativePath(reference_folder, path);
        VideoInfo info = new VideoInfo(path, no_top_level.ToLower().StartsWith("youtube") ? Mode.YOUTUBE_DL : Mode.CONSOLE);
        pathToVideoInfo[path] = info;

        if (!skipmd5)
        {
            // Add MD5 to database
            string md5 = EasyMD5.GetMD5(path);
            md5Database[md5] = info;
            Console.WriteLine($"MD5-ing {cnt2}/{database_paths.Count()} {path}");
        }
    }
}

Console.WriteLine($"--------------- Loading/Generating Database-----------------");
foreach (var fingerPrinter in fingerPrinterCascade)
{
    Console.WriteLine($"Loading or regenerating database {fingerPrinter.GetDirectoryName()}");
    await fingerPrinter.LoadOrRegenerate();
}

string query_folder = "mod";
Console.WriteLine($"--------------- Begin querying [{query_folder}]-----------------");
string[] query_paths = Directory.GetFiles(query_folder, "*.*", SearchOption.AllDirectories); ;

Dictionary<string, VideoInfo?> results = new Dictionary<string, VideoInfo?>();

int cnt = 0;
foreach (string path in query_paths)
{
    cnt++;
    Console.WriteLine($"Processing [{cnt}/{query_paths.Length}]: {path}");

    // Search through each database looking for a match
    VideoInfo? match = null;

    // Check overrides first
    if (match == null)
    {
        string no_top_level = Path.GetRelativePath(query_folder, path);
        string no_ext = GetPathWithoutExtension(no_top_level);
        string fwd_slash = no_ext.Replace("\\", "/");

        if (pathToOverrideDictionary.TryGetValue(fwd_slash, out Override ovr))
        {
            match = ovr.ToVideoInfo();
            Console.WriteLine($"Got match {fwd_slash}: {ovr.name}");
        }
    }

    if (match == null)
    {
        string queryMD5 = EasyMD5.GetMD5(path);

        // Do an exact match using the MD5 of the file
        if (!skipmd5 && md5Database.TryGetValue(queryMD5, out VideoInfo? info) && info != null)
        {
            Console.WriteLine($"{path} EXACT match against {info}");
            match = info;
        }
    }

    if (match == null)
    {
        VideoInfo badMatch = null;

        // Do an approixmate match using the sound fingerprinting library
        // NOTE: The audio fingerprinting library used does not handle short audio files (about less than one second?)
        // Those short files will never be matched, so I've added MD5 matching as well.
        foreach (var fingerPrinter in fingerPrinterCascade)
        {
            var queryResult = await fingerPrinter.QueryPath(path);

            if (queryResult.BestMatch != null && queryResult.BestMatch.Audio != null)
            {
                var confidence = queryResult.BestMatch.Audio.Confidence;
                var possibleMatch = pathToVideoInfo[queryResult.BestMatch.TrackId];

                if (badMatch == null)
                {
                    badMatch = possibleMatch;
                }

                if (confidence > .10)
                {
                    Console.WriteLine($"{queryResult.BestMatch.TrackId} match quality {confidence} QueryRelativeCoverage: {queryResult.BestMatch.Audio.QueryRelativeCoverage} DiscreteTrackCoverageLength: {queryResult.BestMatch.Audio.DiscreteTrackCoverageLength}");

                    match = possibleMatch;
                    break;
                }
                else
                {
                    Console.WriteLine($"IGNORING BAD MATCH: {queryResult.BestMatch.TrackId} match quality {confidence} QueryRelativeCoverage: {queryResult.BestMatch.Audio.QueryRelativeCoverage} DiscreteTrackCoverageLength: {queryResult.BestMatch.Audio.DiscreteTrackCoverageLength}");
                }
            }
        }

        if (match == null && badMatch != null)
        {
            match = badMatch;
        }
    }

    // Do not include file extension in result
    results[GetPathWithoutExtension(path)] = match;

    if (match == null)
    {
        Console.WriteLine($"WARNING: no match for {path}");
    }

    if (cnt >= maxToProcess)
    {
        break;
    }
}

// Write out CSV file
WriteResultsAsCSV(results, "query_result.csv");

// Write out JSON files
WriteResultsAsJSONFiles(results, "result.json");



