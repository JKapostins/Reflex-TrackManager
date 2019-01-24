using System;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace ReflexUtility
{
    public delegate void ZipFileEntryDelegate(ZipArchiveEntry entry);

    public class TrackValidator
    {
        public const int SlotCount = 8;

        public Track ValidateTrack(Track track, ZipFileEntryDelegate zipHandler)
        {
            string ext = Path.GetExtension(track.SourceTrackUrl);
            //We can only run automation on zip files. .rar is a closed format and not accepted.
            if (ext == ".zip")
            {
                track = PeekZipFile(track.SourceTrackUrl, track, zipHandler);
            }
            else
            {
                track.ErrorInfo += string.Format("Expected .zip file, got {0} file; ", ext);
                track.Valid = false;
            }

            return track;
        }

        public Track ValidateLocalTrack(string fileName, Track track, ZipFileEntryDelegate zipHandler)
        {
            string ext = Path.GetExtension(fileName);
            //We can only run automation on zip files. .rar is a closed format and not accepted.
            if (ext == ".zip")
            {
                track = PeekLocalZipFile(fileName, track, zipHandler);
            }
            else
            {
                track.ErrorInfo += string.Format("Expected .zip file, got {0} file; ", ext);
                track.Valid = false;
            }

            return track;
        }

        public static string GetTrackType(string fileName)
        {
            var type = TrackType.Unknown;
            for (int i = 0; i < SlotCount; ++i)
            {
                string nationalPattern = string.Format("Beta_Nat_Track", i+1);
                string supercrossPattern = string.Format("Beta_Sx_Track", i + 1);
                string freeRidePattern = string.Format("Beta_Track", i + 1);

                if(fileName.Contains(nationalPattern))
                {
                    type = TrackType.National;
                    break;
                }
                else if(fileName.Contains(supercrossPattern))
                {
                    type = TrackType.Supercross;
                    break;
                }
                else if (fileName.Contains(freeRidePattern))
                {
                    type = TrackType.FreeRide;
                    break;
                }
            }

            return type;
        }

        public static int GetSlot(string fileName)
        {
            int slot = 0;
            for(int i = 0; i < SlotCount; ++i)
            {
                string format = string.Format("Slot_{0}.dx9", i + 1);
                if(fileName.Contains(format))
                {
                    slot = i + 1;
                    break;
                }
            }

            return slot;
        }

        private Track PeekZipFile(string url, Track track, ZipFileEntryDelegate zipHandler)
        {
            using (WebClient client = new WebClient())
            {
                using (Stream memoryStream = new MemoryStream(client.DownloadData(url)))
                {
                    using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                    {
                        track = ValidateZipArchive(archive, track, zipHandler);
                    }
                }
            }

            return track;
        }

        private Track PeekLocalZipFile(string fileName, Track track, ZipFileEntryDelegate zipHandler)
        {
            using (ZipArchive archive = ZipFile.OpenRead(fileName))
            {
                track = ValidateZipArchive(archive, track, zipHandler);
            }

            return track;
        }

        private Track ValidateZipArchive(ZipArchive archive, Track track, ZipFileEntryDelegate zipHandler)
        {
            int databaseCount = 0;
            int levelCount = 0;
            int packageCount = 0;
            int sceneCount = 0;

            string databaseExt = ".dx9.database";
            string levelExt = ".dx9.level";
            string packageExt = ".dx9.package";
            string sceneExt = ".dx9.scene";

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.Name.EndsWith(databaseExt, StringComparison.OrdinalIgnoreCase))
                {
                    if (track.TrackType == TrackType.Unknown)
                    {
                        track.TrackType = GetTrackType(entry.Name);
                        track.SlotNumber = GetSlot(entry.Name);
                    }
                    ++databaseCount;
                    if(databaseCount == 1)
                    {
                        zipHandler(entry);
                    }
                }
                else if (entry.Name.EndsWith(levelExt, StringComparison.OrdinalIgnoreCase))
                {
                    ++levelCount;
                    if (levelCount == 1)
                    {
                        zipHandler(entry);
                    }
                }
                else if (entry.Name.EndsWith(packageExt, StringComparison.OrdinalIgnoreCase))
                {
                    ++packageCount;
                    if (packageCount == 1)
                    {
                        zipHandler(entry);
                    }
                }
                else if (entry.Name.EndsWith(sceneExt, StringComparison.OrdinalIgnoreCase))
                {
                    ++sceneCount;
                    if (sceneCount == 1)
                    {
                        zipHandler(entry);
                    }
                }
            }

            if (track.TrackType == TrackType.Unknown)
            {
                track.ErrorInfo += "Unknown track type; ";
                track.Valid = false;
            }

            if (track.SlotNumber == 0)
            {
                track.ErrorInfo += "Unknown slot; ";
                track.Valid = false;
            }

            track = CheckForRequiredFiles(track, databaseExt, databaseCount);
            track = CheckForRequiredFiles(track, levelExt, levelCount);
            track = CheckForRequiredFiles(track, packageExt, packageCount);
            track = CheckForRequiredFiles(track, sceneExt, sceneCount);

            return track;
        }

        private Track CheckForRequiredFiles(Track track, string fileExtention, int timesFileAppearsInZip)
        {
            if (timesFileAppearsInZip == 0)
            {
                track.ErrorInfo += string.Format("Missing *{0} file; ", fileExtention);
                track.Valid = false;
            }
            else if (timesFileAppearsInZip > 1)
            {
                track.ErrorInfo += string.Format("Expecting 1 *{0} file but got {1}. Please only upload one track per zip file; ", fileExtention, timesFileAppearsInZip);
                track.Valid = false;
            }
            return track;
        }
    }
}
