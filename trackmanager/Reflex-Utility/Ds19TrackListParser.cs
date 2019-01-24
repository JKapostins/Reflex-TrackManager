using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;

namespace ReflexUtility
{
    public class Ds19TrackListParser : HtmlParser
    {
        public Ds19TrackListParser()
        {
            m_trackNodes = null;
        }

        public override Track ParseTrack(string trackName)
        {
            LazyInitTrackNodes();
            Track track = null;

            foreach (var node in m_trackNodes)
            {
                var currentTrackName = node.InnerHtml.Trim();
                if (node.NodeType == HtmlNodeType.Element && node.Name == "a" && currentTrackName == trackName)
                {
                    track = new Track()
                    {
                        TrackName = trackName
                    };

                    string hrefValue = node.Attributes["href"].Value;
                    string profilePage = string.Format("http://ds19.eu{0}", hrefValue);
                    track = ParseTrackFromProfilePage(track, profilePage);
                }
            }

            return track;
        }

        public string[] GetTrackNames()
        {
            List<string> tracks = new List<string>();
            LazyInitTrackNodes();

            foreach (var node in m_trackNodes)
            {
                if (node.NodeType == HtmlNodeType.Element && node.Name == "a")
                {
                    tracks.Add(node.InnerHtml.Trim());
                }
            }
            return tracks.ToArray();
        }

        public override Track[] ParseTracks()
        {
            throw new NotImplementedException();
        }

        private Track ParseTrackFromProfilePage(Track track, string url)
        {
            string html = GetHtml(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            string baseUrl = "http://ds19.eu/content";
            
            //Handle common profile image                
            HtmlNode previewImageNode = doc.DocumentNode.SelectSingleNode("//*[@id='trackinfo']/img");
            if (previewImageNode != null)
            {
                track.SourceThumbnailUrl = string.Format("{0}/{1}", baseUrl, previewImageNode.Attributes["src"].Value);
            }
            else
            {
                //Handle profile page with full screen preview image
                previewImageNode = doc.DocumentNode.SelectSingleNode("/html/body");

                if (previewImageNode != null)
                {
                    track.SourceThumbnailUrl = string.Format("{0}/{1}", baseUrl, previewImageNode.Attributes["background"].Value);
                }
                else
                {
                    throw new Exception(string.Format("Unable to detect preview image for {0}", track.TrackName));
                }
            }


            //Handle common download url
            HtmlNode trackDataNode = doc.DocumentNode.SelectSingleNode("//*[@id='link']/a");
            if(trackDataNode != null)
            {
                //Some hrefs contain the full path, others partial paths...
                var href = trackDataNode.Attributes["href"].Value.Replace(baseUrl, string.Empty);
                track.SourceTrackUrl = GetFinalUrl(string.Format("{0}/{1}", baseUrl, href));
            }
            else
            {
                //Handle Ironman download url
                trackDataNode = doc.DocumentNode.SelectSingleNode("//*[@id='dllink2']/a[1]");
                if(trackDataNode != null)
                {
                    track.SourceTrackUrl = GetFinalUrl(string.Format("{0}/{1}", baseUrl, trackDataNode.Attributes["href"].Value));
                }
                else
                {
                    throw new Exception(string.Format("Unable to detect traxk data for {0}", track.TrackName));
                }
            }

            //Handle common Author
            HtmlNode authorNode = doc.DocumentNode.SelectSingleNode("//*[@id='trackinfo']/h1[1]/a");
            if(authorNode != null)
            {
                track.Author = authorNode.InnerHtml.Trim();
            }
            //Cmon ds
            else if(track.TrackName == "Ds19 Compound Sx3"
                || track.TrackName == "Ds19 Compound Sx2"
                || track.TrackName == "Ds19 Compound Sx1"
                || track.TrackName == "Ds19 Compound National")
            {
                track.Author = "Darkslides19";
            }
            else if(track.TrackName == "Ironman")
            {
                track.Author = "Garry664 & RDC";
            }

            if(track.Author == string.Empty)
            {
                throw new Exception(string.Format("Unable to detect author for track {0}", track.TrackName));
            }

            //DS doesn't record dates on his website...
            track.CreationTime = TimeUtility.DateTimeToUnixTimeStamp(DateTime.UtcNow);
            return track;
        }

        //Most of the urls cause a redirect, this gets us the final url that we are being redirected to.
        private string GetFinalUrl(string url)
        {
            HttpClient client = new HttpClient();
            var task = client.GetAsync(url);
            task.Wait();
            HttpResponseMessage response = task.Result;
            response.EnsureSuccessStatusCode();
            return response.RequestMessage.RequestUri.ToString();
        }

        public bool MakeRequest(string url)
        {
            using (var client = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip }))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get
                };

                HttpResponseMessage response = client.SendAsync(request).Result;
                var statusCode = (int)response.StatusCode;

                // We want to handle redirects ourselves so that we can determine the final redirect Location (via header)
                if (statusCode >= 300 && statusCode <= 399)
                {
                    var redirectUri = response.Headers.Location;
                    if (!redirectUri.IsAbsoluteUri)
                    {
                        redirectUri = new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority) + redirectUri);
                    }
                    return MakeRequest(redirectUri.ToString());
                }
                else if (!response.IsSuccessStatusCode)
                {
                    throw new Exception();
                }

                return true;
            }
        }
        private void LazyInitTrackNodes()
        {
            if (m_trackNodes == null)
            {
                string html = GetHtml("http://ds19.eu/tracklist.php");
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                m_trackNodes = doc.DocumentNode.SelectNodes("//*[@id='tracklist']/div/a");
            }
        }

        private HtmlNodeCollection m_trackNodes;
    }
}
