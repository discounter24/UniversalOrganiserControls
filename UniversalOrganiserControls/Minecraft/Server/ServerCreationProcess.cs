using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;


namespace UniversalOrganiserControls.Minecraft.Server
{

    public class ServerCreationProcess
    {

        public event EventHandler<string> JavaOutput;
        public event EventHandler<ServerCreationStateArgs> CreationStateChanged;
        public event EventHandler<MinecraftEULA> WaitingForEulaAgreement;

        private DirectoryInfo serverDirectory;
        private AdvancedVersionAPI.VersionType serverType;
        private Task task;
        private ServerVersion serverVersion;


        public ServerCreationFlags Flags = new ServerCreationFlags();

        private FileInfo EngineFile
        {
            get
            {
                return new FileInfo(string.Format("{0}\\engine.jar", serverDirectory.FullName));
            }
        }

        public ServerCreationProcess(ServerCreationFlags flags) : this()
        {
            this.Flags = flags;
        }

        public ServerCreationProcess()
        {
            
        }

        public void Create(DirectoryInfo serverDirectory, AdvancedVersionAPI.VersionType serverType, ServerVersion serverVersion)
        {
            /*
            if (serverType != ServerType.Vanilla)
            {
                throw new NotImplementedException();
            }*/
            this.serverVersion = serverVersion;
            this.serverDirectory = serverDirectory;
            this.serverType = serverType;
            this.task = Run();
        }

        private Task Run()
        {
            return Task.Run(() =>
            {
                JavaCheck();
            });
        }


        private void JavaCheck()
        {
            if (Utils.IsJavaInstalled())
            {
                Prepare();
            }
            else
            {
                CreationStateChanged?.Invoke(this, new ServerCreationCanceledState(ServerCreationCanceledReason.JavaNotInstalled));
            }
        }

        private void Prepare()
        {
            try
            {
                if (!serverDirectory.Exists) serverDirectory.Create();
                if (!Flags.IgnoreNotEmtpyDir && !Utils.IsDirEmpty(serverDirectory))
                {
                    CreationStateChanged?.Invoke(this, new ServerCreationCanceledState(ServerCreationCanceledReason.TargetDirectoryNotEmpty));
                }
                else
                {
                    StartDownload();
                }

            }
            catch (IOException ex)
            {
                CreationStateChanged?.Invoke(this, new ServerCreationCanceledState(ServerCreationCanceledReason.IOException, ex));
            }
            catch (Exception ex)
            {
                CreationStateChanged?.Invoke(this, new ServerCreationCanceledState(ServerCreationCanceledReason.Unknown, ex));
            }
        }


        private void StartDownload()
        {
            Uri uri;

            switch (serverType)
            {
                case AdvancedVersionAPI.VersionType.VANILLA:

                    CreationStateChanged?.Invoke(this, new ServerCreationStateArgs(ServerCreationState.GettingDownloadInformation));
                    uri = new Uri(serverVersion.url);


                    CreationStateChanged?.Invoke(this, new ServerCreationStateArgs(ServerCreationState.StartingDownload));
                    DownloadEngine(uri);
                    break;


                case AdvancedVersionAPI.VersionType.SPIGOT:

                    CreationStateChanged?.Invoke(this, new ServerCreationStateArgs(ServerCreationState.GettingDownloadInformation));

                    uri = new Uri(serverVersion.url);

                    CreationStateChanged?.Invoke(this, new ServerCreationStateArgs(ServerCreationState.StartingDownload));
                    DownloadEngine(uri);
                    break;

                case AdvancedVersionAPI.VersionType.CRAFTBUKKIT:
                    CreationStateChanged?.Invoke(this, new ServerCreationStateArgs(ServerCreationState.GettingDownloadInformation));

                    uri = new Uri(serverVersion.url);

                    CreationStateChanged?.Invoke(this, new ServerCreationStateArgs(ServerCreationState.StartingDownload));
                    DownloadEngine(uri);
                    break;
                default:
                    CreationStateChanged?.Invoke(this, new ServerCreationCanceledState(ServerCreationCanceledReason.NotSupportedServerType));
                    break;
            }
        }


        private void DownloadEngine(Uri uri)
        {
            try
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += (sender, e) => 
                {
                    CreationStateChanged?.Invoke(this, new ServerCreationDownloadState(e.ProgressPercentage, e.BytesReceived, e.TotalBytesToReceive));
                };
                client.DownloadFileCompleted += (sender, e) => 
                {
                    CreationStateChanged?.Invoke(this, new ServerCreationStateArgs(ServerCreationState.FirstStart));
                    if (Flags.RunServerForFirstTime)
                    {
                        RunForFirstTime();
                    }
                    else
                    {
                        CreationStateChanged?.Invoke(this, new ServerCreationStateArgs(ServerCreationState.Finished));
                    }

                };
                client.DownloadFileAsync(uri,EngineFile.FullName);
            }
            catch (WebException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private void RunForFirstTime()
        {
            MinecraftServer server = new MinecraftServer(EngineFile);
            server.ServerOutput += (s, e) => { JavaOutput?.Invoke(s, e); };
            server.ServerStateChanged += Server_FirstRunStates;
            server.Start();
        }

        private void Server_FirstRunStates(object sender, ServerState e)
        {
            switch (e)
            {
                case ServerState.Running:
                    ((MinecraftServer)sender).ServerStateChanged -= Server_FirstRunStates;
                    ((MinecraftServer)sender).ServerStateChanged += Server_ServerStateChanged;
                    Server_ServerStateChanged(sender, e);
                    break;
                case ServerState.Stopped:
                    WaitForEulaAgreement();
                    break;
                default:
                    break;
            }
        }

        private void WaitForEulaAgreement()
        {
            MinecraftEULA eula = new MinecraftEULA(new FileInfo(serverDirectory.FullName + "\\eula.txt"));
            eula.AgreementStateChanged += Eula_AgreementStateChanged;
            WaitingForEulaAgreement?.Invoke(this, eula);
        }


        private void Eula_AgreementStateChanged(object sender, bool e)
        {
            if (e)
            {
                MinecraftServer server = new MinecraftServer(EngineFile);
                server.ServerOutput += (s, e2) => { JavaOutput?.Invoke(s, e2); };
                server.ServerStateChanged += Server_ServerStateChanged;
                server.PreparingSpawnArea += (sender2,e2) => { CreationStateChanged?.Invoke(this, new ServerCreationStateArgs(ServerCreationState.PreparingSpawn)); };
                server.Start();
            }
            else
            {
                CreationStateChanged?.Invoke(this, new ServerCreationCanceledState(ServerCreationCanceledReason.EulaAgreementRequired));
            }

        }

     

        private void Server_ServerStateChanged(object sender, ServerState e)
        {
            switch (e)
            {
                case ServerState.Stopped:
                    CreationStateChanged?.Invoke(this, new ServerCreationStateArgs(ServerCreationState.Finished));
                    break;
                case ServerState.Running:
                    CreationStateChanged?.Invoke(this, new ServerCreationStateArgs(ServerCreationState.Finishing));
                    ((MinecraftServer)sender).Stop();
                    break;
                default:
                    break;
            }
        }
    }

    public class ServerCreationFlags
    {
        public bool RunServerForFirstTime = false;
        public bool IgnoreNotEmtpyDir = false;
    }
     

}
