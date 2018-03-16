using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Backup
{

    public class BackupProgressArgs
    {
        public BackupProgressState State { get; private set; }
        public int FilesDone { get; private set; }
        public int FileTotal { get; private set; }
        public bool Cancel { get; set; }


        public BackupProgressArgs(BackupProgressState state, int filesDone, int totalFiles)
        {
            this.Cancel = false;
            this.State = state;
            this.FilesDone = filesDone;
            this.FileTotal = totalFiles;
        }
    }

    public enum BackupProgressState { Counting, Saving, Canceled, Finished }



}
