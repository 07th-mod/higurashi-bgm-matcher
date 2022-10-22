using System.Text;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;

namespace Info
{

    class EasyFingerPrinting
    {
        InMemoryModelService modelService;
        IAudioService audioService;
        string deserPath;

        public EasyFingerPrinting(string deserPath)
        {
            this.deserPath = deserPath;
            this.modelService = new InMemoryModelService(); // store fingerprints in RAM
            this.audioService = new SoundFingerprintingAudioService(); // default audio library
        }

        public async Task LoadOrRegenerate(string[] database_paths, Dictionary<string, VideoInfo> pathToVideoInfo, bool rebuild)
        {
            if (rebuild || !Directory.Exists(deserPath))
            {
                foreach (string path in database_paths)
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
                modelService = new InMemoryModelService(deserPath);
            }
        }

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

        public async Task QueryPath(Dictionary<string, VideoInfo> pathToVideoInfo, StringBuilder sb, string path)
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

            if (queryResult.BestMatch != null)
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

        void Serialize()
        {
            modelService.Snapshot(deserPath);
        }

    }

}