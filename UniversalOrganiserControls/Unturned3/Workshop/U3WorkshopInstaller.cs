using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using UniversalOrganiserControls.Steam;

namespace UniversalOrganiserControls.Unturned3.Workshop
{


    public class U3WorkshopInstaller
    {
        public event EventHandler<U3WorkshopModInstalledEventArgs> U3WorkshopModInstallationFinished;
        public event EventHandler<U3WorkshopModInstallStateChangedEventArgs> U3WorkshopModInstallStateChanged;
        public event EventHandler Exited;

        private DirectoryInfo MapFolder;
        private DirectoryInfo ContentFolder;

        private U3Server server;
        private SteamInstance steam;
        
        private Queue<String> InstallationQueue = new Queue<String>();
        private String currentID = null;

        public bool QueueEmpty
        {
            get
            {
                return InstallationQueue.Count <= 0;
            }
        }

        private bool Active = true;


        public U3WorkshopInstaller(U3Server server, FileInfo steamcmd)
        {
            this.server = server;

            steam = new SteamInstance(steamcmd);
            steam.ModDownloaded += Steam_ModDownloaded;
            steam.SteamOutput += Steam_SteamOutput;

            if (PrepareSteam(steam).Result)
            {
                MapFolder = new DirectoryInfo(this.server.ServerInformation.ServerDirectory.Parent.Parent.FullName + "\\Bundles\\Workshop\\Maps");
                ContentFolder = new DirectoryInfo(this.server.ServerInformation.ServerDirectory.FullName + "\\Workshop\\Content\\");

                if (!MapFolder.Exists) MapFolder.Create();
                if (!ContentFolder.Exists) ContentFolder.Create();
            }
            else
            {
                Exception ex = new Exception("Unable to start steamcmd correctly!");
                throw ex;
            }
        }

        private void Steam_SteamOutput(object sender, string text)
        {
            Console.WriteLine("[WorkshopInstallerSteamOutput] " + text);
        }

        private void Steam_ModDownloaded(object sender, string folder)
        {
            if (folder == null)
            {
                U3WorkshopModInstallationFinished?.Invoke(this, new U3WorkshopModInstalledEventArgs(currentID, "", false, QueueEmpty));
            }
            else
            {
                U3WorkshopMod_Managed mod = new U3WorkshopMod_Managed(new DirectoryInfo(folder));
                try
                {

                    DirectoryInfo destination = null;
                    switch (mod.Type)
                    {
                        case U3WorkshopModType.Map:
                            destination = new DirectoryInfo(MapFolder.FullName + "\\" + mod.ID);
                            break;
                        case U3WorkshopModType.Content:
                            destination = new DirectoryInfo(ContentFolder.FullName + "\\" + mod.ID);
                            break;
                        default:
                            U3WorkshopModInstallationFinished?.Invoke(this, new U3WorkshopModInstalledEventArgs(mod.ID, mod.Name, false, QueueEmpty));
                            return;
                    }


                    if (destination.Exists)
                    {
                        U3WorkshopModInstallStateChanged?.Invoke(this, new U3WorkshopModInstallStateChangedEventArgs(U3WorkshopModInstallState.RemoveOld, mod.ID, mod.Name));
                        mod.Delete();
                    }

                    U3WorkshopModInstallStateChanged?.Invoke(this, new U3WorkshopModInstallStateChangedEventArgs(U3WorkshopModInstallState.Installing, mod.ID, mod.Name));
                    UniversalOrganiserControls.UtilsGeneral.DirectoryCopy(folder, destination.FullName, true);
                  
                    U3WorkshopModInstallationFinished?.Invoke(this, new U3WorkshopModInstalledEventArgs(mod.ID, mod.Name, true, QueueEmpty));

                }
                catch (Exception)
                {
                    U3WorkshopModInstallationFinished?.Invoke(this, new U3WorkshopModInstalledEventArgs(mod.ID, mod.Name, false, QueueEmpty));
                }
                currentID = null;
            }
        }

        public void InstallMods(params string[] modid)
        {
            foreach (string mod in modid)
            {
                InstallationQueue.Enqueue(mod);
            }
        }

        private Task InstallationProcess()
        {
            return Task.Run(async () =>
            {
                while (Active)
                {
                    if (!QueueEmpty & currentID != null)
                    {
                        currentID = InstallationQueue.Dequeue();
                        if (currentID == "-1")
                        {
                            Exit();
                        }

                        U3WorkshopModInstallStateChangedEventArgs args = new U3WorkshopModInstallStateChangedEventArgs(U3WorkshopModInstallState.Preparing, currentID);
                        U3WorkshopModInstallStateChanged?.Invoke(this, args);

                        string modTitle = U3WorkshopMod.getModTitle(currentID);

                        foreach (U3WorkshopMod_Managed Mod in server.GetWorkshopContentMods().Where((m) => { return m.ID == currentID; }))
                        {
                            args = new U3WorkshopModInstallStateChangedEventArgs(U3WorkshopModInstallState.RemoveOld, Mod.ID, Mod.Name);
                            U3WorkshopModInstallStateChanged?.Invoke(this, args);
                            Mod.Delete();
                        }


                        args = new U3WorkshopModInstallStateChangedEventArgs(U3WorkshopModInstallState.Installing, currentID, modTitle);
                        U3WorkshopModInstallStateChanged?.Invoke(this, args);


                        steam.getWorkshopMod("304930", currentID);
                        steam.sendCommand("help");
                    }
                    else
                    {
                        await Task.Delay(500);
                    }
                }
            });
        }

        public void Update()
        {
            foreach (U3WorkshopMod_Managed mod in server.GetWorkshopContentMods())
            {
                InstallationQueue.Enqueue(mod.ID);
            }
        }

        private void Exit(bool kill = false)
        {
            if (!kill)
            {
                InstallationQueue.Enqueue("-1");
            }
            else
            {
                Exited?.Invoke(this, new EventArgs());
                Active = false;
                steam.close(SteamExitReason.NothingSpecial, 5000);
            }
        }

        private Task<bool> PrepareSteam(SteamInstance steam)
        {
            return Task.Run(() =>
            {
                try
                {
                    foreach (string user in Steam.Utils.publicSteamAccounts.Keys)
                    {
                        LoginResult result = steam.login(user, Steam.Utils.publicSteamAccounts[user], "", 6000);
                        if (result == LoginResult.OK)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }

            });
        }
    }
}
