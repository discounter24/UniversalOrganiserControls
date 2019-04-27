using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;

namespace UniversalOrganiserControls.Minecraft
{
    public static class Utils
    {



        public static bool IsDirEmpty(DirectoryInfo directory)
        {
            try
            {
                return directory.GetFiles().Length == 0 & directory.GetDirectories().Length == 0;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static bool IsJavaInstalled()
        {
            try
            {
                Process java = new Process();
                java.StartInfo.FileName = "java";
                java.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                java.Start();
                java.WaitForExit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


     
    }



}
