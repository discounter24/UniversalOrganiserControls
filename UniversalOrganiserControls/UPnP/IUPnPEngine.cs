using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.UPnP
{

    public interface IUPnPEngine
    {
        
        UPnPSupportState State
        {
            get;
        }

        Task<UPnPSupportState> Prepare();

        Task<PortResult> OpenPort(UPnPPort port);
        Task<PortResult> ClosePort(UPnPPort port);
        Task<PortResult> CheckPort(UPnPPort port);

    }
}
