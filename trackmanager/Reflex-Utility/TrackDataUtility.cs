using System.Collections.Generic;

namespace ReflexUtility
{
    public static class TrackDataUtility
    {
        /// <summary>
        /// Some track names (ones that had multiple tracks in a single zip file) were renamed in our database. This function gets all the 'Original' track names
        /// so we can diff our track set with third-party sources.
        /// </summary>
        /// <returns></returns>
        public static string[] GetThirdPartyTrackNames()
        {
            var currentTracks = HttpUtility.Get<string[]>("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/tracknames");
            List<string> allTrackNames = new List<string>(currentTracks.Length);
            allTrackNames.AddRange(currentTracks);

            var trackTypes = new string[] { TrackType.National, TrackType.Supercross, TrackType.FreeRide, TrackType.Unknown };
            foreach(var trackName in currentTracks)
            {
                foreach(var trackType in trackTypes)
                {
                    for(int i = 0; i < TrackValidator.SlotCount; ++i)
                    {
                        string appendedData = string.Format(" ({0} S_{1})", trackType, i + 1);
                        
                        if(trackName.Contains(appendedData))
                        {
                            string origName = trackName.Replace(appendedData, string.Empty);
                            if (allTrackNames.Contains(origName) == false)
                            {
                                allTrackNames.Add(origName);
                            }
                        }
                    }
                }
            }

            return allTrackNames.ToArray();
        }
    }
}
