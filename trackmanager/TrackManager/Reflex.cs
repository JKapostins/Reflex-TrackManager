using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using TrackManager.Steam;

namespace TrackManager
{
    public class Reflex
    {
        public Reflex()
        {
            InstallLocation = GetInstallLocation();
        }

        public static string InstallLocation { get; private set; }

        private string GetInstallLocation()
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

        private const string ReflexNameInSteam = "MX vs. ATV Reflex";
    }
}
