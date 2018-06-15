using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.UPnP
{

    public enum PortType
    {
        TCP,
        UDP,
        BOTH
    }

    public enum UPnPSupportState
    {
        Supported,
        NotSupported,
        NoPrepared
    }

    public enum PortResult
    {
        EngineNotPrepared,
        EngineNotSupported,

        Opened,
        Closed,
        AlreadyOpened,
        AlreadyClosed,
        FailedNoGateway,
        FailedUnknown
    }

}
