using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.UPnP
{
    public class UPnPPort
    {
        public string Description;
        public PortType Type;

        public UInt16 ExternalPort;

        public UInt16 InternalPort;
        public string InternalAddress;

        public UPnPPort(UInt16 port, PortType type, String InternalAddress)
        {
            this.InternalPort = port;
            this.ExternalPort = port;
            this.Type = type;
            this.InternalAddress = InternalAddress;
        }

        public static bool operator ==(UPnPPort a, UPnPPort b)
        {
            if (a.ExternalPort!=b.ExternalPort) return false;

            return true;
        }

        public static bool operator !=(UPnPPort a, UPnPPort b)
        {
            return !(a == b);
        }

     
    }
}
