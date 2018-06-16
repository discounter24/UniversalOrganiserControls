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
            loadPlugins(RocketPluginStorage.load(PluginDatabaseFile.FullName));
        }


        private RocketPlugin findPlugin(string pluginName, RocketPluginStorage storage)
        {
            return storage.Plugins.First(pl => pl.PluginFileInfo.Name.Equals(pluginName));
        }

        private void loadPlugins(RocketPluginStorage storage)
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
                        RocketPlugin plugin = findPlugin(pfile.Name, storage);
                        add(plugin);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                }

                foreach (FileInfo pfile in PluginFolder.GetFiles("*.dll.inactive"))
                {
                    try
                    {
                        RocketPlugin plugin = findPlugin(pfile.Name, storage);
                        add(plugin);
                    }
                    catch (Exception)
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
            return Plugins.Where(p => p.Website.Equals(website)).DefaultIfEmpty(null).First();
        }


        public bool add(RocketPlugin plugin)
        {
            try
            {
                RocketPlugin existingPlugin = getPluginByName(plugin.Name);
                if (existingPlugin != null)
                {
                    Plugins.Remove(existingPlugin);
                }

                Plugins.Add(plugin);
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
                plugin.PluginFileInfo.Delete();
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
                RocketPluginStorage storage = new RocketPluginStorage();
                foreach (RocketPlugin p in Plugins)
                {
                    storage.Plugins.Add(p);
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
