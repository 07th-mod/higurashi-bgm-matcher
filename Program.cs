using Info;
using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;
using System.Text;
using System.Text.RegularExpressions;


var modelService = new InMemoryModelService(); // store fingerprints in RAM
IAudioService audioService = new SoundFingerprintingAudioService(); // default audio library


async Task StoreForLaterRetrieval(string path)
{
    // For now, use the path as the ID
    string id = path;
    string title = "title";
    string artist = "Unknown";

    var track = new TrackInfo(id, title, artist);

    // create fingerprints
    var avHashes = await FingerprintCommandBuilder.Instance
                                .BuildFingerprintCommand()
                                .From(path)
                                .UsingServices(audioService)
                                .Hash();
								
    // store hashes in the database for later retrieval
    modelService.Insert(track, avHashes);
}

async Task QueryPath(Dictionary<string, VideoInfo> pathToVideoInfo, StringBuilder sb, string path)
{
    int secondsToAnalyse = 10;
    int startAtSecond = 0;

    var queryResult = await QueryCommandBuilder.Instance
                                    .BuildQueryCommand()
                                    .From(path)
                                    // .From(path, secondsToAnalyse, startAtSecond)
                                    .UsingServices(modelService)
                                    .Query();


    // Console.WriteLine($"Best Match: {queryResult.BestMatch.TrackId}");

    sb.Append($"\"{path}\",");

    if(queryResult.BestMatch != null)
    {
        VideoInfo info = pathToVideoInfo[queryResult.BestMatch.TrackId];
        //{entry.TrackCoverageWithPermittedGapsLength:0.00}/{entry.Track.Length},
        sb.Append($"\"{info.name}\",\"{info.source}\",\"{info.url}\",");
    }


    // // iterate over the results if any  
    // foreach(var (entry, _) in queryResult.ResultEntries)
    // {
    //     // output only those tracks that matched at least seconds.
    //     // if(path != entry.Track.Id && entry.TrackCoverageWithPermittedGapsLength >= 5d)
    //     // {
    //         VideoInfo info = pathToVideoInfo[entry.Track.Id];
    //         //{entry.TrackCoverageWithPermittedGapsLength:0.00}/{entry.Track.Length},
    //         sb.Append($"\"{info.name}\",\"{info.source}\",\"{info.url}\",");
    //     // }
    // }

    sb.AppendLine();
}


// TODO: split into different databases, so can prefer the Youtube database first?

string database_folder = "reference";
string query_folder = "mod";
string youtube_folder = "mario";

string[] database_paths = Directory.GetFiles(database_folder, "*.*", SearchOption.AllDirectories);
string[] query_paths = Directory.GetFiles(query_folder, "*.*", SearchOption.AllDirectories);

bool rebuild = false;
string snapshotFolder = "snapshot";

Console.WriteLine($"--------------- Loading/Generating Database [{database_folder}]-----------------");

// StringBuilder sb = new StringBuilder();

Dictionary<string, VideoInfo> pathToVideoInfo = new Dictionary<string, VideoInfo>();

foreach(string path in database_paths)
{
    VideoInfo info = new VideoInfo(path, path.Contains(youtube_folder) ? Mode.YOUTUBE_DL : Mode.CONSOLE);
    // Console.WriteLine($"Parsing filename {info}");
    pathToVideoInfo[path] = info;
}

if(rebuild || !Directory.Exists(snapshotFolder))
{
    foreach(string path in database_paths)
    {
        // VideoInfo info = new VideoInfo(path);
        // Console.WriteLine(path);
        // sb.AppendLine(info.ToString());
        Console.WriteLine(pathToVideoInfo[path]);
        await StoreForLaterRetrieval(path);
    }

// File.WriteAllText("testunicode.txt", sb.ToString());

    modelService.Snapshot("snapshot");
}
else
{
    modelService = new InMemoryModelService(snapshotFolder);
}

Console.WriteLine($"--------------- Begin querying [{query_folder}]-----------------");

StringBuilder sb = new StringBuilder();

sb.AppendLine("path, name1, source1, url1, name2, source2, url2, name3, source3, url3,");

int cnt = 1;
foreach(string path in query_paths)
{
    // if(!path.Contains("hm01_01"))
    // {
    //     continue;
    // }

    Console.WriteLine($"Processing [{cnt}/{query_paths.Length}]: {path}");
    await QueryPath(pathToVideoInfo, sb, path);
    cnt++;
}

File.WriteAllText("query_result.csv", sb.ToString());



