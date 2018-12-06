using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Translation
{
    public class Translation
    {
        private Dictionary<string, string> dict = new Dictionary<string, string>();



        public string getJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public void save(string file)
        {
            File.WriteAllText(file, getJson());
        }

        public static Translation load(string file)
        {
            try
            {
                return JsonConvert.DeserializeObject<Translation>(File.ReadAllText(file));
            }
            catch (Exception)
            {
                return null;
            }
        }

        [JsonIgnore]
        public string this[string key]
        {
            get
            {
                if (dict.Keys.Contains(key.ToUpper()))
                {
                    return dict[key.ToUpper()];
                }
                else
                {
                    return "NO_TRANSLATION_FOUND";
                }
            }
            set
            {
                dict[key.ToUpper()] = value;
            }
        }


    }
}
