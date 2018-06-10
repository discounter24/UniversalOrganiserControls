using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Steam
{
    public static class Utils
    {
        public static Dictionary<string, string> publicSteamAccounts
        {
            get
            {
                Dictionary<string, string> steamAccounts;

                try
                {
                    string json = new UniversalWebClient().DownloadString("http://dl.unturned-server-organiser.com/steam_login.php");
                    steamAccounts = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                }
                catch (Exception)
                {
                    steamAccounts = new Dictionary<string, string>();
                    steamAccounts.Add("unturnedrocksupdate", "force_update");
                }
                return steamAccounts;
            }
        }

    }
}
