using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using ReflexUtility;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace InvalidTrackHandler
{
    class TrackProcessor
    {
        public TrackProcessor()
        {

        }

        public void ProcessZipFiles(Track[] tracks, string pathToZips)
        {
            foreach(var track in tracks)
            {
                string trackZip = string.Format(@"{0}\{1}.zip", pathToZips, track.TrackName.Trim());
                if(File.Exists(trackZip))
                {
                    ProcessTrack(track, trackZip);
                }
            }
        }

        private void ProcessTrack(Track track, string zipFile)
        {
            //Reset to the defaults before re-processing the track
            track.ErrorInfo = string.Empty;
            track.Valid = true;

            TrackValidator validator = new TrackValidator();
            MemoryStream zipStream = new MemoryStream();
            ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true);

            Console.WriteLine(string.Format("Beginning to process {0}", track.TrackName));
            track = validator.ValidateLocalTrack(zipFile, track, (e) =>
            {
                var destFile = zipArchive.CreateEntry(e.Name);

                using (var destStream = destFile.Open())
                using (var srcStream = e.Open())
                {
                    var task = srcStream.CopyToAsync(destStream);
                    task.Wait();
                }
            });
            zipArchive.Dispose();
            zipStream.Position = 0;

            string folderName = Path.GetFileNameWithoutExtension(track.TrackUrl.Replace("+", " "));
            string imageFileName = track.TrackName + Path.GetExtension(track.ThumbnailUrl);
            string trackFileName = track.TrackName + ".zip"; // we know they are all zip files at this point.
            string bucketName = track.Valid ? "reflextracks" : "invalidreflextracks";
            string baseDestUrl = string.Format("https://s3.amazonaws.com/{0}/{1}", bucketName, folderName).Replace(" ", "+");

            track.TrackUrl = string.Format("{0}/{1}", baseDestUrl, trackFileName).Replace(" ", "+");
            string thumbnailDownloadPath = track.ThumbnailUrl;
            track.ThumbnailUrl = string.Format("{0}/{1}", baseDestUrl, imageFileName).Replace(" ", "+");

            using (WebClient webClient = new WebClient())
            {
                using (Stream thumbNailStream = new MemoryStream(webClient.DownloadData(thumbnailDownloadPath)))
                {
                    Console.WriteLine(string.Format("Uploading {0} to s3 {1}...", imageFileName, bucketName));
                    var uploadTask = AwsS3Utility.UploadFileAsync(thumbNailStream, string.Format("{0}/{1}", bucketName, folderName), imageFileName, RegionEndpoint.USEast1);
                    uploadTask.Wait();
                }

                Console.WriteLine(string.Format("Uploading {0} to s3 {1}...", trackFileName, bucketName));
                if (track.Valid)
                {
                    var uploadTask = AwsS3Utility.UploadFileAsync(zipStream, string.Format("{0}/{1}", bucketName, folderName), trackFileName, RegionEndpoint.USEast1);
                    uploadTask.Wait();
                }
                else
                {
                    using (Stream invalidStream = new MemoryStream(webClient.DownloadData(track.SourceTrackUrl)))
                    {
                        var uploadTask = AwsS3Utility.UploadFileAsync(invalidStream, string.Format("{0}/{1}", bucketName, folderName), trackFileName, RegionEndpoint.USEast1);
                        uploadTask.Wait();
                    }
                }
            }

            track.FixEmptyStrings();
            Console.WriteLine(string.Format("Updating {0} in dynamodb...", track.TrackName));
            var client = new AmazonDynamoDBClient(RegionEndpoint.USEast1);
            var context = new DynamoDBContext(client);
            var saveTask = context.SaveAsync(track);
            saveTask.Wait();
            if (track.Valid)
            {
                Console.WriteLine("Deleting {0}", zipFile);
                File.Delete(zipFile);
            }
            else
            {
                Console.WriteLine("[INVALID TRACK DATA] not deleting {0}, due to error ({1}). Please manually inspect..", zipFile, track.ErrorInfo);
            }
            Console.WriteLine(string.Format("Processing {0} is complete!", track.TrackName));
        }
    }
}
