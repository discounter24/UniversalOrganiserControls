using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniversalOrganiserControls.Unturned3.Workshop
{
    public class U3WorkshopAutoUpdaterConfig
    {

        private U3Server server;
        

        public string ModPath
        {
            get => server.ServerInformation.ServerDirectory.FullName + "Workshop\\Steam\\content\\304930";
        }

        private FileInfo Config => new FileInfo(server.ServerInformation.ServerDirectory.FullName + "\\WorkshopDownloadIDs.json");

        public IEnumerable<U3WorkshopMod> InstalledMods
        {
            get
            {
                DirectoryInfo ModFolder = new DirectoryInfo(ModPath);
                if (ModFolder.Exists)
                {
                    foreach(DirectoryInfo dir in ModFolder.GetDirectories())
                    {
                        yield return new U3WorkshopMod(dir.FullName);
                    }
                }
            }
        }


        public IEnumerable<U3WorkshopMod> InstalledMapMods
        {
            get => InstalledMods.Where((m) => { return m.Type == U3WorkshopModType.Map; });
        }

        public IEnumerable<U3WorkshopMod> InstalledContentMods
        {
            get => InstalledMods.Where((m) => { return m.Type == U3WorkshopModType.Content; });
        }


        public List<string> NotYetInstalledMods
        {
            get
            {
                List<string> result = RegistredMods;

                foreach(U3WorkshopMod mod in InstalledMods)
                {
                    result.Remove(mod.ID);
                }

                return result;
            }
        }

        public List<string> RegistredMods
        {
            get
            {
                if (!Config.Directory.Exists) Config.Directory.Create();
                List<string> ids = new List<string>();

                if (Config.Exists)
                {
                    string json = File.ReadAllText(Config.FullName);
                    JArray arr = JArray.Parse(json);

                    ids = arr.ToObject<List<string>>();

                }

                return ids;
            }
        }



        public U3WorkshopAutoUpdaterConfig(U3Server server)
        {
            this.server = server;
        }



        public void add(string id)
        {
            List<string> ids = RegistredMods;

            if (!ids.Contains(id))
            {
                ids.Add(id);
            }

            File.WriteAllText(Config.FullName, JsonConvert.SerializeObject(ids));
        }


        public void remove(U3WorkshopMod mod, bool deleteFromServer = false)
        {
            remove(mod.ID, deleteFromServer);
        }

        public void remove(string id, bool deleteFromServer = false)
        {
            if (Config.Exists)
            {
                List<string> ids = RegistredMods;
                ids.Remove(id);
                File.WriteAllText(Config.FullName, JsonConvert.SerializeObject(ids));

                if (deleteFromServer)
                {
                    foreach (U3WorkshopMod mod in InstalledMods)
                    {
                        if (mod.ID == id)
                        {
                            mod.Delete();
                        }
                    }
                }
            }
        }
    }
}
