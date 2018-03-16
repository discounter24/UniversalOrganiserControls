using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalOrganiserControls;
using UniversalOrganiserControls.Backup;
using System.IO;

namespace ConsoleTest
{
    class Program
    {

        
        static void Main(string[] args)
        {
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
