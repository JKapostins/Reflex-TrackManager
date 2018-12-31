using Grpc.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TrackManagement
{
    class TrackManagerImpl : Trackmanagement.TrackManager.TrackManagerBase
    {
        public TrackManagerImpl(Track[] tracks)
        {
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
                    Image = string.Format("{0}\\Images\\{1}", currentDirectory, Path.GetFileName(t.ThumbnailUrl).Replace("+", " ")),
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

    class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            var tracks = HttpUtility.Get<Track[]>("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/tracks?validation=valid");
            var dir = Environment.CurrentDirectory;
            Server server = new Server
            {
                Services = { Trackmanagement.TrackManager.BindService(new TrackManagerImpl(tracks)) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Track management server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
