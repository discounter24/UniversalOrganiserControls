using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Unturned3.Installer
{
    public class U3OnlineInstaller
    {
        /// <summary>
        /// Triggers when the installation makes progress
        /// </summary>
        public event EventHandler<U3OnlineInstallationProgressArgs> InstallationProgressChanged;

        /// <summary>
        /// Callback event for asking to continue the update. If not used the update will automatically continue
        /// </summary>
        public event EventHandler<U3OnlineInstallerAskForUserToAcceptUpdate> AskForAcceptUpdate;



        private string baseUrl;
        private string indexUrl;
        private DirectoryInfo installDir;
        private List<IndexItem> items;




        /// <summary>
        /// Sets the update interval of the progress changed event
        /// </summary>
        public int UpdateInterval { get; set; }

        /// <summary>
        /// The amount of tasks running in parallel for downloading the game files.
        /// </summary>
        public int DegreeOfParallelism { get; private set; }

        /// <summary>
        /// Gets or sets if the installer will clear old files and redownload all of them.
        /// </summary>
        public bool FreshInstall { get; set; }

        /// <summary>
        /// Gets or sets if the installer will keep server files when the installer does a fresh install
        /// </summary>
        public bool KeepServersOnFreshInstall { get; set; }

        /// <summary>
        /// Gets or sets if the files should be validated with MD5 hashes on updates.
        /// </summary>
        public bool Validate { get; set; }

        /// <summary>
        /// True when the installer received an abort call
        /// </summary>
        public bool Aborted {  get; private set; }

        /// <summary>
        /// Unix timestamp of when the last update where done
        /// </summary>
        public Int64 LastUpdate
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


        /// <summary>
        /// Creates a new installer for Unturned 3
        /// </summary>
        /// <param name="installDir">The directory to which the game should be installed</param>
        /// <param name="degreeOfParallelism"></param>
        public U3OnlineInstaller(DirectoryInfo installDir, int degreeOfParallelism = 500)
        {
            this.baseUrl = "http://dl.unturned-server-organiser.com/unturned/";
            this.indexUrl = "http://dl.unturned-server-organiser.com/unturned/getIndex.php?lastUpdate={0}";
            this.installDir = installDir;
            this.items = new List<IndexItem>();
            this.FreshInstall = false;
            this.KeepServersOnFreshInstall = true;
            this.Validate = false;
            this.DegreeOfParallelism = degreeOfParallelism;
            this.UpdateInterval = 100;
           
        }

        /// <summary>
        /// Installs or updates Unturned 3 game files.
        /// </summary>
        /// <returns></returns>
        public Task<U3InstallationState> update()
        {
           
            return Task.Run(() =>
            {
                this.Aborted = false;
                bool busy = false;
                try
                {
                    busy = Convert.ToBoolean(new WebClient().DownloadString(baseUrl + "/busy.php"));
                }
                catch (Exception) { }

                while (busy  && !Aborted)
                {
                    InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.PausedServerBusy));
                    Task.Delay(5000).Wait();
                    
                    try
                    {
                        busy = Convert.ToBoolean(new WebClient().DownloadString(baseUrl + "/busy.php"));
                    }
                    catch (Exception) { }
                }

                if (Aborted)
                {
                    InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.AbortedByCall));
                    return U3InstallationState.AbortedByCall;
                }

                string json = null;
                try
                {
                    InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.SearchingUpdates));
                    long time = (FreshInstall || Validate) ? 0 : LastUpdate;
                    json = new WebClient().DownloadString(string.Format(indexUrl, time));
                }
                catch (Exception)
                {
                    InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.FailedInternet));
                    return U3InstallationState.FailedInternet;
                }

                if (string.IsNullOrEmpty(json) || json.Equals("[]"))
                {
                    InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.Ok));
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
                    int errors = 0;
                    int total = items.Count;

                    if (total > 0)
                    {
                        U3OnlineInstallerAskForUserToAcceptUpdate args = new U3OnlineInstallerAskForUserToAcceptUpdate();
                        AskForAcceptUpdate?.Invoke(this, args);

                        if (args.cancel)
                        {
                            InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.Ok, executed, total));
                            return U3InstallationState.Ok;
                        }
                    }

                    Task waitHandle = Task.Run(async () =>
                    {
                        while (taskBarrier.CurrentCount != DegreeOfParallelism || executed != total)
                        {
                            InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.Downloading, executed, total));
                            await Task.Delay(UpdateInterval);
                        }
                    });

                    if (FreshInstall)
                    {
                        InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.DeletingOldFiles));
                        deleteOldFiles();
                    }


                    foreach (var item in items)
                    {
                        if (Aborted)
                        {
                            break;
                        }

                        string filename = installDir.FullName + "\\" + item.name.Substring(5);


                        if (item.isFolder)
                        {
                            try
                            {
                                if (!Directory.Exists(filename))
                                {
                                    Directory.CreateDirectory(filename);
                                }
                            }
                            catch (Exception)
                            {
                                errors++;
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
                                    }
                                    else
                                    {
                                        WebClient client = new WebClient();
                                        client.DownloadFile(dl, filename);
                                    }

                              
                                }
                                catch (Exception)
                                {
                                    errors++;
                                }
                                taskBarrier.Release();
                            });
                        }
                        executed++;

                    }


                    waitHandle.Wait();

                    if (Aborted)
                    {
                        InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.AbortedByCall, executed, total, errors));
                        return U3InstallationState.AbortedByCall;
                    }

                    LastUpdate = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    if (errors>0)
                    {
                        InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.FailedSome, executed, total,errors));
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

        /// <summary>
        /// Deletes old game files.
        /// </summary>
        public void deleteOldFiles()
        {
            try
            {
                foreach(FileInfo file in installDir.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception)  {  }
                }

                Parallel.ForEach(installDir.GetDirectories(), (dir) =>
                 {
                     if (dir.Name.Equals("Servers") && KeepServersOnFreshInstall)
                     {
                        //Skip
                     }
                     else
                     {
                         try
                         {
                             dir.Delete(true);
                             InstallationProgressChanged?.Invoke(this, new U3OnlineInstallationProgressArgs(U3InstallationState.DeletingOldFiles));
                         }
                         catch (Exception) { }
                     }

                 });
            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// Aborts the installation process.
        /// </summary>
        public void abort() { Aborted = true; }



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
