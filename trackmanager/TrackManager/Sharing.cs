using ReflexUtility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ReflexUtility;

namespace TrackManager
{
    public static class Sharing
    {
        public static void Initialize()
        {
            m_shareRateLimiter.Add(TrackType.National, 0);
            m_shareRateLimiter.Add(TrackType.Supercross, 0);
            m_shareRateLimiter.Add(TrackType.FreeRide, 0);
            m_nextPollTime = 0;
            m_sharedTracks = HttpUtility.Get<SharedReflexTracks[]>("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/share");
        }

        public static void AddTracksToUploadQueue(string trackType)
        {
            try
            {
                if (TrackInstaller.InstallQueueIsEmpty)
                {
                    if (TimeUtility.DateTimeToUnixTimeStamp(DateTime.UtcNow) >= m_shareRateLimiter[trackType])
                    {
                        if (m_uploadQueue.Any(q => q == trackType) == false)
                        {
                            m_uploadQueue.Enqueue(trackType);
                            Log.Add(Trackmanagement.LogMessageType.LogInfo, string.Format("Added your '{0}' tracks to the upload queue.", trackType));
                        }
                        else
                        {
                            Log.Add(Trackmanagement.LogMessageType.LogWarning, string.Format("Your {0} tracks are already being uploaded. Please wait until the share process is complete.", trackType));
                        }
                    }
                    else
                    {
                        var endTime = TimeUtility.UnixTimeStampToDateTime(m_shareRateLimiter[trackType]);
                        TimeSpan ts = endTime.Subtract(DateTime.UtcNow);
                        Log.Add(Trackmanagement.LogMessageType.LogWarning, string.Format("Share failed. You can share each track type once every {0} seconds.\n" +
                            " You will be able to share your {1} tracks again in {2} seconds", ShareRateLimitInSeconds, trackType, (int)ts.TotalSeconds));
                    }
                }
                else
                {
                    Log.Add(Trackmanagement.LogMessageType.LogWarning, "Cannot share tracks until current install operation is complete.");
                }

            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public static SharedReflexTracks[] GetSharedTracks()
        {
            lock (m_sharedTrackLocker)
            {
                return m_sharedTracks;
            }
        }

        public static bool UploadTracksQueueIsEmpty
        {
            get
            {
                return m_uploadQueue.IsEmpty;
            }
        }

        public static void Process()
        {
            PollSharedTracks();
            ProcessUploadQueue();
        }

        private static void ProcessUploadQueue()
        {
            try
            {
                if (m_uploadQueue.TryPeek(out string type))
                {
                    ShareTracks(type);

                    //We here instead of at the top of the loop because there are concurrent processes checking to ensure the queue is empty.
                    //We don't want external processes to think installation is complete before it really is.
                    m_uploadQueue.TryDequeue(out string dummy);
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        private static void PollSharedTracks()
        {
            if (Reflex.OverlayIsVisible() && TimeUtility.DateTimeToUnixTimeStamp(DateTime.UtcNow) > m_nextPollTime)
            {
                lock (m_sharedTrackLocker)
                {
                    m_sharedTracks = HttpUtility.Get<SharedReflexTracks[]>("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/share");
                }
                m_nextPollTime = TimeUtility.DateTimeToUnixTimeStamp(DateTime.UtcNow) + ServerPollingRateInSeconds;
            }
        }

        private static void ShareTracks(string trackType)
        {
            try
            {

                var currentSharedList = HttpUtility.Get<SharedReflexTracks[]>("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/share");
                var randomNames = HttpUtility.Get<string[]>("http://names.drycodes.com/10?nameOptions=games");
                var trackSetName = randomNames.Where(n => currentSharedList.Any(t => t.Name == n) == false).FirstOrDefault();
                if (trackSetName != null)
                {
                    var installedTracks = LocalSettings.GetTracks().Where(t => t.Installed && t.Type == trackType).Select(t => t.Name).ToArray();

                    SharedReflexTracks tracksToShare = new SharedReflexTracks
                    {
                        Name = trackSetName,
                        Type = trackType,
                        Tracks = string.Join(",", installedTracks),
                        CreationTime = TimeUtility.DateTimeToUnixTimeStamp(DateTime.UtcNow)
                    };

                    bool success = HttpUtility.Post("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/share", tracksToShare);
                    if (success)
                    {
                        Log.Add(Trackmanagement.LogMessageType.LogInfo, string.Format("Uploaded your {0} tracks as '{1}'. Tell your party to install it from the Shared Tracks Window.", trackType, trackSetName));
                        long nextShareTime = TimeUtility.DateTimeToUnixTimeStamp(DateTime.UtcNow) + ShareRateLimitInSeconds;
                        m_shareRateLimiter[trackType] = nextShareTime;
                    }
                    else
                    {
                        Log.Add(Trackmanagement.LogMessageType.LogError, string.Format("An unexpected error occured attempting to share your {0} tracks.", trackType));
                    }
                }
                else
                {
                    Log.Add(Trackmanagement.LogMessageType.LogError, string.Format("Unable to share your {0} tracks because a unique name could not be generated. Please try again later.", trackType));
                }


            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public const int LifeSpanMinutes = 5;
        public const int ServerPollingRateInSeconds = 2;
        public const int ShareRateLimitInSeconds = 60;

        private static ConcurrentQueue<string> m_uploadQueue = new ConcurrentQueue<string>();
        private static SharedReflexTracks[] m_sharedTracks;
        private static readonly object m_sharedTrackLocker = new object();
        private static Dictionary<string, long> m_shareRateLimiter = new Dictionary<string, long>();
        private static long m_nextPollTime;
    }
}
