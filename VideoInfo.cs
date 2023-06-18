using System.Text.RegularExpressions;

namespace Info
{

    public enum Mode
    {
        YOUTUBE_DL,
        CONSOLE
    }

    public class VideoInfo
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

                        // If match fails, try re-matching without URL
                        if (!match.Success || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(url))
                        {
                            Regex re2 = new Regex(@"(.*)\s*-\s*(.*)\s*\..*");
                            Match match2 = re2.Match(filenameFixed);
                            name = match2.Groups[1].Value.Trim();
                            source = match2.Groups[2].Value.Trim();
                            url = "";
                        }
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
                        // Wihtout URL: "semi_r BSFX Cicadas with Phasor (GIN).ogg"
                        // With URL:    "msys23 Dancers7 (GIN) [0Wb-tFSRCnU].ogg"
                        if (match.Groups.Count >= 5)
                        {
                            url = match.Groups[4].Value;
                        }
                    }
                    break;

                default:
                    throw new Exception("Unknown mode for parsing metainfo from filename");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new Exception($"Couldn't parse bgm/se name from {filePathOrName}, mode: {mode}\nPlease check filename format is correct, and be careful of special characters like `()-[]`");
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