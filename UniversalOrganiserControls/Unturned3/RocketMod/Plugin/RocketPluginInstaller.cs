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
       
      
        private RocketPluginManager manager;


        public RocketPluginInstaller(RocketPluginManager manager)
        {
            this.manager = manager;
        }

        private List<FileInfo> dllFiles = new List<FileInfo>();

        public Task<PluginInstallationResult> install(Uri download, Uri website)
        {
            return Task.Run(async () =>
            {
                List<FileInfo> dependecys = new List<FileInfo>();
                UniversalWebClient client = new UniversalWebClient();
                FileInfo tmpFile = new FileInfo(manager.PluginFolder.FullName + "\\plugin.tmp");
               
                await client.DownloadFileTaskAsync(download.ToString(), tmpFile.FullName);


                ZipFile archive = new ZipFile(tmpFile.FullName);
                if (archive.TestArchive(true))
                {
                    try
                    {

                        RocketPlugin plugin = null;

                        ExtractZipResult res = await extractZip(archive);
                        List<FileInfo> dependencys = new List<FileInfo>();
                        FileInfo plFile = null;
                        foreach (ExtractEntryResult entry in res.entries)
                        {
                            switch (entry.Type)
                            {
                                case PluginEntryType.Plugin:
                                    plFile = entry.File;
                                    break;
                                case PluginEntryType.Dependency:
                                    dependencys.Add(entry.File);
                                    break;
                                case PluginEntryType.Unknown:
                                    continue;
                                default:
                                    break;
                            }
                        }
                        plugin = new RocketPlugin(plFile, website.ToString(), plugin.ServerVersion, dependencys);


                        archive.Close();
                        manager.add(plugin);
                        

                        return PluginInstallationResult.OK;
                    }
                    catch (Exception)
                    {
                        return PluginInstallationResult.Failed;
                    }
                }
                else
                {
                    return PluginInstallationResult.FailedNotAZip;
                }

            });
        }

        private void Client_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
           
        }

        public Task<RocketPlugin> installZip(string zipfile)
        {
            return Task.Run(async () =>
            {

                RocketPlugin plugin = null;

                try
                {
                    ZipFile archive = new ZipFile(zipfile);
                    ExtractZipResult res = await extractZip(archive);
                    archive.Close();

                    List<FileInfo> dependencys = new List<FileInfo>();
                    FileInfo plFile = null;
                    foreach(ExtractEntryResult entry in res.entries)
                    {
                        switch (entry.Type)
                        {
                            case PluginEntryType.Plugin:
                                plFile = entry.File;
                                break;
                            case PluginEntryType.Dependency:
                                dependencys.Add(entry.File);
                                break;
                            case PluginEntryType.Unknown:
                                continue;
                            default:
                                break;
                        }
                    }
                    plugin = new RocketPlugin(plFile, "", "", dependencys);
                }
                catch (Exception)
                {
                    return null;
                }

                return plugin;
            });
        }


        public Task<bool> installDll(FileInfo dll)
        {
            return Task.Run(() =>
            {
                try
                {
                    dll.CopyTo(manager.PluginFolder.FullName + "\\" + dll.Name, true);
                    manager.add(new RocketPlugin(dll, "(unknown)", "(unknown)", new List<FileInfo>()));
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }


        private Task<ExtractZipResult> extractZip(ZipFile archive)
        {
            return Task.Run(async () =>
            {
                ExtractZipResult result = new ExtractZipResult();

                foreach (ZipEntry entry in archive)
                {
                    if (entry.IsFile)
                    {
                        if (entry.ExtraData != null)
                        {
                            result.entries.Add(await extractEntry(archive, entry));
                        }
                        else
                        {
                            throw new Exception("data is null");
                        }
                    }
                }
                return result;
            });

        }



        private void cleanTempFiles()
        {
            try
            {
                File.Delete(manager.PluginFolder + "\\plugin.tmp");
            }
            catch (Exception) { }
        }


        private Task<ExtractEntryResult> extractEntry(ZipFile zip, ZipEntry entry)
        {
            return Task.Run(() =>
            {

                ExtractEntryResult result = new ExtractEntryResult();

                if (isDll(entry) & isLibrary(entry))
                {
                    FileInfo file = new FileInfo(manager.LibrariesFolder + "\\" + getFilename(entry));

                    copy(zip, entry, file);

                    result.File = file;
                    result.Type = PluginEntryType.Dependency;

                    return result;
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

                    result.File = file;

                    return result;
                }
                else
                {
                    result.Type = PluginEntryType.Unknown;
                    return result;
                }

            });
   
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
