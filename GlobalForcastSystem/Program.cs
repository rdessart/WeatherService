using System;
using System.IO;
using System.Threading.Tasks;
namespace GlobalForcastSystem
{
    class Program
    {
        static async Task<bool> DownloadAndParseGFS(int observationHour, int forcastHour)
        {
            string url = GFS.GetLatestGFSUrl(forcastHour);
            Console.WriteLine(url);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            string OutPath = $"./resources/{GFS.GenerateName(forcastHour)}";
            if (File.Exists(OutPath))
            {
                Console.WriteLine("File has already been downloaded");
            }
            else
            {
                Downloads download = new Downloads(url)
                {
                    OutFileName = $"./resources/{GFS.GenerateName(forcastHour)}",
                };
                if (await download.DoDownload())
                {
                    watch.Stop();
                    Console.WriteLine($"Download and merge done in {watch.ElapsedMilliseconds} ms");
                }
                else
                {
                    Console.WriteLine($"Downloading of {OutPath} has failed. Skipping");
                }
            }
            return true;
        }
        static async Task  Main(string[] args)
        {
            Console.WriteLine("GFS PARSER 1.0.0.0");
            DateTime latestObs = GFS.GetLatestObservationHour();
            for (int i = 0; i < 210; i += 3)
            {
                if (await DownloadAndParseGFS(latestObs.Hour, i))
                {
                    DateTime fc = latestObs.AddHours(i);
                    GFS gfs = new GFS() { OutputDir = "Output/" };
                    bool res = await gfs.GetData(observation: latestObs.Hour, forcast: i);
                    gfs.DisplayGrids();
                    Console.WriteLine($"Forcast for {fc.Year:0000}/{fc.Month:00}/{fc.Day:00} at {fc.Hour:00}Z done !");
                }
            }
            string[] files = Directory.GetFiles("./resources/weatherData/");
            foreach(string file in files)
            {
                File.Delete(file);
            }
        }
    }
}
