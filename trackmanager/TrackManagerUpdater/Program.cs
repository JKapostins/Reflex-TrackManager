using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace TrackManagerUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                {
                    Console.Error.WriteLine("Unexpected argument count. Usage: TrackManagerUpdater <version>");
                    Console.WriteLine("Press any key to exit.");
                    Console.Read();
                    return;
                }

                Console.WriteLine("Waiting for TrackManager to exit before installing update.");
                string trackManagerManager = "TrackManager";
                Process[] processName = Process.GetProcessesByName(trackManagerManager);
                while (processName.Length > 0)
                {
                    processName = Process.GetProcessesByName(trackManagerManager);
                    System.Threading.Thread.Sleep(1000);
                }


                string reflex = "MXReflex";
                Process[] reflexProcess = Process.GetProcessesByName(reflex);
                if (reflexProcess.Length > 0)
                {
                    Console.WriteLine("Please exit MX vs. ATV Reflex so the track manager can update.");
                }

                while (reflexProcess.Length > 0)
                {
                    reflexProcess = Process.GetProcessesByName(reflex);
                    System.Threading.Thread.Sleep(1000);
                }


                var version = args[0];
                string trackManagerZip = "TrackManager.zip";
                using (var client = new WebClient())
                {
                    string zipFile = string.Format("https://s3.amazonaws.com/reflextrackmanager/{0}/{1}", version, trackManagerZip);
                    Console.WriteLine("Updating track manager to version " + version);
                    using (Stream memoryStream = new MemoryStream(client.DownloadData(zipFile)))
                    {
                        using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                Console.WriteLine(string.Format("Installing {0}", entry.FullName));
                                entry.ExtractToFile(entry.FullName, true);
                            }
                        }
                    }
                }

                //Update the version that appears in add / remove programs
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{2783D510-E6BF-4618-A40A-E7BCE45DFCCC}_is1", true);
                if (key != null)
                {
                    object displayVersionObject = key.GetValue("DisplayVersion");

                    if (displayVersionObject != null)
                    {
                        var currentVersion = displayVersionObject.ToString();
                        if (currentVersion != version)
                        {
                            key.SetValue("DisplayVersion", version, RegistryValueKind.String);
                        }
                    }
                }

                //start up the updated process
                ProcessStartInfo info = new ProcessStartInfo();
                info.UseShellExecute = true;
                info.CreateNoWindow = false;
                info.WindowStyle = ProcessWindowStyle.Normal;
                info.FileName = "TrackManager.exe";
                Process.Start(info);

                Console.WriteLine("Track manager updated!");
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}
