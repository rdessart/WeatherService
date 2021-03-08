using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalForcastSystem
{
    public class DownloadInfo
    {
        public string Url { get; set; }
        public long Start{ get; set; }
        public long Size { get; set; }
        public int Id { get; set; }
    }
    public class Downloads
    {
        public string OutFileName { get; set; }
        protected string pathTemp;
        public string Url { get; set; }
        private HttpClient client;
        private const long DowloadChunkSize = 5 * (1024 * 1024); // 20 Mib
        //private const long DowloadChunkSize = 10 * 1000; // 10 Kib
        public Downloads()
        {
            Url = "";
            pathTemp = "./";
            client = new HttpClient();
        }
        public Downloads(string url) : this()
        {
            Url = url;
        }
        public async Task<bool> DoDownload()
        {
            HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, Url));
            if(response.IsSuccessStatusCode)
            {
                if (response.Content.Headers.ContentLength.HasValue)
                {
                    long fileSize = response.Content.Headers.ContentLength.Value;
                    int split = (int)(fileSize / DowloadChunkSize);
                    long rem = fileSize - (split * DowloadChunkSize);

                    Console.WriteLine($"File is {fileSize} bytes");
                    Console.WriteLine($"Download will be performe in {split} subdownload of {DowloadChunkSize} bytes");
                    if (rem > 0)
                    {
                        Console.WriteLine($"And one download of {rem} bytes");
                    }
                    List<Task> downloadTask = new List<Task>();
                    for (int downId = 0; downId < split; downId++)
                    {
                        DownloadInfo dinfo = new DownloadInfo()
                        {
                            Url = Url,
                            Start = downId * DowloadChunkSize,
                            Size = DowloadChunkSize,
                            Id = downId
                        };
                        downloadTask.Add(DownloadFile(dinfo));
                    }
                    if (rem > 0)
                    {
                        DownloadInfo dinfo = new DownloadInfo()
                        {
                            Url = Url,
                            Start = split * DowloadChunkSize,
                            Size = DowloadChunkSize,
                            Id = split
                        };
                        downloadTask.Add(DownloadFile(dinfo));
                    }
                    await Task.WhenAll(downloadTask.ToArray());
                    if (File.Exists(OutFileName))
                    {
                        File.Delete(OutFileName);
                    }
                    FileStream f = File.OpenWrite(OutFileName);
                    for (int downId = 0; downId <= split; downId++)
                    {
                        byte[] data = File.ReadAllBytes($@"./down_{downId}.tmp");
                        f.Write(data, 0, data.Count());
                        f.Flush();
                        Console.WriteLine($"{downId} added to master file");
                        File.Delete($@"./down_{downId}.tmp");                  
                    }
                    f.Close();
                    return true;
                }
            }
            return false;
        }

        public async Task DownloadFile(DownloadInfo info)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Range = new RangeHeaderValue(info.Start, ((info.Id + 1) * info.Size) - 1);
            using (var stream = await client.GetStreamAsync(info.Url))
            using (var output = File.Create($@"./down_{info.Id}.tmp"))
            {
                await stream.CopyToAsync(output);
            }
            Console.WriteLine($"{info.Id} : {info.Start} -> {((info.Id + 1) * info.Size) - 1} : Downloaded !");

        }
    }
}
