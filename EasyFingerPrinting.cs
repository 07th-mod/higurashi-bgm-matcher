using System.Text;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.Emy;
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
            bool useFFMPEG = true;
            if (useFFMPEG)
            {
                this.audioService = new FFmpegAudioService();
            }
            else
            {
                this.audioService = new SoundFingerprintingAudioService(); // default audio library
            }
            this.maxToProcess = maxToProcess;
        }
        public string GetDirectoryName() => directoryToSearch;
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
            AVHashes avHashes = null;
            try
            {
                avHashes = await FingerprintCommandBuilder.Instance
                                            .BuildFingerprintCommand()
                                            .From(path)
                                            .UsingServices(audioService)
                                            .Hash();
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine($"ERROR: probably missing ffmpeg binary. On Windows, please put the binaries at FFmpeg\\bin\\x64 (including dll files) from https://github.com/GyanD/codexffmpeg/releases/download/4.4.1/ffmpeg-4.4.1-full_build-shared.7z. On Unix-like OS, ensure ffmpeg is installed (probably need version 4.x.x): {e}");
                throw e;
            }

            // There is a bug where snapshot won't load if a file with no hashes is snapshotted.
            // Since such files aren't useful anyway, skip them.
            if (!avHashes.IsEmpty)
            {
                Console.WriteLine($"WARNING: Skipped file {path} because it generated no audio hashes (too short?)");

                // store hashes in the database for later retrieval
                modelService.Insert(track, avHashes);
            }

        }

        public async Task<AVQueryResult> QueryPath(string path)
        {
            // For some reason, if this is not specified, can produce poor quality matches?
            float secondsToAnalyse = 30;
            float startAtSecond = 0;
            return await QueryCommandBuilder.Instance
                                            .BuildQueryCommand()
                                            //.From(path)
                                            .From(path, secondsToAnalyse, startAtSecond)
                                            .UsingServices(modelService, audioService)
                                            .Query();
        }
    }

}