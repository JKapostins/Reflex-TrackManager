using ReflexUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace InvalidTrackHandler
{
    class Slot8Handler
    {
        public Slot8Handler(string zipDestinationDirectory)
        {
            m_destinationFolder = zipDestinationDirectory;
            m_stagingDirectory = string.Format(@"{0}\UnknownSlots", Environment.CurrentDirectory);
            Directory.CreateDirectory(m_stagingDirectory);
        }

        public void ProcessTracks(Track[] tracks)
        {

            foreach (var track in tracks)
            {
                //download the invalid track file
                var trackFileName = string.Format(@"{0}\{1}{2}", m_stagingDirectory, track.TrackName.Trim(), Path.GetExtension(track.TrackUrl)).Replace("+", " ");
                using (WebClient client = new WebClient())
                {
                    Console.WriteLine(string.Format("Downloading {0}", track.TrackUrl));
                    client.DownloadFile(track.TrackUrl, trackFileName);
                }

                bool slot8 = false;
                using (ZipArchive archive = ZipFile.OpenRead(trackFileName))
                {
                    string databaseExt = ".dx9.database";
                    string levelExt = ".dx9.level";
                    string packageExt = ".dx9.package";
                    string sceneExt = ".dx9.scene";
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.Name.EndsWith(databaseExt, StringComparison.OrdinalIgnoreCase)
                            || entry.Name.EndsWith(levelExt, StringComparison.OrdinalIgnoreCase)
                            || entry.Name.EndsWith(packageExt, StringComparison.OrdinalIgnoreCase)
                            || entry.Name.EndsWith(sceneExt, StringComparison.OrdinalIgnoreCase))
                        {
                            int slot = TrackValidator.GetSlot(entry.Name);

                            slot8 = slot == 8;
                            break;
                        }
                    }
                }

                if(slot8)
                {
                    Console.WriteLine(string.Format("Slot 8 detected. Moving {0} to zip procesing folder.", Path.GetFileName(trackFileName)));
                    string fullFileName = string.Format("{0}/{1}", m_destinationFolder, Path.GetFileName(trackFileName));
                    if (File.Exists(fullFileName))
                    {
                        File.Delete(fullFileName);
                    }
                    File.Move(trackFileName, fullFileName);
                }
            }
        }

        private string m_destinationFolder;
        private string m_stagingDirectory;
    }
}
