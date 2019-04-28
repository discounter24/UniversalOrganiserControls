using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.UPnP
{
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public class UPnPPort
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
    {
        public string Description;
        public PortType Type;

        public UInt16 ExternalPort;

        public UInt16 InternalPort;
        public string InternalAddress;

        public UPnPPort(UInt16 port, PortType type, String InternalAddress, String description = "")
        {
            this.InternalPort = port;
            this.ExternalPort = port;
            this.Type = type;
            this.InternalAddress = InternalAddress;
            this.Description = description;
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
