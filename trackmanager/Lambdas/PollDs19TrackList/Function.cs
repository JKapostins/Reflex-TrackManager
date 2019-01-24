
using Amazon.Lambda.Core;
using ReflexUtility;
using System.Collections.Generic;
using System.Linq;

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
                    var track = parser.ParseTrack(newTrack);
                    newTracks.Add(track);
                }
            }
        }
    }
}
