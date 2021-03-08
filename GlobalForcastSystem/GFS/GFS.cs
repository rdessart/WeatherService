#define OS_WIN_64
//#define OS_LIN_ARM
//#define OS_LIN_64

using GlobalForcastSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;



namespace GlobalForcastSystem
{
    public class GFS
    {
        public Dictionary<KeyValuePair<double, double>, GFSGrid> Grids { get; protected set; }
        public DateTime Time { get; set; }
        public DateTime ForcastedAt { get; set; }
        public DateTime ForcastDate { get; set; }
        public DateTime ObservationDate { get; set; }
        public string OutputDir { get; set; }
        public int ObservationHours { get; set; }
        public int ForecastHours { get; set; }
        public static DateTime GetLatestObservationHour()
        {
            DateTime utc = DateTime.UtcNow;
            int latestHoursRel = utc.Hour - (utc.Hour % 6);
            if ((utc.Hour * 60 + utc.Minute) - (latestHoursRel * 60) < 190)
            //NOAA need 3 hours to publish the GFS data
            {
                latestHoursRel -= 6;
            }
            return new DateTime(utc.Year, utc.Month, utc.Day, latestHoursRel, 0, 0); ;
        }

        public static string GenerateName(int forcastHours)
        {
            DateTime latestRelease = GetLatestObservationHour();
            return $"gfs.t{latestRelease.Hour:00}z.pgrb2full.0p50.f{forcastHours:000}";
        }
        public static string GetLatestGFSUrl(int forcastHours)
        {
            DateTime latestRelease = GetLatestObservationHour();
            string url = $"https://nomads.ncep.noaa.gov/pub/data/nccf/com/gfs/prod/gfs.{latestRelease.Year:0000}{latestRelease.Month:00}{latestRelease.Day:00}/{latestRelease.Hour:00}/{GenerateName(forcastHours)}";
            return url;
        }

        protected static string BuildFilename(int observationTime, int forcastHours)
        {
            return $"gfs.t{observationTime:00}z.pgrb2full.0p50.f{forcastHours:000}";
        }

