using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UniversalOrganiserControls.Unturned3.RocketMod.Plugin
{
    public class RocketPlugin
    {
        public RocketPluginInfo Info;


        public FileInfo File
        {
            get
            {
                return Info.PluginFileInfo;
            }
        }

        public string ClientVersion
        {
            get { return Info.ClientVersion == "" ? "(unknown)" : Info.ClientVersion; }
        }

        public string ServerVersion
        {
            get { return Info.ServerVersion;  }
        }


        public bool UpdateAvailable
        {
            get { return !ClientVersion.Equals(ServerVersion); }
        }

        public List<FileInfo> DependencyLibs
        {
            get { return Info.DependencyLibInfos.ToList<FileInfo>(); }
        }

        public string Name
        {
            get
            {
                return File.Name.Substring(0, File.Name.Length - (Active ? File.Extension.Length : File.Extension.Length + 4));
            }
        }

        public bool Active
        {
            get
            {
                return File.Extension.Equals(".dll");
            }
            set
            {
                if (value != Active)
                {
                    string newName = File.Directory.FullName + "\\" + Name + (value ? ".dll" : ".dll.inactive");

                    if (System.IO.File.Exists(newName)) System.IO.File.Delete(newName);
                    File.MoveTo(newName);
                }

            }
        }



        public RocketPlugin(RocketPluginInfo info)
        {
            this.Info = info;
        }

    }
}
