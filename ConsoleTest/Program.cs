using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalOrganiserControls;
using UniversalOrganiserControls.Backup;
using System.IO;
using UniversalOrganiserControls.Steam;
using UniversalOrganiserControls.UPnP;
using System.Diagnostics;
using UniversalOrganiserControls.Unturned3;
using UniversalOrganiserControls.Unturned3.Installer;

namespace ConsoleTest
{
    class Program
    {


        static void Main(string[] args)
        {
            U3OnlineInstaller installer = new U3OnlineInstaller(new DirectoryInfo("C:\\ubuntu"));
            installer.update();

            /*
            Process p = new Process();
            p.StartInfo.WorkingDirectory = @"C:\Users\pasca\Unturned Servers\";
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.OutputDataReceived += P_OutputDataReceived;

            p.Start();

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            Task.Run(() =>
            {
                p.StandardInput.WriteLine("steamcmd.exe");
                Task.Delay(2000).Wait();

                p.StandardInput.WriteLine("login anonymous");
                p.StandardInput.WriteLine(Environment.NewLine);
                Task.Delay(2000).Wait();
                
            });

            
            */



            while (true) Console.ReadKey();
        }

        private static void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private static void S_SteamOutput(object sender, string text)
        {
            Console.WriteLine(text);
        }
    }
}
