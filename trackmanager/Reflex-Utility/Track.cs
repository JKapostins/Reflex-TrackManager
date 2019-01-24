using System;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace ReflexUtility
{
    public class TrackType
    {
        public const string National = "National";
        public const string Supercross = "Supercross";
        public const string FreeRide = "FreeRide";
        public const string Unknown = "Unknown";
    }

    [DynamoDBTable("ReflexTracks")]
    public class Track
    {
        public Track()
        {
            TrackType = ReflexUtility.TrackType.Unknown;
            TrackName = string.Empty;
            TrackUrl = string.Empty;
            ThumbnailUrl = string.Empty;
            SourceTrackUrl = string.Empty;
            SourceThumbnailUrl = string.Empty;
            Author = string.Empty;
            SlotNumber = 0;
            ErrorInfo = string.Empty;
            Valid = true;
            Rating = 0.0f;
            RatingVoteCount = 0;
        }

        public string TrackType { get; set; }
        [DynamoDBHashKey]
        public string TrackName { get; set; }
        public string TrackUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string SourceTrackUrl { get; set; }
        public string SourceThumbnailUrl { get; set; }
        public string Author { get; set; }
        public string ErrorInfo { get; set; }
        public long CreationTime { get; set; }
        public float Rating { get; set; }
        public int RatingVoteCount { get; set; }
        public int SlotNumber { get; set; }
        public bool Valid { get; set; }

        //DynamoDB doesn't allow null or empty string values... 
        public void FixEmptyStrings()
        {
            TrackType = TrackType == string.Empty ? "null" : TrackType;
            TrackName = TrackName == string.Empty ? "null" : TrackName;
            TrackUrl = TrackUrl == string.Empty ? "null" : TrackUrl;
            ThumbnailUrl = ThumbnailUrl == string.Empty ? "null" : ThumbnailUrl;
            SourceTrackUrl = SourceTrackUrl == string.Empty ? "null" : SourceTrackUrl;
            SourceThumbnailUrl = SourceThumbnailUrl == string.Empty ? "null" : SourceThumbnailUrl;
            Author = Author == string.Empty ? "null" : Author;
            ErrorInfo = ErrorInfo == string.Empty ? "null" : ErrorInfo;
        }
    }
}
