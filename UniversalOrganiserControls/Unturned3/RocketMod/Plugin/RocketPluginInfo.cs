﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace UniversalOrganiserControls.Unturned3.RocketMod.Plugin
{
    public class RocketPluginInfo
    {

        private List<string> DependencyLibFiles = new List<string>();


        [JsonIgnore]
        public IEnumerable<FileInfo> DependencyLibInfos
        {
            get
            {
                foreach(string lib in DependencyLibFiles)
                {
                    yield return new FileInfo(lib);
                }
            }
        }



        private string PluginFile = "";

        [JsonIgnore]   
        public FileInfo PluginFileInfo
        {
            get
            {
                return new FileInfo(PluginFile);
            }
        }

        [JsonIgnore]
        public DirectoryInfo PluginFolder
        {
            get
            {
                return PluginFileInfo.Directory;
            }
        }


        public string Website
        {
            get; set;
        }

        [JsonIgnore]
        public string ServerVersionUri
        {
            get
            {
                return string.Format("http://rocketmod.unturned-server-organiser.com/plugin.php?getServerVersion&pluginUrl={0}", Website);
            }
        }

        [JsonIgnore]
        public string ServerVersion
        {
            get
            {
                if (Website == "(unknown)") return "(mirror unknown)";

                try
                {
                    WebClient client = new WebClient();
                    string version = client.DownloadString(ServerVersionUri);

                    return version;
                }
                catch (Exception)
                {
                    return "(mirror not available)";
                }
            }
        }

        public string ClientVersion
        {
            get; set;
        }
        
        [JsonIgnore]
        public string DownloadLink
        {
            get
            {
                if (Website == "(unknown)") return "(mirror unknown)";
                try
                {
                    WebClient client = new WebClient();
                    string link = client.DownloadString(string.Format("http://rocketmod.unturned-server-organiser.com/plugin.php?getLink&pluginUrl={0}", Website));
                    
                    return link;
                }
                catch (WebException)
                {
                    return "(download inalid)";
                }
                catch
                {
                    return "(mirror unavailable)";
                }
            }
        }


        public RocketPluginInfo(FileInfo pluginFile, String website, String clientVersion, List<FileInfo> dependencyLibs)
        {
            this.PluginFile = pluginFile.FullName;
            this.Website = website == "" ? "(unknown)" : website;
            this.ClientVersion = clientVersion == "" ? "(unknown)" : clientVersion;

            foreach(FileInfo lib in dependencyLibs)
            {
                DependencyLibFiles.Add(lib.FullName);
            }
            
        }

    }


    
}
