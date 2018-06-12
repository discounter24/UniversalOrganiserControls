using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace UniversalOrganiserControls.Unturned3.RocketMod.Plugin
{
    public class RocketPluginInfoStorage
    {
        private class SimpleInfo
        {
         
            public SimpleInfo(RocketPluginInfo info)
            {

            }
        }

        
        private List<SimpleInfo> PluginInformation
        {
            get
            {
                List<SimpleInfo> info = new List<SimpleInfo>();

                foreach(RocketPluginInfo i in PluginUpdateInfos)
                {
                    info.Add(new SimpleInfo(i));
                }

                return info;
            }
        }

        [JsonIgnore]
        public List<RocketPluginInfo> PluginUpdateInfos = new List<RocketPluginInfo>();





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

        public static RocketPluginInfoStorage load(string file)
        {
            try
            {
                return JsonConvert.DeserializeObject<RocketPluginInfoStorage>(File.ReadAllText(file));
            }
            catch (Exception)
            {
                return new RocketPluginInfoStorage();
            }
        }


    }
}
