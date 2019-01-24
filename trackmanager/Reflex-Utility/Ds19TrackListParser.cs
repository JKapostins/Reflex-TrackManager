using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace ReflexUtility
{
    public class Ds19TrackListParser : HtmlParser
    {
        public Ds19TrackListParser()
        {
        }

        public override Track ParseTrack(string url)
        {
            throw new NotImplementedException();
        }

        public override Track[] ParseTracks()
        {
            List<Track> tracks = new List<Track>();
            string html = GetHtml("http://ds19.eu/tracklist.php");
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var trackListNodes = doc.DocumentNode.SelectNodes("//div[@id='tracklist']/a");

            foreach (var node in trackListNodes)
            {
                if (node.NodeType == HtmlNodeType.Element && node.Name == "a")
                {
                    string hrefValue = node.Attributes["href"].Value;
                    Track track = new Track
                    {
                        TrackName = node.InnerHtml,
                        SourceTrackUrl = string.Format("http://ds19.eu/{0}", hrefValue)
                    };

                    tracks.Add(track);
                }
            }
            return tracks.ToArray();
        }
    }
}
