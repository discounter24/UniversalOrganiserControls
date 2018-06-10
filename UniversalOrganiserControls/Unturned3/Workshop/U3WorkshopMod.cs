using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using HtmlAgilityPack;


namespace UniversalOrganiserControls.Unturned3.Workshop
{
    public class U3WorkshopMod
    {
        public static string updateUrl = string.Format("http://dl.unturned-server-organiser.com/mod.php?modURL={0}", url);
        private DirectoryInfo path;

        public U3WorkshopMod(DirectoryInfo path)
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
                    return new FileInfo(path.GetDirectories()[0].FullName + "\\level.dat").Exists ? ModType.Map : ModType.Content;
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


        public void Delete()
        {
            try
            {
                path.Delete(true);
            }
            catch (Exception) { }
        }


        string _name;
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
                        List<string> content = new List<string>();
                        content.Add(getModTitle(ID));
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


        public static string[] getIDsFromURL(string url)
        {
            try
            {
                string response = new WebClient().DownloadString(updateUrl);
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

        
        public static string getModTitle(string id)
        {
            try
            {
                WebClient client = new WebClient();
                Uri modUri = new Uri("http://steamcommunity.com/sharedfiles/filedetails/?id=" + id);

                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                string doctext = client.DownloadString(modUri);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(doctext);


                IEnumerable<HtmlNode> elements = doc.DocumentNode.Descendants("div").Where(d =>
                    d.Attributes.Contains("class") && d.Attributes["class"].Value.Equals("workshopItemTitle")
                );

                foreach (HtmlNode node in elements)
                {
                    return WebUtility.UrlDecode(node.InnerText);
                }
                return id;
            }
            catch (Exception)
            {
                return string.Format("NoNameAvailable[{0}]", id);
            }

        }


    }
}
