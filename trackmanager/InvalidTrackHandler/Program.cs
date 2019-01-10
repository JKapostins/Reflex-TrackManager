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
                var invalidTracks = HttpUtility.Get<Track[]>("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/tracks?validation=invalid");
                var rarTracks = invalidTracks.Where(t => Path.GetExtension(t.TrackUrl) == ".rar").ToArray();
                var zipsPendingProcessingDirectory = string.Format(@"{0}\ZipsPendingProcessing", Environment.CurrentDirectory);
                Directory.CreateDirectory(zipsPendingProcessingDirectory);

                //if (rarTracks.Length > 0)
                //{
                //    Console.WriteLine(string.Format("Found {0} tracks that are compressed with WinRAR. Initializing process to convert them to zip.", rarTracks.Length));
                //    RarHandler rarHandler = new RarHandler(zipsPendingProcessingDirectory);
                //    rarHandler.ConvertToZipFiles(rarTracks);
                //}

                TrackProcessor trackProcessor = new TrackProcessor();
                trackProcessor.ProcessZipFiles(invalidTracks, zipsPendingProcessingDirectory);
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
