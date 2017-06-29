using System;
using System.Collections.Immutable;

namespace AI4E.Modularity
{
    public sealed class ModuleDetails
    {
        public ModuleDetails(string name, ModuleVersion version, string descriptiveName, string description, ImmutableArray<VersionedModule> dependencies)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The argument must neither be null nor an empty string or a string that consists of whitespace only.", nameof(name));

            if (string.IsNullOrWhiteSpace(descriptiveName))
                throw new ArgumentException("The argument must neither be null nor an empty string or a string that consists of whitespace only.", nameof(descriptiveName));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("The argument must neither be null nor an empty string or a string that consists of whitespace only.", nameof(description));

            Name = name;
            Version = version;
            DescriptiveName = descriptiveName;
            Description = description;
            Dependencies = dependencies;
        }

        public string Name { get; }

        public ModuleVersion Version { get; }

        public string DescriptiveName { get; }

        public string Description { get; }

        public ImmutableArray<VersionedModule> Dependencies { get; }
    }
}
