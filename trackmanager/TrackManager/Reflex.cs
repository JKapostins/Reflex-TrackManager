using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using TrackManagement;
using TrackManager.Steam;
using System.Linq;

namespace TrackManager
{
    public class Reflex
    {
        public Reflex()
        {
            Tracks = HttpUtility.Get<Track[]>("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/tracks?validation=valid");
            m_uiFiles = new string[]
            {
                  "MXTables_DLC008.dx9.database"
                , "MXTables_DLC008.dx9.package"
                , "MXTables_DLC008a.dx9.database"
                , "MXTables_DLC008a.dx9.package"
                , "MXUI_DLC008.dx9.database"
                , "MXUI_DLC008.dx9.package"
            };
        }

        public void ValidateInstallation()
        {
            InstallPath = GetInstallPath();
            DatabasePath = InstallPath + @"\Database";
            LocalImagePath = Environment.CurrentDirectory + @"\Images";
            LocalTrackPath = Environment.CurrentDirectory + @"\Tracks";

            if(Directory.Exists(LocalImagePath) == false)
            {
                Directory.CreateDirectory(LocalImagePath);
            }

            if (Directory.Exists(LocalTrackPath) == false)
            {
                Directory.CreateDirectory(LocalTrackPath);
            }

            Console.WriteLine("Detected Mx vs Atv Reflex install path: " + InstallPath);

            if (BetaSlotsInstalled() == false)
            {
                Console.WriteLine("Installing beta slots...");
                InstallBetaSlots();
                Console.WriteLine("Beta slots install complete.");
            }
            else
            {
                Console.WriteLine("Beta slots already installed.");
            }
        }

        public void DownloadImages()
        {
            var serverTracks = Tracks.Select(t => t.TrackName.Trim()).ToArray();
            var localImages = GetImageFilesOnDisk();
            var newImages = serverTracks.Except(localImages).ToArray();

            if(newImages.Length > 0)
            {
                Console.WriteLine(string.Format("Preparing to download {0} new preview images...", newImages.Length));
            }

            foreach(var image in newImages)
            {
                Console.WriteLine(string.Format("Downloading preview image \"{0}\"", image));
                TrackInstaller.DownloadImage(image);
            }

            if (newImages.Length > 0)
            {
                Console.WriteLine("Preview image downloads complete.");
            }
        }

        public string[] GetImageFilesOnDisk()
        {
            var files = Directory.GetFiles(LocalImagePath);
            return files.Select(t => Path.GetFileNameWithoutExtension(t.Trim())).ToArray();
        }

        public static string InstallPath { get; private set; } = string.Empty;
        public static string DatabasePath { get; private set; } = string.Empty;
        public static string LocalImagePath { get; private set; } = string.Empty;
        public static string LocalTrackPath { get; private set; } = string.Empty;

        public static Track[] Tracks { get; private set; } = null;

        #region BetaSlots
        private bool BetaSlotsInstalled()
        {
            if (File.Exists(string.Format(@"{0}\{1}", InstallPath, "MX07Leaderboards.bxml")) == false)
            {
                return false;
            }

            foreach(var uiFile in m_uiFiles)
            {
                if (File.Exists(string.Format(@"{0}\Database\{1}", InstallPath, uiFile)) == false)
                {
                    return false;
                }
            }
            return true;
        }

        private void InstallBetaSlots()
        {
            string betaSlotUrl = "https://s3.amazonaws.com/reflextracks/BetaSlots";
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(string.Format("{0}/{1}", betaSlotUrl, "MX07Leaderboards.bxml"), string.Format(@"{0}\{1}", InstallPath, "MX07Leaderboards.bxml"));

                foreach (var uiFile in m_uiFiles)
                {
                    client.DownloadFile(string.Format("{0}/{1}", betaSlotUrl, uiFile), string.Format(@"{0}\{1}", DatabasePath, uiFile));
                }
            }
        }
        #endregion


        #region InstallPath
        private string GetInstallPath()
        {
            string installPath = string.Empty;
            var steamInstallPath = GetSteamInstallPath(@"SOFTWARE\Valve\Steam");

            //If we didn't find it in the 32bit registry path search the 64 bit registry path
            if (steamInstallPath.Length == 0)
            {
                steamInstallPath = GetSteamInstallPath(@"SOFTWARE\Wow6432Node\Valve\Steam");
            }

            if (steamInstallPath.Length > 0)
            {
                installPath = GetInstallPath(steamInstallPath);
                if(installPath.Length == 0)
                {
                    var libraryFolders = GetLibraryFolders(steamInstallPath);
                    foreach(var folder in libraryFolders)
                    {
                        installPath = GetInstallPath(folder);
                        if(installPath.Length > 0)
                        {
                            break;
                        }
                    }
                }

                if(installPath.Length == 0)
                {
                    throw new Exception("Unable to detect MX vs. ATV Reflex install folder. Please ensure you have installed it via Steam.");
                }
            }
            else
            {
                throw new Exception("Unable to detect Steam install location. Please ensure Steam and MX vs. ATV Reflex are installed.");
            }

            return installPath;
        }

        private string GetSteamInstallPath(string steamKey)
        {
            string path = string.Empty;
            RegistryKey key = Registry.LocalMachine.OpenSubKey(steamKey);
            if(key != null)
            {
                object installpath = key.GetValue("InstallPath");
                if(installpath != null)
                {
                    path = installpath.ToString();
                }
            }

            return path;
        }

        private string[] GetLibraryFolders(string steamInstallPath)
        {
            List<string> libraryFolders = new List<string>();
            string libraryFoldersManifest = string.Format(@"{0}\{1}\libraryfolders.vdf", steamInstallPath, "steamapps");

            if (File.Exists(libraryFoldersManifest))
            {
                var reader = new AcfReader(libraryFoldersManifest);
                if (reader.CheckIntegrity())
                {
                    var manifestObject = reader.ACFFileToStruct();
                    for (int i = 1; ; ++i) //infinite loop
                    {
                        if (manifestObject.SubACF["LibraryFolders"].SubItems.ContainsKey(i.ToString()))
                        {
                            libraryFolders.Add(manifestObject.SubACF["LibraryFolders"].SubItems[i.ToString()].Replace(@"\\", @"\"));
                        }
                        else // no more steam libraries
                        {
                            break;
                        }
                    }
                }
            }

            return libraryFolders.ToArray();
        }

        private string GetInstallPath(string steamInstallPath)
        {
            string installPath = string.Empty;
            string basePath = string.Format(@"{0}\{1}", steamInstallPath, "steamapps");

            string[] files = Directory.GetFiles(basePath, "*.acf");

            foreach(var file in files)
            {
                var reader = new AcfReader(file);
                if (reader.CheckIntegrity())
                {
                    var manifestObject = reader.ACFFileToStruct();
                    if(manifestObject.SubACF["AppState"].SubItems["name"] == ReflexNameInSteam)
                    {
                        installPath = string.Format(@"{0}\steamapps\common\{1}", steamInstallPath, manifestObject.SubACF["AppState"].SubItems["installdir"]);
                        break;
                    }
                }
            }

            return installPath;
        }
        #endregion

        private const string ReflexNameInSteam = "MX vs. ATV Reflex";
        private readonly string[] m_uiFiles;
    }
}
