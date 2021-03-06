﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Microsoft.Win32;

namespace UniversalOrganiserControls.Unturned3.Utils
{
    public static class Game
    {
        /// <summary>
        /// Holds the Steam app id for Unturned 3
        /// </summary>
        public const int APP_ID = 304930;


        public static string getOfficialVersion()
        {
            try
            {
                return new WebClient().DownloadString(new Uri("http://dl.unturned-server-organiser.com/unturned/hash.php"));
            }
            catch (Exception)
            {
                return "(unable to retrieve version)";
            }
        }


        public static DirectoryInfo getLocalUnturnedInstallation()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam");
            try
            {
                string value = (string)key.GetValue("UninstallString", null);
                return new FileInfo(value).Directory;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
