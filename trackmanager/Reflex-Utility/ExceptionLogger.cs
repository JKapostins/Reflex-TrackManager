using System;

namespace ReflexUtility
{
    public class ExceptionLogger
    {
        public static void LogException(Exception e)
        {
            Console.Error.WriteLine(e.Message);
            Console.Error.WriteLine("Call Stack:");
            Console.Error.WriteLine(e.StackTrace);
            Console.WriteLine("");

            if (e.InnerException != null)
            {
                LogException(e.InnerException);
            }
        }
    }
}
