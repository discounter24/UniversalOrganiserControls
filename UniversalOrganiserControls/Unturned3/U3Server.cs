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
    public class U3Server
    {

        public event EventHandler<string> CompleteConsoleOutput;
        public event EventHandler<string> ConsoleOutput;

        public event EventHandler UCBConnected;
        public event EventHandler UCBDisconnected;

        public event EventHandler<U3ServerRenamedArgs> ServerRenamed;
        public event EventHandler<U3ServerState> ServerStateChanged;
        public event EventHandler<List<string>> PlayerListUpdated;
        public event EventHandler<bool> UPnPStateChanged;

        public IntPtr MainWindowHandle
        {
            get
            {
                try
                {
                    return process.MainWindowHandle;
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

        private bool consoleVisibility = true;
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
                    //IntPtr handle = process.MainWindowHandle;

                    if (value)
                    {
                        UniversalOrganiserControls.UtilsGeneral.ShowWindow(process.MainWindowHandle, UniversalOrganiserControls.UtilsGeneral.WinApiWindowState.Show);
                    }
                    else
                    {
                        UniversalOrganiserControls.UtilsGeneral.ShowWindow(process.MainWindowHandle, UniversalOrganiserControls.UtilsGeneral.WinApiWindowState.Hide);
                    }
                    consoleVisibility = value;
                }
                catch (Exception)
                {  }

            }
        }

        public U3ServerEngineSettings ServerInformation
        {
            get; set;
        }

        #region BattlEye

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

        private FileInfo BattlEyeLauncher
        {
            get
            {
                return new FileInfo(ServerInformation.Executable.Directory.FullName + "\\Unturned_BE.exe");
            }
        }

        public bool installBattlEyeService(bool installAgain = false)
        {      
            if (!BattlEyeInstalled | installAgain)
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

        #endregion

        public string LastPID
        {
            get
            {
                try
                {
                    string pid = File.ReadAllText(ServerInformation.ServerDirectory.FullName + "\\server.pid");
                    return pid;
                }
                catch (Exception)
                {
                    return "-1";
                }
            }
        }

        private List<string> _PlayerList = new List<string>();
        public List<string> PlayerList
        {
            get
            {
                return _PlayerList;
            }
            set
            {
                _PlayerList = value;
                PlayerListUpdated.Invoke(this, _PlayerList);
            }
        }

        public bool RocketMod
        {
            get
            {
                return File.Exists(ServerInformation.ServerDirectory.Parent.Parent.FullName + "\\Modules\\Rocket.Unturned\\Rocket.Core.dll");
            }
        }

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

        private U3ServerState _state = U3ServerState.Stopped;
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

        public RocketPluginManager PluginManager { get; private set; }

        public CommandsConfig ServerConfig
        {
            get
            {
               
                return new CommandsConfig(this.ServerInformation.ServerDirectory);
            }
        }

        public AdvancedConfig AdvancedConfig
        {
            get
            {
                return AdvancedConfig.loadJson(this);
            }
        }

        private Process process;


        private RocketBridgeServer _RocketBridge = null;
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

        private UCBManager _UCBManager = null;
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

        private void UCBManager_ServerDisconnected(object sender, U3Server e)
        {
            UCBDisconnected?.Invoke(this, new EventArgs());
        }

        private void UCBManager_ServerIdentified(object sender, U3Server e)
        {
            if (e==this)
            {
                State = U3ServerState.Running;
                
                if (process == null)
                {
                    int PID = Convert.ToInt32(UCBManager.getConnection(this).PID);
                    process = Process.GetProcessById(PID);
                    process.EnableRaisingEvents = true;
                    process.Exited += (p, e2) => { State = U3ServerState.Stopped; };
                }

                UCBConnected?.Invoke(this, new EventArgs());

            }
        }


        private void UCBManager_PackageReceived(object sender, UCBManager.PackageReceivedEventArgs e)
        {
            if (e.Package.Header== NetPackageHeader.GameOutputLine)
            {
                if (e.Server == this)
                {
                    StringPackage package = (StringPackage)e.Package;
                    ConsoleOutput?.Invoke(this, package.Message);
                }
            } else if (e.Package.Header == NetPackageHeader.GameOutput)
            {
                if (e.Server==this)
                {
                    StringPackage package = (StringPackage)e.Package;
                    CompleteConsoleOutput?.Invoke(this, package.Message);
                    Console.WriteLine(package.Message);
                }
            }
        }

        private bool DisableAutoRestartOnceFlag = false;
        private bool EnableAutoRestartOnceFlag = false;


        public U3Server(U3ServerEngineSettings info) : this(info, null) { }

        public U3Server(U3ServerEngineSettings info, RocketBridgeServer rocketBridge)
        {
            this.ServerInformation = info;
            this.RocketBridge = rocketBridge;
            this.PluginManager = new RocketPluginManager(this);
        }

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
                            process.WaitForInputIdle(5000);
                        }
                        catch (Exception) {  }
                        return true;
                    });


                    process = new UniversalProcess(props);


                    //process.StartInfo.WorkingDirectory = ServerInformation.GameDirectory.FullName;
                    process.StartInfo.WorkingDirectory = ServerInformation.ServerDirectory.FullName;
                    process.StartInfo.Arguments = String.Format(ServerInformation.ArgumentLine, LanServer ? "lanserver" : "internetserver", ServerInformation.ServerID);
                  

                    if (ServerInformation.StartHidden)
                    {
                        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    }

                    if (process is UniversalProcess)
                    {
                        ((UniversalProcess)process).ProcessStateChanged += Process_ProcessStateChanged;
                    }

                    process.Start();

                    

                    process.PriorityClass = ServerInformation.HighPriorityProcess ? ProcessPriorityClass.High : ProcessPriorityClass.Normal;

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


        public void requestServerLog()
        {
            UCBManager.sendPackage(this, new OPackage(NetPackageHeader.GameOutputRequest));
        }

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

        public void Kill(bool restart)
        {
            if (process != null)
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
                if (restart)
                {
                    Start();
                }
            }
        }


        public void say(string text)
        {
            sendCommand("/broadcast " + text);
        }

        public void sendCommand(string command)
        {
            if (UCBManager == null || !UCBManager.sendCommand(this,command))
            {
                if (process != null && !process.HasExited)
                {

                    IntPtr currentFocus = UniversalOrganiserControls.UtilsGeneral.GetForegroundWindow();
                    UniversalOrganiserControls.UtilsGeneral.SetForegroundWindow(process.MainWindowHandle);


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
            if (RocketMod & RocketBridge != null)
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

        public IEnumerable<U3WorkshopMod> getWorkshopContentMods()
        {
            List<U3WorkshopMod> installed = new List<U3WorkshopMod>();
            DirectoryInfo contentMods = new DirectoryInfo(ServerInformation.ServerDirectory.FullName + "\\Workshop\\Content");
            if (contentMods.Exists)
            {
                foreach (DirectoryInfo ModFile in contentMods.GetDirectories())
                {
                    yield return new U3WorkshopMod(ModFile);
                }
            }
            else
            {
                contentMods.Create();
            }
        }

        public U3WorkshopMod getModByName(string name)
        {
            return getWorkshopContentMods().First((s) => { return s.Name == name; });
        }


    }
}
