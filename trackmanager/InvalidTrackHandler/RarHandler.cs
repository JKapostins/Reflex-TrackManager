using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using ReflexUtility;
using System.IO.Compression;

namespace InvalidTrackHandler
{
    class RarHandler
    {
        public RarHandler(string zipDestinationDirectory)
        {
            m_winRarPath = string.Empty;
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\WinRAR.exe");
            if (key != null)
            {
                object installpath = key.GetValue("Path");
                if (installpath != null)
                {
                    m_winRarPath = installpath.ToString();
                }
                else
                {
                    throw new Exception ("Unable to detect winrar path. Please make sure its installed.");
                }
            }
            else
            {
                throw new Exception("Unable to detect winrar path. Please make sure its installed.");
            }
            Console.WriteLine(string.Format("Detected WinRAR installation path: {0}", m_winRarPath));

            m_zipDest = zipDestinationDirectory;
            m_rarDownloadPath = string.Format(@"{0}\RarFiles", Environment.CurrentDirectory);
            Directory.CreateDirectory(m_rarDownloadPath);
        }

        public void ConvertToZipFiles(Track[] tracks)
        {
            foreach (var track in tracks)
            {
                var rarFileName = string.Format(@"{0}\{1}{2}", m_rarDownloadPath, track.TrackName.Trim(), Path.GetExtension(track.TrackUrl)).Replace("+", " ");
                using (WebClient client = new WebClient())
                {
                    Console.WriteLine(string.Format("Downloading {0}", track.TrackUrl));
                    client.DownloadFile(track.TrackUrl, rarFileName);
                }

                string extractionPath = rarFileName.Replace(Path.GetExtension(rarFileName), string.Empty);
                ExtractRar(rarFileName, extractionPath);

                
                string zipPath = string.Format(@"{0}\{1}{2}", m_zipDest, track.TrackName.Trim(), ".zip");
                Console.WriteLine(string.Format("Creating zip file: {0}", zipPath));
                ZipFile.CreateFromDirectory(extractionPath, zipPath, CompressionLevel.NoCompression, false);
                Console.WriteLine(string.Format("Deleting {0}", extractionPath));
                Directory.Delete(extractionPath, true);
            }
        }

        private void ExtractRar(string fileName, string destination)
        {
            Console.WriteLine(string.Format("Extracting {0}", fileName));
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = string.Format(@"{0}\WinRAR.exe", m_winRarPath),
                    Arguments = string.Format("X \"{0}\" *.* \"{1}\"\\", fileName, destination)
                }
            };
            process.Start();
            process.WaitForExit();
            Console.WriteLine("Extraction complete!");
            Console.WriteLine(string.Format("Deleting {0}", fileName));
            File.Delete(fileName);
        }



        private string m_rarDownloadPath;
        private string m_winRarPath;
        private string m_zipDest;
    }
}
