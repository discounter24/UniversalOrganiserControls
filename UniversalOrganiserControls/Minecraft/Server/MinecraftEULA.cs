using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace UniversalOrganiserControls.Minecraft.Server
{
    public class MinecraftEULA
    {
        public event EventHandler<bool> AgreementStateChanged;

        public const string officialEULA = "https://account.mojang.com/documents/minecraft_eula";

        private FileInfo eulaSettings;

        public bool Agreed
        {
            get
            {
                foreach(string line in File.ReadAllLines(eulaSettings.FullName))
                {
                    if (line.Equals("eula=true"))
                    {
                        return true;
                    }
                }
                return false;
            }
            set
            {
                string content = File.ReadAllText(eulaSettings.FullName);
                content = Regex.Replace(content, "eula=(true|false)", string.Format("eula={0}", value.ToString().ToLower()), RegexOptions.IgnoreCase);
                File.WriteAllText(eulaSettings.FullName, content);
                AgreementStateChanged?.Invoke(this, value);
            }
        }


        public MinecraftEULA(FileInfo eulaSettings)
        {
            if (eulaSettings.Exists)
            {
                this.eulaSettings = eulaSettings;
            }
            else
            {
                throw new FileNotFoundException();
            }
        }
    }

}
