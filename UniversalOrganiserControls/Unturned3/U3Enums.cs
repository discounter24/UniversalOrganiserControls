using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Unturned3
{
    public enum U3ServerState { Starting, Restarting, Stopping, Running, Stopped}
    public enum U3ServerStartResult { OK, AlreadyRunning, BattlEyeFail, ProcessCreationFailure}
    public enum U3InstallationState { SearchingUpdates, CalculatingFileDifferences, Downloading, Ok, FailedSome, FailedInternet, FailedUnknown, FailedInvalidResponse }
}
