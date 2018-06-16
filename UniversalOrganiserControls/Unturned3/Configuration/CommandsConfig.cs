using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UniversalOrganiserControls.Unturned3.Configuration
{
    public class CommandsConfig
    {
        public List<string> content;
        private FileInfo dat;



        public CommandsConfig(DirectoryInfo serverFolder)
        {
            dat = prepareFile(serverFolder);
            load();
        }


        private FileInfo prepareFile(DirectoryInfo serverdir)
        {

            if (!serverdir.Exists) serverdir.Create();
            FileInfo file = new FileInfo(serverdir.FullName + "\\Commands.dat");
            if (!file.Exists) file.Create().Close();

            return file;
        }


        private void load()
        {
            content = File.ReadAllLines(dat.FullName).ToList<string>();
        }

        public void save()
        {
            if (!dat.Directory.Exists) dat.Directory.Create();


            try
            {
                File.WriteAllLines(dat.FullName, content);
            }
            catch (FileNotFoundException)
            {
                dat.Create().Close();
                File.WriteAllLines(dat.FullName, content);
            }
        }



        private void setSetting(string identifier, List<string> values)
        {
            removeSetting(identifier);
            foreach (string value in values)
            {
                if (value == "")
                {
                    content.Add(identifier + " ");
                }
                else
                {
                    content.Add(string.Format("{0} {1}", identifier, value));
                }
            }

            if (values.Count == 0) content.Add(identifier + " ");

        }

        private void removeSetting(string identifier)
        {

            content.RemoveAll(
                delegate (string attribut)
                {
                    return attribut.StartsWith(identifier + " ");
                });


        }


        private List<string> getSetting(string identifier)
        {
            List<string> settings = new List<string>();
            foreach (string setting in content)
            {
                if (setting.StartsWith(identifier + " "))
                {
                    settings.Add(setting.Remove(0, (identifier + " ").Length));
                }
            }
            return settings;
        }

        public string Name
        {
            get
            {
                List<string> settings = getSetting("name");
                if (settings.Count == 0)
                {
                    return "Server hosted by USO!";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                attribute.Add(value);
                setSetting("name", attribute);
            }
        }

        public string SyncKey
        {
            get
            {
                List<string> settings = getSetting("sync");
                if (settings.Count == 0)
                {
                    return "";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                if (value.TrimEnd().Length == 0)
                {
                    removeSetting("sync");
                }
                else
                {
                    attribute.Add(value);
                    setSetting("sync", attribute);
                }

            }
        }

        public string Timeout
        {
            get
            {
                List<string> settings = getSetting("timeout");
                if (settings.Count == 0)
                {
                    return "300";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                if (value.TrimEnd().Length == 0)
                {
                    removeSetting("timeout");
                }
                else
                {
                    attribute.Add(value);
                    setSetting("timeout", attribute);
                }

            }
        }

        public string Bind
        {
            get
            {
                List<string> settings = getSetting("bind");
                if (settings.Count == 0)
                {
                    return "0.0.0.0";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                if (value.TrimEnd().Length == 0)
                {
                    removeSetting("bind");
                }
                else
                {
                    attribute.Add(value);
                    setSetting("bind", attribute);
                }

            }
        }


        public string Owner
        {
            get
            {
                List<string> settings = getSetting("owner");
                if (settings.Count == 0)
                {
                    return "";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                if (value.TrimEnd().Length == 0)
                {
                    removeSetting("owner");
                }
                else
                {
                    attribute.Add(value);
                    setSetting("owner", attribute);
                }
            }
        }

        public string MaxPlayers
        {
            get
            {
                List<string> settings = getSetting("maxplayers");
                if (settings.Count == 0)
                {
                    return "24";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                attribute.Add(value);
                setSetting("maxplayers", attribute);
            }
        }

        public string Welcome
        {
            get
            {
                List<string> settings = getSetting("welcome");
                if (settings.Count == 0)
                {
                    return "Welcome! You are playing on a server hosted by USO.";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                attribute.Add(value);
                setSetting("welcome", attribute);
            }
        }

        public string Password
        {
            get
            {
                List<string> settings = getSetting("password");
                if (settings.Count == 0)
                {
                    return "";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                attribute.Add(value);
                setSetting("password", attribute);
            }
        }


        public string Map
        {
            get
            {
                List<string> settings = getSetting("map");
                if (settings.Count == 0)
                {
                    return "PEI";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                attribute.Add(value);
                setSetting("map", attribute);
            }
        }

        public string Port
        {
            get
            {
                List<string> settings = getSetting("port");
                if (settings.Count == 0)
                {
                    return "27015";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                attribute.Add(value);
                setSetting("port", attribute);
            }
        }

        public string Cycle
        {
            get
            {
                List<string> settings = getSetting("cycle");
                if (settings.Count == 0)
                {
                    return "43200";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                attribute.Add(value);
                setSetting("cycle", attribute);
            }
        }

        public string Chatrate
        {
            get
            {
                List<string> settings = getSetting("ChatRate");
                if (settings.Count == 0)
                {
                    return "0";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                attribute.Add(value);
                setSetting("ChatRate", attribute);
            }
        }

        public string QueueSize
        {
            get
            {
                List<string> settings = getSetting("Queue_Size");
                if (settings.Count == 0)
                {
                    return "0";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                attribute.Add(value);
                setSetting("Queue_Size", attribute);
            }
        }

        public string Perspective
        {
            get
            {
                List<string> settings = getSetting("perspective");
                if (settings.Count == 0)
                {
                    return "both";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                attribute.Add(value);
                setSetting("perspective", attribute);
            }
        }

        public string Mode
        {
            get
            {
                List<string> settings = getSetting("mode");
                if (settings.Count == 0)
                {
                    return "normal";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                attribute.Add(value);
                setSetting("mode", attribute);
            }
        }

        public bool PvP
        {
            get
            {
                List<string> settings = getSetting("pvp");
                return (settings.Count != 0);
            }
            set
            {
                if (value.ToString().ToLower().Equals("true"))
                {
                    removeSetting("pve");
                    setSetting("pvp", new List<string>());
                }
                else
                {
                    setSetting("pve", new List<string>());
                    removeSetting("pvp");
                }

            }
        }

        public bool Gold
        {
            get
            {
                List<string> settings = getSetting("gold");
                return (settings.Count != 0);
            }
            set
            {
                if (value.ToString().ToLower().Equals("true"))
                {
                    setSetting("gold", new List<string>());
                }
                else
                {
                    removeSetting("gold");
                }

            }
        }

        public bool HideAdmins
        {
            get
            {
                List<string> settings = getSetting("hide_admins");
                return (settings.Count != 0);
            }
            set
            {
                if (value.ToString().ToLower().Equals("true"))
                {
                    setSetting("hide_admins", new List<string>());
                }
                else
                {
                    removeSetting("hide_admins");
                }

            }
        }

        public bool Cheats
        {
            get
            {
                List<string> settings = getSetting("cheats");
                return (settings.Count != 0);
            }
            set
            {
                if (value.ToString().ToLower().Equals("true"))
                {
                    setSetting("cheats", new List<string>());
                }
                else
                {
                    removeSetting("cheats");
                }

            }
        }


        public List<string> Loadout
        {
            get
            {
                List<string> settings = getSetting("loadout");
                if (settings.Count == 0)
                {
                    settings.Add("255");
                }
                return settings;
            }
            set
            {
                setSetting("loadout", value);
            }
        }

        public string Votify
        {
            get
            {
                List<string> settings = getSetting("votify");
                if (settings.Count == 0)
                {
                    return "";
                }
                return settings[0];
            }

            set
            {
                List<string> attribute = new List<string>();
                if (value.StartsWith("N"))
                {
                    removeSetting("votify");
                }
                else
                {
                    attribute.Add(value);
                    setSetting("votify", attribute);
                }

            }
        }
    }
}
