using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using ImageMagick;
using Newtonsoft.Json;
using ReflexUtility;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace UploadReflexTrackToS3
{
    public class UploadReflexTrackToS3
    {
        public void FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
        {
            if (sqsEvent.Records.Count != 1)
            {
                throw new Exception(string.Format("Recived {0} records to process. Only 1 is supported.", sqsEvent.Records.Count));
            }

            var track = JsonConvert.DeserializeObject<Track>(sqsEvent.Records[0].Body);
            if (track == null)
            {
                throw new Exception("There was an error parsing the lambda input");
            }

            TrackValidator validator = new TrackValidator();
            MemoryStream zipStream = new MemoryStream();
            ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true);

            context.Logger.LogLine(string.Format("Beginning to process {0}", track.TrackName));
            track = validator.ValidateTrack(track, (e) =>
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


            string folderName = track.TrackName;
            string imageFileName = track.TrackName + ".jpg";
            string trackFileName = track.TrackName + Path.GetExtension(track.SourceTrackUrl);
            bool googleDrive = track.SourceTrackUrl.Contains("drive.google.com");

            string bucketName = track.Valid ? "reflextracks" : "invalidreflextracks";
            string baseDestUrl = string.Format("https://s3.amazonaws.com/{0}/{1}", bucketName, folderName).Replace(" ", "+");

            track.TrackUrl = string.Format("{0}/{1}", baseDestUrl, trackFileName).Replace(" ", "+");
            if (googleDrive)
            {
                track.TrackUrl = track.SourceTrackUrl;
            }
            track.ThumbnailUrl = string.Format("{0}/{1}", baseDestUrl, imageFileName).Replace(" ", "+");


            using (WebClient client = new WebClient())
            {
                context.Logger.LogLine(string.Format("Attempting to load {0}", track.SourceThumbnailUrl));
                using (Stream thumbNailStream = new MemoryStream(client.DownloadData(track.SourceThumbnailUrl)))
                {
                    using (var jpegStream = new MemoryStream())
                    {
                        context.Logger.LogLine(string.Format("Ensuring {0} is a jpg file and resizing it to 640 x 360.", track.SourceThumbnailUrl));
                        MagickImage sourceImage = new MagickImage(thumbNailStream);
                        sourceImage.Resize(640, 360);
                        sourceImage.Write(jpegStream, MagickFormat.Jpg);
                        jpegStream.Position = 0;

                        context.Logger.LogLine(string.Format("Uploading {0} to s3.", track.ThumbnailUrl));
                        var uploadTask = AwsS3Utility.UploadFileAsync(jpegStream, string.Format("{0}/{1}", bucketName, folderName), imageFileName, RegionEndpoint.USEast1);
                        uploadTask.Wait();
                    }
                }

                if (track.Valid)
                {
                    var uploadTask = AwsS3Utility.UploadFileAsync(zipStream, string.Format("{0}/{1}", bucketName, folderName), trackFileName, RegionEndpoint.USEast1);
                    uploadTask.Wait();
                }
                else if (googleDrive == false) //no support for downloading data directly from google drive
                {
                    context.Logger.LogLine(string.Format("Attempting to copy {0} to invalid track bucket.", track.SourceTrackUrl));
                    try
                    {
                        using (Stream invalidStream = new MemoryStream(client.DownloadData(track.SourceTrackUrl)))
                        {
                            var uploadTask = AwsS3Utility.UploadFileAsync(invalidStream, string.Format("{0}/{1}", bucketName, folderName), trackFileName, RegionEndpoint.USEast1);
                            uploadTask.Wait();
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Failed to download track from source: {track.SourceTrackUrl}");
                    }
                }
            }

            track.FixEmptyStrings();
            var dynamoContext = new DynamoDBContext(new AmazonDynamoDBClient(RegionEndpoint.USEast1));
            dynamoContext.SaveAsync(track).Wait();

            context.Logger.LogLine(string.Format("Processing {0} is complete!", track.TrackName));
        }
    }
}
