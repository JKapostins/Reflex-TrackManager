using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using System.IO;

namespace TrackManager
{
    public class TrackInstaller
    {
        public static bool InstallTrack(string trackName)
        {
            bool success = false;

            return success;
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
    }
}
