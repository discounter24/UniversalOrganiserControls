using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Core;
using System.IO;

namespace UniversalOrganiserControls.Backup
{
    public class BackupPackage
    {



        public BackupInfo BackupInfo
        {
            get;
            private set;
        }

        private FileInfo _package = null;
        public FileInfo Package
        {
            get
            {
                return _package;
            }
            private set
            {
                _package = value;
            }
        }
        

        public BackupPackage(DirectoryInfo folder, BackupInfo info)
        {
            this.BackupInfo = info;
            this.Package = new FileInfo(folder.FullName + "\\" + info.Hash + ".data");
        }


        public BackupProcess Unzip(string target)
        {
            BackupProcess process = new BackupProcess();
            process.Extract(this, target);
            return process;
        }

        public bool Delete()
        {
            try
            {
                FileInfo pack = Package;
                pack.Delete();

                FileInfo info = new FileInfo(pack.Directory.FullName + "\\" + BackupInfo.Hash + ".bck");
                info.Delete();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

  

    }


}
