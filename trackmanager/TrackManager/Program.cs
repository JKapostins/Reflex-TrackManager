using System;
using System.Threading.Tasks;
using Grpc.Core;
using System.Linq;
using Google.Protobuf.Collections;

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
            return Task.FromResult(new Trackmanagement.TrackResponse
            {
                Tracks = { m_tracks.Select(t => new Trackmanagement.Track
                {
                    Name = t.TrackName
                }).ToArray() }
            });
        }

        private Track[] m_tracks;
    }

    class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            var tracks = HttpUtility.Get<Track[]>("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/tracks?validation=valid");

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
