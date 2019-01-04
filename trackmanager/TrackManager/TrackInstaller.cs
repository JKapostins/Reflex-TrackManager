using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using TrackManagement;

namespace TrackManager
{
    public static class TrackInstaller
    {
        public static string InstallStatus { get; private set; } = string.Empty;
        public static bool TrackInstallQueueEmpty
        {
            get
            {
                return m_trackQueue.IsEmpty;
            }
        }

        public static bool InstallTrack(string trackName)
        {
            bool success = false;

            return success;
        }

        public static void EnqueueRandomRandomTracks(string trackType)
        {
            try
            {
                if (TrackInstallQueueEmpty)
                {
                    Random rnd = new Random();
                    for (int i = 0; i < Reflex.SlotCount; ++i)
                    {
                        int slot = i + 1;
                        var randomTrack = Reflex.Tracks.Where(t => t.TrackType == trackType && t.SlotNumber == slot).Select(t => t.TrackName).OrderBy(item => rnd.Next()).FirstOrDefault();
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
            catch(Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }
        public static void AddTrackToInstallQueue(string trackName)
        {
            try
            {
                var track = Reflex.Tracks.Where(t => t.TrackName == trackName).SingleOrDefault();

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
            catch(Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        private static string DownloadTrack(string trackName)
        {
            string path = string.Empty;
            try
            {
                var track = Reflex.Tracks.Where(t => t.TrackName.Trim() == trackName.Trim()).Single();
                DownloadFile(track.TrackUrl, string.Format(@"{0}\{1}{2}", Reflex.LocalTrackPath, trackName, Path.GetExtension(track.TrackUrl)));
            }
            catch(Exception e)
            {
                ExceptionLogger.LogException(e);
            }
            return path;
        }

        public static string DownloadImage(string trackName)
        {
            
            string path = string.Empty;
            try
            {
                var track = Reflex.Tracks.Where(t => t.TrackName.Trim() == trackName.Trim()).Single();
                DownloadFile(track.ThumbnailUrl, string.Format(@"{0}\{1}{2}", Reflex.LocalImagePath, trackName, Path.GetExtension(track.ThumbnailUrl)));
            }
            catch (Exception e)
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
