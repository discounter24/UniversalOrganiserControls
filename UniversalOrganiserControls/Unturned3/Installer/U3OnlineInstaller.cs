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

        public event EventHandler<U3OnlineInstallationProgressArgs> InstallationProgressChanged;

        private string baseUrl;
        private string indexUrl;
        private DirectoryInfo installDir;
        private List<IndexItem> items;

        public int UpdateInterval { get; set; }
        public int DegreeOfParallelism { get; private set; }
        public bool Resinstall { get; set; }
        public bool Validate { get; set; }

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
                catch (Exception) { }
            }
        }

        public U3OnlineInstaller(DirectoryInfo installDir, int degreeOfParallelism = 500)
        {
            this.baseUrl = "http://dl.unturned-server-organiser.com/unturned/";
            this.indexUrl = "http://dl.unturned-server-organiser.com/unturned/getIndex.php?lastUpdate={0}";
            this.installDir = installDir;
            this.items = new List<IndexItem>();
            this.Resinstall = false;
            this.Validate = false;
            this.DegreeOfParallelism = degreeOfParallelism;
            this.UpdateInterval = 100;
        }

        public Task<U3InstallationState> update()
        {
            return Task.Run(() =>
            {
                bool busy = false;
                try
                {
                    busy = Convert.ToBoolean(new WebClient().DownloadString(baseUrl + "/busy.php"));
                }
                catch (Exception) { }

                while (busy)
                {
                    InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.PausedServerBusy, 0, 0));
                    Task.Delay(5000).Wait();

                    try
                    {
                        busy = Convert.ToBoolean(new WebClient().DownloadString(baseUrl + "/busy.php"));
                    }
                    catch (Exception) { }
                }

                string json = null;
                try
                {
                    InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.SearchingUpdates, 0, 0));
                    long time = (Resinstall || Validate) ? 0 : lastUpdate;
                    json = new WebClient().DownloadString(string.Format(indexUrl, time));
                }
                catch (Exception)
                {
                    InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.FailedInternet, 0, 0));
                    return U3InstallationState.FailedInternet;
                }

                if (string.IsNullOrEmpty(json) || json.Equals("[]"))
                {
                    InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.Ok, 0, 0));
                    return U3InstallationState.Ok;
                }
                else
                {
                    try
                    {
                        JObject index = JObject.Parse(json);
                        foreach (var item in index)
                        {
                            string file = item.Key;
                            string hash = (string)item.Value["hash"];
                            bool isFolder = (bool)item.Value["isFolder"];
                            long lastModified = (long)item.Value["lastModified"];

                            IndexItem i = new IndexItem(file, hash, isFolder, lastModified);
                            items.Add(i);
                        }
                    }
                    catch (Exception)
                    {
                        InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.FailedInvalidResponse, 0, 0));
                        return U3InstallationState.FailedInvalidResponse;
                    }




                    SemaphoreSlim taskBarrier = new SemaphoreSlim(DegreeOfParallelism);
                    int executed = 0;
                    int total = items.Count;
                    bool error = false;

                    Task waitHandle = Task.Run(async () =>
                    {
                        while (taskBarrier.CurrentCount != DegreeOfParallelism)
                        {
                            InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.Downloading, executed, total));
                            await Task.Delay(UpdateInterval);
                        }
                    });

                    foreach (var item in items)
                    {
                        string filename = installDir.FullName + "\\" + item.name.Substring(5);


                        if (item.isFolder)
                        {
                            try
                            {
                                if (!Directory.Exists(filename))
                                {
                                    Directory.CreateDirectory(filename);
                                    executed++;
                                }
                            }
                            catch (Exception)
                            {
                                error = true;
                            }
                        }
                        else
                        {
                            taskBarrier.Wait();
                            Task.Run(() =>
                            {
                                string dl = this.baseUrl + item.name;
                                try
                                {
                                    FileInfo info = new FileInfo(filename);
                                    if (!info.Directory.Exists) info.Directory.Create();

                                    if (Validate && File.Exists(filename))
                                    {
                                        string originMD5 = item.hash;
                                        string fileMD5 = UtilsGeneral.GetMD5HashOfFile(filename);
                                        if (!fileMD5.Equals(originMD5))
                                        {
                                            WebClient client = new WebClient();
                                            client.DownloadFile(dl, filename);
                                        }
                                        executed++;
                                    }
                                    else
                                    {
                                        WebClient client = new WebClient();
                                        client.DownloadFile(dl, filename);
                                        executed++;
                                    }

                              
                                }
                                catch (Exception ex)
                                {
                                    error = true;
                                }
                                taskBarrier.Release();
                            });
                        }
                    }


                    waitHandle.Wait();

                    lastUpdate = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    if (error)
                    {
                        InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.FailedSome, executed, total));
                        return U3InstallationState.FailedSome;
                    }
                    else
                    {
                        InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.Ok, executed, total));
                        return U3InstallationState.Ok;
                    }
                }


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
