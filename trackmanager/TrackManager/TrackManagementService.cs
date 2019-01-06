using Grpc.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TrackManagement;

namespace TrackManager
{
    public class TrackManagementService : Trackmanagement.TrackManager.TrackManagerBase
    {

        public bool OverlayClientConnected { get; set; }

        public TrackManagementService()
        {
            OverlayClientConnected = false;
        }

        public override Task<Trackmanagement.TrackResponse> GetTracks(Trackmanagement.TrackRequest request, ServerCallContext context)
        {
            Trackmanagement.Track[] tracks = tracks = Reflex.GetTracks().Select(t => new Trackmanagement.Track
            {
                Name = t.TrackName,
                Type = t.TrackType,
                Image = string.Format("{0}\\{1}{2}", Reflex.LocalImagePath, t.TrackName, Path.GetExtension(t.ThumbnailUrl)).Replace("\\", "/"),
                Author = t.Author,
                Slot = t.SlotNumber,
                Date = TimeUtility.UnixTimeStampToString(t.CreationTime),
                Downloads = 0,
                Favorite = GetFavorite(t.TrackName),

            }).ToArray();

            if (request.TrackType != "All Track Types")
            {
                tracks = tracks.Where(t => t.Type == request.TrackType).ToArray();
            }

            if (request.Slot != "All Slots")
            {
                int slot = Convert.ToInt32(request.Slot);
                tracks = tracks.Where(t => t.Slot == slot).ToArray();
            }

            switch(request.SortBy)
            {
                case "Name":
                    {
                        tracks = tracks.OrderBy(t => t.Name).ToArray();
                        break;
                    }
                case "Slot":
                    {
                        tracks = tracks.OrderBy(t => t.Slot).ToArray();
                        break;
                    }
                case "Type":
                    {
                        tracks = tracks.OrderBy(t => t.Type).ToArray();
                        break;
                    }
                case "Author":
                    {
                        tracks = tracks.OrderBy(t => t.Author).ToArray();
                        break;
                    }
                case "Date Created":
                    {
                        tracks = tracks.OrderBy(t => t.Date).ToArray();
                        break;
                    }
                case "Downloads":
                    {
                        tracks = tracks.OrderBy(t => t.Downloads).ToArray();
                        break;
                    }
                case "Favorite":
                    {
                        tracks = tracks.OrderByDescending(t => t.Favorite).ToArray();
                        break;
                    }
            }
            return Task.FromResult(new Trackmanagement.TrackResponse
            {
                Tracks = { tracks }
            });
        }
        public override Task<Trackmanagement.InstallStatusResponse> GetInstallStatus(Trackmanagement.Empty request, ServerCallContext context)
        {
            return Task.FromResult(new Trackmanagement.InstallStatusResponse
            {
                InstallStatus = ""
            });
        }

        public override Task<Trackmanagement.Empty> InstallRandomNationals(Trackmanagement.Empty request, ServerCallContext context)
        {
            TrackInstaller.EnqueueRandomRandomTracks(TrackType.National);
            return Task.FromResult(new Trackmanagement.Empty());
        }

        public override Task<Trackmanagement.Empty> InstallRandomSupercross(Trackmanagement.Empty request, ServerCallContext context)
        {
            TrackInstaller.EnqueueRandomRandomTracks(TrackType.Supercross);
            return Task.FromResult(new Trackmanagement.Empty());
        }
        public override Task<Trackmanagement.Empty> InstallRandomFreeRides(Trackmanagement.Empty request, ServerCallContext context)
        {
            TrackInstaller.EnqueueRandomRandomTracks(TrackType.FreeRide);
            return Task.FromResult(new Trackmanagement.Empty());
        }
        public override Task<Trackmanagement.Empty> InstallSelectedTrack(Trackmanagement.InstallTrackRequest request, ServerCallContext context)
        {
            TrackInstaller.AddTrackToInstallQueue(request.TrackName);
            return Task.FromResult(new Trackmanagement.Empty());
        }

        public override Task<Trackmanagement.Empty> InstallSharedTracks(Trackmanagement.InstallTrackRequest request, ServerCallContext context)
        {
            TrackInstaller.EnqueueSharedTracks(request.TrackName);
            return Task.FromResult(new Trackmanagement.Empty());
        }

        public override Task<Trackmanagement.Empty> ToggleFavorite(Trackmanagement.InstallTrackRequest request, ServerCallContext context)
        {
            LocalSettings.ToggleFavorite(request.TrackName);
            return Task.FromResult(new Trackmanagement.Empty());
        }

        public override Task<Trackmanagement.Empty> ShareTracks(Trackmanagement.InstallTrackRequest request, ServerCallContext context)
        {
            Sharing.AddTracksToUploadQueue(request.TrackName);
            return Task.FromResult(new Trackmanagement.Empty());
        }

        public override Task<Trackmanagement.Empty> SetOverlayVisible(Trackmanagement.ToggleMessage request, ServerCallContext context)
        {
            Reflex.SetOverlayVisibility(request.Toggle);
            return Task.FromResult(new Trackmanagement.Empty());
        }


        public override Task<Trackmanagement.SharedTrackResponse> GetSharedTracks(Trackmanagement.Empty request, ServerCallContext context)
        {
            var lists = Sharing.GetSharedTracks().Where(t => TimeUtility.Expired(t.CreationTime, Sharing.LifeSpanMinutes) == false).Select(t => new Trackmanagement.SharedTrackList
            {
                Name = t.Name,
                Type = t.Type,
                Expires = TimeUtility.TimeToExpire(t.CreationTime, Sharing.LifeSpanMinutes)

            })
            .OrderByDescending(t => t.Expires)
            .ToArray();
            return Task.FromResult(new Trackmanagement.SharedTrackResponse
            {
                SharedTracks = { lists }
            });
        }

        public override Task<Trackmanagement.LogResponse> GetLogMessages(Trackmanagement.Empty request, ServerCallContext context)
        {
            var messages = Log.TryDequeueAll();
            return Task.FromResult(new Trackmanagement.LogResponse
            {
                Messages = { messages }
            });
        }

        public override Task<Trackmanagement.TrackResponse> GetInstalledTracks(Trackmanagement.InstalledTrackRequest request, ServerCallContext context)
        {
            return Task.FromResult(new Trackmanagement.TrackResponse
            {
                Tracks = { LocalSettings.GetTracks().Where(t => t.Installed == true && t.Type == request.TrackType)
                .Select(t => new Trackmanagement.Track
                {
                    Name = t.Name,
                    Type = t.Type,
                    Image = t.Image,
                    Author = t.Author,
                    Slot = t.Slot,
                    Date = TimeUtility.UnixTimeStampToString(t.CreationTime),
                    Downloads = t.TotalDownloads,
                    Favorite = t.Favorite
                })
                .OrderBy(t => t.Slot)
                .ToArray() }
            });
        }

        private bool GetFavorite(string trackName)
        {
            bool favorite = false;
            var localTrack = LocalSettings.GetTracks().Where(t => t.Name == trackName).SingleOrDefault();
            if(localTrack != null)
            {
                favorite = localTrack.Favorite;
            }
            return favorite;
        }
    }
}
