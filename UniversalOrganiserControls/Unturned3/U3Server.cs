using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.ServiceProcess;
using System.Windows.Forms;

using UniversalOrganiserControls;
using UniversalOrganiserControls.Unturned3.Configuration;
using UniversalOrganiserControls.Unturned3.RocketMod;
using UniversalOrganiserControls.Unturned3.Workshop;
using UniversalOrganiserControls.Unturned3.UCB;
using OrganiserNetworking.Packages;

using System.Xml;
using UniversalOrganiserControls.Unturned3.RocketMod.Plugin;
using System.Threading;

namespace UniversalOrganiserControls.Unturned3
{


    /// <summary>
    /// Represents an Unturned 3 server
    /// </summary>
    public class U3Server
    {
           
        /// <summary>
        /// Triggered when the server process sends the complete console log through UCB
        /// </summary>
        public event EventHandler<string> CompleteConsoleOutput;

        /// <summary>
        /// Tiggered when the server process send a new log line through UCB
        /// </summary>
        public event EventHandler<string> ConsoleOutput;

        /// <summary>
        /// Tiggered when the server process connected to the UCB Manager
        /// </summary>
        public event EventHandler UCBConnected;

        /// <summary>
        /// Tiggered when the the connection between the server process and the UCB Manager got lost 
        /// </summary>
        public event EventHandler UCBDisconnected;


        /// <summary>
        /// Tiggered when the server has been renamed.
        /// </summary>
        public event EventHandler<U3ServerRenamedArgs> ServerRenamed;

        /// <summary>
        /// Tiggered when the server state changed.
        /// </summary>
        public event EventHandler<U3ServerState> ServerStateChanged;

        /// <summary>
        /// Triggers when a player has been connected or disconnected from the server
        /// </summary>
        public event EventHandler<List<string>> PlayerListUpdated;

        /// <summary>
        /// Triggers when the ports for the current server have been opend or closed via UPnP
        /// </summary>
        public event EventHandler<bool> UPnPStateChanged;

        public IntPtr MainWindowHandle
        {
            get
            {
                try
                {
                    return _process.MainWindowHandle;
                }
                catch (Exception)
                {
                    /*
                    if (UCBManager.isIdentifed(this))
                    {
                        ManualResetEvent wait = new ManualResetEvent(false);
                        IntPtr ptr = new IntPtr();
                        Task.Run(() =>
                        {
                            UCBManager.PackageReceived += (sender, e) =>
                              {
                                  if (e.Server.Equals(this))
                                  {
                                      if (e.Package.Header == PackageHeader.MainWindowHandleAnswer)
                                      {
                                          ptr = new IntPtr(Convert.ToInt32(((MainWindowHandleAnswerPackage)e.Package).Content));
                                          wait.Set();
                                      }
                                  }
                              };
                            UCBManager.sendPackage(this, new SimpleRequestPackage(PackageHeader.MainWindowHandleRequest));

                        });
                        wait.WaitOne();
                        return ptr;
                    }*/
                    throw;
                }
            }
        }


        private UCBManager _UCBManager = null;
        private List<string> _playerlist = new List<string>();
        private Process _process = null;
        private RocketBridgeServer _RocketBridge = null;
        private bool DisableAutoRestartOnceFlag = false;
        private bool EnableAutoRestartOnceFlag = false;
        private bool consoleVisibility = true;
        private U3ServerState _state = U3ServerState.Stopped;


        /// <summary>
        /// Sets or gets the visibility of the Unturned server console
        /// </summary>
        public bool ConsoleVisible
        {
            get
            {
                return consoleVisibility;
            }
            set
            {
                try
                {
                    if (value)
                    {
                        UtilsGeneral.ShowWindow(_process.MainWindowHandle, UtilsGeneral.WinApiWindowState.Show);
                    }
                    else
                    {
                        UtilsGeneral.ShowWindow(_process.MainWindowHandle, UtilsGeneral.WinApiWindowState.Hide);
                    }
                    consoleVisibility = value;
                }
                catch (Exception) {  }

            }
        }

        /// <summary>
        /// Gets or sets the server egnine settings e.g. server start information of the current server.
        /// </summary>
        public U3ServerEngineSettings ServerInformation { get; set; }

