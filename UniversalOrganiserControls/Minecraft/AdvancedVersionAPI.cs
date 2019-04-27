using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Minecraft
{
    public class AdvancedVersionAPI
    {
        public const string Address = "http://dl.minecraft-server-organiser.com/engines/versions.php";


        public Version_manifest Manifest;


        private AdvancedVersionAPI(string json)
        {
            Manifest = JsonConvert.DeserializeObject<Version_manifest>(json);
        }

        public List<MSOVersion> GetVersionList(VersionType type)
        {
        
            switch (type)
            {
                case VersionType.VANILLA:
                    return Manifest.vanilla;

                case VersionType.CRAFTBUKKIT:
                    return Manifest.craftbukkit;

                case VersionType.SPIGOT:
                    return Manifest.spigot;

                case VersionType.FORGE:
                    return Manifest.forge;


                default:
                    break;
            }

            return new List<MSOVersion>();

        }


        public static AdvancedVersionAPI Get()
        {
            try
            {
                WebClient client = new WebClient();
                string source = client.DownloadString(new Uri(Address));
                return new AdvancedVersionAPI(source);
            }
            catch (WebException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public enum VersionType
        {
            VANILLA, CRAFTBUKKIT, SPIGOT, FORGE
        }

        public class Version_manifest
        {
            public List<MSOVersion> vanilla = new List<MSOVersion>();
            public List<MSOVersion> craftbukkit = new List<MSOVersion>();
            public List<MSOVersion> spigot = new List<MSOVersion>();
            public List<MSOVersion> forge = new List<MSOVersion>();

        }


        public class MSOVersion : ServerVersion
        {
            public string size;
            public string mcversion;
        }

    }
}
