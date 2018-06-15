using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UniversalOrganiserControls.Unturned3.RocketMod.Plugin
{
    public class RocketPluginManager
    {
        private U3Server server;

        private FileInfo PluginDatabaseFile
        {
            get { return new FileInfo(server.ServerInformation.ServerDirectory.FullName + "\\Rocket\\PluginStorage.uso"); }
        }

        public DirectoryInfo LibrariesFolder
        {
            get { return new DirectoryInfo(server.ServerInformation.ServerDirectory.FullName + "\\Rocket\\Libraries\\"); }
        }

        public DirectoryInfo PluginFolder
        {
            get { return new DirectoryInfo(server.ServerInformation.ServerDirectory.FullName + "\\Rocket\\Plugins\\"); }
        }


        private List<RocketPlugin> Plugins = new List<RocketPlugin>();
        public List<RocketPlugin> getPlugins()
        {
            return Plugins;
        }



        public RocketPluginManager(U3Server instance)
        {
            this.server = instance;
            reload();
        }

        public void reload()
        {
            loadPlugins(RocketPluginInfoStorage.load(PluginDatabaseFile.FullName));
        }


        private RocketPluginInfo findPluginInfo(string pluginName, RocketPluginInfoStorage storage)
        {
            return storage.PluginUpdateInfos.First(info => info.PluginFileInfo.Name.Equals(pluginName));
        }

        private void loadPlugins(RocketPluginInfoStorage storage)
        {
            Plugins.Clear();

            if (!PluginFolder.Exists)
            {
                PluginFolder.Create();
                return;
            }

            try
            {
                foreach (FileInfo pfile in PluginFolder.GetFiles("*.dll"))
                {
                    try
                    {
                        RocketPluginInfo pinfo = findPluginInfo(pfile.Name, storage);
                        add(pinfo);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                }

                foreach (FileInfo pfile in PluginFolder.GetFiles("*.dll.inactive"))
                {
                    try
                    {
                        RocketPluginInfo pinfo = findPluginInfo(pfile.Name, storage);
                        add(pinfo);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                }
            }
            catch (Exception)
            {
                storage.save(PluginDatabaseFile);
            }


        }


        public RocketPlugin getPluginByName(string name)
        {
            return Plugins.Where(p => p.Name.Equals(name)).DefaultIfEmpty(null).First();
        }

        public RocketPlugin getPluginByWebsite(string website)
        {
            return Plugins.Where(p => p.Info.Website.Equals(website)).DefaultIfEmpty(null).First();
        }


        public bool add(RocketPluginInfo pinfo)
        {
            try
            {
                
                RocketPlugin p = new RocketPlugin(pinfo);
                RocketPlugin existingPlugin = getPluginByName(p.Name);
                if (existingPlugin != null)
                {
                    Plugins.Remove(existingPlugin);
                }

                Plugins.Add(p);
                saveDatabase();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }



        public bool remove(RocketPlugin plugin)
        {
            try
            {
                plugin.File.Delete();
                Plugins.Remove(plugin);
                saveDatabase();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }


        public bool saveDatabase()
        {
            try
            {
                RocketPluginInfoStorage storage = new RocketPluginInfoStorage();
                foreach (RocketPlugin p in Plugins)
                {
                    storage.PluginUpdateInfos.Add(p.Info);
                }
                return storage.save(PluginDatabaseFile);
            }
            catch (Exception)
            {
                return false;
            }

        }




    }
}
