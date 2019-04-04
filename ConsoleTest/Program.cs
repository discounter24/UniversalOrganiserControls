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
            installer.InstallationProgressChanged += Installer_InstallationProgressChanged;
            installer.Validate = true;
            installer.update();




            while (true) Console.ReadKey();
        }

        private static void Installer_InstallationProgressChanged(object sender, U3OnlineInstallationProgressArgs e)
        {
            Console.Clear();
            switch (e.state)
            {
                case U3InstallationState.SearchingUpdates:
                    Console.WriteLine("Searching for updates..");
                    break;
                case U3InstallationState.CalculatingFileDifferences:
                    Console.WriteLine("Calculating changes..");
                    break;
                case U3InstallationState.Downloading:
                    Console.WriteLine(string.Format("Updating files {0}/{1}..",e.processed,e.total));
                    break;
                case U3InstallationState.Ok:
                    Console.WriteLine("Finished successfully!");
                    break;
                case U3InstallationState.FailedSome:
                    Console.WriteLine(string.Format("Finished with {0} failed executions.", e.processed, e.total));
                    break;
                case U3InstallationState.FailedInternet:
                    Console.WriteLine("Failed: internet connection");
                    break;
                case U3InstallationState.FailedUnknown:
                    Console.WriteLine("Failed: unknown");
                    break;
                case U3InstallationState.FailedInvalidResponse:
                    Console.WriteLine("Failed: invalid response");
                    break;
                default:
                    break;
            }
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
