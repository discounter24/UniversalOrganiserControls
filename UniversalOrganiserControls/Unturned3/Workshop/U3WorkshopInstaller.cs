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
        public bool QueueEmpty
        {
            get
            {
                return InstallationQueue.Count <= 0;
            }
        }

        private bool Active;


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
            throw new NotImplementedException();
        }

        private void Steam_ModDownloaded(object sender, string folder)
        {
            throw new NotImplementedException();
        }

        public void installMods(params string[] modid)
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
                    if (!QueueEmpty)
                    {
                        string id = InstallationQueue.Dequeue();
                        if (id == "-1")
                        {
                            exit();
                        }

                        U3WorkshopModInstallStateChangedEventArgs args = new U3WorkshopModInstallStateChangedEventArgs(U3WorkshopModInstallState.Preparing, id);
                        U3WorkshopModInstallStateChanged?.Invoke(this, args);

                        string modTitle = U3WorkshopMod.getModTitle(id);

                        foreach (U3WorkshopMod Mod in server.getWorkshopContentMods().Where((m) => { return m.ID == id; }))
                        {
                            args = new U3WorkshopModInstallStateChangedEventArgs(U3WorkshopModInstallState.RemoveOld, Mod.ID, Mod.Name);
                            U3WorkshopModInstallStateChanged?.Invoke(this, args);
                            Mod.Delete();
                        }


                        args = new U3WorkshopModInstallStateChangedEventArgs(U3WorkshopModInstallState.Installing, id, modTitle);
                        U3WorkshopModInstallStateChanged?.Invoke(this, args);


                        steam.getWorkshopMod("304930", id);
                        steam.sendCommand("help");
                    }
                    else
                    {
                        await Task.Delay(500);
                    }
                }
            });
        }

        public void updateServer()
        {
            foreach (U3WorkshopMod mod in server.getWorkshopContentMods())
            {
                InstallationQueue.Enqueue(mod.ID);
            }
        }

        private void exit(bool kill = false)
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
