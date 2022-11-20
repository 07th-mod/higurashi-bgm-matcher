using Info;

namespace Matcher.Override
{
    public class Override
    {
        public string path { get; set; }
        public string comment { get; set; } = "";
        public string name { get; set; }
        public string source { get; set; } = "";
        public string url { get; set; } = "";

        public VideoInfo ToVideoInfo()
        {

            return new VideoInfo(name, source, url);
        }
    }
}