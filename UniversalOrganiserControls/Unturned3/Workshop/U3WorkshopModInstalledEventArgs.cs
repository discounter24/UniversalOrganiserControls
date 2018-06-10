using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Unturned3.Workshop
{
    public class U3WorkshopModInstalledEventArgs
    {
        public string ID;
        public string Name;
        public bool Successfull;
        public bool QueueEmpty;

        public U3WorkshopModInstalledEventArgs(string ID, string Name, bool Successfull, bool QueueEmpty)
        {
            this.ID = ID; ;
            this.Name = Name;
            this.Successfull = Successfull;
            this.QueueEmpty = QueueEmpty;
        }
    }
}                                  
