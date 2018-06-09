using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.ServiceProcess;

using UniversalOrganiserControls;
using UniversalOrganiserControls.Unturned3.Configuration;
using UniversalOrganiserControls.Unturned3;

namespace UniversalOrganiserControls.Unturned3
{
    public class U3Server
    {

        public event ServerStateChanged ServerStateChanged;

        private bool consoleVisibility = true;
        public bool ConsoleVisible
        {
            get
            {
                return consoleVisibility;
            }
            set
            {
                UniversalOrganiserControls.Utils.ShowWindow(process.MainWindowHandle, 
                    value ? UniversalOrganiserControls.Utils.WinApiWindowState.Show : UniversalOrganiserControls.Utils.WinApiWindowState.Hide);
                consoleVisibility = value;
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
        }

        public bool AutoStart
        {
            get { return new FileInfo(ServerInformation.ServerDirectory.FullName + "\\autostart.uso").Exists; }
            set
            {
                FileInfo insecure = new FileInfo(ServerInformation.ServerDirectory.FullName + "\\autostart.uso");
                if (value)
                {
                    if (!insecure.Exists) insecure.Create().Close();
                }
                else
                {
                    if (insecure.Exists) insecure.Delete();
                }
            }
        }

        public bool LanServer
        {
            get { return new FileInfo(ServerInformation.ServerDirectory.FullName + "\\lan.uso").Exists; }
            set
            {
                FileInfo lan = new FileInfo(ServerInformation.ServerDirectory.FullName + "\\lan.uso");
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
                ServerStateChanged?.Invoke(this, value);
            }
        }

        private UniversalProcess process;
        
        

        public U3Server(U3ServerEngineSettings info)
        {
            this.ServerInformation = info;
        }

        public U3ServerStartResult Start()
        {
            if (State != U3ServerState.Stopped)
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
                    props.HideWindow = true;



                    process = new UniversalProcess(props);
                    process.StartInfo.WorkingDirectory = ServerInformation.ServerDirectory.FullName;
                    process.StartInfo.Arguments = String.Format(ServerInformation.ArgumentLine, LanServer ? "lanserver" : "internetserver", ServerInformation.ServerID);


                    process.Start();


                    process.PriorityClass = ServerInformation.HighPriorityProcess ? ProcessPriorityClass.High : ProcessPriorityClass.Normal;


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
        
    }
}
