using ReflexUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace InvalidTrackHandler
{
    class MultipleTracksPerZipHandler
    {
        public MultipleTracksPerZipHandler(string zipDestinationDirectory)
        {
            m_destinationFolder = zipDestinationDirectory;
            m_stagingDirectory = string.Format(@"{0}\MultipleTracks", Environment.CurrentDirectory);
            Directory.CreateDirectory(m_stagingDirectory);
        }

        public Track[] SeperateTracks(Track[] tracks)
        {
            List<Track> newTracks = new List<Track>();
            foreach (var track in tracks)
            {
                //download the invalid track file
                var trackFileName = string.Format(@"{0}\{1}{2}", m_stagingDirectory, track.TrackName.Trim(), Path.GetExtension(track.TrackUrl)).Replace("+", " ");
                //using (WebClient client = new WebClient())
                //{
                //    Console.WriteLine(string.Format("Downloading {0}", track.TrackUrl));
                //    client.DownloadFile(track.TrackUrl, trackFileName);
                //}

                var splitTracks = SplitTracks(track, trackFileName);
                if (splitTracks.Length > 0)
                {
                    newTracks.AddRange(splitTracks);
                }
            }
            return newTracks.ToArray();
        }

        private Track[] SplitTracks(Track originalTrack, string fileName)
        {
            List<Track> newTracks = new List<Track>();
            using (ZipArchive archive = ZipFile.OpenRead(fileName))
            {
                string databaseExt = ".dx9.database";
                string levelExt = ".dx9.level";
                string packageExt = ".dx9.package";
                string sceneExt = ".dx9.scene";

                Dictionary<string, List<ZipArchiveEntry>> trackBuckets = new Dictionary<string, List<ZipArchiveEntry>>();
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name.EndsWith(databaseExt, StringComparison.OrdinalIgnoreCase)
                        || entry.Name.EndsWith(levelExt, StringComparison.OrdinalIgnoreCase)
                        || entry.Name.EndsWith(packageExt, StringComparison.OrdinalIgnoreCase)
                        || entry.Name.EndsWith(sceneExt, StringComparison.OrdinalIgnoreCase))
                    {
                        string type = TrackValidator.GetTrackType(entry.Name);
                        int slot = TrackValidator.GetSlot(entry.Name);

                        string key = string.Format("{0}:{1}", type, slot);
                        if (trackBuckets.ContainsKey(key) == false)
                        {
                            trackBuckets[key] = new List<ZipArchiveEntry>();
                        }

                        trackBuckets[key].Add(entry);
                    }
                }

                foreach (var bucket in trackBuckets)
                {
                    const int trackFileCount = 4;
                    if (bucket.Value.Count == trackFileCount)
                    {
                        const int keyValueCount = 2;
                        var keyValues = bucket.Key.Split(":");
                        if (keyValues.Length == keyValueCount)
                        {
                            string trackName = string.Format("{0} ({1} S_{2})", originalTrack.TrackName, keyValues[0], keyValues[1]);
                            string path = string.Format(@"{0}\{1}.zip", m_destinationFolder, trackName);
                            Console.WriteLine("Creating new track ({0}) from {1}.", trackName, originalTrack.TrackName);
                            //Write track to file.
                            using (Stream zipStream = new FileStream(path, FileMode.Create))
                            {
                                using (ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                                {
                                    foreach(var entry in bucket.Value)
                                    {
                                        var destFile = zipArchive.CreateEntry(entry.Name);
                                        using (var destStream = destFile.Open())
                                        using (var srcStream = entry.Open())
                                        {
                                            var task = srcStream.CopyToAsync(destStream);
                                            task.Wait();
                                        }
                                    }
                                }
                            }

                            var newTrack = new Track
                            {
                                TrackName = trackName,
                                SourceTrackUrl = originalTrack.SourceTrackUrl,
                                SourceThumbnailUrl = originalTrack.SourceThumbnailUrl,
                                TrackUrl = originalTrack.TrackUrl,
                                ThumbnailUrl = originalTrack.ThumbnailUrl,
                                Author = originalTrack.Author,
                                CreationTime = originalTrack.CreationTime
                            };

                            newTracks.Add(newTrack);
                        }
                        else
                        {
                            Console.Error.WriteLine(string.Format("Unexpected key values: {0}, for track {1}", bucket.Key, fileName));
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine(string.Format("Unexpected track file counts in bucket {0}, for track {1}", bucket.Key, fileName));
                    }
                }
            }

            return newTracks.ToArray();
        }

        private string m_destinationFolder;
        private string m_stagingDirectory;
    }
}
