using Amazon.DynamoDBv2.DataModel;

namespace ReflexUtility
{
    [DynamoDBTable("SharedReflexTrackLists")]
    public class TrackList
    {
        public string Name { get; set; }
        public long CreationTime { get; set; }
        public string Tracks { get; set; }
        public string Type { get; set; }
    }
}
