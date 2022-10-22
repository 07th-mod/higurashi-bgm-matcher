using System.Text;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;
using SoundFingerprinting.Query;

namespace Info
{
    class EasyFingerPrinting
    {
        InMemoryModelService modelService;
        IAudioService audioService;
        string directoryToSearch;
        string deserPath;
        bool rebuild;
        int? maxToProcess;

        public EasyFingerPrinting(string directoryToSearch, string deserPath, bool rebuild = false, int? maxToProcess = null)
        {
            this.rebuild = rebuild;
            this.directoryToSearch = directoryToSearch;
            this.deserPath = deserPath;
            this.modelService = new InMemoryModelService(); // store fingerprints in RAM
            this.audioService = new SoundFingerprintingAudioService(); // default audio library
            this.maxToProcess = maxToProcess;
        }

        public async Task LoadOrRegenerate()
        {
            if (rebuild || !Directory.Exists(deserPath))
            {
                string[] database_paths = Directory.GetFiles(directoryToSearch, "*.*", SearchOption.AllDirectories);

                int i = 0;
                foreach (string path in database_paths)
                {
                    i++;
                    // VideoInfo info = new VideoInfo(path);
                    Console.WriteLine($"Processing {i}/{database_paths.Count()}: {path}");
                    // sb.AppendLine(info.ToString());
                    // Console.WriteLine(pathToVideoInfo[path]);
                    await StoreForLaterRetrieval(path);

                    if (maxToProcess != null && i >= maxToProcess)
                    {
                        break;
                    }
                }

                // File.WriteAllText("testunicode.txt", sb.ToString());

                modelService.Snapshot(deserPath);
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

        public async Task<AVQueryResult> QueryPath(Dictionary<string, VideoInfo> pathToVideoInfo, StringBuilder sb, string path)
        {
            return await QueryCommandBuilder.Instance
                                            .BuildQueryCommand()
                                            .From(path)
                                            // .From(path, secondsToAnalyse, startAtSecond)
                                            .UsingServices(modelService)
                                            .Query();
        }
    }

}