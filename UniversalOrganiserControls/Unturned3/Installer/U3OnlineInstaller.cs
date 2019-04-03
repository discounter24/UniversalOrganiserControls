using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace UniversalOrganiserControls.Unturned3.Installer
{
    public class U3OnlineInstaller
    {
        private string baseUrl;
        private string indexUrl;
        private DirectoryInfo installDir;
        private List<IndexItem> items;


        public Int64 lastUpdate
        {
            get
            {
                try
                {
                    return Convert.ToInt64(File.ReadAllText(installDir.FullName + "\\lastUpdate.time"));
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            set
            {
                try
                {
                    File.WriteAllText(installDir.FullName + "\\lastUpdate.time", value.ToString());
                }
                catch (Exception)  { }
            }
        }

        public U3OnlineInstaller(DirectoryInfo installDir)
        {
            this.baseUrl = "http://dl.unturned-server-organiser.com/unturned/";
            this.indexUrl = "http://dl.unturned-server-organiser.com/unturned/getIndex.php?lastUpdate={0}";
            this.installDir = installDir;
            this.items = new List<IndexItem>();
        }

        public Task<U3InstallationResult> update()
        {
            return Task.Run(() =>
            {
                string json = new WebClient().DownloadString(string.Format(indexUrl,lastUpdate));
                JObject index = JObject.Parse(json);
                foreach(var item in index)
                {
                    string file = item.Key;
                    string hash = (string)item.Value["hash"];
                    bool isFolder = (bool)item.Value["isFolder"];
                    long lastModified = (long)item.Value["lastModified"];

                    IndexItem i = new IndexItem(file, hash, isFolder, lastModified);
                    items.Add(i);
                }

                int maxCount = 500;
                SemaphoreSlim maxTasks = new SemaphoreSlim(maxCount);
                int executed = 0;

                foreach(var item in items)
                {
                    string filename = installDir.FullName + "\\" + item.name.Substring(5);
                    if (item.isFolder)
                    {
                        if (!Directory.Exists(filename))
                        {
                            Directory.CreateDirectory(filename);
                            executed++;
                            Console.Clear();
                            Console.WriteLine(string.Format("{0}/{1}", executed, items.Count));
                        }
                    }
                    else
                    {
                        maxTasks.Wait();
                        Task.Run(() =>
                        {
                            WebClient client = new WebClient();
                            string dl = this.baseUrl + item.name;
                            client.DownloadFile(dl, filename);

                            executed++;
                            Console.Clear();
                            Console.WriteLine(string.Format("{0}/{1}",executed,items.Count));
                           
                            maxTasks.Release();
                        });
                    }
                }


                Task.Run(async () =>
                {
                    while(maxTasks.CurrentCount != maxCount)
                    {
                        await Task.Delay(500);
                    }

                }).Wait();

                Console.WriteLine("Downloaded all!");
                return U3InstallationResult.OK;
            });
        }


      

        public struct IndexItem
        {
            public string name, hash;
            public long lastModified;
            public bool isFolder;

            public IndexItem(string name, string hash, bool isFolder, long lastModified)
               {
                this.name = name;
                this.hash = hash;
                this.isFolder = isFolder;
                this.lastModified = lastModified;
            }
        }


    }
}
