using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalOrganiserControls;
using UniversalOrganiserControls.Backup;
using System.IO;
using UniversalOrganiserControls.Steam;

namespace ConsoleTest
{
    class Program
    {


        static void Main(string[] args)
        {
            Console.Title = "Unturned engine updater";

            SteamInstance.killAll();
            
            Console.WriteLine("Updating steam modules..");
            SteamInstaller installer = new SteamInstaller(@"C:\USOServerFiles\");
            if (!installer.Installed)
            {
                installer.installSteam();
                SteamInstance.killAll();
            }



            Console.Clear();
            Console.WriteLine("Initiating steam modules..");
            SteamInstance instance = new SteamInstance(new FileInfo(@"C:\USOServerFiles\steamcmd.exe"));

            Console.Clear();
            Console.WriteLine("Login..");
            LoginResult r = instance.login("deventuretech4", "$openPassword$","",30000);


            switch (r)
            {
                case LoginResult.OK:
                    Console.Clear();

                    Console.WriteLine("Initiating installer..");

                    SteamGameInstaller ginstaller = new SteamGameInstaller(instance);
                    ginstaller.AppUpdateStateChanged += Ginstaller_AppUpdateStateChanged;

                    Console.Clear();
                    Console.WriteLine("Installing..");
                    ginstaller.installGame(304930, "game", true);
                    ginstaller.AppUpdated += Ginstaller_AppUpdated;

                    break;
                default:
                    Console.WriteLine(r);
                    break;
            }


            while (true)
            {
                Console.ReadLine();
            }


            /*
            while(true)
            {
                Console.Clear();

                string source = @"C:\Users\Pascal Devant\Documents\GitHub\MinecraftServerOrganiser\";
                string target = @"C:\backups\";

                BackupSpace space = new BackupSpace(new DirectoryInfo(target));
                foreach (BackupPackage b in space.GetBackups())
                {
                    Console.WriteLine(string.Format("Identifier: {0} -- Game: {1} -- Date: {2} -- Hash: {3}",b.BackupInfo.CustomIdentifier,b.BackupInfo.GameName,b.BackupInfo.Created.ToString(),b.BackupInfo.Hash));
                }

                Console.ReadLine();

                BackupProcess p = space.Create(source, "MSO", "Server Organiser");
                p.BackupProgressChanged += P_BackupProgressChanged;
                

                Console.ReadLine();
            }
            */

            /*
            ProcessProperties props = new ProcessProperties();
            props.Executable = new System.IO.FileInfo("cmd.exe");
            UniversalProcess p = new UniversalProcess(props);
            p.OutputDataReceived += Cmd_OutputDataReceived;
            p.Start();
           
            string cmd = "";
            do
            {
                cmd = Console.ReadLine();
            } while (cmd != "exit");
            p.Kill();
            */



        }

        private static void Ginstaller_AppUpdated(object sender, bool error = false)
        {
            Console.WriteLine("Installing..");
            SteamGameInstaller ginstaller = (SteamGameInstaller)sender;

            ginstaller.installGame(304930, "gameVanilla", true);
            ginstaller.AppUpdated -= Ginstaller_AppUpdated;
            ginstaller.AppUpdated += Ginstaller_AppUpdated1;

        }

        private static void Ginstaller_AppUpdated1(object sender, bool error = false)
        {
            Console.Clear();
            Console.WriteLine("Unturned updated.");
        }

        private static void Instance_SteamOutput(object sender, string text)
        {
            Console.Clear();
            Console.WriteLine(text);
        }

        private static void Ginstaller_Output(object sender, string e)
        {
           
        }

        private static void Ginstaller_AppUpdateStateChanged(object sender, SteamAppUpdateState state)
        {
            Console.Clear();
            switch (state.stage)
            {
                case UpdateStateStage.Validating:
                    Console.WriteLine(string.Format("Validating game files {0}%..", state.percentage));
                    break;
                case UpdateStateStage.Downloading:
                    Console.WriteLine(string.Format("Downloading game files {0}%..", state.percentage));
                    break;
                case UpdateStateStage.Commiting:
                    Console.WriteLine(string.Format("Commiting game files {0}%..", state.percentage));
                    break;
                case UpdateStateStage.Preallocating:
                    Console.WriteLine(string.Format("Preallocating game files {0}%..", state.percentage));
                    break;
                default:
                    break;
            }
        }

        private static void P_BackupProgressChanged(object sender, BackupProgressArgs e)
        {
            Console.WriteLine(e.State.ToString() + ", done: " + e.FilesDone + ", total: " + e.FileTotal);
        }

        private static void Cmd_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}
