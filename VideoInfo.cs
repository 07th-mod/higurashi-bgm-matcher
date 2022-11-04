using System.Text.RegularExpressions;

namespace Info
{

    enum Mode
    {
        YOUTUBE_DL,
        CONSOLE
    }

    class VideoInfo
    {
        public string name;
        public string source;
        public string url;

        public VideoInfo(string filePathOrName, Mode mode)
        {
            string filenameFixed = Path.GetFileName(filePathOrName).Replace("07th-Mod", "(07th_mod)");

            switch (mode)
            {
                case Mode.YOUTUBE_DL:
                    {
                        Regex re = new Regex(@"(.*)\s*-\s*(.*)\s*\[(.*)\].*");
                        Match match = re.Match(filenameFixed);
                        name = match.Groups[1].Value.Trim();
                        source = match.Groups[2].Value.Trim();
                        url = match.Groups[3].Value.Trim();
                    }
                    break;

                case Mode.CONSOLE:
                    {
                        Regex re = new Regex(@"([^ ]*)\s+(.*?)(\s*\[(.{11})\])?\..*");
                        Match match = re.Match(filenameFixed);
                        string filename = match.Groups[1].Value.Trim();
                        name = $"{match.Groups[2].Value}".Trim();
                        source = "".Trim();
                        url = "".Trim();

                        // URL is optional
                        // Wihtout URL: "semi_r BSFX Cicadas with Phasor (GIN).wav"
                        // With URL:    "msys23 Dancers7 (GIN) [0Wb-tFSRCnU].wav"
                        if (match.Groups.Count >= 5)
                        {
                            url = match.Groups[4].Value;
                        }
                    }
                    break;

                default:
                    throw new Exception("Unknown mode for parsing metainfo from filename");
            }
        }

        public VideoInfo(string name, string source, string url)
        {
            this.name = name;
            this.source = source;
            this.url = url;
        }

        public override string ToString()
        {
            return $"url: {url,-11} name: {name,-60} source: {source}";
        }
    }

}