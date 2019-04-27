using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalOrganiserControls.Minecraft.Server
{

    public enum ServerCreationState {Canceled, Finishing, PreparingSpawn, Preparing, GettingDownloadInformation, StartingDownload, Downloading, FirstStart, Finished }

    public enum ServerCreationCanceledReason { EulaAgreementRequired, NotSupportedServerType, JavaNotInstalled, TargetDirectoryNotEmpty, IOException, DownloadFailed, Unknown }


    public class ServerCreationStateArgs
    {
       
        private ServerCreationState state;
        public ServerCreationState State
        {
            get { return state; }
            protected set { state = value; }
        }


        public ServerCreationStateArgs(ServerCreationState state)
        {
            State = state;
        }
    }


    public class ServerCreationDownloadState : ServerCreationStateArgs
    {
        private int progressPercentage;
        public int ProgressPercentage
        {
            get { return progressPercentage; }
        }

        private long bytesReceived = 0;
        public long BytesReceived
        {
            get { return bytesReceived; }
        }

        private long bytesTotal = 0;
        public long BytesToReceive
        {
            get { return bytesTotal; }
        }


        public ServerCreationDownloadState(int progressPercentage, long bytesReceived, long bytesTotal) : base(ServerCreationState.Downloading)
        {
            this.progressPercentage = progressPercentage;
            this.bytesReceived = bytesReceived;
            this.bytesTotal = bytesTotal;
        }

    }

    public class ServerCreationCanceledState : ServerCreationStateArgs
    {
        private Exception exception;
        public Exception Exception
        {
            get { return exception; }
        }

        private ServerCreationCanceledReason reason;
        public ServerCreationCanceledReason Reason
        {
            get { return reason; }
        }

        public ServerCreationCanceledState(ServerCreationCanceledReason reason, Exception exception = null) : base(ServerCreationState.Canceled)
        {
            this.reason = reason;
            this.exception = exception;
        }

    }





}
