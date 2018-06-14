using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NATUPNPLib;

namespace UniversalOrganiserControls.UPnP
{
    public class WindowsUPnP : IUPnPEngine
    {
        UPnPNAT manager;

        
        public UPnPSupportState State {
            get;set;
        }

        public WindowsUPnP()
        {
            State = UPnPSupportState.Unknown;
        }

        public Task<UPnPSupportState> Prepare()
        {
            return Task<UPnPSupportState>.Run(async () =>
            {
                try
                {
                    manager = new UPnPNAT();
                    UPnPPort testPort = new UPnPPort();
                    testPort.InternalPort = 1;

                    PortResult result = await CheckPort(testPort);
                    switch (result)
                    {
                        case PortResult.Opened:
                            //No more checks needed
                            State = UPnPSupportState.Supported;
                            break;
                        case PortResult.Closed:
                            //Let us check if we are able to open and close the test port.
                            PortResult r = await OpenPort(testPort);
                            if (r == PortResult.Opened)
                            {
                                r = await ClosePort(testPort);
                                if (r == PortResult.Closed)
                                {
                                    State = UPnPSupportState.Supported;
                                }
                                else
                                {
                                    State = UPnPSupportState.NotSupported;
                                }
                            }
                            else
                            {
                                State = UPnPSupportState.NotSupported;
                            }

                            break;
                        default:
                            //Something went wrong
                            State = UPnPSupportState.Unknown;
                            break;
                    }

                }
                catch (Exception)
                {
                    State = UPnPSupportState.NotSupported;
                }

                return State;
            });
        }

        public Task<PortResult> CheckPort(UPnPPort port)
        {
            return Task<PortResult>.Run(async () =>
            {
                try
                {
                    List<UPnPPort> ports = await getOpenedPorts();

                    foreach(UPnPPort p in ports)
                    {
                        if (p == port) return PortResult.Opened;
                    }

                    return PortResult.Closed;
                }
                catch (Exception)
                {
                    return PortResult.FailedUnknown;
                }

            });
        }

        public Task<List<UPnPPort>> getOpenedPorts()
        {
            return Task<List<UPnPPort>>.Run(()=>
            {
                List<UPnPPort> ports = new List<UPnPPort>();

                IDynamicPortMappingCollection dynMaps = manager.DynamicPortMappingCollection;
                IStaticPortMappingCollection staticMaps = manager.StaticPortMappingCollection;


                if (dynMaps != null)
                {
                    foreach (IDynamicPortMapping mapping in dynMaps)
                    {
                        UPnPPort port = new UPnPPort();

                        port.Description = mapping.Description;
                        port.ExternalPort = (UInt16)mapping.ExternalPort;
                        port.InternalPort = (UInt16)mapping.InternalPort;
                        switch (mapping.Protocol.ToUpper())
                        {
                            case "TCP":
                                port.Type = PortType.TCP;
                                break;
                            case "UDP":
                                port.Type = PortType.UDP;
                                break;
                            default:
                                port.Type = PortType.BOTH;
                                break;
                        }
                        
                        ports.Add(port);
                    }
                }

                if (staticMaps != null)
                {
                    foreach (IStaticPortMapping mapping in staticMaps)
                    {
                        UPnPPort port = new UPnPPort();
                      
                        port.Description = mapping.Description;
                        port.ExternalPort = (UInt16)mapping.ExternalPort;
                        port.InternalPort = (UInt16)mapping.InternalPort;
                        switch (mapping.Protocol.ToUpper())
                        {
                            case "TCP":
                                port.Type = PortType.TCP;
                                break;
                            case "UDP":
                                port.Type = PortType.UDP;
                                break;
                            default:
                                port.Type = PortType.BOTH;
                                break;
                        }

                        ports.Add(port);
                    }
                }

                return ports;
            });
        }


        public Task<PortResult> ClosePort(UPnPPort port)
        {
            return Task<PortResult>.Run(() =>
            {

                return PortResult.FailedUnknown;
            });
        }

        public Task<PortResult> OpenPort(UPnPPort port)
        {
            return Task<PortResult>.Run(() =>
            {

                return PortResult.FailedUnknown;
            });
        }

    }


}
