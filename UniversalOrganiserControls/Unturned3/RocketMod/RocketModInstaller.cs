using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;

namespace UniversalOrganiserControls.Unturned3.RocketMod
{
    public delegate void InstallationCompleted(object sender, RocketModInstallationCompletedType e);

    public enum RocketModInstallationCompletedType
    {
        Success,
        FailedCanceledByUser,
        FailedNoIOPermissions,
        FailedNoInternetAccessOrNoIOPermissions,
        FailedInvalidModVersion,
        FailedUnknownException
    }

    public class RocketModInstaller
    {

        public event InstallationCompleted RocketInstallationCompleted;


        private static Uri downloadAddr = new Uri("https://ci.rocketmod.net/job/Rocket.Unturned/lastSuccessfulBuild/artifact/Rocket.Unturned/bin/Release/Rocket.zip");
        private static Uri versionAddr = new Uri("http://rocketmod.unturned-server-organiser.com/version.php");


        private DirectoryInfo gamedir;


        public async Task<string> getServerVersion()
        {

            var task = Task.Run(() =>
            {
                string result;
                try
                {
                    result = new WebClient().DownloadString(versionAddr);
                }
                catch (Exception)
                {
                    result = "(not available)";
                }
                return result;
            });

            return await task;
        }

        public string LocalVersion
        {
            get
            {
                try
                {
                    return File.ReadAllText(gamedir.FullName + "\\rocketversion.uso");
                }
                catch (Exception)
                {
                    return "(not installed)";
                }
            }
            set
            {
                try
                {
                    File.WriteAllText(gamedir.FullName + "\\rocketversion.uso",value);
                }
                catch (Exception) { }
            }
  
        }



        public async Task<bool> isUpdateAvailable()
        {
            var task = Task.Run(() =>
            {
                bool result = !LocalVersion.Equals(getServerVersion().Result);
                return result;
            });

            return await task;
        }


        public RocketModInstaller(DirectoryInfo gamedir)
        {
            this.gamedir = gamedir;
        }


        public async Task<RocketModInstallationCompletedType> install(bool clean = false)
        {
            var task = Task.Run(() =>
            {
                try
                {
                    bool updateAvailable = isUpdateAvailable().Result;
                    if (clean | updateAvailable)
                    {
                        string v = getServerVersion().Result;
                        try
                        {
                            string tmpFile = gamedir.FullName + "\\rocketmod.tmp";

                            ManualResetEvent waitInstallCompleted = new ManualResetEvent(false);

                            bool failed = false;
                            UniversalWebClient client = new UniversalWebClient();
                            client.DownloadFileCompleted += (sender, e) =>
                            {
                                try
                                {
                                    var info = new FileInfo(tmpFile);
                                    if (info.Length > 0)
                                    {
                                        extract(info, gamedir);
                                        LocalVersion = v;
                                    }
                                    else
                                    {
                                        failed = true;
                                    }
                                }
                                catch (Exception)
                                {
                                    failed = true;
                                }

                                waitInstallCompleted.Set();
                            };


                            if (File.Exists(tmpFile)) File.Delete(tmpFile);
                            client.DownloadFileAsync(downloadAddr, tmpFile);


                            waitInstallCompleted.WaitOne();

                            if (failed)
                            {
                                RocketInstallationCompleted?.Invoke(this, RocketModInstallationCompletedType.FailedUnknownException);
                                return RocketModInstallationCompletedType.FailedUnknownException;
                            }
                            else
                            {
                                RocketInstallationCompleted?.Invoke(this, RocketModInstallationCompletedType.Success);
                                return RocketModInstallationCompletedType.Success;
                            }
                        }
                        catch (Exception)
                        {
                            RocketInstallationCompleted?.Invoke(this, RocketModInstallationCompletedType.FailedNoInternetAccessOrNoIOPermissions);
                            return RocketModInstallationCompletedType.FailedNoInternetAccessOrNoIOPermissions;
                        }
                    }
                    else
                    {
                        RocketInstallationCompleted?.Invoke(this, RocketModInstallationCompletedType.Success);
                        return RocketModInstallationCompletedType.Success;
                    }

                }
                catch (Exception ex)
                {
                    RocketInstallationCompleted?.Invoke(this, RocketModInstallationCompletedType.FailedUnknownException);
                    return RocketModInstallationCompletedType.FailedUnknownException;
                }
            });

            return await task;
        }


        private void extract(FileInfo file, DirectoryInfo dest)
        {
            if (!dest.Exists) dest.Create();
            
            foreach (var Entry in ZipFile.OpenRead(file.FullName).Entries)
            {
                try
                {
                    string dir = Path.GetFullPath(Path.Combine(dest.FullName, Entry.FullName));

                    if (!Directory.Exists(Path.GetDirectoryName(dir)))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    if (Entry.Name != "")
                    {
                        Entry.ExtractToFile((dir), true);
                    }
                }
                catch (Exception) { }
            }
        }



    }
}
