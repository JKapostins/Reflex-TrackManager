using Amazon;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Transfer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace ReflexUtility
{
    public class JsonInt
    {
        public int TrackId { get; set; }
    }

    public class PollTracks
    {
        public static void Poll(Amazon.Lambda.SQSEvents.SQSEvent sqsEvent, ILambdaContext context, bool isFirstRun)
        {
            int trackId = 0;
            if (isFirstRun == false)
            {
                if (sqsEvent.Records.Count != 1)
                {
                    throw new Exception(string.Format("Recived {0} records to process. Only 1 is supported.", sqsEvent.Records.Count));
                }

                var idObject = JsonConvert.DeserializeObject<JsonInt>(sqsEvent.Records[0].Body);

                if (idObject != null && idObject.TrackId > 0)
                {
                    trackId = idObject.TrackId;
                }
                else
                {
                    throw new Exception("There was an error parsing the lambda input");
                }
            }
            else
            {
                trackId = 1;
            }

            //Processing to many tracks at once kills the reflex central server. This ensurs we give them time to breath between requests.
            int NumberOfTracksToEvaluate = 25;
            context.Logger.LogLine(string.Format("Beginning to poll reflex central profile ids {0} - {1}", trackId, trackId + NumberOfTracksToEvaluate));
            ReflexCentralParser parser = new ReflexCentralParser();


            List<Track> tracks = new List<Track>(NumberOfTracksToEvaluate);
            for (int i = 0; i < NumberOfTracksToEvaluate; ++i)
            {
                context.Logger.LogLine(string.Format("Attempting to parse track id {0}", trackId));
                var track = parser.ParseTrack(string.Format("http://reflex-central.com/track_profile.php?track_id={0}", trackId));
                if (track != null)
                {
                    context.Logger.LogLine(string.Format("Found track at id {0}. Adding it to the track list", trackId));
                    tracks.Add(track);
                }
                else
                {
                    context.Logger.LogLine(string.Format("No track found at id {0}", trackId));
                }                
                ++trackId;
            }

            context.Logger.LogLine(string.Format("{0} tracks were returned from reflex central", tracks.Count));
            if (tracks.Count > 0)
            {
                context.Logger.LogLine("Pulling list of current tracks from our databse.");
                var existingTrackNames = TrackDataUtility.GetThirdPartyTrackNames();
                var newTracks = tracks.Where(t => existingTrackNames.Any(e => e == t.TrackName) == false).ToArray();

                context.Logger.LogLine("Pulling list of current tracks from our databse.");
                foreach (var track in newTracks)
                {
                    context.Logger.LogLine(string.Format("Sending new track ({0}) to be prcoessed UploadReflexTrackToS3 lambda.", track.TrackName));
                    HttpUtility.Post("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/processtrack", track);
                }

                if (newTracks.Length == 0)
                {
                    context.Logger.LogLine(string.Format("No new tracks to process in the current batch ({0} - {1}).", trackId, trackId + NumberOfTracksToEvaluate));
                }
                context.Logger.LogLine(string.Format("Kicking off another polling job beginning with profile id {0}.", trackId));
                HttpUtility.Post("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/pollreflexcentral", new JsonInt { TrackId = trackId });
            }
            else
            {
                context.Logger.LogLine("No more profile pages to poll.");
                context.Logger.LogLine("Polling complete!");
            }
        }
    }
}