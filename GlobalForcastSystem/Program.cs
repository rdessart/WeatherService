using System;
using System.Threading.Tasks;
namespace GlobalForcastSystem
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("GFS PARSER 1.0.0.0");
            Console.WriteLine(GFS.GetLatestGFSUrl());
            GFS gfs = new GFS();
            bool res = await gfs.GetData();
            Console.WriteLine(res);
            gfs.DisplayGrids();
        }
    }
}
