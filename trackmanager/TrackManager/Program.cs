using Grpc.Core;
using System;
using ReflexUtility;

namespace TrackManager
{
    class Program
    {
        const int Port = 50051;
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Checking for updates...");
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
                reflex.InstallRandomTracksOnFirstRun();

                var managementService = new TrackManagementService();
                Server server = new Server
                {
                    Services = { Trackmanagement.TrackManager.BindService(managementService) },
                    Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
                };
                server.Start();

                Console.WriteLine("Track management server listening on port " + Port);

                if(System.Diagnostics.Process.GetProcessesByName("MXReflex").Length == 0)
                {
                    Console.WriteLine("Waiting for you to launch MX vs. ATV Reflex...");
                }

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
