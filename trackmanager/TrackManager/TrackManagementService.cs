using Grpc.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TrackManagement;

namespace TrackManager
{
    public class TrackManagementService : Trackmanagement.TrackManager.TrackManagerBase
    {

        public bool OverlayClientConnected { get; set; }

        public TrackManagementService(Track[] tracks)
        {
            OverlayClientConnected = false;
            m_tracks = tracks;
        }

        // Server side handler of the SayHello RPC
        public override Task<Trackmanagement.TrackResponse> GetTracks(Trackmanagement.Empty request, ServerCallContext context)
        {
            string currentDirectory = Environment.CurrentDirectory;
            return Task.FromResult(new Trackmanagement.TrackResponse
            {
                Tracks = { m_tracks.Select(t => new Trackmanagement.Track
                {
                    Name = t.TrackName,
                    Type = t.TrackType,
                    Image = string.Format("{0}\\{1}{2}", Reflex.LocalImagePath, t.TrackName, Path.GetExtension(t.ThumbnailUrl)),
                    Author = t.Author,
                    Slot = t.SlotNumber,
                    Date = UnixTimeStampToString(t.CreationTime),
                    Downloads = 0,
                    Favorite = false,

                }).ToArray() }
            });
        }

        public static string UnixTimeStampToString(long unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime.ToString("yyyy-MM-dd");
        }

        private Track[] m_tracks;
    }
}
