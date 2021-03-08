using System;
using System.Threading.Tasks;
namespace GlobalForcastSystem
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("GFS PARSER 1.0.0.0");
            string url = GFS.GetLatestGFSUrl(6);
            Console.WriteLine(url);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Downloads download = new Downloads(url)
            {
                OutFileName = $"./resources/{GFS.GenerateName(6)}",
            };
            bool res = await download.DoDownload();
            Console.WriteLine(res);
            watch.Stop();
            Console.WriteLine($"Download and merge done in {watch.ElapsedMilliseconds} ms");
            GFS gfs = new GFS();
            res = await gfs.GetData();
            gfs.DisplayGrids();
        }
    }
}
