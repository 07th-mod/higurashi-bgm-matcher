using CsvHelper;
using Info;
using Matcher.Override;
using MODJSONClasses;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;


Dictionary<string, VideoInfo> VideoInfoByFilename = new Dictionary<string, VideoInfo>()
{
    {"heri1", new VideoInfo("BSFX helicopter 1")},
    {"heri2", new VideoInfo("BSFX helicopter 2")},
    {"a1_03052", new VideoInfo("BSFX helicopter 1")},
    {"a1_03055", new VideoInfo("BSFX helicopter 2")},
    {"a1_12222", new VideoInfo("BSFX clock")},
    {"a3_01005", new VideoInfo("BSFX wind")},
    {"a3_02012", new VideoInfo("BSFX heavy rain")},
    {"a3_05045", new VideoInfo("BSFX stormy ocean waves")},
    {"a3_05051", new VideoInfo("BSFX ocean waves and seagulls")},
    {"a5_01033", new VideoInfo("BSFX white noise flowing water")},
    {"a5_14446", new VideoInfo("BSFX howling wind")},
    {"ame", new VideoInfo("BSFX rain")},
    {"ame2", new VideoInfo("BSFX rain")},
    {"ame_9", new VideoInfo("BSFX muted rain")},
    {"arasi_fr", new VideoInfo("BSFX phasor wind")},
    {"b7_17370", new VideoInfo("BSFX helicopter")},
    {"bell", new VideoInfo("BSFX antique bell phone")},
    {"bird", new VideoInfo("BSFX bird")},
    {"boira", new VideoInfo("BSFX boiler 1")},
    {"boiraa", new VideoInfo("BSFX boiler 2")},
    {"car_me1", new VideoInfo("BSFX car traffic 1")},
    {"car_me2", new VideoInfo("BSFX car traffic 2")},
    {"child_og", new VideoInfo("BSFX children background noises")},
    {"densha3", new VideoInfo("BSFX train 1")},
    {"densya", new VideoInfo("BSFX train 2")},
    {"denwa", new VideoInfo("BSFX digital phone")},
    {"denwa2", new VideoInfo("BSFX antique phone")},
    {"doukutu_suiryu", new VideoInfo("BSFX fast dripping water")},
    {"doukutu_suiteki", new VideoInfo("BSFX slow dripping water")},
    {"flangefemale", new VideoInfo("BSFX female ghost flange")},
    {"futousurunabe", new VideoInfo("BSFX bubbling sound")},
    {"gaya0", new VideoInfo("BSFX background voices 1")},
    {"gaya1", new VideoInfo("BSFX background voices 2")},
    {"hakushu", new VideoInfo("BSFX clapping")},
    {"higurashi", new VideoInfo("BSFX cicadas 1")},
    {"higurasi", new VideoInfo("BSFX cicadas 2")},
    {"husigitokei", new VideoInfo("BSFX heavy echoing clock ticking")},
    {"kawa", new VideoInfo("BSFX flowing river")},
    {"kaze", new VideoInfo("BSFX phasor wind")},
    {"kaze_b", new VideoInfo("BSFX phasor wind 2")},
    {"kaze_fr1", new VideoInfo("BSFX loud phasor wind")},
    {"koware_wind", new VideoInfo("BSFX phasor wind 3")},
    {"kuroden_bell2", new VideoInfo("BSFX bell phone ring")},
    {"kuroden_bell2_far", new VideoInfo("BSFX bell phoen ring far away")},
    {"lgsk_warai", new VideoInfo("BSFX scary echo laughing")},
    {"lg_musi_yoru", new VideoInfo("BSFX night time pond noises")},
    {"mati", new VideoInfo("BSFX driving noises 1")},
    {"mati_kotu", new VideoInfo("BSFX driving noises 2")},
    {"mati_kotu2", new VideoInfo("BSFX driving noises 3")},
    {"me_101", new VideoInfo("BSFX outside background noise 1")},
    {"me_102", new VideoInfo("BSFX outside background noise 2")},
    {"me_103", new VideoInfo("BSFX outside background noise 3")},
    {"me_104", new VideoInfo("BSFX outside background noise 4")},
    {"me_105", new VideoInfo("BSFX outside background noise 5")},
    {"me_106", new VideoInfo("BSFX outside background noise 6")},
    {"me_107", new VideoInfo("BSFX outside background noise 7")},
    {"rain4_long", new VideoInfo("BSFX rain 4")},
    {"semi", new VideoInfo("BSFX cicadas")},
    {"semi_r", new VideoInfo("BSFX phasor cicadas")},
    {"storm2", new VideoInfo("BSFX storm")},
    {"suiteki", new VideoInfo("BSFX dripping water")},
    {"suzu", new VideoInfo("BSFX sleigh bells")},
    {"suzume", new VideoInfo("BSFX soft birds chirping")},
    {"taip", new VideoInfo("BSFX satisfying typing noises")},
    {"tokeil4", new VideoInfo("BSFX clock ticking")},
    {"tokei_f3", new VideoInfo("BSFX echoing clock ticking 1")},
    {"tokei_fr3", new VideoInfo("BSFX echoing clock ticking 2")},
    {"tokei_loop_l4", new VideoInfo("BSFX clock ticking loop")},
    {"town2", new VideoInfo("BSFX town road driving noises")},
    {"yoru", new VideoInfo("BSFX night time insect chirping")},
};

bool getVideoInfoByName(string name, out VideoInfo? videoInfo)
{
    return VideoInfoByFilename.TryGetValue(name.Trim().ToLower(), out videoInfo);
}

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

// Use this in case you convert the files in the `references/manual_matches` folder in a way they are no longer 100% identical to files in the `mod` folder
// Otherwise you shouldn't need to use this
void regenerate()
{
    string[] manual = Directory.GetFiles("reference\\manual_matches", "*.*", SearchOption.AllDirectories);

    string mod_folder = "mod";

    foreach (var pathx in manual)
    {
        var path = pathx.Replace("reference\\manual_matches\\", "");
        var folder = Path.GetDirectoryName(path);
        var filename = Path.GetFileName(path);
        var filename_original = filename.Split(" ", 2)[0];
        var path_original = Path.Combine(mod_folder, Path.Combine(folder, filename_original + ".opus"));
        Console.WriteLine($"folder: {folder} filename: {filename_original} path: {path_original}");

        if (!File.Exists(path_original))
        {
            Console.WriteLine($"ERROR: {path_original} doesn't exist!");
        }

        var output_path = Path.Combine("c:\\temp\\output_higurashi", path);
        Directory.CreateDirectory(Path.GetDirectoryName(output_path));
        File.Copy(path_original, output_path);
    }
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

    if (match == null)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        if (getVideoInfoByName(name, out VideoInfo? info))
        {
            match = info;
        }
    }

    // Do not include file extension in result
    string key = GetPathWithoutExtension(path);
    if (results.TryGetValue(key, out VideoInfo value))
    {
        throw new Exception($"Error (developer error?): same bgm [{key}] added twice {value}");
    }

    results[key] = match;

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



