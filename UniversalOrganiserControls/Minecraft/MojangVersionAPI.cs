using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniversalOrganiserControls.Minecraft
{
    public class MojangVersionAPI
    {

        public const string Address = "https://launchermeta.mojang.com/mc/game/version_manifest.json";

        public Version_manifest Manifest;

        private MojangVersionAPI(string json)
        {
            Manifest = JsonConvert.DeserializeObject<Version_manifest>(json);
        }



        public List<MojangVersion> GetVersionList(VersionType filter)
        {
            List<MojangVersion> found = new List<MojangVersion>();
            foreach (MojangVersion v in Manifest.versions)
            {
                VersionType type = (VersionType)Enum.Parse(typeof(VersionType), v.type.ToUpper());

                if (filter==VersionType.ALL | type == filter)
                {
                    found.Add(v);
                }
            }
            return found;
        }

        public MojangVersion getLastRelease()
        {
            foreach (MojangVersion v in Manifest.versions)
            {
                if (v.id == Manifest.latest.release)
                {
                    return v;
                }
            }
            throw new Exception("NotFound");
        }

        public static string GetDownloadUrl(MojangVersion version)
        {
            try
            {
                WebClient client = new WebClient();
                string source = client.DownloadString(version.url);
                dynamic d = JsonConvert.DeserializeObject(source);
                return (string)d["downloads"]["server"]["url"];
            }
            catch (Exception)
            {
                throw;
            }
            
        }
        

        public static MojangVersionAPI Get()
        {
            try
            {
                WebClient client = new WebClient();
                string source = client.DownloadString(new Uri(Address));
                return new MojangVersionAPI(source);
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
            ALL, SNAPSHOT, RELEASE, OLD_ALPHA, OLD_BETA
        }

        public struct Version_manifest
        {
            public Version_manifest_latest latest;
            public MojangVersion[] versions;

        }

        public struct Version_manifest_latest
        {
            public string snapshot;
            public string release;
        }

        public class MojangVersion : ServerVersion
        {
            public string type;
            public string time;
        }

    }
}
