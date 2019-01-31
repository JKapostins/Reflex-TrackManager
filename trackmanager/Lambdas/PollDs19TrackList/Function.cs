
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
using ImageMagick;
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
            context.Logger.LogLine("Checking Ds19's track website for new tracks...");
            Ds19TrackListParser parser = new Ds19TrackListParser();
            var ds19TrackNames = parser.GetTrackNames();
            var gnarlyTrackNames = TrackDataUtility.GetThirdPartyTrackNames();
            var newTrackNames = ds19TrackNames.Except(gnarlyTrackNames).ToArray();

            foreach (var newTrack in newTrackNames)
            {
                //GNARLY_TODO: Get DS to fix these
                if (newTrack == "2016 Las Vegas SX"
                    || newTrack == "2017 Monster Energy Cup")
                {
                    continue;
                }

                context.Logger.LogLine(string.Format("Attempting to parse new track ({0})", newTrack));
                
                var parsedTrack = parser.ParseTrack(newTrack);
                if (parsedTrack != null)
                {
                    context.Logger.LogLine(string.Format("Sending new track ({0}) to be prcoessed by UploadReflexTrackToS3 lambda.", parsedTrack.TrackName));
                    HttpUtility.Post("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/processtrack", parsedTrack);
                }
            }

            if (newTrackNames.Length > 0)
            {
                context.Logger.LogLine("Parsing new tracks complete!");
            }
            else
            {
                context.Logger.LogLine("No new tracks found.");
            }
        }
    }
}
