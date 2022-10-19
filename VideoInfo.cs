using System.Text.RegularExpressions;

namespace Info {

    enum Mode {
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

            switch (mode) {
                case Mode.YOUTUBE_DL:
                {
                    Regex re = new Regex(@"(.*)\s*-\s*(.*)\s*\[(.*)\].*");
                    Match match = re.Match(filenameFixed);
                    name = match.Groups[1].Value;
                    source = match.Groups[2].Value;
                    url = match.Groups[3].Value;
                }
                break;

                case Mode.CONSOLE:
                {
                    Regex re = new Regex(@"([^ ]*)\s+(.*)\..*");
                    Match match = re.Match(filenameFixed);
                    string filename = match.Groups[1].Value;
                    name = $"{match.Groups[2].Value}";
                    source = "";
                    url = "";
                }
                break;

                default:
                    throw new Exception("Unknown mode for parsing metainfo from filename");
            }

        }

        public override string ToString()
        {
            return $"url: {url,-11} name: {name,-60} source: {source}";
        }
    }

}