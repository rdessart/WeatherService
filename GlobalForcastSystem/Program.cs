using System;
using System.Threading.Tasks;
namespace GlobalForcastSystem
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(GFS.GetLatestGFSUrl());
            //Console.WriteLine("GFS PARSER 1.0.0.0");
            //GFS gfs = new GFS();
            //bool res = await gfs.GetData();
            //gfs.DisplayGrids();
            //Console.WriteLine(res);
        }

        
    }
}
