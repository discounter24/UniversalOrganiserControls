using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Security.Cryptography;

namespace UniversalOrganiserControls
{
    public static class UtilsGeneral
    {

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr FindWindow(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr hWnd, WinApiWindowState flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr hWnd, WinApiWindowCommands nCmdShow);


        public enum WinApiWindowCommands
        {
            HIDE = 0,
            SHOWNORMAL = 1,
            SHOWMINIMIZED = 2,
            MAXIMIZE = 3,
            SHOWNOACTIVATE = 4,
            SHOW = 5,
            MINIMIZE = 6,
            SHOWMINNOACTIVE = 7,
            SHOWNA = 8,
            RESTORE = 9,
            SHOWDEFAULT = 10,
            FORCEMINIMIZE = 11
        }

        public enum WinApiWindowState
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        }

        public static string GetMD5HashOfFile(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    throw new Exception("Could not delete: " + file + "\n" + ex.ToString(), ex);
                }
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            try
            {
                Directory.Delete(target_dir, true);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not delete: " + target_dir + "\n" + ex.ToString(), ex);
            }
        }

        public static void extractZip(FileInfo file, DirectoryInfo dest)
        {
            if (!dest.Exists) dest.Create();


            Parallel.ForEach(ZipFile.OpenRead(file.FullName).Entries, entry =>
            {
                try
                {
                    string f = Path.GetFullPath(Path.Combine(dest.FullName, entry.FullName));
                    FileInfo fileInfo = new FileInfo(f);

                    if (!fileInfo.Directory.Exists)
                    {
                        fileInfo.Directory.Create();
                    }

                    /*if (!Directory.Exists(Path.GetDirectoryName(file)))
                    {
                        Directory.CreateDirectory(file);
                    }*/

                    if (entry.Name != "")
                    {
                        entry.ExtractToFile(f, true);
                    }
                }
                catch (Exception) { }
            });
        }

        public static void extractZipSerial(FileInfo file, DirectoryInfo dest)
        {
            if (!dest.Exists) dest.Create();


            foreach(ZipArchiveEntry entry in ZipFile.OpenRead(file.FullName).Entries)
            {
                try
                {
                    string dir = Path.GetFullPath(Path.Combine(dest.FullName, entry.FullName));

                    if (!Directory.Exists(Path.GetDirectoryName(dir)))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    if (entry.Name != "")
                    {
                        entry.ExtractToFile((dir), true);
                    }
                }
                catch (Exception) { }
            }
        }



        /// <summary>
        /// Searches the given directory for all its containing files.
        /// </summary>
        /// <param name="dir">The directory searched.</param>
        /// <returns>A list of all files in the searched directory.</returns>
        public static List<FileInfo> getFilesSync(DirectoryInfo dir)
        {
            List<FileInfo> files = new List<FileInfo>();
            try
            {
                files.AddRange(dir.EnumerateFiles());
            }
            catch (Exception)
            {
                //Rather skip some files than just crashing.
            }
            
            foreach (DirectoryInfo subdir in dir.GetDirectories())
            {
                try
                {
                    files.AddRange(getFilesSync(subdir));
                }
                catch (Exception)
                {
                    //Rather skip some files than just crashing.
                }

            }

            return files;
        }


        /// <summary>
        /// Calculates the md5 hashes for a given list of files.
        /// </summary>
        /// <param name="files">The files for which we caluclate the hashes</param>
        /// <param name="hashes">A dictionary with the results.</param>
        /// <returns>The parallel task running until all hashes have been calculated.</returns>
        public static Task calculateFileHashes(List<FileInfo> files, Dictionary<string, string> hashes)
        {
            return Task.Run(() =>
            {
                List<Task> tasks = new List<Task>();
                foreach (var file in files)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        hashes.Add(file.FullName, GetMD5HashOfFile(file.FullName));
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            });
        }


    }


}
