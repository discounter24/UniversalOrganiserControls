using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace UniversalOrganiserControls.Unturned3.RocketMod.Plugin
{
    public class RocketPluginStorage
    {


        public List<RocketPlugin> Plugins = new List<RocketPlugin>();
        
        public bool save(FileInfo PluginDatabaseFile)
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                if (PluginDatabaseFile.Exists) PluginDatabaseFile.Delete();
                File.WriteAllText(PluginDatabaseFile.FullName, json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public static RocketPluginStorage load(string file)
        {
            try
            {
                return JsonConvert.DeserializeObject<RocketPluginStorage>(File.ReadAllText(file));
            }
            catch (Exception)
            {
                return new RocketPluginStorage();
            }
        }


    }
}
