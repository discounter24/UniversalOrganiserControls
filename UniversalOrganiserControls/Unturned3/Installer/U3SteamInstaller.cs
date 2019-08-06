using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UniversalOrganiserControls.Steam;

namespace UniversalOrganiserControls.Unturned3.Installer
{
    public class U3SteamInstaller : SteamGameInstaller
    {
        public const int APP_ID = 1110390;

        public U3SteamInstaller(SteamInstance steam) : base(steam) {}

        public void installGame(string folder, bool validate=false)
        {
            base.installGame(APP_ID, folder, validate);
        }
        

    }
}
