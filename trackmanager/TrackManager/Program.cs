using Grpc.Core;
using System;
using TrackManagement;

namespace TrackManager
{
    class Program
    {
        const int Port = 50051;
        const string Version = "0.1";
        public static void Main(string[] args)
        {
            try
            {
                ApplicationUpdater updater = new ApplicationUpdater();
                if (updater.IsActiveVersion() == false)
                {
                    updater.DownloadAndLaunchUpdater();
                    return;
                }
                Sharing.Initialize();
                Reflex reflex = new Reflex();
                reflex.ValidateInstallation();
                reflex.DownloadImages();
                LocalSettings.Load();

                var managementService = new TrackManagementService();
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
                    reflex.Process();
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
