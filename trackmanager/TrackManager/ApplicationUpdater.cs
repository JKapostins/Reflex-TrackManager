using System;
using System.Collections.Generic;
using System.Text;
using TrackManagement;
using ReflexUtility;
using System.Linq;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace TrackManager
{
    class ApplicationUpdater
    {
        public bool IsActiveVersion()
        {
            bool active = false;
            var version = HttpUtility.Get<ReflexManagerVersion[]>("https://spptqssmj8.execute-api.us-east-1.amazonaws.com/test/version");

            m_activeVersion = version.Where(v => v.Active == true).Select(v => v.Version).FirstOrDefault();

            active = m_activeVersion == Version;
            if (active == false)
            {
                Console.WriteLine(string.Format("Update requred: (v{0})", m_activeVersion));
            }

            return active;
        }

        public void DownloadAndLaunchUpdater()
        {
            string updaterExe = "TrackManagerUpdater.exe";
            using (var client = new WebClient())
            {
                string updater = string.Format("https://s3.amazonaws.com/reflextrackmanager/{0}/{1}", m_activeVersion, updaterExe);
                client.DownloadFile(updater, Path.GetFileName(updater));
            }

            ProcessStartInfo info = new ProcessStartInfo();
            info.UseShellExecute = true;
            info.CreateNoWindow = false;
            info.WindowStyle = ProcessWindowStyle.Normal;
            info.FileName = updaterExe;
            info.Arguments = m_activeVersion;
            Process.Start(info);
        }

        private string m_activeVersion;
        private const string Version = "0.01";
    }
}
