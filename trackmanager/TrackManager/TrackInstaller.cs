using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using TrackManagement;

namespace TrackManager
{
    public static class TrackInstaller
    {
        public static string InstallStatus { get; private set; } = string.Empty;
        public static bool InstallQueueIsEmpty
        {
            get
            {
                return m_trackQueue.IsEmpty;
            }
        }

        public static void ProcessDownloadQueue()
        {
            try
            {
                if (m_trackQueue.TryPeek(out Track track))
                {
                    var localTracks = Reflex.GetTracksOnDisk();
                    if (localTracks.Contains(track.TrackName) == false)
                    {
                        Log.Add(Trackmanagement.LogMessageType.LogInfo, string.Format("Downloading '{0}' from server.", track.TrackName));
                        DownloadFile(track.TrackUrl, string.Format(@"{0}\{1}{2}", Reflex.LocalTrackPath, track.TrackName, Path.GetExtension(track.TrackUrl)));
                    }

                    Log.Add(Trackmanagement.LogMessageType.LogInfo, string.Format("Installing '{0}' to databse folder.", track.TrackName));
                    InstallTrack(track);

                    //We here instead of at the top of the loop because there are concurrent processes checking to ensure the queue is empty.
                    //We don't want external processes to think installation is complete before it really is.
                    m_trackQueue.TryDequeue(out Track dummy);
                    if (m_trackQueue.IsEmpty)
                    {
                        Log.Add(Trackmanagement.LogMessageType.LogInfo, "Installation Complete!");
                    }
                }
            }
            catch(Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public static string DownloadImage(string trackName)
        {
            string path = string.Empty;
            try
            {
                var track = Reflex.GetTracks().Where(t => t.TrackName.Trim() == trackName.Trim()).Single();
                DownloadFile(track.ThumbnailUrl, string.Format(@"{0}\{1}{2}", Reflex.LocalImagePath, trackName, Path.GetExtension(track.ThumbnailUrl)));
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
            return path;
        }

        public static void EnqueueRandomRandomTracks(string trackType)
        {
            try
            {
                if (Sharing.UploadTracksQueueIsEmpty)
                {
                    if (m_trackQueue.IsEmpty)
                    {
                        Random rnd = new Random();
                        for (int i = 0; i < Reflex.SlotCount; ++i)
                        {
                            int slot = i + 1;
                            var randomTrack = Reflex.GetTracks().Where(t => t.TrackType == trackType && t.SlotNumber == slot).Select(t => t.TrackName).OrderBy(item => rnd.Next()).FirstOrDefault();
                            if (randomTrack != null)
                            {
                                AddTrackToInstallQueue(randomTrack);
                            }
                        }
                    }
                    else
                    {
                        Log.Add(Trackmanagement.LogMessageType.LogWarning, string.Format("Please wait for the current install operation to complete before selecting random {0} tracks.", trackType));
                    }
                }
                else
                {
                    Log.Add(Trackmanagement.LogMessageType.LogWarning, string.Format("Your tracks are being processed for sharing. Wait for the upload operation to complete before installing new random {0} tracks.", trackType));
                }
            }
            catch(Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public static void EnqueueSharedTracks(string listName)
        {
            try
            {
                if (Sharing.UploadTracksQueueIsEmpty)
                {
                    if (m_trackQueue.IsEmpty)
                    {
                        var item = Sharing.GetSharedTracks().Where(t => t.Name == listName).SingleOrDefault();
                        if (item != null)
                        {
                            var tracks = item.Tracks.Split(",");
                            Log.Add(Trackmanagement.LogMessageType.LogInfo, string.Format("Preparing to install {0} tracks from shared list '{1}'.", tracks.Length, listName));
                            foreach (var track in tracks)
                            {
                                AddTrackToInstallQueue(track);
                            }
                        }
                        else
                        {
                            Log.Add(Trackmanagement.LogMessageType.LogError, string.Format("Failed to install shared tracks ({0}) because they were not found'.", listName));
                        }
                    }
                    else
                    {
                        Log.Add(Trackmanagement.LogMessageType.LogWarning, string.Format("Please wait for the current install operation to complete installing shared tracks '{0}'.", listName));
                    }
                }
                else
                {
                    Log.Add(Trackmanagement.LogMessageType.LogWarning, string.Format("Your tracks are being processed for sharing. Wait for the upload operation to complete before installing {0} tracks.", listName));
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }
        public static void AddTrackToInstallQueue(string trackName)
        {
            try
            {
                if (Sharing.UploadTracksQueueIsEmpty)
                {
                    var track = Reflex.GetTracks().Where(t => t.TrackName == trackName).SingleOrDefault();

                    if (track != null)
                    {
                        if (m_trackQueue.Any(q => q.TrackType == track.TrackType && q.SlotNumber == track.SlotNumber) == false)
                        {
                            m_trackQueue.Enqueue(track);
                            Log.Add(Trackmanagement.LogMessageType.LogInfo, string.Format("Added '{0}' ({1} Slot {2}) to the install queue.", track.TrackName, track.TrackType, track.SlotNumber));
                        }
                        else
                        {
                            Log.Add(Trackmanagement.LogMessageType.LogWarning, string.Format("There is already a track being installed to '{0} Slot {1}'. Wait for the current install process to finish before installing '{2}'.", track.TrackType, track.SlotNumber, track.TrackName));
                        }
                    }
                    else
                    {
                        Log.Add(Trackmanagement.LogMessageType.LogWarning, string.Format("Unable to find '{0}'. Install operation cancelled.", trackName));
                    }
                }
                else
                {
                    Log.Add(Trackmanagement.LogMessageType.LogWarning, string.Format("Your tracks are being processed for sharing. Wait for the upload operation to complete before installing {0}.", trackName));
                }
            }
            catch(Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public static void InstallTrack(Track track)
        {
            string trackPath = string.Format("{0}\\{1}.zip", Reflex.LocalTrackPath, track.TrackName);
            if(File.Exists(trackPath))
            {
                using (Stream fileStream = File.OpenRead(trackPath))
                {
                    using (ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
                    {
                        WriteTrackFiles(archive);
                    }
                }

                LocalSettings.HandleTrackInstall(track, trackPath);
                LocalSettings.SaveTracks();
            }
            else
            {
                Log.Add(Trackmanagement.LogMessageType.LogError, string.Format("Installation Failed. The track file does not exist {0}", trackPath));
            }
        }

        private static void WriteTrackFiles(ZipArchive archive)
        {
            string databaseExt = ".dx9.database";
            string levelExt = ".dx9.level";
            string packageExt = ".dx9.package";
            string sceneExt = ".dx9.scene";

            //Ensure only valid files get copied
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.EndsWith(databaseExt, StringComparison.OrdinalIgnoreCase)
                    || entry.FullName.EndsWith(levelExt, StringComparison.OrdinalIgnoreCase)
                    || entry.FullName.EndsWith(packageExt, StringComparison.OrdinalIgnoreCase)
                    || entry.FullName.EndsWith(sceneExt, StringComparison.OrdinalIgnoreCase)
                    )
                {
                    entry.ExtractToFile(string.Format(@"{0}\{1}",Reflex.DatabasePath, entry.Name), true);
                }
            }


        }

        private static string DownloadTrack(string trackName)
        {
            string path = string.Empty;
            try
            {
                var track = Reflex.GetTracks().Where(t => t.TrackName.Trim() == trackName.Trim()).Single();
                DownloadFile(track.TrackUrl, string.Format(@"{0}\{1}{2}", Reflex.LocalTrackPath, trackName, Path.GetExtension(track.TrackUrl)));
            }
            catch(Exception e)
            {
                ExceptionLogger.LogException(e);
            }
            return path;
        }

        
        private static bool DownloadFile(string url, string destFile)
        {
            bool success = false;
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(url, destFile);
                }
                success = true;
            }
            catch(Exception e)
            {
                ExceptionLogger.LogException(e);
            }

            return success;
        }

        private static ConcurrentQueue<Track> m_trackQueue = new ConcurrentQueue<Track>();
    }
}
