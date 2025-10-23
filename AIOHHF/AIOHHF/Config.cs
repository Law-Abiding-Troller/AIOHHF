using Nautilus.Json;
using Nautilus.Options.Attributes;

namespace AIOHHF;
    [Menu("AIOHHF")]
    public class Config : ConfigFile
    {
        [Toggle("Enable Debug Mode? (requires restart)")]
        public bool DebugMode = false;
    }