using System;
using System.Collections.Generic;
using System.Text;

namespace TrackManager
{
    public class ExceptionLogger
    {
        public static void LogException(Exception e)
        {
            Console.Error.WriteLine(e.Message);
            if (e.InnerException != null)
            {
                LogException(e.InnerException);
            }
        }
    }
}
