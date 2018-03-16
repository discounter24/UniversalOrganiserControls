using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace UniversalOrganiserControls
{
 
    public enum ProcessState { Running, Starting, Stoppting, Restarting, Stopped }

    public class UniversalProcess : Process
    {
        

        public event EventHandler<ProcessState> ProcessStateChanged;


        private ProcessState _pstate = ProcessState.Stopped;
        public ProcessState ProcessState
        {
            get
            {
                return _pstate;
            }
            protected set
            {
                _pstate = value;
                ProcessStateChanged?.Invoke(this, value);
            }
        }
        private ProcessProperties Properties = new ProcessProperties();

        public UniversalProcess(ProcessProperties properties = null) :  base()
        {
            if (properties != null) this.Properties = properties;
            this.EnableRaisingEvents = true;
            this.StartInfo.UseShellExecute = false;
            this.StartInfo.RedirectStandardError = true;
            this.StartInfo.RedirectStandardOutput = true;
            this.StartInfo.RedirectStandardInput = true;

            this.StartInfo.CreateNoWindow = this.Properties.HideWindow;
            this.StartInfo.FileName = this.Properties.Executable==null ? this.StartInfo.FileName : this.Properties.Executable.ToString();

            this.Exited += (sender, e) => 
            {
                if (this.ProcessState == ProcessState.Restarting)
                {
                    this.Start();
                }
                else
                {
                    this.ProcessState = ProcessState.Stopped;
                }

            };

           
        }
        

        public new void Start()
        {

            try
            {
                this.ProcessState = ProcessState.Starting;
                base.Start();
                this.BeginErrorReadLine();
                this.BeginOutputReadLine();
                Task.Run(() =>
                {
                    Task<bool> wait = this.Properties.StartedCallbackTask;
                    wait.Start();
                    if (this.ProcessState == ProcessState.Stoppting)
                    {
                        this.ProcessState = wait.Result ? ProcessState.Running : ProcessState.Stopped;
                    }
                });

            }
            catch (Exception ex)
            {
                this.ProcessState = ProcessState.Stopped;
                Console.WriteLine(ex.ToString());
                throw;
            }
            
        }

        public void Start(params string[] args)
        {
            foreach(string arg in args)
            {
                this.StartInfo.Arguments += arg;
            }
            this.Start();
        }

        public void StartAsAdmin(params string[] args)
        {
            this.StartInfo.Verb = "runas";
            this.Start(args);
        }

        public void TimedInput(int seconds, params string[] commands)
        {
            Task.Run(async () =>
            {
                await Task.Delay(seconds * 1000);
                Input(commands);
            });
        }

        public void Input(params string[] commands)
        {
            foreach(string command in commands)
            {
                this.StandardInput.WriteLine(command);
                this.StandardInput.Flush();
            }
            if (this.Properties.SendEmptyFinishingCommand)
            {
                this.StandardInput.WriteLine(Environment.NewLine);
                this.StandardInput.Flush();
            }

        }

        public void Stop(uint delay = 0)
        {
            Task.Run(async () =>
            {
                this.ProcessState = ProcessState.Restarting;

                if (this.ProcessState != ProcessState.Restarting)
                {
                    this.ProcessState = ProcessState.Stoppting;
                }


                if (Properties.ShutdownCommand != null) Input(Properties.ShutdownCommand);
                await Task.Delay((int)(delay * 1000));
                if (!this.HasExited)
                {
                    this.Kill();
                }


            });
        }

        public void Restart(uint delay = 0)
        {
            this.ProcessState = ProcessState.Restarting;
            Stop(delay);
        }
    }
}
