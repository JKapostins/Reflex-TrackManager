using System;

namespace ReflexUtility
{
    public static class TimeUtility
    {
        public static string UnixTimeStampToString(long unixTimeStamp)
        {
            var dtDateTime = UnixTimeStampToDateTime(unixTimeStamp);
            return dtDateTime.ToString("yyyy-MM-dd");
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dtDateTime.AddSeconds(unixTimeStamp);
        }

        public static long DateTimeToUnixTimeStamp(DateTime dateTime)
        {
            return (long)(dateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc))).TotalSeconds;
        }

        public static bool Expired(long unixTimeStamp, int expirationOffsetInMinutes)
        {
            long currentTime = DateTimeToUnixTimeStamp(DateTime.UtcNow);
            return currentTime > (unixTimeStamp + (60 * expirationOffsetInMinutes));
        }

        public static string TimeToExpire(long unixTimeStamp, int expirationOffsetInMinutes)
        {
            long expirationTime = unixTimeStamp + (60 * expirationOffsetInMinutes);
            var endTime = UnixTimeStampToDateTime(expirationTime);
            TimeSpan ts = endTime.Subtract(DateTime.UtcNow);
            return string.Format("{0}:{1:D2}", ts.Minutes, ts.Seconds, ts.Milliseconds);
        }
    }
}
