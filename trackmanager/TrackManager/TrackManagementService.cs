using Grpc.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReflexUtility;

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
            var tracks = FilterAndSortTracks(request.Sorting);

            int trackCount = request.EndIndex - request.StartIndex;
            Trackmanagement.Track[] trackSubset = new Trackmanagement.Track[trackCount];
            Array.Copy(tracks, request.StartIndex, trackSubset, 0, trackCount);
            Trackmanagement.Track firstTrackInList = null;
            if(tracks.Length > 0)
            {
                firstTrackInList = tracks[0];
            }
            return Task.FromResult(new Trackmanagement.TrackResponse
            {
                FirstTrackInList = firstTrackInList,
                Tracks = { trackSubset }
            });
        }

        public override Task<Trackmanagement.NumberMessage> GetTrackCount(Trackmanagement.SortRequest request, ServerCallContext context)
        {
            int trackCount = FilterTracks(request).Length;
            return Task.FromResult(new Trackmanagement.NumberMessage
            {
                Value = trackCount
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
            var lists = Sharing.GetSharedTracks().Where(t => TimeUtility.Expired(t.CreationTime, TrackSharing.LifeSpanMinutes) == false).Select(t => new Trackmanagement.SharedTrackList
            {
                Name = t.Name,
                Type = t.Type,
                Expires = TimeUtility.TimeToExpire(t.CreationTime, TrackSharing.LifeSpanMinutes)

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
                    Installs = t.TotalDownloads,
                    MyInstalls = t.MyDownloads,
                    Favorite = t.Favorite
                })
                .OrderBy(t => t.Slot)
                .ToArray() }
            });
        }

        private Trackmanagement.Track[] FilterTracks(Trackmanagement.SortRequest request)
        {
            var tracks = Reflex.GetDisplayTracks();

            if (request.TrackType != "All Track Types")
            {
                tracks = tracks.Where(t => t.Type == request.TrackType).ToArray();
            }

            if (request.Slot != "All Slots")
            {
                int slot = Convert.ToInt32(request.Slot);
                tracks = tracks.Where(t => t.Slot == slot).ToArray();
            }
            return tracks;
        }
        private Trackmanagement.Track[] FilterAndSortTracks(Trackmanagement.SortRequest request)
        {
            var tracks = FilterTracks(request);

            switch (request.SortBy)
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
                        tracks = tracks.OrderByDescending(t => t.Date).ToArray();
                        break;
                    }
                case "Installs":
                    {
                        tracks = tracks.OrderByDescending(t => t.Installs).ToArray();
                        break;
                    }
                case "My Installs":
                    {
                        tracks = tracks.OrderByDescending(t => t.MyInstalls).ToArray();
                        break;
                    }
                case "Favorite":
                    {
                        tracks = tracks.OrderByDescending(t => t.Favorite).ToArray();
                        break;
                    }
            }
            return tracks;
        }
    }
}
