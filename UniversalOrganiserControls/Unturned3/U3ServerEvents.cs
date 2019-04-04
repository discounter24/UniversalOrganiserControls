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
        U3Server Server;
        string NewName;
        string OldName;

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

        public U3OnlineInstallationProgressArgs(U3InstallationState state, int processed = 0, int total = 0)
        {
            this.state = state;
            this.processed = processed;
            this.total = total;
        }
    }




}
