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
            State = UPnPSupportState.NoPrepared;
        }

        public Task<UPnPSupportState> Prepare()
        {
            return Task<UPnPSupportState>.Run(async () =>
            {
                try
                {
                    manager = new UPnPNAT();
                    UPnPPort testPort = new UPnPPort(1,PortType.TCP,Utils.GetLocalIP());
                  
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
                            State = UPnPSupportState.NoPrepared;
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
                if (manager.StaticPortMappingCollection != null)
                {
                    foreach (IStaticPortMapping mapping in manager.StaticPortMappingCollection)
                    {
                        ports.Add(convert(mapping));
                    }
                }

                return ports;
            });
        }

        private UPnPPort convert(IStaticPortMapping mapping)
        {
            UPnPPort port = new UPnPPort((UInt16)mapping.InternalPort, PortType.TCP, "");

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
            return port;
        }

        public Task<PortResult> ClosePort(UPnPPort port)
        {
            return Task<PortResult>.Run(async() =>
            {
                switch (State)
                {
                    case UPnPSupportState.Supported:
                        try
                        {
                            List<UPnPPort> ports = await getOpenedPorts();

                            if (!ports.Contains(port))
                            {
                                return PortResult.AlreadyClosed;
                            }

                            IStaticPortMappingCollection maps = manager.StaticPortMappingCollection;
                            foreach(IStaticPortMapping map in maps)
                            {
                                UPnPPort port2 = convert(map);
                                if (port==port2)
                                {
                                    maps.Remove(map.ExternalPort,map.Protocol);
                                    return PortResult.Closed;
                                }
                            }
                            return PortResult.FailedUnknown;
                        }
                        catch (Exception)
                        {
                            return PortResult.FailedUnknown;
                        }
                    case UPnPSupportState.NotSupported:
                        return PortResult.EngineNotSupported;
                    case UPnPSupportState.NoPrepared:
                        return PortResult.EngineNotPrepared;
                    default:
                        break;
                }

                return PortResult.FailedUnknown;
            });
        }

        public Task<PortResult> OpenPort(UPnPPort port)
        {
            return Task<PortResult>.Run(async () =>
            {
                try
                {
                    switch (State)
                    {
                        case UPnPSupportState.Supported:

                            try
                            {
                                List<UPnPPort> ports = await getOpenedPorts();

                                if (ports.Contains(port))
                                {
                                    return PortResult.AlreadyOpened;
                                }
                         
                                IStaticPortMappingCollection maps = manager.StaticPortMappingCollection;
                                switch (port.Type)
                                {
                                    case PortType.TCP:
                                        maps.Add(port.ExternalPort, "TCP", port.InternalPort, port.InternalAddress, true, port.Description);
                                        break;
                                    case PortType.UDP:
                                        maps.Add(port.ExternalPort, "UDP", port.InternalPort, port.InternalAddress, true, port.Description);
                                        break;
                                    case PortType.BOTH:
                                        maps.Add(port.ExternalPort, "TCP", port.InternalPort, port.InternalAddress, true, port.Description);
                                        maps.Add(port.ExternalPort, "UDP", port.InternalPort, port.InternalAddress, true, port.Description);
                                        break;
                                    default:
                                        break;
                                }
                                return PortResult.Opened;

                            }
                            catch (Exception)
                            {
                                return PortResult.FailedUnknown;
                            }
                            
                        case UPnPSupportState.NotSupported:
                            return PortResult.EngineNotSupported;
                        case UPnPSupportState.NoPrepared:
                            return PortResult.EngineNotPrepared;
                        default:
                            return PortResult.FailedUnknown;
                    }
                }
                catch (Exception)
                {
                    return PortResult.FailedUnknown;
                }
                
            });
        }


    }


}
