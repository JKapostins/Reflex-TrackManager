using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace ReflexUtility
{
    public class Ds19TrackListParser : HtmlParser
    {
        public Ds19TrackListParser()
        {
            m_tracks = HttpUtility.Get<DS19Track[]>("http://ds19.eu/content/trackinstallerlist.php");
        }

        public override Track ParseTrack(string trackName)
        {
            Track track = null;

            var ds19Track = m_tracks.Where(t => t.Name == trackName).FirstOrDefault();
            if(ds19Track != null)
            {
                track = new Track
                {
                    TrackName = ds19Track.Name,
                    SourceTrackUrl = ds19Track.Tracklink,
                    SourceThumbnailUrl = ds19Track.Imagelink,
                    Author = ds19Track.Creator,
                    CreationTime = TimeUtility.DateTimeToUnixTimeStamp(DateTime.UtcNow)
                };
            }

            return track;
        }

        public string[] GetTrackNames()
        {
            return m_tracks.Select(t => t.Name).ToArray();
        }

        public override Track[] ParseTracks()
        {
            throw new NotImplementedException();
        }

        private DS19Track[] m_tracks;
    }
}
