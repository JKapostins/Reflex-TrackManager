using System.Diagnostics;

namespace TrackManagerUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            //kill the track manager if its running

            //download the update

            //re-launch the process
            Process process = new Process();
            Process.Start("TrackManager.exe");
        }
    }
}
