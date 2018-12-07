using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UniversalOrganiserControls.Unturned3
{
    public class U3ServerEngineSettings
    {

        public string ArgumentLine { get; set; }

        public string ShutdownMessage { get; set;} 
        
        public FileInfo Executable { get; private set; }

        public int ShutdownMessageIntervall { get; set; }

        public int AutoSaveIntervall { get; set; }

        public bool AutomaticRestart { get; set; }

        public bool HighPriorityProcess { get; set; }

        public bool StartHidden { get; set; }

        public string ServerID
        {
            get;set;
        }

        public DirectoryInfo GameDirectory
        {
            get
            {
                return Executable.Directory;
            }
        }

        public DirectoryInfo ServerDirectory
        {
            get
            {
                return new DirectoryInfo(string.Format("{0}\\Servers\\{1}\\",GameDirectory.FullName,ServerID));
            }
        }

        public U3ServerEngineSettings(FileInfo executable, string serverid)
        {
            this.Executable = executable;
            this.ServerID = serverid;
            this.ArgumentLine = "-batchmode -nogrpahics +{0}/{1}";
            this.ShutdownMessage = "The Server will shutdown in {0} seconds. Search a safe place to stay!";
            this.ShutdownMessageIntervall = 0;
            this.AutoSaveIntervall = 0;
            this.AutomaticRestart = false;
            this.HighPriorityProcess = false;
        }

    }
}
