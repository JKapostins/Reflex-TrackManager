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

        private static void LoadTracks()
        {
            string fileName = string.Format("{0}\\{1}", Reflex.LocalSettingsPath, TrackSettingsFile);
            if (File.Exists(fileName))
            {
                using (StreamReader r = new StreamReader(fileName))
                {
                    string json = r.ReadToEnd();
                    Tracks = JsonConvert.DeserializeObject<List<LocalTrack>>(json);
                }
            }
        }

        public static void SaveTracks()
        {
            using (StreamWriter file = File.CreateText(string.Format("{0}\\{1}", Reflex.LocalSettingsPath, TrackSettingsFile)))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Tracks);
            }
        }

        public static void HandleTrackInstall(TrackManagement.Track track, string dataPath)
        {
            //Uninstall the track in the current slot if it exists.
            var currentlyInstalledTrack = Tracks.Where(t => t.Installed == true && t.Type == track.TrackType && t.Slot == track.SlotNumber).SingleOrDefault();
            if(currentlyInstalledTrack != null)
            {
                currentlyInstalledTrack.Installed = false;
            }

            var localTrack = Tracks.Where(t => t.Name == track.TrackName && t.Type == track.TrackType).SingleOrDefault();
            if(localTrack != null)
            {
                ++localTrack.TotalDownloads;
                ++localTrack.MyDownloads;
                localTrack.Installed = true;
            }
            else
            {
                Tracks.Add(new LocalTrack
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

        public static void ToggleFavorite(string trackName)
        {
            var track = Reflex.Tracks.Where(t => t.TrackName == trackName).SingleOrDefault();
            if (track != null)
            {
                var existing = Tracks.Where(t => t.Name == track.TrackName && t.Type == track.TrackType).SingleOrDefault();
                if (existing != null)
                {
                    existing.Favorite = !existing.Favorite;
                }
                else
                {
                    Tracks.Add(new LocalTrack
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

        public static List<LocalTrack> Tracks { get; private set; } = new List<LocalTrack>();

        private const string TrackSettingsFile = "Tracks.json";
    }
}
