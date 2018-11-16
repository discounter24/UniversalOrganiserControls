using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCBNetworking.Packages;
using UCBNetworking;

namespace UniversalOrganiserControls.Unturned3.UCB
{
    public class UCBManager
    {
        public class PackageReceivedEventArgs
        {
            public U3Server Server { get; private set; }
            public NetPackage Package { get; private set; }
            public PackageReceivedEventArgs(U3Server server, NetPackage package)
            {
                this.Server = server;
                this.Package = package;
            }
        }
        

        public event EventHandler<PackageReceivedEventArgs> PackageReceived;
        public event EventHandler<U3Server> ServerIdentified;
        public event EventHandler<U3Server> ServerDisconnected;

        public UCBServer server { get; private set; }

        //private List<UCBServerConnection> Identiefed = new List<UCBServerConnection>();
        public List<U3Server> AvailableServers { get; private set; }

  

        public Dictionary<UCBServerConnection, U3Server> IdentifiedServers = new Dictionary<UCBServerConnection, U3Server>();

        

        public UCBManager(int port)
        {
            AvailableServers = new List<U3Server>();

            server = new UCBServer(port);
            server.ConnectionIdentified += Server_ConnectionIdentified;
            server.PackageReceived += Server_PackageReceived;
            server.ConnetionClosed += Server_ConnetionClosed;
        }

        public void shutdown()
        {
            try
            {
                server.shutdown();
            }
            catch (Exception)  { }
        
        }

        private void Server_ConnetionClosed(object sender, UCBServerConnection e)
        {
            if (IdentifiedServers.ContainsKey(e))
            {
                ServerDisconnected?.Invoke(this, IdentifiedServers[e]);
                IdentifiedServers.Remove(e);
            }


        }

        private void Server_PackageReceived(object sender, UCBServer.UCBServerPackageRecveivedEventArgs e)
        {
            if (IdentifiedServers.ContainsKey(e.Connection))
            {
                PackageReceivedEventArgs arg = new PackageReceivedEventArgs(IdentifiedServers[e.Connection], e.Package);
                PackageReceived?.Invoke(this, arg);
            }
        }

        private void Server_ConnectionIdentified(object sender, UCBServerConnection e)
        {
            foreach(U3Server server in AvailableServers)
            {
                if (server.LastPID == e.PID)
                {
                    if (!IdentifiedServers.ContainsKey(e))
                    {
                        IdentifiedServers.Add(e, server);
                        ServerIdentified?.Invoke(this, server);
                        return;
                    }
                }
            }
        }

        public bool isIdentifed(U3Server server)
        {
            return IdentifiedServers.ContainsValue(server);
        }

        public UCBServerConnection getConnection(U3Server server)
        {
            if (isIdentifed(server))
            {
                foreach (UCBServerConnection con in IdentifiedServers.Keys)
                {
                    if (IdentifiedServers[con] == server)
                    {
                        return con;
                    }
                }
            }
            return null;
        }


        public bool sendPackage(U3Server server, NetPackage package)
        {
            if (isIdentifed(server))
            {
                try
                {

                    UCBServerConnection con = getConnection(server);
                    if (con != null)
                    {
                        con.send(package);
                        return true;
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool sendCommand(U3Server server, string command)
        {

            SPackage package = new SPackage(NetPackageHeader.Command, command);
            return sendPackage(server,package);
        }
                    
    }
}
