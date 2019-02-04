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
                reflex.InitializeTrackList();
                reflex.InstallRandomTracksOnFirstRun();

                if (args.Length == 0)
                {
                    var managementService = new TrackManagementService();
                    Server server = new Server
                    {
                        Services = { Trackmanagement.TrackManager.BindService(managementService) },
                        Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
                    };
                    server.Start();

                    Console.WriteLine("Track management server listening on port " + Port);

                    if (System.Diagnostics.Process.GetProcessesByName("MXReflex").Length == 0)
                    {
                        Console.WriteLine("Waiting for you to launch MX vs. ATV Reflex...");
                    }

                    while (true)
                    {
                        reflex.Process();
                    }
                }
                else
                {
                    if(args.Length == 1 && args[0] == "-downloadalltracks")
                    {
                        reflex.DownloadAllTracks();
                    }
                    else
                    {
                        Console.Error.WriteLine(string.Format("Invalid arguments provided to application ({0})", string.Join(",", args)));
                        Console.WriteLine("Usage:");
                        Console.WriteLine("\t-Normal execution mode (UI Overlay): TrackManager.exe");
                        Console.WriteLine("\t-Download all tracks mode: TrackManager.exe -downloadalltracks");
                    }
                }
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
