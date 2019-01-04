namespace TrackManager
{
    /// <summary>
    /// This class is used to store track information locally. It will come in handy when implementing offline mode. (We currently require an internet connection to use the tool)
    /// </summary>
    public class LocalTrack
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Image { get; set; }
        public string Data { get; set; }
        public string Author { get; set; }
        public int Slot { get; set; }
        public long CreationTime { get; set; }
        public int TotalDownloads { get; set; }
        public int MyDownloads { get; set; }
        public bool Favorite { get; set; }
        public bool Installed { get; set; }
    }

}
