using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Unturned3
{

    public delegate void RocketBridgeConnectionStateChanged(object instance, bool state);


    public class U3ServerRenamedArgs
    {
        public U3Server Server;
        public string NewName;
        public string OldName;

        public U3ServerRenamedArgs(U3Server Server, string NewName, string OldName)
        {
            this.Server = Server;
            this.NewName = NewName;
            this.OldName = OldName;
        }
    }

    public class U3OnlineInstallationProgressArgs
    {
        public U3InstallationState state;
        public int processed;
        public int total;

        public double percentage
        {
            get
            {
                double d = Convert.ToDouble(processed) / Convert.ToDouble(total) * 100d;
                return Math.Round(d, 2); 
            }
        }

        public U3OnlineInstallationProgressArgs(U3InstallationState state, int processed = 0, int total = 0, int errors = 0)
        {
            this.state = state;
            this.processed = processed;
            this.total = total;
        }
    }

    public class U3OnlineInstallerAskForUserToAcceptUpdate
    {
        public bool cancel = false;
    }




}
