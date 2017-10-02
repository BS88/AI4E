using System;

namespace AI4E.Modularity
{
    // A handle for a module (f.e. AI4E.Clustering)
    public struct ModuleIdentifier : IEquatable<ModuleIdentifier>
    {
        public static ModuleIdentifier UnknownModule { get; } = new ModuleIdentifier();

        public ModuleIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("The argument must neither be null nor an empty string or a string that consists of whitespace only.", nameof(name));

            Name = name;
        }

        public string Name { get; }

        public bool Equals(ModuleIdentifier other)
        {
            return other.Name == Name;
        }

        public override bool Equals(object obj)
        {
            return obj is ModuleIdentifier module && Equals(module);
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            if (this == UnknownModule)
                return "Unknown module";

            return Name;
        }

        public static bool operator ==(ModuleIdentifier left, ModuleIdentifier right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ModuleIdentifier left, ModuleIdentifier right)
        {
            return !left.Equals(right);
        }
    }
}
