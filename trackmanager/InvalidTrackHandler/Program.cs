using System;
using ReflexUtility;
using System.Linq;
using System.IO;

namespace InvalidTrackHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Requesting invalid tracks from the database.");
                var invalidTracks = HttpUtility.Get<Track[]>("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/tracks?validation=invalid").ToList();
                var zipsPendingProcessingDirectory = string.Format(@"{0}\ZipsPendingProcessing", Environment.CurrentDirectory);
                Directory.CreateDirectory(zipsPendingProcessingDirectory);

                //{ //Handle rars
                //    var rarTracks = invalidTracks.Where(t => Path.GetExtension(t.TrackUrl) == ".rar").ToArray();
                //    if (rarTracks.Length > 0)
                //    {
                //        Console.WriteLine(string.Format("Found {0} tracks that are compressed with WinRAR. Initializing process to convert them to zip.", rarTracks.Length));
                //        RarHandler rarHandler = new RarHandler(zipsPendingProcessingDirectory);
                //        rarHandler.ConvertToZipFiles(rarTracks);
                //    }
                //}

                //{ //Handle multiple tracks
                //    var multipleTracks = invalidTracks.Where(t => t.ErrorInfo.Contains("Please only upload one track per zip file")).ToArray();
                //    if (multipleTracks.Length > 0)
                //    {
                //        MultipleTracksPerZipHandler handler = new MultipleTracksPerZipHandler(zipsPendingProcessingDirectory);
                //        invalidTracks.AddRange(handler.SeperateTracks(multipleTracks));
                //    }
                //}

                //{ //Handle multiple tracks
                //    var slot8Tracks = invalidTracks.Where(t => t.ErrorInfo == "Unknown slot; ").ToArray();
                //    if (slot8Tracks.Length > 0)
                //    {
                //        Slot8Handler handler = new Slot8Handler(zipsPendingProcessingDirectory);
                //        handler.ProcessTracks(slot8Tracks);
                //    }
                //}

                TrackProcessor trackProcessor = new TrackProcessor();
                trackProcessor.ProcessZipFiles(invalidTracks.ToArray(), zipsPendingProcessingDirectory);
            }
            catch(Exception e)
            {
                ExceptionLogger.LogException(e);
            }

            Console.WriteLine("Press any key to close this window.");
            Console.ReadKey();
        }
    }
}
