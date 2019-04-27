using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace UniversalOrganiserControls.Minecraft.Server.Properties
{

    public class ServerProperties : IEnumerable<Property>
    {

        #region STATIC

        public static ServerProperties DefaultProperties
        {
            get
            {
                ServerProperties p = new ServerProperties();
                p["generator-settings"] = "";
                p["op-permission-level"] = "4";
                p["allow-nether"] = "true";
                p["level-name"] = "world";
                p["enable-query"] = "false";
                p["allow-flight"] = "false";
                p["prevent-proxy-connections"] = "false";
                p["level-type"] = "DEFAULT";
                p["enable-rcon"] = "false";
                p["announce-player-achievements"] = "true";
                p["level-seed"] = "";
                p["force-gamemode"] = "false";
                p["server-ip"] = "";
                p["network-compression-threshold"] = "256";
                p["max-build-height"] = "256";
                p["spawn-npcs"] = "true";
                p["white-list"] = "false";
                p["spawn-animals"] = "true";
                p["hardcore"] = "false";
                p["snooper-enabled"] = "true";
                p["resource-pack-hash"] = "";
                p["resource-pack-sha1"] = "";
                p["online-mode"] = "true";
                p["resource-pack"] = "";
                p["pvp"] = "true";
                p["max-world-size"] = "29999984";
                p["difficulty"] = "1";
                p["enable-command-block"] = "false";
                p["gamemode"] = "0";
                p["player-idle-timeout"] = "0";
                p["max-players"] = "20";
                p["max-tick-time"] = "60000";
                p["spawn-monsters"] = "true";
                p["view-distance"] = "10";
                p["generate-structures"] = "true";
                p["motd"] = "A Minecraft Server";
                p["broadcast-console-to-ops"] = "true";
                p["enable-command-block"] = "false";
                p["query.port"] = "25565";
                p["rcon.password"] = "";
                p["rcon.port"] = "25575";
                p["spawn-protection"] = "16";
                p["server-port"] = "25565";

                return p;
            }
        }

        public static Dictionary<string,string> Descriptions
        {
            get
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("enable-command-block", "Enable command blocks ");
                dic.Add("query.port", "Query port");
                dic.Add("rcon.port", "Remote control port");
                dic.Add("rcon.password", "Remote control password");
                dic.Add("enable-rcon", "Remote control");
                dic.Add("max-build-height", "Maximum build height");
                dic.Add("max-players", "Maximum Players");
                dic.Add("resource-pack", "Ressource pack URL");
                dic.Add("pvp", "Player vs Player");
                dic.Add("motd", "Motd");
                dic.Add("generate-structures", "Generate Structures");
                dic.Add("spawn-monsters", "Monsters");
                dic.Add("hardcore", "Hardcore");
                dic.Add("gamemode", "Default gamemode");
                dic.Add("difficulty", "Difficulty");
                dic.Add("white-list", "Whitelist");
                dic.Add("spawn-animals", "Animals");
                dic.Add("force-gamemode", "Force default gamemode");
                dic.Add("online-mode", "Online Mode / Disallow cracked");
                dic.Add("spawn-npcs", "NPCs");
                dic.Add("server-ip", "Bind ServerIP");
                dic.Add("allow-flight", "Allow-Flight");
                dic.Add("level-name", "World-Name");
                dic.Add("enable-query", "Query");
                dic.Add("resource-pack-hash", "Resource pack hash (old)");
                dic.Add("resource-pack-sha1", "Resource pack sha1");
                dic.Add("allow-nether", "Nether");
                dic.Add("snooper-enabled", "Snooper");
                dic.Add("op-permission-level", "OP permission level");
                dic.Add("max-world-size", "Max World Size");
                dic.Add("level-seed", "Seed");
                dic.Add("server-port", "Server port");
                return dic;
            }
        }




        public static ServerProperties getServerProperties(MinecraftServer server)
        {
            FileInfo propFile = new FileInfo(server.ServerDirectory.FullName + "\\server.properties");
            return getServerProperties(propFile);
        }

        public static ServerProperties getServerProperties(FileInfo propFile)
        {
            ServerProperties props = new ServerProperties();
            if (!propFile.Directory.Exists) propFile.Directory.Create();
            if (!propFile.Exists) propFile.Create().Close();
            props.load(propFile);
            return props;
        }

        #endregion


        private List<Property> props = new List<Property>();

        public List<Property> Properties
        {
            get => props;
        }


        public ServerProperties()
        {
           
        }

        public void load(FileInfo file)
        {
            if (!file.Exists) file.Create().Close();
            foreach (string line in File.ReadAllLines(file.FullName))
            {
                if (line.StartsWith("#")) continue;
                add(getPropertyFromLine(line));
            }
        }

        private Property getPropertyFromLine(string line)
        {
            Property prop = null;

            string boolMatch = @"^([^\=]+)\=(true|false)$";
            string intMatch = @"^([^\=]+)\=([0-9]+)$";
            string stringMatch = @"^([^\=]+)\=(.*)$";


            MatchCollection matches = null;

            if (Regex.IsMatch(line, boolMatch))
            {
                matches = Regex.Matches(line, boolMatch);
                prop = new BoolProperty(matches[0].Groups[1].ToString(), Convert.ToBoolean(matches[0].Groups[2].ToString()));

            }
            else if (Regex.IsMatch(line, intMatch))
            {
                matches = Regex.Matches(line, intMatch);
                prop = new IntProperty(matches[0].Groups[1].ToString(), Convert.ToInt32(matches[0].Groups[2].ToString()));
            }
            else
            {
                matches = Regex.Matches(line, stringMatch);
                prop = Property.get(matches[0].Groups[1].ToString(), matches[0].Groups[2].ToString());
            }

            return prop;
        }


        public bool add(Property prop)
        {
            if (get(prop.Key) == null)
            {
                props.Add(prop);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool remove(Property prop)
        {
            prop = get(prop.Key);
            
            if (prop != null)
            {
                props.Remove(prop);
                return true;
            }
            else
            {
                return false;
            }
        }


        public Property get(string key)
        {
            foreach(Property prop in props)
            {
                if (prop.Key.Equals(key))
                {
                    return prop;
                }
            }
            return null;
        }

        public void reset()
        {
            foreach(Property p in props)
            {
                p.Value = null;
            }
        }

        public void save(MinecraftServer server)
        {
            save(new FileInfo(server.ServerDirectory.FullName + "\\server.properties"));
        }


        public void save(FileInfo file)
        {
            File.WriteAllText(file.FullName, this.ToString());
        }


        new public string ToString()
        {
            string content = "";
            foreach (Property prop in props)
            {
                content += prop.ToString() + "\n";
            }
            return content;
        }


        #region EXTENDS

        public IEnumerator<Property> GetEnumerator()
        {
            return props.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return props.GetEnumerator();
        }



        public string this[string s]
        {
            get
            {
                Property p = get(s);
                if (p==null)
                {
                    return null;
                }
                else
                {
                    return p.Value;
                }
            }
            set
            {
                Property p = get(s);
                if (p==null)
                {
                    add(Property.get(s, value));
                }
                else
                {
                    p.Value = value;
                }
            }
        }




        #endregion







    }

    public enum PropertyType
    {
        STRING, INT, BOOL
    }








}
