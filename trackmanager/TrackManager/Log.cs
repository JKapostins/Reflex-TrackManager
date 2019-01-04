using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Trackmanagement;

namespace TrackManager
{
    public static class Log
    {
        public static void Add(LogMessageType type, string message)
        {
            var date = DateTime.Now;
            string line = string.Format("[{0}] {1}", DateTime.Now.ToString("h:mm:ss tt"), message);
            if(type == LogMessageType.LogError)
            {
                Console.Error.WriteLine(line);
            }
            else
            {
                Console.WriteLine(line);
            }

            m_messages.Enqueue(new LogMessage { Type = type, Message = line });
        }

        public static LogMessage[] TryDequeueAll()
        {
            List<LogMessage> messages = new List<LogMessage>();
            while (m_messages.TryDequeue(out LogMessage message))
            {
                messages.Add(message);
            }
            return messages.ToArray();
        }

        private static ConcurrentQueue<LogMessage> m_messages = new ConcurrentQueue<LogMessage>();
    }
}
