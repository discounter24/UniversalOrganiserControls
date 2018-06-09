using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;


namespace UniversalOrganiserControls.Unturned3.RocketMod.Configuration
{



    public class RocketConfig
    {


        private DirectoryInfo rocketFolder;


        private XmlDocument RocketXML = new XmlDocument();
        private XmlDocument RocketUnturnedXML = new XmlDocument();



        public RocketConfig(DirectoryInfo rocketFolder)
        {

            this.rocketFolder = rocketFolder;

            if (this.rocketFolder.Exists)
            {
                FileInfo RocketUnturnedFile = new FileInfo(this.rocketFolder.FullName + "\\Rocket.Unturned.config.xml");
                FileInfo RocketFile = new FileInfo(this.rocketFolder.FullName + "\\Rocket.config.xml");

                if (RocketUnturnedFile.Exists & RocketFile.Exists)
                {
                    RocketUnturnedXML.Load(RocketUnturnedFile.FullName);
                    RocketXML.Load(RocketFile.FullName);
                }
                else
                {
                    throw new Exception("Configs not generated.");
                }

            }
            else
            {
                throw new Exception("Rocket folder does not exist.");
            }

        }

        public void save()
        {
            try
            {
                DirectoryInfo path = new DirectoryInfo(instance.ServerDirectory.FullName + "\\Rocket\\");
                FileInfo RocketUnturnedFile = new FileInfo(path.FullName + "\\Rocket.Unturned.config.xml");
                FileInfo RocketFile = new FileInfo(path.FullName + "\\Rocket.config.xml");

                RocketXML.Save(RocketFile.FullName);
                RocketUnturnedXML.Save(RocketUnturnedFile.FullName);
            }
            catch (Exception)
            {
                throw;
            }
            
        }


        public bool CommunityBans
        {
            get
            {
                XmlNode node = RocketUnturnedXML.DocumentElement.SelectSingleNode("/UnturnedSettings/RocketModObservatory");
                return Convert.ToBoolean(node.Attributes["CommunityBans"]?.InnerText);
            }
            set
            {
                RocketUnturnedXML.DocumentElement.SelectSingleNode("/UnturnedSettings/RocketModObservatory").Attributes["CommunityBans"].InnerText = value.ToString().ToLower();
            }
        }

        public bool KickLimitedAccounts
        {
            get
            {
                XmlNode node = RocketUnturnedXML.DocumentElement.SelectSingleNode("/UnturnedSettings/RocketModObservatory");
                return Convert.ToBoolean(node.Attributes["KickLimitedAccounts"]?.InnerText);
            }
            set
            {
                RocketUnturnedXML.DocumentElement.SelectSingleNode("/UnturnedSettings/RocketModObservatory").Attributes["KickLimitedAccounts"].InnerText = value.ToString().ToLower();
            }
        }

        public bool KickTooYoungAccounts
        {
            get
            {
                XmlNode node = RocketUnturnedXML.DocumentElement.SelectSingleNode("/UnturnedSettings/RocketModObservatory");
                return Convert.ToBoolean(node.Attributes["KickTooYoungAccounts"]?.InnerText);
            }
            set
            {
                RocketUnturnedXML.DocumentElement.SelectSingleNode("/UnturnedSettings/RocketModObservatory").Attributes["KickTooYoungAccounts"].InnerText = value.ToString().ToLower();
            }
        }

        public string AccountMinimumAge
        {
            get
            {
                XmlNode node = RocketUnturnedXML.DocumentElement.SelectSingleNode("/UnturnedSettings/RocketModObservatory");
                return node.Attributes["MinimumAge"]?.InnerText;
            }
            set
            {
                RocketUnturnedXML.DocumentElement.SelectSingleNode("/UnturnedSettings/RocketModObservatory").Attributes["MinimumAge"].InnerText = value;
            }
        }



