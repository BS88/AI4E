using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace AI4E.Modularity
{
    [TypeConverter(typeof(ModuleVersionConverter))]
    public struct ModuleVersion : IEquatable<ModuleVersion>, IComparable<ModuleVersion>
    {
        public static ModuleVersion Unknown { get; } = new ModuleVersion();

        public ModuleVersion(int majorVersion, int minorVersion, int revision, bool isPreRelease)
        {
            if (majorVersion < 0)
                throw new ArgumentOutOfRangeException(nameof(majorVersion));

            if (minorVersion < 0)
                throw new ArgumentOutOfRangeException(nameof(minorVersion));

            if (revision < 0)
                throw new ArgumentOutOfRangeException(nameof(revision));

            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            Revision = revision;
            IsPreRelease = isPreRelease;
        }

        public int MajorVersion { get; }
        public int MinorVersion { get; }
        public int Revision { get; }
        public bool IsPreRelease { get; }

        public bool Equals(ModuleVersion other)
        {
            return MajorVersion == other.MajorVersion &&
                   MinorVersion == other.MinorVersion &&
                   Revision == other.Revision &&
                   IsPreRelease == other.IsPreRelease;
        }

        public override bool Equals(object obj)
        {
            return obj is ModuleVersion other && Equals(other);
        }

        public int CompareTo(ModuleVersion other)
        {
            var comparison = MajorVersion.CompareTo(other.MajorVersion);

            if (comparison != 0)
            {
                return comparison;
            }

            comparison = MinorVersion.CompareTo(other.MinorVersion);

            if (comparison != 0)
            {
                return comparison;
            }

            comparison = Revision.CompareTo(other.Revision);

            if (comparison != 0)
            {
                return comparison;
            }

            if (IsPreRelease && !other.IsPreRelease)
            {
                return -1;
            }

            if (other.IsPreRelease && !IsPreRelease)
            {
                return 1;
            }

            return 0;
        }

        public override int GetHashCode()
        {
            return MajorVersion.GetHashCode() ^
                   MinorVersion.GetHashCode() ^
                   Revision.GetHashCode() ^
                   IsPreRelease.GetHashCode();
        }

        public override string ToString()
        {
            var result = $"{GetComponentString(MajorVersion)}.{GetComponentString(MinorVersion)}.{GetComponentString(Revision)}";

            if (IsPreRelease)
                return result + "-pre";

            return result;
        }

        public static bool operator ==(ModuleVersion left, ModuleVersion right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ModuleVersion left, ModuleVersion right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(ModuleVersion left, ModuleVersion right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(ModuleVersion left, ModuleVersion right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(ModuleVersion left, ModuleVersion right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(ModuleVersion left, ModuleVersion right)
        {
            return left.CompareTo(right) >= 0;
        }

        private static string GetComponentString(int? component)
        {
            if (component == null)
                return "*";

            return component.ToString();
        }

        public static ModuleVersion Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullOrWhiteSpaceException(nameof(value));

            var components = value.Split('.');

            if (components.Length == 0 || components.Length > 3)
                throw new FormatException();

            var isPreRelease = false;

            if (components.Last().EndsWith("-pre"))
            {
                isPreRelease = true;
                components[components.Length - 1] = components[components.Length - 1].Substring(0, components.Last().Length - 4);
            }

            var c0 = components[0].Trim();

            var major = int.Parse(c0);
            var minor = 0;
            var revision = 0;

            if (components.Length > 1)
            {
                var c1 = components[1].Trim();

                minor = int.Parse(c1);
            }

            if (components.Length > 2)
            {
                var c2 = components[2].Trim();

                revision = int.Parse(c2);
            }

            return new ModuleVersion(major, minor, revision, isPreRelease);
        }
    }

    public sealed class ModuleVersionConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(ModuleVersion) || sourceType == typeof(ModuleVersion?) || sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(ModuleVersion) || destinationType == typeof(ModuleVersion?) || destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            switch (value)
            {
                case ModuleVersion moduleVersion:
                    return moduleVersion;

                case string str:
                    return ModuleVersion.Parse(str);

                default:
                    return base.ConvertFrom(context, culture, value);
            }
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is ModuleVersion moduleVersion)
            {
                if (destinationType == typeof(ModuleVersion) || destinationType == typeof(ModuleVersion?))
                {
                    return moduleVersion;
                }

                if (destinationType == typeof(string))
                {
                    return moduleVersion.ToString();
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
