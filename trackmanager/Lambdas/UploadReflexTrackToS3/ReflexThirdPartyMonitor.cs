using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
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
            try
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

                string folderName = Path.GetFileNameWithoutExtension(track.SourceTrackUrl.Replace("%20", " "));
                string imageFileName = Path.GetFileName(track.SourceThumbnailUrl).Replace("%20", " ");
                string trackFileName = Path.GetFileName(track.SourceTrackUrl).Replace("%20", " ");
                string bucketName = track.Valid ? "reflextracks" : "invalidreflextracks";
                string baseDestUrl = string.Format("https://s3.amazonaws.com/{0}/{1}", bucketName, folderName).Replace(" ", "+");

                track.TrackUrl = string.Format("{0}/{1}", baseDestUrl, trackFileName).Replace(" ", "+");
                track.ThumbnailUrl = string.Format("{0}/{1}", baseDestUrl, imageFileName).Replace(" ", "+");

                using (WebClient client = new WebClient())
                {
                    using (Stream thumbNailStream = new MemoryStream(client.DownloadData(track.SourceThumbnailUrl)))
                    {
                        var uploadTask = AwsS3Utility.UploadFileAsync(thumbNailStream, string.Format("{0}/{1}", bucketName, folderName), imageFileName, RegionEndpoint.USEast1);
                        uploadTask.Wait();
                    }

                    if (track.Valid)
                    {
                        var uploadTask = AwsS3Utility.UploadFileAsync(zipStream, string.Format("{0}/{1}", bucketName, folderName), trackFileName, RegionEndpoint.USEast1);
                        uploadTask.Wait();
                    }
                    else
                    {
                        using (Stream invalidStream = new MemoryStream(client.DownloadData(track.SourceTrackUrl)))
                        {
                            var uploadTask = AwsS3Utility.UploadFileAsync(invalidStream, string.Format("{0}/{1}", bucketName, folderName), trackFileName, RegionEndpoint.USEast1);
                            uploadTask.Wait();
                        }
                    }
                }

                track.FixEmptyStrings();
                var success = HttpUtility.Post("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/tracks", track);

                context.Logger.LogLine(string.Format("Processing {0} is complete!", track.TrackName));
            }
            catch (Exception e)
            {
                context.Logger.LogLine(e.Message);
            }
        }
    }
}
