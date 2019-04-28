using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

using UniversalOrganiserControls.Unturned3;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;

namespace UniversalOrganiserControls.Unturned3.RocketMod
{

    public delegate void PlayerListUpdatedByRocketEvent(U3Server u3server, List<string> steamid);
    public delegate void OutputReceivedByRocketBridgeEvent(U3Server sender, string[] content);

    public delegate void ClientDisconnectedEvent(ClientConnection con);
    public delegate void ClientConnectedEvent(ClientConnection con);


    public delegate void U3ServerDisconnectedEvent(U3Server u3server);
    public delegate void U3ServerConnectedEvent(U3Server u3server);


    public class RocketBridgeServer
    {



        private TcpListener listener;
        private List<ClientConnection> clients = new List<ClientConnection>();
        

        private Task AcceptionLoop;
        private bool running = true;

        public event PlayerListUpdatedByRocketEvent PlayerListUpdated;
        public event U3ServerDisconnectedEvent U3ServerDisconnected;
        public event U3ServerConnectedEvent U3ServerConnected;

        public List<U3Server> AllowedServers { get; set; }

        public readonly int port;
        

        public RocketBridgeServer(int port, List<U3Server> AllowedServers)
        {
            this.port = port;
            this.listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, port));
            this.listener.Start();

            AcceptionLoop = Accept();
        }


        private Task Accept()
        {
            running = true;
            return Task.Run(() =>
            {
                while (running)
                {
                    try
                    {
                        ClientConnection con = new ClientConnection(listener.AcceptTcpClient(), AllowedServers);
                        con.ClientDisconnected += Con_ClientDisconnected;
                        con.ReceivedPlayerList += Con_ReceivedPlayerList;
                        con.InstanceConnected += Con_InstanceConnected;
                        clients.Add(con);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            });
        }

        private void Con_ReceivedPlayerList(U3Server sender, List<string> playerlist)
        {
            if (sender != null)
            {
                sender.PlayerList = playerlist;
            }
        }

        private void Con_InstanceConnected(U3Server instance)
        {
            U3ServerConnected?.Invoke(instance);
        }

        private void Con_ClientDisconnected(ClientConnection con)
        {
            clients.Remove(con);
            foreach(U3Server server in AllowedServers.Where((s) => { return s.ServerInformation.ServerID == con.sid; }))
            {
                U3ServerConnected?.Invoke(server);
            }

        }

        private void Diconnected(ClientConnection con)
        {
            clients.Remove(con);
        }


        public bool Send(U3Server userver, string command)
        {

            ClientConnection con = GetConnection(userver);
            if (con != null)
            {
                return con.Send(command);
            }
            else
            {
                return false;
            }

        }

        private ClientConnection GetConnection(U3Server userver)
        {
            foreach (ClientConnection client in clients)
            {
                if (client.sid.Equals(userver.ServerInformation.ServerID))
                {
                    return client;
                }
            }
            return null;
        }


        public bool IsConnected(U3Server userver)
        {
            foreach (ClientConnection client in clients)
            {
                if (client.sid.Equals(userver.ServerInformation.ServerID))
                {
                    return true;
                }
            }
            return false;
        }


        public static bool CheckPort(int port)
        {
            bool isAvailable = true;

            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }
            return isAvailable;

        }


        public void Shutdown()
        {
            running = false;
            listener.Stop();
        }


    }

    public class ClientConnection
    {

        public event ClientDisconnectedEvent ClientDisconnected;
        public event PlayerListUpdatedByRocketEvent ReceivedPlayerList;
        public event U3ServerConnectedEvent InstanceConnected;

        private TcpClient client;
        public string sid = "";
        List<U3Server> servers = new List<U3Server>();


        public ClientConnection(TcpClient client, List<U3Server> servers)
        {
            this.servers = servers;
            this.client = client;
            Task.Run(() =>
            {
                Listen();
            });
        }


        public U3Server GetU3Server(string id)
        {
            foreach(U3Server server in servers.Where((s) => { return s.ServerInformation.ServerID == id; }))
            {
                return server;
            }
            return null;
        }


        public bool Send(string cmd)
        {
            try
            {
                StreamWriter writer = new StreamWriter(client.GetStream());
                writer.WriteLine(cmd);
                writer.Flush();
                return true;
            }
            catch (Exception)
            {
                ClientDisconnected?.Invoke(this);
                return false;
            }
        }



        private void Listen()
        {
            while (client.Connected)
            {

                try
                {
                    StreamReader reader = new StreamReader(client.GetStream());
                    string line = reader.ReadLine();

                    lock (client)
                    {
                        if (line.StartsWith("sid://"))
                        {

                            U3Server s = GetU3Server(line.Remove(0, 6));
                            if (s.State == U3ServerState.Running | s.State == U3ServerState.Starting)
                            {
                                sid = line.Remove(0, 6);
                                InstanceConnected?.Invoke(GetU3Server(sid));
                            }
                            else
                            {
                                Send("unauthorized_server");
                                ClientDisconnected?.Invoke(this);
                            }

                        }
                        else if (line.StartsWith("players://"))
                        {
                            line = line.Remove(0, 10);
                            List<string> players = line.Split(';').ToList<string>();
                            players.Remove("");

                            U3Server instance = GetU3Server(sid);
                            ReceivedPlayerList?.Invoke(instance,players);
                        }
                        else if (line.StartsWith("consolelog://"))
                        {
                            Console.WriteLine(line);
                        }
                    }


                }
                catch (Exception) { }
            }
            ClientDisconnected?.Invoke(this);
        }

    }
}
