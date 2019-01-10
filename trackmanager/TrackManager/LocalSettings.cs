using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TrackManager
{
    public static class LocalSettings
    {
        public static void Load()
        {
            LoadTracks();
        }

        public static LocalTrack[] GetTracks()
        {
            lock(m_locker)
            {
                return m_tracks.ToArray();
            }
        }

        public static bool TrackSettingsExist()
        {
            bool exist = false;
            string fileName = string.Format("{0}\\{1}", Reflex.LocalSettingsPath, TrackSettingsFile);
            if (File.Exists(fileName))
            {
                exist = true;
            }
            return exist;
        }

        private static void LoadTracks()
        {
            string fileName = string.Format("{0}\\{1}", Reflex.LocalSettingsPath, TrackSettingsFile);
            if (File.Exists(fileName))
            {
                using (StreamReader r = new StreamReader(fileName))
                {
                    string json = r.ReadToEnd();
                    lock (m_locker)
                    {
                        m_tracks = JsonConvert.DeserializeObject<List<LocalTrack>>(json);
                    }
                }
            }
        }

        public static void SaveTracks()
        {
            using (StreamWriter file = File.CreateText(string.Format("{0}\\{1}", Reflex.LocalSettingsPath, TrackSettingsFile)))
            {
                JsonSerializer serializer = new JsonSerializer();
                lock (m_locker)
                {
                    serializer.Serialize(file, m_tracks);
                }
            }
        }

        public static void HandleTrackInstall(ReflexUtility.Track track, string dataPath)
        {
            //Uninstall the track in the current slot if it exists.
            lock (m_locker)
            {
                var currentlyInstalledTrack = m_tracks.Where(t => t.Installed == true && t.Type == track.TrackType && t.Slot == track.SlotNumber).SingleOrDefault();
                if (currentlyInstalledTrack != null)
                {
                    currentlyInstalledTrack.Installed = false;
                }

                var localTrack = m_tracks.Where(t => t.Name == track.TrackName && t.Type == track.TrackType).SingleOrDefault();
                if (localTrack != null)
                {
                    localTrack.TotalDownloads = track.RatingVoteCount;
                    ++localTrack.MyDownloads;
                    localTrack.Installed = true;
                }
                else
                {
                    m_tracks.Add(new LocalTrack
                    {
                        Name = track.TrackName,
                        Type = track.TrackType,
                        Image = string.Format("{0}\\{1}{2}", Reflex.LocalImagePath, track.TrackName, Path.GetExtension(track.ThumbnailUrl)).Replace("\\", "/"),
                        Data = dataPath.Replace("\\", "/"),
                        Author = track.Author,
                        Slot = track.SlotNumber,
                        CreationTime = track.CreationTime,
                        TotalDownloads = track.RatingVoteCount, //GNARLY_TODO: covert to downloads
                        MyDownloads = 1,
                        Favorite = false,
                        Installed = true
                    });
                }
            }
        }

        public static void ToggleFavorite(string trackName)
        {
            lock (m_locker)
            {
                var track = Reflex.GetTracks().Where(t => t.TrackName == trackName).SingleOrDefault();
                if (track != null)
                {
                    var existing = m_tracks.Where(t => t.Name == track.TrackName && t.Type == track.TrackType).SingleOrDefault();
                    if (existing != null)
                    {
                        existing.Favorite = !existing.Favorite;
                    }
                    else
                    {
                        m_tracks.Add(new LocalTrack
                        {
                            Name = track.TrackName,
                            Type = track.TrackType,
                            Image = string.Format("{0}\\{1}{2}", Reflex.LocalImagePath, track.TrackName, Path.GetExtension(track.ThumbnailUrl)).Replace("\\", "/"),
                            Data = "",
                            Author = track.Author,
                            Slot = track.SlotNumber,
                            CreationTime = track.CreationTime,
                            TotalDownloads = track.RatingVoteCount, //GNARLY_TODO: covert to downloads
                            MyDownloads = 1,
                            Favorite = true,
                            Installed = false
                        });
                    }
                    SaveTracks();
                }
            }
        }

        private static List<LocalTrack> m_tracks = new List<LocalTrack>();
        private static readonly object m_locker = new object();
        private const string TrackSettingsFile = "Tracks.json";
    }
}