        #region BattlEye

        /// <summary>
        /// Checks if the BattlEye anti cheat service is installed.
        /// </summary>
        public bool BattlEyeInstalled
        {
            get
            {
                ServiceController ctl = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "BEService");
                if (ctl == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }


        /// <summary>
        /// Gets or sets if the BattlyEye anti cheat service will be used.
        /// </summary>
        public bool BattlEyeEnabled
        {
            get
            {
                try
                {
                    AdvancedConfig config = AdvancedConfig.loadJson(ServerInformation.ServerDirectory + "\\config.json");
                    if (config == null)
                    {
                        return true;
                    }
                    else
                    {
                        return config.Server.BattlEye_Secure;
                    }
                }
                catch (Exception)
                {
                    return true;
                }

            }

            set
            {
                try
                {
                    AdvancedConfig config = AdvancedConfig.loadJson(ServerInformation.ServerDirectory + "\\config.json");
                    config.Server.BattlEye_Secure = value;
                    File.WriteAllText(ServerInformation.ServerDirectory + "\\config.json", config.getJson());
                }
                catch (Exception) {}

            }
        }

        /// <summary>
        /// Returns the game launcher that starts the BattlyExe anti cheat service.
        /// </summary>
        private FileInfo BattlEyeLauncher
        {
            get
            {
                return new FileInfo(ServerInformation.Executable.Directory.FullName + "\\Unturned_BE.exe");
            }
        }




        #endregion


        /// <summary>
        /// The last PID of the server. -1 if no PID found.
        /// </summary>
        public string LastPID
        {
            get
            {
                try
                {
                    string pid = File.ReadAllText(ServerInformation.ServerDirectory.FullName + "\\UCB\\server.pid");
                    return pid;
                }
                catch (Exception)
                {
                    return "-1";
                }
            }
        }


        /// <summary>
        /// Gets the current list of players playing on the server
        /// </summary>
        public List<string> PlayerList
        {
            get
            {
                return _playerlist;
            }
            set
            {
                _playerlist = value;
                PlayerListUpdated.Invoke(this, _playerlist);
            }
        }

        /// <summary>
        /// Gets the installation state of RocketMod for Unturned
        /// </summary>
        public bool RocketModInstalled
        {
            get
            {
                return File.Exists(ServerInformation.ServerDirectory.Parent.Parent.FullName + "\\Modules\\Rocket.Unturned\\Rocket.Core.dll");
            }
        }

        /// <summary>
        /// Returns the external ip address of the current host computer
        /// </summary>
        public String ExternalIP
        {
            get
            {
                try
                {
                    return new System.Net.WebClient().DownloadString(new Uri("http://manage.unturned-server-organiser.com/myip.php"));
                }
                catch (Exception)
                {
                    return "(not available)";
                }

            }
        }

        /// <summary>
        /// Returns all available maps for the server
        /// </summary>
        public IEnumerable<string> AvailableMaps
        {
            get
            {
                DirectoryInfo standardMaps = new DirectoryInfo(ServerInformation.ServerDirectory.Parent.Parent.FullName + "\\Maps\\");


                if (!standardMaps.Exists)
                {
                    try
                    {
                        standardMaps.Create();
                    }
                    catch (Exception ex) { }
                }
                else
                {
                    foreach (DirectoryInfo map in standardMaps.GetDirectories())
                    {
                        yield return map.Name;
                    }
                }
                
                DirectoryInfo workshopMaps1 = new DirectoryInfo(ServerInformation.ServerDirectory.Parent.Parent.FullName + "\\Bundles\\Workshop\\Maps");
                if (workshopMaps1.Exists)
                {
                    foreach (DirectoryInfo workshopmapMod in workshopMaps1.GetDirectories())
                    {
                        foreach (DirectoryInfo map in workshopmapMod.GetDirectories())
                        {
                            yield return map.Name;
                        }
                    }

                }
                else
                {
                    try
                    {
                        workshopMaps1.Create();
                    } catch (Exception) { }
                }

                DirectoryInfo autoUpdateMaps = new DirectoryInfo(ServerInformation.ServerDirectory.FullName + "Workshop\\Steam\\content\\304930");
                if (autoUpdateMaps.Exists)
                {
                    foreach (DirectoryInfo anyMod in autoUpdateMaps.GetDirectories())
                    {
                        FileInfo mapMeta = new FileInfo(anyMod.FullName + "\\Map.meta");
                        if (mapMeta.Exists && anyMod.GetDirectories().Length > 0)
                        {
                            yield return anyMod.GetDirectories()[0].Name;
                        }
                    }
                }

            }
        }


        /// <summary>
        /// Gets or sets if the Valve anti cheat module will be used for this server.
        /// </summary>
        public bool VAC
        {
            get
            {
                try
                {
                    AdvancedConfig config = AdvancedConfig.loadJson(ServerInformation.ServerDirectory + "\\config.json");
                    if (config == null)
                    {
                        return true;
                    }
                    else
                    {
                        return config.Server.VAC_Secure;
                    }
                }
                catch (Exception)
                {
                    return true;
                }

            }
            set
            {
                try
                {
                    AdvancedConfig config = AdvancedConfig.loadJson(ServerInformation.ServerDirectory + "\\config.json");
                    config.Server.VAC_Secure = value;
                    File.WriteAllText(ServerInformation.ServerDirectory + "\\config.json", config.getJson());
                }
                catch (Exception)   { }
            }
        }


        /// <summary>
        /// Gets or sets if the server will be automatically started when Unturned Server Organiser launches.
        /// </summary>
        public bool AutoStart
        {
            get { return new FileInfo(ServerInformation.ServerDirectory.FullName + "\\autostart.uso").Exists; }
            set
            {
                FileInfo autostart = new FileInfo(ServerInformation.ServerDirectory.FullName + "\\autostart.uso");
                if (!autostart.Directory.Exists)
                {
                    autostart.Directory.Create();
                } 
                if (value)
                {
                    if (!autostart.Exists) autostart.Create().Close();
                }
                else
                {
                    if (autostart.Exists) autostart.Delete();
                }
            }
        }


        /// <summary>
        /// Gets or sets if Unturned Server Organiser launches the server in lan or public mode.
        /// </summary>
        public bool LanServer
        {
            get { return new FileInfo(ServerInformation.ServerDirectory.FullName + "\\lan.uso").Exists; }
            set
            {
                FileInfo lan = new FileInfo(ServerInformation.ServerDirectory.FullName + "\\lan.uso");
                if (!lan.Directory.Exists)
                {
                    lan.Directory.Create();
                }
                if (value)
                {
                    if (!lan.Exists) lan.Create().Close();
                }
                else
                {
                    if (lan.Exists) lan.Delete();
                }
            }
        }


        /// <summary>
        /// Represents the current state of the server
        /// </summary>
        public U3ServerState State
        {
            get
            {
                return _state;
            }
            private set
            {
                _state = value;

                if (value == U3ServerState.Stopped)
                {
                    if ((this.ServerInformation.AutomaticRestart & !DisableAutoRestartOnceFlag) | EnableAutoRestartOnceFlag)
                    {
                        EnableAutoRestartOnceFlag = false;
                        DisableAutoRestartOnceFlag = false;
                        Start();
                    }
                }
                

                ServerStateChanged?.Invoke(this, value);
            }
        }


        /// <summary>
        /// Represents a plugin manager for the current server
        /// </summary>
        public RocketPluginManager PluginManager { get; private set; }
       
        /// <summary>
        /// Holds the CommandConfig for the server
        /// </summary>
        public CommandsConfig ServerConfig {  get; private set; }


        /// <summary>
        /// Holds the AdvancedConfig for the server
        /// </summary>
        public AdvancedConfig AdvancedConfig {  get; private set; }

        /// <summary>
        /// Holds the WorkshopUpdate config for the server
        /// </summary>
        public U3WorkshopAutoUpdaterConfig WorkshopAutoUpdaterConfig { get; private set; }



        public RocketBridgeServer RocketBridge
        {
            get
            {
                return _RocketBridge;
            }
            set
            {
                if (_RocketBridge!=null)
                {
                    _RocketBridge.PlayerListUpdated -= _RocketBridge_PlayerListUpdated;
                }

                _RocketBridge = value;

                if (_RocketBridge != null)
                {
                    _RocketBridge.PlayerListUpdated += _RocketBridge_PlayerListUpdated;
                }
            }
        }



        /// <summary>
        /// The UCBManager for the server
        /// </summary>
        public UCBManager UCBManager
        {
            get => _UCBManager;
            set
            {
                if (UCBManager != null)
                {
                    UCBManager.PackageReceived -= UCBManager_PackageReceived;
                    UCBManager.ServerIdentified -= UCBManager_ServerIdentified;
                    UCBManager.AvailableServers.Remove(this);
                }

                _UCBManager = value;

                if (UCBManager != null)
                {
                    UCBManager.PackageReceived += UCBManager_PackageReceived;
                    UCBManager.ServerIdentified += UCBManager_ServerIdentified;
                    UCBManager.ServerDisconnected += UCBManager_ServerDisconnected;
                    UCBManager.AvailableServers.Add(this);
                }
            }
        }





        /// <summary>
        /// Creates a new Unturned 3 server object
        /// </summary>
        /// <param name="info">Server process information</param>
        public U3Server(U3ServerEngineSettings info) : this(info, null) { }


        /// <summary>
        /// Creates a new Unturned 3 server object
        /// </summary>
        /// <param name="info">Server process information</param>
        /// <param name="rocketBridge">A rocket bridge server (outdated)</param>
        public U3Server(U3ServerEngineSettings info, RocketBridgeServer rocketBridge)
        {
            this.ServerInformation = info;
            this.RocketBridge = rocketBridge;
            this.PluginManager = new RocketPluginManager(this);
            this.ServerConfig = new CommandsConfig(ServerInformation.ServerDirectory); 
            this.AdvancedConfig = AdvancedConfig.loadJson(this);
            this.WorkshopAutoUpdaterConfig = new U3WorkshopAutoUpdaterConfig(this);
        }

        /// <summary>
        /// Tries to save all server configs
        /// </summary>
        public void saveConfigs()
        {
            try
            {
                this.ServerConfig.save();
                AdvancedConfig.save(this, this.AdvancedConfig);
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Starts the server
        /// </summary>
        /// <returns>Returns if the start was successfull or the reason for a failed start.</returns>
        public U3ServerStartResult Start()
        {
            if (State == U3ServerState.Stopped)
            {
                State = U3ServerState.Starting;

                try
                {
                    if (BattlEyeEnabled)
                    {
                        if (!startBattlEyeService())
                        {
                            State = U3ServerState.Stopped;
                            return U3ServerStartResult.BattlEyeFail;
                        }
                    }

                    ProcessProperties props = new ProcessProperties();
                    props.Executable = this.ServerInformation.Executable;
                    props.HideWindow = false;
                    props.RedirectStd = false;

                    props.StartedCallbackTask = new Task<bool>(() =>
                    {
                        try
                        {
                            _process.WaitForInputIdle(5000);
                        }
                        catch (Exception) {  }
                        return true;
                    });


                    _process = new UniversalProcess(props);


                    _process.StartInfo.WorkingDirectory = ServerInformation.GameDirectory.FullName;

                    _process.StartInfo.Arguments = String.Format(ServerInformation.ArgumentLine, LanServer ? "lanserver" : "internetserver", ServerInformation.ServerID);
                  

                    if (ServerInformation.StartHidden)
                    {
                        _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    }

                    if (_process is UniversalProcess)
                    {
                        ((UniversalProcess)_process).ProcessStateChanged += Process_ProcessStateChanged;
                    }

                    _process.Start();

                    

                    _process.PriorityClass = ServerInformation.HighPriorityProcess ? ProcessPriorityClass.High : ProcessPriorityClass.Normal;

                    if (UCBManager == null) State = U3ServerState.Running;

                    return U3ServerStartResult.OK;
                }
                catch (Exception)
                {
                    return U3ServerStartResult.ProcessCreationFailure;
                }
            }
            else
            {
                return U3ServerStartResult.AlreadyRunning;
            }
            
        }


        /// <summary>
        /// Requests the UCB Manager to get a complete console log
        /// </summary>
        public void requestServerLog()
        {
            UCBManager.sendPackage(this, new OPackage(NetPackageHeader.GameOutputRequest));
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        /// <param name="countdown">Waits cooldown seconds until the server will be stopped</param>
        public void Stop(int countdown)
        {
            initShutdownCountdown(countdown);

            DisableAutoRestartOnceFlag = true;
            State = U3ServerState.Stopping;
            if (countdown == 0)
            {
                sendCommand("shutdown 0");
            }
            else
            {
                sendCommand(string.Format("shutdown {0}", countdown));
            }
        }


        /// <summary>
        /// Restarts the server
        /// </summary>
        /// <param name="countdown">Waits cooldown seconds until the server will be restarted</param>
        public void Restart(int countdown)
        {
            initShutdownCountdown(countdown);
            EnableAutoRestartOnceFlag = true;
            State = U3ServerState.Restarting;
            if (countdown == 0)
            {
                sendCommand("shutdown 0");
            }
            else
            {
                sendCommand(string.Format("shutdown {0}", countdown));
            }
        }


        


        /// <summary>
        /// Instantly kills the current server process. Will lead to a loss of savegame progess. 
        /// </summary>
        /// <param name="restart">Restart the ser</param>
        public void Kill(bool restart)
        {
            if (_process != null)
            {
                if (!_process.HasExited)
                {
                    _process.Kill();
                }
                if (restart)
                {
                    Start();
                }
            }
        }


        /// <summary>
        /// Boradcasts a message on the server
        /// </summary>
        /// <param name="text">The message to be broadcasted.</param>
        public void say(string text)
        {
            sendCommand("/broadcast " + text);
        }

        /// <summary>
        /// Tries to execute a command in the server console.
        /// </summary>
        /// <param name="command">The command to be executed.</param>
        public void sendCommand(string command)
        {
            if (UCBManager == null || !UCBManager.sendCommand(this,command))
            {
                if (_process != null && !_process.HasExited)
                {

                    IntPtr currentFocus = UniversalOrganiserControls.UtilsGeneral.GetForegroundWindow();
                    UniversalOrganiserControls.UtilsGeneral.SetForegroundWindow(_process.MainWindowHandle);


                    SendKeys.SendWait("{ENTER}");
                    SendKeys.Flush();
                    SendKeys.SendWait(command);
                    SendKeys.Flush();

                    SendKeys.SendWait("{ENTER}");
                    SendKeys.Flush();

                    UniversalOrganiserControls.UtilsGeneral.SetForegroundWindow(currentFocus);
                }
            }
        }

        /// <summary>
        /// Gets all Steam workshop mods installed on the server
        /// </summary>
        /// <returns>A list of workshop mods</returns>
        public IEnumerable<U3WorkshopMod_Managed> getWorkshopContentMods()
        {
            List<U3WorkshopMod_Managed> installed = new List<U3WorkshopMod_Managed>();
            DirectoryInfo contentMods = new DirectoryInfo(ServerInformation.ServerDirectory.FullName + "\\Workshop\\Content");
            if (contentMods.Exists)
            {
                foreach (DirectoryInfo ModFile in contentMods.GetDirectories())
                {
                    yield return new U3WorkshopMod_Managed(ModFile);
                }
            }
            else
            {
                contentMods.Create();
            }
        }

        /// <summary>
        /// Searches after a specific workshop mod in the list of Steam workshop mods
        /// </summary>
        /// <param name="name">The name of the mod to search for</param>
        /// <returns>The mod found</returns>
        public U3WorkshopMod_Managed getModByName(string name)
        {
            return getWorkshopContentMods().First((s) => { return s.Name == name; });
        }

        /// <summary>
        /// Installs the BattlEye anti cheat service on the current system.
        /// </summary>
        /// <param name="ignoreAlreadyInstalled">Set for ignoring the current installation state</param>
        /// <returns>Returns true when the service has been installed successfully</returns>
        public bool installBattlEyeService(bool ignoreAlreadyInstalled = false)
        {
            if (!BattlEyeInstalled | ignoreAlreadyInstalled)
            {
                try
                {
                    Process battleye = new Process();
                    battleye.StartInfo.FileName = BattlEyeLauncher.FullName;
                    battleye.StartInfo.Arguments = "1 0";
                    battleye.StartInfo.WorkingDirectory = ServerInformation.ServerDirectory.FullName;
                    battleye.Start();
                    battleye.WaitForExit();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Starts the BattlEye anti cheat service
        /// </summary>
        /// <returns>Returns true if started successfully</returns>
        public bool startBattlEyeService()
        {
            if (!installBattlEyeService())
            {
                return false;
            }

            try
            {
                ServiceController s = new ServiceController("BEService");
                if (s.Status != ServiceControllerStatus.Running)
                {
                    s.Start();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }



        private void Process_ProcessStateChanged(object sender, ProcessState e)
        {
            switch (e)
            {
                case ProcessState.Running:
                    this.State = U3ServerState.Running;
                    break;
                case ProcessState.Starting:
                    this.State = U3ServerState.Starting;
                    break;
                case ProcessState.Stoppting:
                    this.State = U3ServerState.Stopping;
                    break;
                case ProcessState.Restarting:
                    this.State = U3ServerState.Restarting;
                    break;
                case ProcessState.Stopped:
                    this.State = U3ServerState.Stopped;
                    break;
                default:
                    break;
            }
        }

        private void updateRocketBridgeConfig()
        {
            if (RocketModInstalled & RocketBridge != null)
            {
                DirectoryInfo dir = new DirectoryInfo(ServerInformation.ServerDirectory.FullName + "\\Rocket\\Plugins\\rocketbridge");
                if (!dir.Exists)
                {
                    dir.Create();
                }

                FileInfo file = new FileInfo(dir.FullName + "\\rocketbridge.configuration.xml");
                if (file.Exists)
                {
                    file.Delete();
                }
                XmlDocument RocketBridgeXML = new XmlDocument();
                RocketBridgeXML.LoadXml("<RocketBridgeConfig xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n<port>" + RocketBridge.port.ToString() + "</port>\n</RocketBridgeConfig>");
                RocketBridgeXML.Save(file.FullName);
            }
        }


        private void _RocketBridge_PlayerListUpdated(U3Server u3server, List<string> steamid)
        {
            if (u3server.ServerInformation.ServerID.Equals(this.ServerInformation.ServerID))
            {
                this.PlayerListUpdated?.Invoke(this, steamid);
            }
        }


        private void UCBManager_ServerDisconnected(object sender, U3Server e)
        {
            UCBDisconnected?.Invoke(this, new EventArgs());
        }

        private void UCBManager_ServerIdentified(object sender, U3Server e)
        {
            if (e == this)
            {
                State = U3ServerState.Running;

                if (_process == null)
                {
                    int PID = Convert.ToInt32(UCBManager.getConnection(this).PID);
                    _process = Process.GetProcessById(PID);
                    _process.EnableRaisingEvents = true;
                    _process.Exited += (p, e2) => { State = U3ServerState.Stopped; };
                }

                UCBConnected?.Invoke(this, new EventArgs());

            }
        }


        private void UCBManager_PackageReceived(object sender, UCBManager.PackageReceivedEventArgs e)
        {
            if (e.Package.Header == NetPackageHeader.GameOutputLine)
            {
                if (e.Server == this)
                {
                    StringPackage package = (StringPackage)e.Package;
                    ConsoleOutput?.Invoke(this, package.Message);
                }
            }
            else if (e.Package.Header == NetPackageHeader.GameOutput)
            {
                if (e.Server == this)
                {
                    StringPackage package = (StringPackage)e.Package;
                    CompleteConsoleOutput?.Invoke(this, package.Message);
                    Console.WriteLine(package.Message);
                }
            }
        }

        private void initShutdownCountdown(int countdown)
        {
            if (countdown != 0)
            {

                try
                {
                    say(string.Format(ServerInformation.ShutdownMessage, countdown.ToString()));
                }
                catch (Exception)
                {
                    say(ServerInformation.ShutdownMessage);
                }

                Task.Run(async () =>
                {
                    sendCommand(string.Format("shutdown {0}", countdown));
                    for (int i = countdown; i >= 0; i--)
                    {
                        if (i % ServerInformation.ShutdownMessageIntervall == 0)
                        {
                            try
                            {
                                say(string.Format(ServerInformation.ShutdownMessage, i.ToString()));
                            }
                            catch (Exception)
                            {
                                say(ServerInformation.ShutdownMessage);
                            }
                        }
                        await Task.Delay(1000);
                    }

                });
            }
        }

    }
}
