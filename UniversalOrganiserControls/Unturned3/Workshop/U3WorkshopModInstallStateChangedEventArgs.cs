using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Unturned3.Workshop
{
   
    public class U3WorkshopModInstallStateChangedEventArgs
    {
        public U3WorkshopModInstallState State;
        public string ID;
        public string Name;

        public U3WorkshopModInstallStateChangedEventArgs(U3WorkshopModInstallState State, string ID, string Name="")
        {
            this.State = State;
            this.ID = ID;
            this.Name = Name;
        }
    }
}
