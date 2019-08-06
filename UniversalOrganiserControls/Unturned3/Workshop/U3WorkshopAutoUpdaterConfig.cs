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

        private FileInfo ConfigFile => new FileInfo(server.ServerInformation.ServerDirectory.FullName + "\\WorkshopDownloadConfig.json");
        private U3Server server;
        

        public string ModPath
        {
            get => server.ServerInformation.ServerDirectory.FullName + "Workshop\\Steam\\content\\304930";
        }


        private WorkshopFileJsonObject _tmpJson = null;
        private WorkshopFileJsonObject JsonObject
        {
            get
            {
                if (_tmpJson != null) return _tmpJson;

                _tmpJson = new WorkshopFileJsonObject();
                try
                {

                    var obj = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(ConfigFile.FullName));
                    _tmpJson = obj.ToObject<WorkshopFileJsonObject>();
                }
                catch (Exception ex) {
                    
                }

                return _tmpJson;
            }
        }
       

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
                if (!ConfigFile.Directory.Exists) ConfigFile.Directory.Create();
                return JsonObject.File_IDs;
            }
        }



        public U3WorkshopAutoUpdaterConfig(U3Server server)
        {
            this.server = server;
        }



        public void Add(string id)
        {
            JsonObject.File_IDs = RegistredMods;
            if (!JsonObject.File_IDs.Contains(id))
            {
                JsonObject.File_IDs.Add(id);
            }

            JsonObject.save(ConfigFile.FullName);
        }


        public void Remove(U3WorkshopMod mod, bool deleteFromServer = false)
        {
            Remove(mod.ID, deleteFromServer);
        }

        public void Remove(string id, bool deleteFromServer = false)
        {
            if (ConfigFile.Exists)
            {
                JsonObject.File_IDs = RegistredMods;
                JsonObject.File_IDs.Remove(id);

                JsonObject.save(ConfigFile.FullName);

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

        private class WorkshopFileJsonObject
        {
            public List<string> File_IDs = new List<string>();
            public int Query_Cache_Max_Age_Seconds = 600;
            public int Max_Query_Retries = 2;

            public void save(string file)
            {
                File.WriteAllText(file, JsonConvert.SerializeObject(this));
            }
        }
    }
}
