using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UniversalOrganiserControls.Unturned3.RocketMod.Plugin
{


    public class ExtractZipResult
    {
        public List<ExtractEntryResult> entries;

    }

    public class ExtractEntryResult
    {
        public PluginEntryType Type;
        public FileInfo File;
    }

    public enum PluginEntryType
    {
        Plugin,
        Dependency,
        Unknown
    }
    

    public enum PluginInstallationResult
    {
        OK,
        FailedNotAZip,
        Failed
    }
}
