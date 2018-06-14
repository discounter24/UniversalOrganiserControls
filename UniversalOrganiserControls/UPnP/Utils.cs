using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace UniversalOrganiserControls.UPnP
{
    public static class Utils
    {
        public static string GetLocalIP()
        {
            TcpClient client = null;
            EndPoint localEndpoint = null;
            String localIp = null;

            try
            {
                client = new TcpClient();
                client.Connect("www.google.com", 80);
                localEndpoint = client.Client.LocalEndPoint;
                client.Close();
            }
            catch (SocketException)
            {
                client = new TcpClient();
                client.Connect("www.microsoft.com", 80);
                localEndpoint = client.Client.LocalEndPoint;
                client.Close();
            }
            catch (Exception)
            {
                return null;
            }


            if (localEndpoint != null)
            {
                localIp = localEndpoint.ToString();
                int end = localIp.IndexOf(":");
                return (localIp.Remove(end));
            }
            return null;
        }

    }
}
