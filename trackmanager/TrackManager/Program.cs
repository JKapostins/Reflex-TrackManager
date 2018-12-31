using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace Trackmanagement
{
    class TrackManagerImpl : TrackManager.TrackManagerBase
    {
        // Server side handler of the SayHello RPC
        public override Task<TrackResponse> GetTracks(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new TrackResponse {
                Tracks =
                {
                      new Track { Name = "Track 1" }
                    , new Track { Name = "Track 2" }
                }
            });
        }
    }

    class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = { TrackManager.BindService(new TrackManagerImpl()) },
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
