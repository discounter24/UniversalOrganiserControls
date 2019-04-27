using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;

namespace UniversalOrganiserControls.Minecraft.Server.Plugins
{

    public class PluginSpace
    {

        private MinecraftServer server;
        public DirectoryInfo PluginFolder
        {
            get => server.PluginDirectory;
        }

        public IEnumerable<FileInfo> Plugins
        {
            get
            {
                foreach(FileInfo jarFile in PluginFolder.GetFiles().Where((file) => file.Extension == ".jar"))
                {
                    yield return jarFile;
                }
            }
        }

        public PluginSpace(MinecraftServer server)
        {
            this.server = server;
        }

      
        public void ImportJar(FileInfo jarFile)
        {
            jarFile.CopyTo(PluginFolder.FullName + "\\" + jarFile.Name);
        }

        public void ImportFromZip(FileInfo zipFile)
        {
            FastZipEvents events = new FastZipEvents();
            FastZip zip = new FastZip(events);
            zip.CreateEmptyDirectories = true;
            zip.ExtractZip(zipFile.FullName,PluginFolder.FullName,FastZip.Overwrite.Always,(b)=> { return true; },"","",false);
        }
        


        private void Extract(string zipFile)
        {
     
        }


    }
}
