using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrganiserNetworking.Packages;
using OrganiserNetworking;

namespace UniversalOrganiserControls.Unturned3.UCB
{
    public class UCBManager
    {
        public class PackageReceivedEventArgs
        {
            public U3Server Server { get; private set; }
            public OPackage Package { get; private set; }
            public PackageReceivedEventArgs(U3Server server, OPackage package)
            {
                this.Server = server;
                this.Package = package;
            }
        }
        

        public event EventHandler<PackageReceivedEventArgs> PackageReceived;
        public event EventHandler<U3Server> ServerIdentified;
        public event EventHandler<U3Server> ServerDisconnected;

        public UCBServer Server { get; private set; }

        public List<U3Server> AvailableServers { get; private set; }

  

        public Dictionary<UCBServerConnection, U3Server> IdentifiedServers = new Dictionary<UCBServerConnection, U3Server>();

        

        public UCBManager(int port)
        {
            AvailableServers = new List<U3Server>();

            Server = new UCBServer(port);
            Server.ConnectionIdentified += Server_ConnectionIdentified;
            Server.PackageReceived += Server_PackageReceived;
            Server.ConnetionClosed += Server_ConnetionClosed;
        }

        public void Shutdown()
        {
            try
            {
                Server.shutdown();
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

        public bool IsIdentifed(U3Server server)
        {
            return IdentifiedServers.ContainsValue(server);
        }

        public UCBServerConnection GetConnection(U3Server server)
        {
            if (IsIdentifed(server))
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


        public bool SendPackage(U3Server server, OPackage package)
        {
            if (IsIdentifed(server))
            {
                try
                {

                    UCBServerConnection con = GetConnection(server);
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

        public bool SendCommand(U3Server server, string command)
        {

            StringPackage package = new StringPackage(NetPackageHeader.Command, command);
            return SendPackage(server,package);
        }
                    
    }
}
