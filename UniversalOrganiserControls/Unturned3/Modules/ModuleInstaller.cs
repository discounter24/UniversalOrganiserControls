using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UniversalOrganiserControls.Unturned3.Modules
{

    public enum ModuleInstalltionResult { OK, FAILED, FAILED_SOURCE, FAILED_DEST }

    public static class ModuleInstaller
    {

        public static Task<ModuleInstalltionResult> InstallModule(string modZipFile, string modulesFolder)
        {
            return Task.Run(() =>
            {
                
                try
                {
                    DirectoryInfo dest = new DirectoryInfo(modulesFolder);
                    FileInfo source = new FileInfo(modZipFile);
                    if (!source.Exists)
                    {
                        return ModuleInstalltionResult.FAILED;
                    }
                    try
                    {
                        if (!dest.Exists) dest.Create();
                    }
                    catch (Exception)
                    {
                        return ModuleInstalltionResult.FAILED_DEST;
                    }
                    UtilsGeneral.extractZip(new FileInfo(modZipFile), new DirectoryInfo(modulesFolder));
                    return ModuleInstalltionResult.OK;
                }
                catch (Exception)
                {
                    return ModuleInstalltionResult.FAILED;
                }
            });
        }

        public static Task<ModuleInstalltionResult> InstallModule(string modZipFile, U3Server server)
        {
            string modulesFolder = server.ServerInformation.GameDirectory.FullName + "\\Modules\\";
            return InstallModule(modZipFile,modulesFolder);
        }


    }
}
