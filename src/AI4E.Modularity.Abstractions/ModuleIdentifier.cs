using System;
using System.ComponentModel;
using System.Globalization;

namespace AI4E.Modularity
{
    // A handle for a module (f.e. AI4E.Clustering)
    [TypeConverter(typeof(ModuleIdentfierTypeConverter))]
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

    public sealed class ModuleIdentfierTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(ModuleIdentifier) ||
                   sourceType == typeof(ModuleIdentifier?) ||
                   sourceType == typeof(string) ||
                   base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(ModuleIdentifier) ||
                   destinationType == typeof(ModuleIdentifier?) ||
                   destinationType == typeof(string) ||
                   base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is ModuleIdentifier identifier)
            {
                return value;
            }
            else if (value is ModuleIdentifier?)
            {
                return ((ModuleIdentifier?)value).Value;
            }
            else if (value is string str)
            {
                return new ModuleIdentifier(str);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is ModuleIdentifier identifier)
            {
                if (destinationType == typeof(ModuleIdentifier))
                {
                    return identifier;
                }
                else if (destinationType == typeof(ModuleIdentifier?))
                {
                    return new ModuleIdentifier?(identifier);
                }
                else if (destinationType == typeof(string))
                {
                    return identifier.Name;
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
