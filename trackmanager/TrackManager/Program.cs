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
                reflex.ValidateInstallation();
                reflex.DownloadImages();
                var managementService = new TrackManagementService(Reflex.Tracks);
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
                ExceptionLogger.LogException(e);
            }

            Console.WriteLine("Press any key to close this window.");
            Console.ReadKey();
        }

        
    }
}
