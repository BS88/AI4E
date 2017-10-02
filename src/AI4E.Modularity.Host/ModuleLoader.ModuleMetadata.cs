using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AI4E.Modularity
{
    public sealed partial class ModuleLoader
    {
        private sealed class ModuleMetadata : IModuleMetadata
        {
            private string _descriptiveName;

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("version")]
            public ModuleVersion Version { get; set; }

            [JsonProperty("release-date")]
            public DateTime ReleaseDate { get; set; }

            [JsonProperty("descriptive-name")]
            public string DescriptiveName
            {
                get => _descriptiveName ?? Name;
                set => _descriptiveName = value;
            }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonIgnore]
            public ModuleIcon Icon { get; set; }

            [JsonProperty("author")]
            public string Author { get; set; }

            [JsonProperty("reference-page")]
            public string ReferencePageUri { get; set; }

            [JsonProperty("entry-assembly-path")]
            public string EntryAssemblyPath { get; set; }

            ICollection<IModuleDependency> IModuleMetadata.Dependencies => Dependencies.ToArray();

            [JsonProperty("dependencies")]
            List<ModuleDependency> Dependencies { get; } = new List<ModuleDependency>();
        }
    }
}
