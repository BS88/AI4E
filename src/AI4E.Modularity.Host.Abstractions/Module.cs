using System;

namespace AI4E.Modularity
{
    // A handle for a module (f.e. AI4E.Clustering)
    public struct Module : IEquatable<Module>
    {
        private static Module Unknown { get; } = new Module();

        public Module(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The argument must neither be null nor an empty string or a string that consists of whitespace only.", nameof(name));

            Name = name;
        }

        public string Name { get; }

        public bool Equals(Module other)
        {
            return other.Name == Name;
        }

        public override bool Equals(object obj)
        {
            return obj is Module module && Equals(module);
        }

        public override int GetHashCode()
        {
            return Name == null ? 0 : Name.GetHashCode();
        }

        public override string ToString()
        {
            if (this == Unknown)
                return "Unknown module";

            return Name;
        }

        public static bool operator ==(Module left, Module right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Module left, Module right)
        {
            return !left.Equals(right);
        }
    }

    // A handle for a versioned module (d.e. AI4E.Clustering [1.0.0-Pre])
    public struct VersionedModule : IEquatable<VersionedModule>
    {
        public static VersionedModule Unknown { get; } = new VersionedModule();

        public VersionedModule(string name, ModuleVersion version)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The argument must neither be null nor an empty string or a string that consists of whitespace only.", nameof(name));

            Name = name;
            Version = version;
        }

        public ModuleVersion Version { get; }
        public string Name { get; }

        public bool Equals(VersionedModule other)
        {
            return other.Version == Version &&
                   other.Name == Name;
        }

        public override bool Equals(object obj)
        {
            return obj is VersionedModule module && Equals(module);
        }

        public override int GetHashCode()
        {
            return (Name == null ? 0 : Name.GetHashCode()) ^ Version.GetHashCode();
        }

        public override string ToString()
        {
            if (this == Unknown)
                return "Unknown module";

            return $"{Name} {Version}";
        }

        public static bool operator ==(VersionedModule left, VersionedModule right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VersionedModule left, VersionedModule right)
        {
            return !left.Equals(right);
        }
    }
}