        protected void ExtractGFSFileLatLon(double latitude, double longitude, int observationTime, int forcastHours)
        {
            string GfsFileName = BuildFilename(observationTime, forcastHours);
            string slat = Math.Round(latitude).ToString("00");
            string slon = Math.Round(longitude).ToString("000");
            ObservationDate = DateTime.Today.AddHours(observationTime);
            ForcastDate = ObservationDate.AddHours(forcastHours);

            DateTime t0 = DateTime.Today.AddHours(observationTime).AddHours(forcastHours);
#if OS_WIN_64
            string args = $"./resources/{GfsFileName} -s -lon {slat} {slon}";
            string filePath = string.Format(@".\resources\weatherData\gfs{0:0000}{1:00}{2:00}{3:00}-{4}-{5}.grid",t0.Year, t0.Month, t0.Day, t0.Hour, slat, slon);
            if (!Directory.Exists(@".\resources\weatherData\"))
            {
                Directory.CreateDirectory(@".\resources\weatherData\");
            }
#else
 string args = $"./resources/{GfsFileName} -s -lon {slat} {slon}";
            string filePath = string.Format(@"./resources/weatherData/gfs{0:0000}{1:00}{2:00}{3:00}-{4}-{5}.grid",t0.Year, t0.Month, t0.Day, t0.Hour, slat, slon);
            if (!Directory.Exists(@"./resources/weatherData/"))
            {
                Directory.CreateDirectory(@"./resources/WeatherData/");
            }
#endif
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            StreamWriter gfsStreamW = File.CreateText(filePath);

            Process extract = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    Arguments = args.ToString(),
#if OS_WIN_64
                    FileName = @".\Process\wgrib2_win_64.exe",
#elif OS_LIN_64
                    FileName = @"./Process/wgrib2_lin_64",
#elif OS_LIN_ARM
                    FileName = @"./Process/wgrib2_arm64",
#endif
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                }
            };
            extract.Start();
            gfsStreamW.Write(extract.StandardOutput.ReadToEnd());
            extract.WaitForExit();
            gfsStreamW.Flush();
            gfsStreamW.Close();
            ParseFile(filePath);
        }
        public void ParseFile(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }
            string content = File.ReadAllText(path);
            string[] lines = content.Split('\n');
            Console.WriteLine($"{path} >> They are {lines.Length} lines in the file");
            List<GFSLine> GFSLines = new List<GFSLine>();
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    GFSLines.Add(new GFSLine(line));
                }
            }
            var levels = GFSLines.Where(g => g.Unit != GFSLine.Units.UNKNOWN && g.Pressure > double.MinValue && g.IsPressureAltitude == false).OrderBy(g => g.Pressure).GroupBy(g => g.Pressure);
            List<GFSLayer> layers = new List<GFSLayer>();
            foreach (var level in levels)
            {
                layers.Add(new GFSLayer(level.Key, level.ToList()));
            }
            layers = layers.OrderByDescending(l => l.GeoAltitudeFt).ToList();
            GFSGrid gGrid = new GFSGrid()
            {
                Layers = layers,
                Latitude = GFSLines[0].Latitude,
                Longitude = GFSLines[0].Longitude
            };
            Grids.Add(new KeyValuePair<double, double>(gGrid.Latitude, gGrid.Longitude), gGrid);
        }

        public void DisplayGrids()
        {

            foreach (KeyValuePair<KeyValuePair<double, double>, GFSGrid> grid in Grids)
            {
                string text = JsonSerializer.Serialize<GFSGrid>(grid.Value);
                string fileName = $"GFS-{ForcastDate.Day:00}{ForcastDate.Month:00}{ForcastDate.Year:0000}_{ForcastDate.Hour:00}-{grid.Key.Key}-{grid.Key.Value}.json";
                string path = Path.Join(OutputDir, fileName);
                if(File.Exists(path))
                {
                    File.Delete(path);
                }
                File.WriteAllText(path, text);
            }
        }

        public GFS()
        {
            Grids = new Dictionary<KeyValuePair<double, double>, GFSGrid>();
        }

        public async Task<bool> GetData(int observation, int forcast)
        {
            if(!Directory.Exists(OutputDir))
            {
                Directory.CreateDirectory(OutputDir);
            }

            List<List<double>> RequestedGrid = new List<List<double>>()
            {
               new List<double>(){ 52.0, 2.0 },
               new List<double>(){ 52.0, 3.0 },
               new List<double>(){ 52.0, 4.0 },
               new List<double>(){ 52.0, 5.0 },
               new List<double>(){ 52.0, 6.0 },
               new List<double>(){ 52.0, 7.0 },
               new List<double>(){ 51.0, 2.0 },
               new List<double>(){ 51.0, 3.0 },
               new List<double>(){ 51.0, 4.0 },
               new List<double>(){ 51.0, 5.0 },
               new List<double>(){ 51.0, 6.0 },
               new List<double>(){ 51.0, 7.0 },
               new List<double>(){ 50.0, 2.0 },
               new List<double>(){ 50.0, 3.0 },
               new List<double>(){ 50.0, 4.0 },
               new List<double>(){ 50.0, 5.0 },
               new List<double>(){ 50.0, 6.0 },
               new List<double>(){ 50.0, 7.0 },
               new List<double>(){ 49.0, 2.0 },
               new List<double>(){ 49.0, 3.0 },
               new List<double>(){ 49.0, 4.0 },
               new List<double>(){ 49.0, 5.0 },
               new List<double>(){ 49.0, 6.0 },
               new List<double>(){ 49.0, 7.0 },
            };
            List<Task> ExtractTask = new List<Task>();
            foreach (List<double> latlon in RequestedGrid)
            {
                object arg = latlon;
                var task = new TaskFactory().StartNew((test) => this.ExtractGFSFileLatLon((test as List<double>)[0], (test as List<double>)[1], observation, forcast), arg);
                ExtractTask.Add(task);
            }
            await Task.WhenAll(ExtractTask.ToArray());
            return true;
        }
    }
}
