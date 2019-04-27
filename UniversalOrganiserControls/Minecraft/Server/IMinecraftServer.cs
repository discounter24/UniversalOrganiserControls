using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UniversalOrganiserControls.Minecraft.Server
{
    public interface IMinecraftServer
    {
       
        StartResult Start();


        StopResult Stop(int seconds = 0);
        StopResult Kill();

        CommandResult Execute(string command);
        CommandResult Say(string message);
    }
    
    public enum CommandResult { OK, ServerNotRunning}

    public enum StartResult { OK, AlreadyRunning, JavaNotInstalled, IOFailture, ProcessAccessFailture }
    public enum StopResult { OK, AlreadyStopped, ProcessAccessFailture }
    
    public enum ServerState { Starting, Stopping, Stopped, Running}


}