        public bool AutoSave
        {
            get
            {
                XmlNode node = RocketUnturnedXML.DocumentElement.SelectSingleNode("/UnturnedSettings/AutomaticSave");
                return Convert.ToBoolean(node.Attributes["Enabled"]?.InnerText);
            }
            set
            {
                RocketUnturnedXML.DocumentElement.SelectSingleNode("/UnturnedSettings/AutomaticSave").Attributes["Enabled"].InnerText = value.ToString().ToLower();
            }
        }

        public string AutoSaveInterval
        {
            get
            {
                XmlNode node = RocketUnturnedXML.DocumentElement.SelectSingleNode("/UnturnedSettings/AutomaticSave");
                return node.Attributes["Interval"]?.InnerText;
            }
            set
            {
                RocketUnturnedXML.DocumentElement.SelectSingleNode("/UnturnedSettings/AutomaticSave").Attributes["Interval"].InnerText = value;
            }
        }


        public bool LogSuspiciousPlayerMovement
        {
            get
            {
                XmlNode node = RocketUnturnedXML.DocumentElement.SelectSingleNode("/UnturnedSettings/LogSuspiciousPlayerMovement");
                return Convert.ToBoolean(node.InnerText);
            }
            set
            {
                RocketUnturnedXML.DocumentElement.SelectSingleNode("/UnturnedSettings/LogSuspiciousPlayerMovement").InnerText = value.ToString().ToLower();
            }
        }

        public string MaxFrames
        {
            get
            {
                XmlNode node = RocketXML.DocumentElement.SelectSingleNode("/RocketSettings/MaxFrames");
                return node.InnerText;
            }
            set
            {
                RocketXML.DocumentElement.SelectSingleNode("/RocketSettings/MaxFrames").InnerText = value.ToString();
            }
        }

        public bool RCON
        {
            get
            {
                XmlNode node = RocketXML.DocumentElement.SelectSingleNode("/RocketSettings/RCON");
                return Convert.ToBoolean(node.Attributes["Enabled"]?.InnerText);
            }
            set
            {
                RocketXML.DocumentElement.SelectSingleNode("/RocketSettings/RCON").Attributes["Enabled"].InnerText = value.ToString().ToLower();
            }
        }

        public string RCONPort
        {
            get
            {
                XmlNode node = RocketXML.DocumentElement.SelectSingleNode("/RocketSettings/RCON");
                return node.Attributes["Port"]?.InnerText;
            }
            set
            {
                RocketXML.DocumentElement.SelectSingleNode("/RocketSettings/RCON").Attributes["Port"].InnerText = value.ToString();
            }
        }

        public string RCONPassword
        {
            get
            {
                XmlNode node = RocketXML.DocumentElement.SelectSingleNode("/RocketSettings/RCON");
                return node.Attributes["Password"]?.InnerText;
            }
            set
            {
                RocketXML.DocumentElement.SelectSingleNode("/RocketSettings/RCON").Attributes["Password"].InnerText = value.ToString();
            }
        }

        public bool AutoShutdown
        {
            get
            {
                XmlNode node = RocketXML.DocumentElement.SelectSingleNode("/RocketSettings/AutomaticShutdown");
                return Convert.ToBoolean(node.Attributes["Enabled"]?.InnerText);
            }
            set
            {
                RocketXML.DocumentElement.SelectSingleNode("/RocketSettings/AutomaticShutdown").Attributes["Enabled"].InnerText = value.ToString().ToLower();
            }
        }

        public string AutoShutdownInterval
        {
            get
            {
                XmlNode node = RocketXML.DocumentElement.SelectSingleNode("/RocketSettings/AutomaticShutdown");
                return node.Attributes["Interval"]?.InnerText;
            }
            set
            {
                RocketXML.DocumentElement.SelectSingleNode("/RocketSettings/AutomaticShutdown").Attributes["Interval"].InnerText = value.ToString();
            }
        }



    }



}
