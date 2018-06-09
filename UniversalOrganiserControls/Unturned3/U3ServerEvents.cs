using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Unturned3
{

    public delegate void ServerIDChanged(object sender, U3ServerRenamedArgs e);

    public delegate void ServerStateChanged(object sender, U3ServerState state);
    public delegate void PlayerlistUpdated(object sender, List<string> playerlist);
    public delegate void UPnPStateChanged(object sender, bool value);

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


}
