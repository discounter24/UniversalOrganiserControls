using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UniversalOrganiserControls.Backup
{
    public class BackupSpace
    {
      
        private List<BackupPackage> Backups = new List<BackupPackage>();
        private DirectoryInfo Target = null;

        public BackupSpace(DirectoryInfo dir)
        {
            if (!dir.Exists) dir.Create();
            this.Target = dir;
            Reload();
        }

        public BackupProcess Create(string source, string identifier, string game = "")
        {
            BackupProcess process = new BackupProcess();
            process.CreateBackup(new BackupInfo(game, identifier), source, Target.FullName);
            return process;
        }
        
        public void Reload()
        {
            Backups.Clear();
            foreach(FileInfo file in Target.GetFiles().Where((i)=> { return i.Extension.Equals(".bck"); }))
            {
                try
                {
                    Backups.Add(new BackupPackage(file.Directory,BackupInfo.read(file.FullName)));
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid backup info: " + file.Name.ToString());
                }
            }
        }

        public IEnumerable<BackupPackage> GetBackups()
        {
            foreach(BackupPackage package in Backups)
            {
                yield return package;
            }
        }

    }
}
