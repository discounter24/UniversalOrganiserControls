using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;

namespace UniversalOrganiserControls.Unturned3.Workshop
{
    public class U3WorkshopMod
    {
        private static string API_MODINFO = "http://dl.unturned-server-organiser.com/workshop/workshopFileInfo.php?id={0}";
        private static string API_MODID = "http://dl.unturned-server-organiser.com/workshop/getModIDFromSite.php?modURL={0}";

        public DirectoryInfo Path
        {
            get;
            private set;
        }

        public U3WorkshopModType Type
        {
            get
            {
                if (Path.Exists)
                {
                    if (File.Exists(Path.FullName + "\\Map.meta"))
                    {
                        return U3WorkshopModType.Map;
                    }
                    else
                    {
                        return U3WorkshopModType.Content;
                    }
                }

                return U3WorkshopModType.Unresolveable;
            }
        }
        
        public IEnumerable<string> Tags
        {
            get => getModTags(ID);
        }

        public string ID
        {
            get => Path.Name;
        }

        public string Title
        {
            get => getModTitle(ID);
        }
        

        public string ImageURL
        {
            get => getModImage(ID);
        }

        public U3WorkshopMod(string path)
        {
            this.Path = new DirectoryInfo(path);
        }


        public void Delete()
        {
            Path.Delete(true);
        }


        public static string getModTitle(string id)
        {
            try
            {
                string url = string.Format(API_MODINFO, id);
                string result = new WebClient().DownloadString(url);
                if (result != "STEAM_ERROR")
                {
                    JObject o = JObject.Parse(result);
                    JArray arr = JArray.Parse(o["response"]["publishedfiledetails"].ToString());
                    JToken token = arr.Children().First();
                    return token["title"].ToString();
                } else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                return string.Format("Unresolveable", id);
            }
        }

        public static string getModImage(string id)
        {
            try
            {
                string url = string.Format(API_MODINFO, id);
                string result = new WebClient().DownloadString(url);
                if (result != "STEAM_ERROR")
                {
                    JObject o = JObject.Parse(result);
                    JArray arr = JArray.Parse(o["response"]["publishedfiledetails"].ToString());
                    JToken token = arr.Children().First();
                    return token["preview_url"].ToString();
                }
                else
                {
                    return result;
                }
            }
            catch (Exception)
            {
                return string.Format("Unresolveable", id);
            }
        }

        public static IEnumerable<string> getModTags(string id)
        {
            try
            {
                string url = string.Format(API_MODINFO, id);

                string result = new WebClient().DownloadString(url);
                if (result != "STEAM_ERROR")
                {
                    JObject o = JObject.Parse(result);
                    JArray arr = JArray.Parse(o["response"]["publishedfiledetails"].ToString());
                    JToken token = arr.Children().First();
                   
                    foreach(JObject child in token["tags"].Children())
                    {
                        yield return child["tag"].ToString();
                    }
                }
            } finally { }
        }

        public static string[] getIDFromWorkshopSite(string url)
        {
            try
            {
                string updateURL = string.Format(API_MODID, url);
                string response = new WebClient().DownloadString(updateURL);


                if (response == "invalid request")
                {
                    return new string[] { };
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
                return new string[] { };
            }
        }
    }
}
