using System;

namespace AI4E.Modularity
{
    public struct ModuleReleaseIdentifier : IEquatable<ModuleReleaseIdentifier>
    {
        public static ModuleReleaseIdentifier UnknownModuleRelease { get; } = default;

        public ModuleReleaseIdentifier(ModuleIdentifier module, ModuleVersion version)
        {
            if (module == ModuleIdentifier.UnknownModule || version == ModuleVersion.Unknown)
            {
                this = default;
            }
            else
            {
                Module = module;
                Version = version;
            }
        }

        public ModuleReleaseIdentifier(string name, ModuleVersion version)
        {
            if (version == ModuleVersion.Unknown)
            {
                this = default;
            }
            else
            {
                Module = new ModuleIdentifier(name);
                Version = version;
            }
        }

        public ModuleIdentifier Module { get; }
        public ModuleVersion Version { get; }

        public bool Equals(ModuleReleaseIdentifier other)
        {
            return other.Module == Module &&
                   other.Version == Version;
        }

        public override bool Equals(object obj)
        {
            return obj is ModuleReleaseIdentifier id && Equals(id);
        }

        public override int GetHashCode()
        {
            return Module.GetHashCode() ^ Version.GetHashCode();
        }

        public override string ToString()
        {
            if (this == UnknownModuleRelease)
                return "Unknown module release";

            return $"{Module} {Version}";
        }

        public static bool operator ==(ModuleReleaseIdentifier left, ModuleReleaseIdentifier right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ModuleReleaseIdentifier left, ModuleReleaseIdentifier right)
        {
            return !left.Equals(right);
        }
    }
}
