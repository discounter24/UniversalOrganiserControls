using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Backup
{
    public class BackupProcess
    {
        public event EventHandler<BackupProgressArgs> BackupProgressChanged;

        private BackupInfo info = null;
        private string source = "";
        private string target = "";

        private BackupProgressArgs _lastProgress = null;
        private BackupProgressArgs LastProgress
        {
            get
            {
                return _lastProgress;
            }
            set
            {
                _lastProgress = value;
                BackupProgressChanged?.Invoke(this, _lastProgress);
            }
        }



        public void CreateBackup(BackupInfo info, string source, string target)
        {
            DirectoryInfo dir = new DirectoryInfo(target);
            if (!dir.Exists) dir.Create();
            this.info = info;
            this.source = source;
            this.target = target;
            if (LastProgress != null) throw new Exception("Instance already used or still in use!");
            Task.Run(async () =>
            {
                LastProgress = new BackupProgressArgs(BackupProgressState.Counting, 0, 0);
                int totalFiles = await CountFiles(new DirectoryInfo(this.source));

                LastProgress = new BackupProgressArgs(BackupProgressState.Saving, 0, totalFiles);

                CreateBackup(this.target + "\\" + info.Hash + ".data", info.Hash, this.source);

                info.save(new DirectoryInfo(this.target));

                LastProgress = new BackupProgressArgs(BackupProgressState.Finished, totalFiles, totalFiles);

            });
        }

        private void CreateBackup(string outPathname, string password, string folderName)
        {

            FileStream fsOut = File.Create(outPathname);
            ZipOutputStream zipStream = new ZipOutputStream(fsOut);

            zipStream.SetLevel(3);
            zipStream.Password = password;

            int folderOffset = folderName.Length + (folderName.EndsWith("\\") ? 0 : 1);

            CompressFolder(folderName, zipStream, folderOffset);

            zipStream.IsStreamOwner = true;
            zipStream.Close();
        }

        private void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {

            string[] files = Directory.GetFiles(path);

            foreach (string filename in files)
            {
                LastProgress = new BackupProgressArgs(BackupProgressState.Saving, LastProgress.FilesDone+1, LastProgress.FileTotal);

                FileInfo fi = new FileInfo(filename);

                string entryName = filename.Substring(folderOffset);
                entryName = ZipEntry.CleanName(entryName);
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime;
                newEntry.AESKeySize = 128;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }

        private Task<int> CountFiles(DirectoryInfo dir)
        {
            return Task<int>.Run(() =>
            {
                int result = dir.GetFiles().Length;
                List<Task<int>> tasks = new List<Task<int>>();
                foreach(DirectoryInfo subdir in dir.GetDirectories())
                {
                    tasks.Add(CountFiles(subdir));
                }
                Task.WaitAll(tasks.ToArray<Task<int>>());
                foreach(Task<int> t in tasks)
                {
                    result += t.Result;
                }

                return result;
            });
        }

        public void Extract(BackupPackage backup, string target)
        {
            Task.Run(() =>
            {
                DirectoryInfo dir = new DirectoryInfo(target);
                if (!dir.Exists) dir.Create();
                LastProgress = new BackupProgressArgs(BackupProgressState.Saving, 0,0);
                Extract(backup.Package.FullName, backup.BackupInfo.Hash, target);
                LastProgress = new BackupProgressArgs(BackupProgressState.Finished, LastProgress.FilesDone, LastProgress.FileTotal);
            });
        }

        private void Extract(string archiveFilenameIn, string password, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                LastProgress = new BackupProgressArgs(BackupProgressState.Saving, LastProgress.FilesDone + 1, (int)zf.Count);
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;     
                }
                foreach (ZipEntry zipEntry in zf)
                {
                    try
                    {
                        if (!zipEntry.IsFile)
                        {
                            continue;
                        }
                        String entryFileName = zipEntry.Name;


                        byte[] buffer = new byte[4096];
                        Stream zipStream = zf.GetInputStream(zipEntry);


                        String fullZipToPath = Path.Combine(outFolder, entryFileName);
                        string directoryName = Path.GetDirectoryName(fullZipToPath);
                        if (directoryName.Length > 0)
                            Directory.CreateDirectory(directoryName);

                        using (FileStream streamWriter = File.Create(fullZipToPath))
                        {
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unable to extract entry: " + ex.ToString());
                    }
   
                    LastProgress = new BackupProgressArgs(BackupProgressState.Saving, LastProgress.FilesDone + 1, LastProgress.FileTotal);
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; 
                    zf.Close(); 
                }
            }
        }

    }
}
