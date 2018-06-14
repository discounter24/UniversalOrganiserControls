using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace UniversalOrganiserControls.Unturned3.RocketMod.Plugin
{
    public class RocketPluginInstaller
    {
        public event RocketPluginDownloaded Downloaded;
        public event RocketPluginInstallationCompleted InstallationCompleted;

        private PluginManager manager;


        public RocketPluginInstaller(RocketPluginManager manager)
        {
            this.manager = manager;
        }

        private List<FileInfo> dllFiles = new List<FileInfo>();

        public void install(Uri download, Uri website)
        {

            new Thread(() =>
            {
                dllFiles = new List<FileInfo>();

                UniversalWebClient downloader = new UniversalWebClient();
                string tmpFile = manager.PluginFolder + "\\plugin.tmp";
                downloader.DownloadFile(download, tmpFile);
                Downloaded?.Invoke();

                ZipFile archive = new ZipFile(tmpFile);

                if (archive.TestArchive(true))
                {
                    try
                    {
                        RocketPluginInfo info = new PluginInfo("", manager.PluginFolder.FullName, "", "", new List<string>());

                        extractZip(archive, info);
                        info.Website = website.ToString();
                        info.ClientVersion = PluginInfo.getServerVersion(info.Website);

                        foreach (FileInfo dll in dllFiles)
                        {
                            string dll_name = dll.Name.Remove(dll.Name.Length - dll.Extension.Length).ToLower();
                            if (dll_name.Contains(website.ToString().ToLower()) | website.ToString().ToLower().Contains(dll_name))
                            {
                                info.PluginFile = dll.Name;
                            }
                        }


                        archive.Close();
                        manager.add(info);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "data is null")
                        {
                            FastZip fast = new FastZip();
                            fast.ExtractZip(tmpFile, manager.PluginFolder.ToString(), "dll");
                        }
                        else
                        {
                            throw ex;
                        }
                    }

                }
                else
                {
                    installDll(new FileInfo(tmpFile));
                }
                cleanTempFiles();
                InstallationCompleted?.Invoke();

            }).Start();
        }

        public void installZip(string zipfile)
        {
            new Thread(new ThreadStart(delegate
            {
                RocketPluginInfo info = new RocketPluginInfo("", manager.PluginFolder.FullName, "", "", new List<string>());
                ZipFile archive = new ZipFile(zipfile);
                extractZip(archive, info);
                archive.Close();
                manager.add(info);
                InstallationCompleted?.Invoke();

            })).Start();
        }


        public void installDll(FileInfo dll)
        {
            new Thread(new ThreadStart(delegate
            {
                try
                {
                    dll.CopyTo(manager.PluginFolder.FullName + "\\" + dll.Name, true);
                    manager.add(new RocketPluginInfo(dll, "(unknown)", "(unknown)", new List<FileInfo>()));
                    InstallationCompleted?.Invoke();
                }
                catch (Exception)  { }

            })).Start();
        }


        private void extractZip(ZipFile archive, RocketPluginInfo info)
        {
            foreach (ZipEntry entry in archive)
            {
                if (entry.IsFile)
                {
                    if (entry.ExtraData != null)
                    {
                        extractEntry(archive, entry, info);
                    }
                    else
                    {
                        throw new Exception("data is null");
                    }

                }

            }
        }



        private void cleanTempFiles()
        {
            try
            {
                File.Delete(manager.PluginFolder + "\\plugin.tmp");
            }
            catch (Exception) { }
        }


        private void extractEntry(ZipFile zip, ZipEntry entry, PluginInfo info)
        {

            if (isDll(entry) & isLibrary(entry))
            {
                FileInfo file = new FileInfo(manager.LibrariesFolder + "\\" + getFilename(entry));
                info.Libraries.Add(file.Name);

                copy(zip, entry, file);
            }
            else if (isDll(entry))
            {
                FileInfo file = new FileInfo(manager.PluginFolder + "\\" + getFilename(entry));
                FileInfo inactive = new FileInfo(file.FullName + ".inactive");
                if (inactive.Exists)
                {
                    inactive.Delete();
                }

                copy(zip, entry, file);
                dllFiles.Add(file);
            }
        }



        private void copy(ZipFile zip, ZipEntry entry, FileInfo destination)
        {
            if (!destination.Directory.Exists) destination.Directory.Create();

            byte[] buffer = new byte[4096];
            Stream stream = zip.GetInputStream(entry);
            using (FileStream streamWriter = File.Create(destination.FullName))
            {
                StreamUtils.Copy(stream, streamWriter, buffer);
            }

            //File.WriteAllBytes(destination.FullName, entry.ExtraData);
        }



        private string getFilename(ZipEntry entry)
        {
            string filename = entry.Name.Split('/')[entry.Name.Split('/').Length - 1];
            return filename;
        }


        private bool isLibrary(ZipEntry entry)
        {
            return (entry.Name.Contains("Libraries/") | entry.Name.Contains("Library/"));
        }

        private bool isDll(ZipEntry entry)
        {
            if (entry.IsFile)
            {
                return entry.Name.EndsWith(".dll");
            }
            else
            {
                return false;
            }
        }





    }
}
