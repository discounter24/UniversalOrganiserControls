using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalOrganiserControls;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
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

        }

        private static void Cmd_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}
