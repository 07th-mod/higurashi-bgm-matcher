using Info;
using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;
using System.Text;
using System.Text.RegularExpressions;

// string snapshotFolder = "snapshot";

EasyFingerPrinting fp = new EasyFingerPrinting("snapshot");

// TODO: split into different databases, so can prefer the Youtube database first?

string database_folder = "reference";
string query_folder = "mod";
string youtube_folder = "mario";

string[] database_paths = Directory.GetFiles(database_folder, "*.*", SearchOption.AllDirectories);
string[] query_paths = Directory.GetFiles(query_folder, "*.*", SearchOption.AllDirectories);

bool rebuild = false;

Console.WriteLine($"--------------- Loading/Generating Database [{database_folder}]-----------------");

Dictionary<string, VideoInfo> pathToVideoInfo = new Dictionary<string, VideoInfo>();

foreach (string path in database_paths)
{
    VideoInfo info = new VideoInfo(path, path.Contains(youtube_folder) ? Mode.YOUTUBE_DL : Mode.CONSOLE);
    // Console.WriteLine($"Parsing filename {info}");
    pathToVideoInfo[path] = info;
}

await fp.LoadOrRegenerate(database_paths, pathToVideoInfo, rebuild: false);

Console.WriteLine($"--------------- Begin querying [{query_folder}]-----------------");

StringBuilder sb = new StringBuilder();

sb.AppendLine("path, name1, source1, url1, name2, source2, url2, name3, source3, url3,");

int cnt = 1;
foreach (string path in query_paths)
{
    // if(!path.Contains("hm01_01"))
    // {
    //     continue;
    // }

    Console.WriteLine($"Processing [{cnt}/{query_paths.Length}]: {path}");
    await fp.QueryPath(pathToVideoInfo, sb, path);
    cnt++;
}

File.WriteAllText("query_result.csv", sb.ToString());



