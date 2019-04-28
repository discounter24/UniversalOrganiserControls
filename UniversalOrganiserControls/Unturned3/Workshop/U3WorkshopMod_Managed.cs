using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using HtmlAgilityPack;


namespace UniversalOrganiserControls.Unturned3.Workshop
{
    public class U3WorkshopMod_Managed
    {


        
        private DirectoryInfo path;

        public U3WorkshopMod_Managed(DirectoryInfo path)
        {
            this.path = path;
        }



        public String ID
        {
            get
            {
                return path.Name;
            }
        }

        public U3WorkshopModType Type
        {
            get
            {
                try
                {
                    return new FileInfo(path.GetDirectories()[0].FullName + "\\level.dat").Exists ? U3WorkshopModType.Map : U3WorkshopModType.Content;
                }
                catch (Exception)
                {
                    return U3WorkshopModType.Unresolveable;
                }
            }
        }

        public string[] Contents
        {
            get
            {
                List<string> contents = new List<string>();

                foreach (DirectoryInfo content in path.GetDirectories()) contents.Add(content.Name);


                return contents.ToArray<string>();
            }
        }

        private static string GetUpdateUrl(string SteamSiteUrl)
        {
            return string.Format("http://dl.unturned-server-organiser.com/mod.php?modURL={0}", SteamSiteUrl);
        }

        public void Delete()
        {
            try
            {
                path.Delete(true);
            }
            catch (Exception) { }
        }

        
        public string Name
        {
            get
            {
                if (Type == U3WorkshopModType.Map)
                {
                    if (path.GetDirectories().Length == 0)
                    {
                        return "Unknown";
                    }
                    else
                    {
                        return path.GetDirectories()[0].Name;
                    }
                }
                else
                {

                    FileInfo modinfo = new FileInfo(path.FullName + "\\mod.usoinfo");
                    if (modinfo.Exists)
                    {
                        return File.ReadAllLines(modinfo.FullName)[0];
                    }
                    else
                    {
                        var content = new List<string>();
                        content.Add(U3WorkshopMod.getModTitle(ID));
                        content.Add(ID);
                        try
                        {
                            File.WriteAllLines(modinfo.FullName, content.ToArray<string>());

                        }
                        catch (Exception)
                        {
                            return content[0];
                        }
                        return Name;

                    }
                }

            }
        }


        public static string[] GetIDsFromURL(string url)
        {
            try
            {
                string response = new WebClient().DownloadString(GetUpdateUrl(url));
                if (response == "invalid request")
                {
                    return new string[] { "invalid request" };
                }
                else
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<string[]>(response);
                    }
                    catch (Exception)
                    {
                        return new string[] { JsonConvert.DeserializeObject<string>(response) };
                    }
                }

            }
            catch (Exception)
            {
                return new string[0];
            }
           
        }

        



    }
}
