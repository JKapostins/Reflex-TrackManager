using Grpc.Core;
using System;
using TrackManagement;

namespace TrackManager
{
    class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            try
            {
                Reflex reflex = new Reflex();
                Console.WriteLine("Detected Mx vs Atv Reflex install path: " + Reflex.InstallLocation);
                var tracks = HttpUtility.Get<Track[]>("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/tracks?validation=valid");

                var managementService = new TrackManagementService(tracks);
                Server server = new Server
                {
                    Services = { Trackmanagement.TrackManager.BindService(managementService) },
                    Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
                };
                server.Start();

                Console.WriteLine("Track management server listening on port " + Port);

                //GNARLY_TODO: Add exit logic.
                while (true)
                {
                }

                server.ShutdownAsync().Wait();
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }
    }
}
