
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using ReflexUtility;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace PollDs19TrackList
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public void FunctionHandler(ILambdaContext context)
        {
            Ds19TrackListParser parser = new Ds19TrackListParser();
            var ds19TrackNames = parser.GetTrackNames();
            var gnarlyTrackNames = TrackDataUtility.GetThirdPartyTrackNames();
            var newTrackNames = ds19TrackNames.Except(gnarlyTrackNames).ToArray();

            List<Track> newTracks = new List<Track>();
            foreach(var newTrack in newTrackNames)
            {
                //GNARLY_TODO: remove the ignored tracks when DS Fixes his site.
                if (newTrack != "High Point")
                {
                    var parsedTrack = parser.ParseTrack(newTrack);
                    if (parsedTrack != null)
                    {
                        newTracks.Add(parsedTrack);
                    }
                }
            }

            //GNARLY_TODO: send sqs event to UploadReflexTrackToS3 instead of doing this here.
            foreach (var newTrack in newTracks)
            {
                var track = newTrack;
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


                track.FixEmptyStrings();
                var dynamoContext = new DynamoDBContext(new AmazonDynamoDBClient(RegionEndpoint.USEast1));
                dynamoContext.SaveAsync(track).Wait();

                using (WebClient client = new WebClient())
                {
                    using (Stream thumbNailStream = new MemoryStream(client.DownloadData(track.SourceThumbnailUrl)))
                    {
                        using (var jpegStream = new MemoryStream())
                        {
                            Image sourceImage = Image.FromStream(thumbNailStream);
                            sourceImage = ImageExtension.ResizeImage(sourceImage, 640, 360);
                            sourceImage.Save(jpegStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                            jpegStream.Position = 0;

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
                        using (Stream invalidStream = new MemoryStream(client.DownloadData(track.SourceTrackUrl)))
                        {
                            var uploadTask = AwsS3Utility.UploadFileAsync(invalidStream, string.Format("{0}/{1}", bucketName, folderName), trackFileName, RegionEndpoint.USEast1);
                            uploadTask.Wait();
                        }
                    }
                }

                context.Logger.LogLine(string.Format("Processing {0} is complete!", track.TrackName));
            }
        }
    }
}
