using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Linq;
using System.Diagnostics;
using System.Globalization;

namespace ReflexUtility
{
    public class ReflexCentralParser : HtmlParser
    {
        public ReflexCentralParser()
        {
        }

        public override Track ParseTrack(string url)
        {
            var profileHtml = GetHtml(url);

            var reflexProfileDoc = new HtmlDocument();
            reflexProfileDoc.LoadHtml(profileHtml);

            var trackNameNode = reflexProfileDoc.DocumentNode.SelectNodes("//*[@id='maincontent']/font[1]/b")?.SingleOrDefault();

            if(trackNameNode == null)
            {
                return null;
            }

            var trackName = trackNameNode.InnerHtml.Trim();
            var imageNode = reflexProfileDoc.DocumentNode.SelectNodes("//*[@id='track_preview_frame']/img")?.SingleOrDefault();
            var imageUrl = imageNode != null ? string.Format("http://reflex-central.com/{0}", imageNode.Attributes["src"].Value) : string.Empty;
            var authorNode = reflexProfileDoc.DocumentNode.SelectNodes("//*[@id='maincontent']/font[3]/b/a")?.SingleOrDefault();
            var author = authorNode != null ? authorNode.InnerHtml.Trim() : string.Empty;
            var dateUploadedNode = reflexProfileDoc.DocumentNode.SelectNodes("//*[@id='maincontent']/font[6]")?.SingleOrDefault();
            var dateUploadedExpanded = dateUploadedNode != null ? dateUploadedNode.InnerHtml.Trim() : string.Empty;

            long uploadTimestamp = 0;
            if (dateUploadedExpanded != null)
            {
                var splitDateTime = dateUploadedExpanded.Split(" - ");
                if (splitDateTime.Length == 2)
                {
                    var date = splitDateTime[0].Trim();
                    int firstComma = date.IndexOf(',', StringComparison.Ordinal);
                    date = date.Remove(0, firstComma + 1).Trim().Replace(",", string.Empty);
                    var splitDate = date.Split(' ');

                    string[] monthNames = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
                    int month = Array.IndexOf(monthNames, splitDate[0]) + 1;
                    int day = Convert.ToInt32(splitDate[1]);
                    int year = Convert.ToInt32(splitDate[2]);

                    var time = splitDateTime[1].Trim();
                    var splitTime = time.Split(' ');
                    var hoursMin = splitTime[0].Trim().Split(':');

                    int hours = Convert.ToInt32(hoursMin[0]);
                    int min = Convert.ToInt32(hoursMin[1]);

                    var amPm = splitTime[1].Trim();
                    if (amPm == "pm" && hours > 12)
                    {
                        hours += 12;
                    }
                    else if (amPm == "am" && hours == 12)
                    {
                        hours = 0;
                    }

                    DateTime dateTime = new DateTime(year, month, day, hours, min, 0, DateTimeKind.Utc);
                    uploadTimestamp = (long)(dateTime.Subtract(new DateTime(1970, 1, 1,0,0,0,DateTimeKind.Utc))).TotalSeconds;
                }
            }

            //The data files have various formats, this addresses all of the current ones.
            var sourceUrl = string.Empty;
            string[] variations = new string[]
            {
                string.Format("http://reflex-central.com/tracks/{0}", trackName), // trimmed track name
                string.Format("http://reflex-central.com/tracks/{0}", trackNameNode.InnerHtml), // untrimmed track name
                string.Format("http://reflex-central.com/tracks/ {0}", trackName), // trimmed track name with space in front
                string.Format("http://reflex-central.com/tracks/ {0}", trackNameNode.InnerHtml), // untrimmedtrimmed track name with space in front
                string.Format("http://reflex-central.com/tracks/{0} ", trackName), // trimmed track name with space after
                string.Format("http://reflex-central.com/tracks/{0} ", trackNameNode.InnerHtml) // untrimmedtrimmed track name with space after
            };

            string[] fileExtensions = new string[]
            {
                ".zip",
                ".rar"
            };

            foreach(var downloadPath in variations)
            {
                foreach(var ext in fileExtensions)
                {
                    if(FileExistsOnServer(downloadPath + ext))
                    {
                        sourceUrl = downloadPath + ext;
                        break;
                    }
                }

                if(sourceUrl.Length > 0)
                {
                    break;
                }
            }

            if(sourceUrl == string.Empty)
            {
                throw new Exception(string.Format("Unable to find data for {0} at {1}", trackName, url));
            }

            Track track = new Track
            {
                TrackName = trackName,
                SourceTrackUrl = sourceUrl,
                SourceThumbnailUrl = imageUrl,
                Author = author,
                CreationTime = uploadTimestamp
            };

            return track;
        }

        public override Track[] ParseTracks()
        {
            List<Track> tracks = new List<Track>();

            int invalidTrackCount = 0;
            const int InvalidThreshold = 100;
            for(int i = 1; ; ++i)
            {
                //run through all track id's until we hit one without a track name. We assume if there is no track names, the are no more tracks to parse
                var profileUrl = string.Format("http://reflex-central.com/track_profile.php?track_id={0}", i);

                var track = ParseTrack(profileUrl);
                if (track == null)
                {
                    ++invalidTrackCount;
                    //There are some false positives in the id list. To avoid getting stopped before we really got all the tracks,
                    //we look for multiple invalid tracks in a row.
                    if (invalidTrackCount > InvalidThreshold)
                    {
                        break;
                    }
                    continue; // don't add the non existant track to the list
                }

                invalidTrackCount = 0;
                tracks.Add(track);

            }
            return tracks.ToArray();
        }
    }
}
