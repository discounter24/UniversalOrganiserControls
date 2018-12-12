using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;

namespace UniversalOrganiserControls.Backup
{
    public class BackupInfo
    {
        public string Hash
        {
            get;
            set;
        }
        public string GameName
        {
            get;
            set;
        }

        public string SubGame
        {
            get;
            set;
        }

        public string CustomIdentifier
        {
            get;
            set;
        }

        public DateTime Created
        {
            get;
            set;
        }


        [JsonIgnore]
        public BackupPackage Data { get; private set; }


        public BackupInfo(string gameName, string subgame, string customIdentifier = "")
        {
            this.Created = DateTime.Now;
            this.GameName = gameName;
            this.CustomIdentifier = customIdentifier;
            this.SubGame = subgame;

            string clearName = GameName + ";" + CustomIdentifier + ";" + Created.ToString() + ";" + subgame;
            this.Hash = UtilsGeneral.GetHashString(clearName);
        }




        public static BackupInfo read(string file)
        {
            try
            {
                string content = File.ReadAllText(file);
                JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
                jsonSettings.NullValueHandling = NullValueHandling.Ignore;
                jsonSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

                BackupInfo info = JsonConvert.DeserializeObject<BackupInfo>(content);

                DirectoryInfo dir = new DirectoryInfo(new FileInfo(file).Directory.FullName);
                info.Data = new BackupPackage(dir, info);

                return info;
            }
            catch (Exception)
            {
                throw new Exception("Unexpected reading error!");
            }
        
        }

        public void save(DirectoryInfo folder)
        {
            try
            {
                FileInfo file = new FileInfo(folder.FullName + "\\" + this.Hash + ".bck");
                string content = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(file.FullName, content);
            }
            catch (Exception)
            {
                throw new Exception("Unexpected writing error!");
            }
        }
    }
}
