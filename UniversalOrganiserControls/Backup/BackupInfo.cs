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
            private set;
        }
        public string GameName
        {
            get;
            private set;
        }
        public string CustomIdentifier
        {
            get;
            private set;
        }
        public DateTime Created
        {
            get;
            private set;
        }
        
        [JsonIgnore]
        public BackupPackage Data { get; private set; }


        public BackupInfo(string gameName, string customIdentifier = "")
        {
            this.Created = DateTime.Now;
            this.GameName = gameName;
            this.CustomIdentifier = customIdentifier;

            string clearName = GameName + ";" + CustomIdentifier + ";" + Created.ToString();
            this.Hash = GetHashString(clearName);
        }

        private static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }


        public static BackupInfo read(string file)
        {
            try
            {
                BackupInfo info = null;
                string content = File.ReadAllText(file);
                JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
                jsonSettings.NullValueHandling = NullValueHandling.Ignore;
                jsonSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                info = JsonConvert.DeserializeObject<BackupInfo>(content);
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
