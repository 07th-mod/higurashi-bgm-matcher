namespace MODJSONClasses
{
    public class BGMInfoDict
    {
        public Dictionary<string, BGMInfo> bgmList;

        public BGMInfoDict()
        {
            bgmList = new Dictionary<string, BGMInfo>();
        }
    }

    public class BGMInfo
    {
        public string comment;
        public string name;
        public string source;
        public string url;

        public BGMInfo()
        {
            comment = "Unknown";
            name = "Unknown BGM";
            source = "Unknown";
            url = "Unknown";
        }

        public BGMInfo(string comment, string name, string source, string url)
        {
            this.comment = comment;
            this.name = name;
            this.source = source;
            this.url = url;
        }
    }
}