using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UniversalOrganiserControls.Minecraft;
using UniversalOrganiserControls.Minecraft.Server.Properties;

namespace UniversalOrganiserControls.Minecraft.Server
{
    public class MinecraftServer : IMinecraftServer
    {
        public event EventHandler<ServerState> ServerStateChanged;
        public event EventHandler PreparingSpawnArea;
        public event EventHandler FailedPortBind;
        public event EventHandler<string> ServerOutput;
        public event EventHandler<MinecraftEULA> EulaAgreementRequired;


        private bool restart = false;
        public int initMem = 1024, maxMem = 1024;

        private Process process = null;
        protected Process JavaEngine
        {
            get => process; 
            private set => process = value; 
        }

        private ServerState state = ServerState.Stopped;
        public ServerState ServerState
        {
            get => state; 
            protected set
            {
                if (state!=value)
                {
                    state = value;
                    ServerStateChanged?.Invoke(this, value);
                    if (value == ServerState.Stopped && restart)
                    {
                        restart = false;
                        Start();
                    }
                }
            }
        }


        private AdvancedVersionAPI.VersionType serverType;
        public AdvancedVersionAPI.VersionType ServerType
        {
            get => serverType;
            protected set =>  serverType = value;
        }

        private FileInfo engineFile;
        public FileInfo EngineFile
        {
            get => engineFile; 
            private set => engineFile = value; 
        }

        public DirectoryInfo ServerDirectory
        {
            get => EngineFile.Directory; 
        }

        public string ID
        {
            get => ServerDirectory.Name;
        }

        public bool SupportsPlugins
        {
            get
            {
                return PluginDirectory.Exists;
            }
        }

        public DirectoryInfo PluginDirectory
        {
            get
            {
                return new DirectoryInfo(ServerDirectory.FullName + "\\plugins\\");
            }
        }

        public ServerProperties Properties
        {
            get
            {
                return ServerProperties.getServerProperties(this);
            }
        }


        public MinecraftServer(FileInfo engine)
        {
            EngineFile = engine;
        }


        

        public CommandResult Execute(string command)
        {
            try
            {
                process.StandardInput.WriteLine(command);
                process.StandardInput.Flush();
                return CommandResult.OK;
            }
            catch (Exception)
            {
                return CommandResult.ServerNotRunning;
            }

        }

        public StopResult Kill()
        {
            try
            {
                JavaEngine.Kill();
                return StopResult.OK;
            }
            catch (InvalidOperationException)
            {
                return StopResult.ProcessAccessFailture;
            }
            catch (Exception)
            {
                return StopResult.ProcessAccessFailture;
            }
        }

 

        public CommandResult Say(string message)
        {
            return Execute(string.Format("say {0}", message));
        }

        public StartResult Start()
        {
            if (ServerState == ServerState.Stopped)
            {
                try
                {
                    ServerState = ServerState.Starting;
                    JavaEngine = new Process();

                    JavaEngine.StartInfo.FileName = "JEmu.exe";
                    JavaEngine.StartInfo.WorkingDirectory = ServerDirectory.FullName;

                    string java_args = string.Format("-Xms{0}M -Xmx{1}M -jar {2} nogui", initMem, maxMem, EngineFile.Name);

                    JavaEngine.StartInfo.Arguments = string.Format(@"""{0}"" ""{1}""", java_args, ServerDirectory.FullName);

                    JavaEngine.StartInfo.RedirectStandardError = true;
                    JavaEngine.StartInfo.RedirectStandardOutput = true;
                    JavaEngine.StartInfo.RedirectStandardInput = true;

                    JavaEngine.StartInfo.UseShellExecute = false;

                    JavaEngine.EnableRaisingEvents = true;

                    JavaEngine.StartInfo.CreateNoWindow = true;
                    JavaEngine.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    JavaEngine.OutputDataReceived += JavaEngine_OutputDataReceived;
                    JavaEngine.Exited += JavaEngine_Exited;

                    JavaEngine.Start();

                    JavaEngine.BeginOutputReadLine();
                    JavaEngine.BeginErrorReadLine();

                    

                    return StartResult.OK;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return StartResult.ProcessAccessFailture;
                }
            }
            else
            {
                return StartResult.AlreadyRunning;
            }

        }

        private void JavaEngine_Exited(object sender, EventArgs e)
        {
            ServerState = ServerState.Stopped;
        }

        private void JavaEngine_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {

         
                if (Regex.IsMatch(e.Data, ".+ Done ((.*))! For help, type \"help\" or \"?\""))
                {
                    ServerState = ServerState.Running;
                }
                

                //if (Regex.IsMatch(e.Data, "^Closing listening thread$")) ServerState = ServerState.Stopped;
                //if (Regex.IsMatch(e.Data, @".+\[Server Shutdown Thread\/INFO\\]\: Stopping server")) ServerState = ServerState.Stopped;
                //if (Regex.IsMatch(e.Data, @".+Stopping server$")) ServerState = ServerState.Stopped;

                if (Regex.IsMatch(e.Data, "Preparing spawn area: ([0-9]*)%")) PreparingSpawnArea?.Invoke(this,new EventArgs());
                if (Regex.IsMatch(e.Data, @"You need to agree to the EULA in order to run the server\. Go to eula\.txt for more info\.$"))
                {
                    Kill();
                    MinecraftEULA eula = new MinecraftEULA(new FileInfo(ServerDirectory.FullName + "\\eula.txt"));
                    eula.AgreementStateChanged += Eula_AgreementStateChanged;
                    EulaAgreementRequired?.Invoke(this, eula);
                }
                if (Regex.IsMatch(e.Data, @".+\*\*\*\* FAILED TO BIND TO PORT!$")) FailedPortBind?.Invoke(this, new EventArgs());


                ServerOutput?.Invoke(this, e.Data);
            }
        }

        private void Eula_AgreementStateChanged(object sender, bool e)
        {
            if (e) this.Start();
        }

        public StopResult Restart()
        {
            ServerState = ServerState.Stopping;
            if (Execute("stop") == CommandResult.OK)
            {
                restart = true;
                return StopResult.OK;
            }
            else
            {
                return StopResult.AlreadyStopped;
            }
        }

        public StopResult Stop(int seconds = 0)
        {
            ServerState = ServerState.Stopping;
            if (Execute("stop") == CommandResult.OK)
            {
                return StopResult.OK;
            }
            else
            {
                return StopResult.AlreadyStopped;
            }
        }
    }


}