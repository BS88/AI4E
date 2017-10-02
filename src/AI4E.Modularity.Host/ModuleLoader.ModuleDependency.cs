using Newtonsoft.Json;

namespace AI4E.Modularity
{
    public sealed partial class ModuleLoader
    {
        private sealed class ModuleDependency : IModuleDependency
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("version")]
            public ModuleVersionFilter Version { get; set; }
        }
    }
}
