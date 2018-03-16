using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace UniversalOrganiserControls
{

 
    public class ProcessProperties
    {

        public ProcessProperties()
        {
            StartedCallbackTask = new Task<bool>(() => { return true; });
            Executable = null;
            ShutdownCommand = null;
            SendEmptyFinishingCommand = false;
            HideWindow = false;
        }

        public Task<bool> StartedCallbackTask
        {
            get; set;
        }
        
        private FileInfo _executable;
        public FileInfo Executable
        {
            get
            {
                return _executable;
            }
            set
            {
                _executable = value;
            }
        }

        public string ShutdownCommand
        {
            get; set;
        }

        public bool SendEmptyFinishingCommand
        {
            get;set;
        }

        public bool HideWindow
        {
            get;set;
        }




    }
}
