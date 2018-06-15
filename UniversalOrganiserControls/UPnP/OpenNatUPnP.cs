using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Open.Nat;

namespace UniversalOrganiserControls.UPnP
{
    public class OpenNatUPnP : IUPnPEngine
    {
        public UPnPSupportState State { get; private set; }

        private NatDiscoverer discoverer;
        private NatDevice device;

        public Task<PortResult> CheckPort(UPnPPort port)
        {
            return Task.Run(async () =>
            {
                if (State == UPnPSupportState.NoPrepared)
                {
                    return PortResult.EngineNotPrepared;
                }
                else if (State == UPnPSupportState.NotSupported)
                {
                    return PortResult.EngineNotSupported;
                }
                else
                {
                    try
                    {
                        IEnumerable<Mapping> mappings = await device.GetAllMappingsAsync();
                        foreach(Mapping map in mappings)
                        {
                            if (map.PublicPort == port.ExternalPort)
                            {
                                if (port.Type== PortType.BOTH)
                                {
                                    return PortResult.Opened;
                                }
                                else if (port.Type == PortType.TCP && map.Protocol == Protocol.Tcp)
                                {
                                    return PortResult.Opened;
                                }
                                else if (port.Type == PortType.UDP && map.Protocol == Protocol.Udp)
                                {
                                    return PortResult.Opened;
                                }
                                else
                                {
                                    return PortResult.Closed;
                                }
                            }
                        }
                        return PortResult.Closed;
                    }
                    catch (Exception)
                    {
                        return PortResult.FailedUnknown;
                    }
                }
            });
        }

        public Task<PortResult> ClosePort(UPnPPort port)
        {
            return Task.Run(async () =>
            {
                if (State == UPnPSupportState.NoPrepared)
                {
                    return PortResult.EngineNotPrepared;
                }
                else if (State == UPnPSupportState.NotSupported)
                {
                    return PortResult.EngineNotSupported;
                }
                else
                {
                    try
                    {
                        PortResult opened = await CheckPort(port);
                        switch (opened)
                        {
                            case PortResult.Opened:
                                switch (port.Type)
                                {
                                    case PortType.TCP:
                                        await device.DeletePortMapAsync(new Mapping(Protocol.Tcp, port.InternalPort, port.ExternalPort, port.Description));
                                        return PortResult.Closed;
                                    case PortType.UDP:
                                        await device.DeletePortMapAsync(new Mapping(Protocol.Udp, port.InternalPort, port.ExternalPort, port.Description));
                                        return PortResult.Closed;
                                    case PortType.BOTH:
                                        await device.DeletePortMapAsync(new Mapping(Protocol.Tcp, port.InternalPort, port.ExternalPort, port.Description));
                                        await device.DeletePortMapAsync(new Mapping(Protocol.Udp, port.InternalPort, port.ExternalPort, port.Description));
                                        return PortResult.Closed;
                                    default:
                                        return PortResult.FailedUnknown;
                                }
                            case PortResult.Closed:
                                return PortResult.AlreadyClosed;
             
                            default:
                                return PortResult.FailedUnknown;
                        }
                    }
                    catch (Exception)
                    {
                        return PortResult.FailedUnknown;
                    }
                }
            });
        }

        public Task<PortResult> OpenPort(UPnPPort port)
        {
            return Task.Run(async () =>
            {
                if (State==UPnPSupportState.NoPrepared)
                {
                    return PortResult.EngineNotPrepared;
                }
                else if (State==UPnPSupportState.NotSupported)
                {
                    return PortResult.EngineNotSupported;
                }
                else
                {
                    try
                    {
                        PortResult opened = await CheckPort(port);
                        switch (opened)
                        {
                            case PortResult.Opened:
                                return PortResult.AlreadyOpened;
                            case PortResult.Closed:
                                switch (port.Type)
                                {
                                    case PortType.TCP:
                                        await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, port.InternalPort, port.ExternalPort, port.Description));
                                        return PortResult.Opened;
                                    case PortType.UDP:
                                        await device.CreatePortMapAsync(new Mapping(Protocol.Udp, port.InternalPort, port.ExternalPort, port.Description));
                                        return PortResult.Opened;
                                    case PortType.BOTH:
                                        await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, port.InternalPort, port.ExternalPort, port.Description));
                                        await device.CreatePortMapAsync(new Mapping(Protocol.Udp, port.InternalPort, port.ExternalPort, port.Description));
                                        return PortResult.Opened;
                                    default:
                                        return PortResult.FailedUnknown;
                                }
                            default:
                                return PortResult.FailedUnknown;
                        }
                    }
                    catch (Exception)
                    {
                        return PortResult.FailedUnknown;
                    }
                }
            });
     
        }

        public Task<UPnPSupportState> Prepare()
        {
            return Task.Run(async () =>
            {
                try
                {
                    discoverer = new NatDiscoverer();
                    device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, new CancellationTokenSource(10000));


                    if (device != null)
                    {
                        State = UPnPSupportState.Supported;
                        return UPnPSupportState.Supported;
                    }
                }
                catch (Exception)  { }

                State = UPnPSupportState.NotSupported;
                return UPnPSupportState.NotSupported;
            });
        }
    }
}
