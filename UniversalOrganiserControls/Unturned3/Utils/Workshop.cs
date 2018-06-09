using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UniversalOrganiserControls.Unturned3.Utils
{
    public static class Workshop
    {
        /// <summary>
        /// If not already existant, this method creates needed workshop folders for a given Unturned game.
        /// </summary>
        /// <param name="gameFolder"></param>
        public static void fixWorkshopFolders(string gameFolder)
        {

            string extension = "\\Bundles\\Workshop\\";
            string w_global_base = gameFolder + extension;

            DirectoryInfo workshop = new DirectoryInfo(w_global_base);
            DirectoryInfo workshopContent = new DirectoryInfo(w_global_base + "Content");
            DirectoryInfo workshopMaps = new DirectoryInfo(w_global_base + "Maps");

            if (!workshop.Exists) workshop.Create();
            if (!workshopContent.Exists) workshopContent.Create();
            if (!workshopMaps.Exists) workshopMaps.Create();


            foreach (DirectoryInfo dir in new DirectoryInfo(gameFolder + "\\Servers\\").GetDirectories())
            {
                DirectoryInfo s_workshop = new DirectoryInfo(dir.FullName + "\\Bundles");
                DirectoryInfo s_workshopContent = new DirectoryInfo(dir.FullName + "\\Bundles\\Content");
                DirectoryInfo s_workshopMaps = new DirectoryInfo(dir.FullName + "\\Bundles\\Maps");

                if (!s_workshop.Exists) s_workshop.Create();
                if (!s_workshopContent.Exists) s_workshopContent.Create();
                if (!s_workshopMaps.Exists) s_workshopMaps.Create();
            }

        }
    }
}
