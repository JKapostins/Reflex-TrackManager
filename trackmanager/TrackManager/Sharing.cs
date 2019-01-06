using Newtonsoft.Json;
using ReflexUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using TrackManagement;

namespace TrackManager
{
    public static class Sharing
    {
        public static void Initialize()
        {
            ShareRateLimiter.Add(TrackType.National, 0);
            ShareRateLimiter.Add(TrackType.Supercross, 0);
            ShareRateLimiter.Add(TrackType.FreeRide, 0);
            NextPollTime = 0;
            SharedTracks = HttpUtility.Get<SharedReflexTracks[]>("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/share");
        }

        public static void ShareTracks(string trackType)
        {
            try
            {
                if (TrackInstaller.InstallQueueIsEmpty)
                {
                    if (TimeUtility.DateTimeToUnixTimeStamp(DateTime.UtcNow) >= ShareRateLimiter[trackType])
                    {
                        var currentSharedList = HttpUtility.Get<SharedReflexTracks[]>("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/share");
                        var randomNames = HttpUtility.Get<string[]>("http://names.drycodes.com/10?nameOptions=games");
                        var trackSetName = randomNames.Where(n => currentSharedList.Any(t => t.Name == n) == false).FirstOrDefault();
                        if (trackSetName != null)
                        {
                            var installedTracks = LocalSettings.Tracks.Where(t => t.Installed && t.Type == trackType).Select(t => t.Name).ToArray();

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
                                ShareRateLimiter[trackType] = nextShareTime;
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
                    else
                    {
                        var endTime = TimeUtility.UnixTimeStampToDateTime(ShareRateLimiter[trackType]);
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
            catch(Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }
        public static void PollSharedTracks()
        {
            if (Reflex.OverlayVisible && TimeUtility.DateTimeToUnixTimeStamp(DateTime.UtcNow) > NextPollTime)
            {
                SharedTracks = HttpUtility.Get<SharedReflexTracks[]>("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/share");
                NextPollTime = TimeUtility.DateTimeToUnixTimeStamp(DateTime.UtcNow) + ServerPollingRateInSeconds;
            }
        }

        public static SharedReflexTracks[] SharedTracks { get; private set; }
        public static Dictionary<string, long> ShareRateLimiter { get; private set; } = new Dictionary<string, long>();
        public static long NextPollTime;
        public const int LifeSpanMinutes = 60;
        public const int ServerPollingRateInSeconds = 2;
        public const int ShareRateLimitInSeconds = 60;
    }
}
