/* Summary
 * --------------------------------------------------------------------------------------------------------------------
 * Filename:        TypeMeta.cs 
 * Types:           AI4E.Modularity.TypeMeta
 *                  --TODO--
 * Version:         1.0
 * Author:          Andreas Trütschel
 * Last modified:   19.10.2017 
 * --------------------------------------------------------------------------------------------------------------------
 */

/* License
 * --------------------------------------------------------------------------------------------------------------------
 * This file is part of the AI4E distribution.
 *   (https://gitlab.com/EnterpriseApplicationEquipment/AI4E)
 * Copyright (c) 2017 Andreas Trütschel.
 * 
 * AI4E is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU Lesser General Public License as   
 * published by the Free Software Foundation, version 3.
 *
 * AI4E is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 * --------------------------------------------------------------------------------------------------------------------
 */

#region Based on

//
// System.TypeIdentifier.cs
//
// Author:
//   Aleksey Kliger <aleksey@xamarin.com>
//
//
// System.Type.cs
//
// Author:
//   Rodrigo Kumpera <kumpera@gmail.com>
//
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AI4E.Modularity
{
    public sealed class TypeMeta
    {
        private TypeMeta()
        {

        }

        public static TypeMeta FromType(Type type)
        {
            throw new NotImplementedException();
        }

        public static TypeMeta FromStringRepresentation(string type)
        {
            throw new NotImplementedException();
        }

        public bool TryResolve(out Type type)
        {
            throw new NotImplementedException();
        }

        public bool TryResolve(bool ignoreCase, out Type type)
        {
            throw new NotImplementedException();
        }

        public bool TryResolve(IAssemblyResolver assemblyResolver, ITypeResolver typeResolver, out Type type)
        {
            throw new NotImplementedException();
        }

        public bool TryResolve(IAssemblyResolver assemblyResolver, ITypeResolver typeResolver, bool ignoreCase, out Type type)
        {
            throw new NotImplementedException();
        }
    }

    public interface IAssemblyResolver
    {
        Assembly Resolve(AssemblyName assemblyName);
    }

    public interface ITypeResolver
    {
        Type Resolve(Assembly assembly, string name, bool ignoreCase);
    }

    public sealed class TypeSpecification
    {
        #region Fields

        private readonly TypeIdentifier _name;
        private readonly string _assemblyName;
        private readonly List<TypeIdentifier> _nested;
        private readonly List<TypeSpecification> _genericParams;
        private readonly List<Modifier> _modifiers;
        private readonly bool _isByref;

        private readonly Lazy<string> _displayFullname;

        #endregion

        #region C'tor

        private TypeSpecification(TypeIdentifier name,
                         string assemblyName,
                         List<TypeIdentifier> nested,
                         List<TypeSpecification> genericParams,
                         List<Modifier> modifiers,
                         bool isByref)
        {
            _name = name;
            _assemblyName = assemblyName;
            _nested = nested;
            _genericParams = genericParams;
            _modifiers = modifiers;
            _isByref = isByref;

            _displayFullname = new Lazy<string>(() => GetDisplayFullName(DisplayNameFormat.Default));
        }

        #endregion

        #region Properties

        public bool HasModifiers => _modifiers?.Any() ?? false;

        public bool IsNested => _nested?.Any() ?? false;

        public bool IsByRef => _isByref;

        internal TypeName Name => _name;

        public IEnumerable<TypeName> Nested => _nested ?? Enumerable.Empty<TypeName>();

        public IEnumerable<Modifier> Modifiers => _modifiers ?? Enumerable.Empty<Modifier>();

        public string DisplayFullName => _displayFullname.Value;

        public TypeName TypeName => TypeName.FromTypeSpecification(this, true);

        #endregion

        #region ToString

        public override string ToString()
        {
            return GetDisplayFullName(DisplayNameFormat.IncludeAssembly);
        }

        internal string GetDisplayFullName(DisplayNameFormat flags)
        {
            var includeAssembly = (flags & DisplayNameFormat.IncludeAssembly) != 0;
            var includeModifiers = (flags & DisplayNameFormat.NoModifiers) == 0;
            var stringBuilder = new StringBuilder(_name.DisplayName);

            if (_nested != null)
            {
                foreach (var n in _nested)
                    stringBuilder.Append('+').Append(n.DisplayName);
            }

            if (_genericParams != null)
            {
                stringBuilder.Append('[');
                for (var i = 0; i < _genericParams.Count; ++i)
                {
                    if (i > 0)
                        stringBuilder.Append(", ");

                    if (_genericParams[i]._assemblyName != null)
                    {
                        stringBuilder.Append('[').Append(_genericParams[i].DisplayFullName).Append(']');
                    }
                    else
                    {
                        stringBuilder.Append(_genericParams[i].DisplayFullName);
                    }
                }
                stringBuilder.Append(']');
            }

            if (includeModifiers)
                GetModifierString(stringBuilder);

            if (_assemblyName != null && includeAssembly)
                stringBuilder.Append(", ").Append(_assemblyName);

            return stringBuilder.ToString();
        }

        private StringBuilder GetModifierString(StringBuilder sb)
        {
            if (_modifiers != null)
            {
                foreach (var md in _modifiers)
                {
                    md.Append(sb);
                }
            }

            if (_isByref)
                sb.Append('&');

            return sb;
        }

        #endregion

        #region Parse

        public static TypeSpecification Parse(string typeName)
        {
            var pos = 0;
            if (typeName == null)
                throw new ArgumentNullException("typeName");

            var res = Parse(typeName, ref pos, false, true);

            if (pos < typeName.Length)
                throw new ArgumentException("Count not parse the whole type name", "typeName");

            return res;
        }

        // TODO: TryParse

        private static TypeSpecification Parse(string name, ref int p, bool is_recurse, bool allow_aqn)
        {
            // Invariants:
            //  - On exit p, is updated to pos the current unconsumed character.
            //
            //  - The callee peeks at but does not consume delimiters following
            //    recurisve parse (so for a recursive call like the args of "Foo[P,Q]"
            //    we'll return with p either on ',' or on ']'.  If the name was aqn'd
            //    "Foo[[P,assmblystuff],Q]" on return p with be on the ']' just
            //    after the "assmblystuff")
            //
            //  - If allow_aqn is True, assembly qualification is optional.
            //    If allow_aqn is False, assembly qualification is prohibited.
            var pos = p;
            int nameStart;
            var inModifiers = false;

            // results
            var typeName = default(TypeIdentifier);
            var assemblyName = default(string);
            var nested = default(List<TypeIdentifier>);
            var genericParams = default(List<TypeSpecification>);
            var modifierSpec = default(List<Modifier>);
            var isByref = default(bool);

            SkipSpace(name, ref pos);

            nameStart = pos;

            for (; pos < name.Length; ++pos)
            {
                switch (name[pos])
                {
                    case '+':
                        AddName(name.Substring(nameStart, pos - nameStart));
                        nameStart = pos + 1;
                        break;
                    case ',':
                    case ']':
                        AddName(name.Substring(nameStart, pos - nameStart));
                        nameStart = pos + 1;
                        inModifiers = true;
                        if (is_recurse && !allow_aqn)
                        {
                            p = pos;
                            return BuildResult();
                        }
                        break;
                    case '&':
                    case '*':
                    case '[':
                        if (name[pos] != '[' && is_recurse)
                            throw new ArgumentException("Generic argument can't be byref or pointer type", "typeName");
                        AddName(name.Substring(nameStart, pos - nameStart));
                        nameStart = pos + 1;
                        inModifiers = true;
                        break;
                    case '\\':
                        pos++;
                        break;
                }
                if (inModifiers)
                    break;
            }

            if (nameStart < pos)
                AddName(name.Substring(nameStart, pos - nameStart));

            if (inModifiers)
            {
                for (; pos < name.Length; ++pos)
                {
                    switch (name[pos])
                    {
                        case '&':
                            if (isByref)
                                throw new ArgumentException("Can't have a byref of a byref", "typeName");

                            isByref = true;
                            break;
                        case '*':
                            if (isByref)
                                throw new ArgumentException("Can't have a pointer to a byref type", "typeName");
                            // take subsequent '*'s too
                            var pointer_level = 1;
                            while (pos + 1 < name.Length && name[pos + 1] == '*')
                            {
                                ++pos;
                                ++pointer_level;
                            }
                            AddModifier(new PointerModifier(pointer_level));
                            break;
                        case ',':
                            if (is_recurse && allow_aqn)
                            {
                                var end = pos;
                                while (end < name.Length && name[end] != ']')
                                    ++end;
                                if (end >= name.Length)
                                    throw new ArgumentException("Unmatched ']' while parsing generic argument assembly name");
                                assemblyName = name.Substring(pos + 1, end - pos - 1).Trim();
                                p = end;
                                return BuildResult();
                            }
                            if (is_recurse)
                            {
                                p = pos;
                                return BuildResult();
                            }
                            if (allow_aqn)
                            {
                                assemblyName = name.Substring(pos + 1).Trim();
                                pos = name.Length;
                            }
                            break;
                        case '[':
                            if (isByref)
                                throw new ArgumentException("Byref qualifier must be the last one of a type", "typeName");
                            ++pos;
                            if (pos >= name.Length)
                                throw new ArgumentException("Invalid array/generic spec", "typeName");
                            SkipSpace(name, ref pos);

                            if (name[pos] != ',' && name[pos] != '*' && name[pos] != ']')
                            {//generic args
                                var args = new List<TypeSpecification>();
                                if (modifierSpec != null)
                                    throw new ArgumentException("generic args after array spec or pointer type", "typeName");

                                while (pos < name.Length)
                                {
                                    SkipSpace(name, ref pos);
                                    var aqn = name[pos] == '[';
                                    if (aqn)
                                        ++pos; //skip '[' to the start of the type
                                    args.Add(Parse(name, ref pos, true, aqn));
                                    BoundCheck(pos, name);
                                    if (aqn)
                                    {
                                        if (name[pos] == ']')
                                            ++pos;
                                        else
                                            throw new ArgumentException("Unclosed assembly-qualified type name at " + name[pos], "typeName");
                                        BoundCheck(pos, name);
                                    }

                                    if (name[pos] == ']')
                                        break;
                                    if (name[pos] == ',')
                                        ++pos; // skip ',' to the start of the next arg
                                    else
                                        throw new ArgumentException("Invalid generic arguments separator " + name[pos], "typeName");

                                }
                                if (pos >= name.Length || name[pos] != ']')
                                    throw new ArgumentException("Error parsing generic params spec", "typeName");
                                genericParams = args;
                            }
                            else
                            { //array spec
                                var dimensions = 1;
                                var bound = false;
                                while (pos < name.Length && name[pos] != ']')
                                {
                                    if (name[pos] == '*')
                                    {
                                        if (bound)
                                            throw new ArgumentException("Array spec cannot have 2 bound dimensions", "typeName");
                                        bound = true;
                                    }
                                    else if (name[pos] != ',')
                                        throw new ArgumentException("Invalid character in array spec " + name[pos], "typeName");
                                    else
                                        ++dimensions;

                                    ++pos;
                                    SkipSpace(name, ref pos);
                                }
                                if (pos >= name.Length || name[pos] != ']')
                                    throw new ArgumentException("Error parsing array spec", "typeName");
                                if (dimensions > 1 && bound)
                                    throw new ArgumentException("Invalid array spec, multi-dimensional array cannot be bound", "typeName");
                                AddModifier(new ArrayModifier(dimensions, bound));
                            }

                            break;
                        case ']':
                            if (is_recurse)
                            {
                                p = pos;
                                return BuildResult();
                            }
                            throw new ArgumentException("Unmatched ']'", "typeName");
                        default:
                            throw new ArgumentException("Bad type def, can't handle '" + name[pos] + "'" + " at " + pos, "typeName");
                    }
                }
            }

            p = pos;

            void AddName(string type_name)
            {
                if (typeName == null)
                {
                    typeName = ParsedTypeIdentifier(type_name);
                }
                else
                {
                    if (nested == null)
                        nested = new List<TypeIdentifier>();

                    nested.Add(ParsedTypeIdentifier(type_name));
                }
            }

            void AddModifier(Modifier md)
            {
                if (modifierSpec == null)
                    modifierSpec = new List<Modifier>();

                modifierSpec.Add(md);
            }

            TypeSpecification BuildResult()
            {
                return new TypeSpecification(typeName, assemblyName, nested, genericParams, modifierSpec, isByref);
            }

            return BuildResult();
        }

        private static void SkipSpace(string name, ref int pos)
        {
            var p = pos;
            while (p < name.Length && char.IsWhiteSpace(name[p]))
            {
                ++p;
            }

            pos = p;
        }

        #endregion

        public Type Resolve(Func<AssemblyName, Assembly> assemblyResolver, Func<Assembly, string, bool, Type> typeResolver, bool throwOnError, bool ignoreCase)
        {
            Assembly asm = null;
            if (assemblyResolver == null && typeResolver == null)
                return Type.GetType(DisplayFullName, throwOnError, ignoreCase);

            if (_assemblyName != null)
            {
                if (assemblyResolver != null)
                    asm = assemblyResolver(new AssemblyName(_assemblyName));
                else
                    asm = Assembly.Load(_assemblyName);

                if (asm == null)
                {
                    if (throwOnError)
                        throw new FileNotFoundException("Could not resolve assembly '" + _assemblyName + "'");
                    return null;
                }
            }

            Type type = null;
            if (typeResolver != null)
                type = typeResolver(asm, _name.DisplayName, ignoreCase);
            else
                type = asm.GetType(_name.DisplayName, false, ignoreCase);

            if (type == null)
            {
                if (throwOnError)
                    throw new TypeLoadException("Could not resolve type '" + _name + "'");

                return null;
            }

            if (_nested != null)
            {
                foreach (var n in _nested)
                {
                    var tmp = type.GetNestedType(n.DisplayName, BindingFlags.Public | BindingFlags.NonPublic);
                    if (tmp == null)
                    {
                        if (throwOnError)
                            throw new TypeLoadException("Could not resolve type '" + n + "'");

                        return null;
                    }
                    type = tmp;
                }
            }

            if (_genericParams != null)
            {
                var args = new Type[_genericParams.Count];
                for (var i = 0; i < args.Length; ++i)
                {
                    var tmp = _genericParams[i].Resolve(assemblyResolver, typeResolver, throwOnError, ignoreCase);
                    if (tmp == null)
                    {
                        if (throwOnError)
                            throw new TypeLoadException("Could not resolve type '" + _genericParams[i]._name + "'");
                        return null;
                    }
                    args[i] = tmp;
                }
                type = type.MakeGenericType(args);
            }

            if (_modifiers != null)
            {
                foreach (var md in _modifiers)
                    type = md.Resolve(type);
            }

            if (_isByref)
                type = type.MakeByRefType();

            return type;
        }

        private static void BoundCheck(int idx, string s)
        {
            if (idx >= s.Length)
                throw new ArgumentException("Invalid generic arguments spec", "typeName");
        }

        private static TypeIdentifier ParsedTypeIdentifier(string displayName)
        {
            return TypeIdentifier.FromDisplay(displayName);
        }
    }

    [Flags]
    internal enum DisplayNameFormat
    {
        Default = 0x0,
        IncludeAssembly = 0x1,
        NoModifiers = 0x2,
    }

    #region Modifier

    public abstract class Modifier
    {
        internal Modifier() { }

        public abstract Type Resolve(Type type);
        public abstract StringBuilder Append(StringBuilder sb);
    }

    public sealed class ArrayModifier : Modifier
    {
        // dimensions == 1 and bound, or dimensions > 1 and !bound
        private readonly int _dimensions;
        private readonly bool _bound;

        internal ArrayModifier(int dimensions, bool bound)
        {
            if (dimensions <= 0 || dimensions > 32)
                throw new ArgumentOutOfRangeException(nameof(dimensions));

            if (dimensions > 1 && bound)
                throw new ArgumentException();

            _dimensions = dimensions;
            _bound = bound;
        }

        public override Type Resolve(Type type)
        {
            if (_bound)
                return type.MakeArrayType(1);

            if (_dimensions == 1)
                return type.MakeArrayType();

            return type.MakeArrayType(_dimensions);
        }

        public override StringBuilder Append(StringBuilder sb)
        {
            if (_bound)
                return sb.Append("[*]");

            return sb.Append('[')
                     .Append(',', _dimensions - 1)
                     .Append(']');

        }

        public override string ToString()
        {
            return Append(new StringBuilder()).ToString();
        }

        public int Rank => _dimensions;

        public bool IsBound => _bound;
    }

    public sealed class PointerModifier : Modifier
    {
        private readonly int _pointerLevel;

        internal PointerModifier(int pointerLevel)
        {
            _pointerLevel = pointerLevel;
        }

        public override Type Resolve(Type type)
        {
            for (var i = 0; i < _pointerLevel; ++i)
                type = type.MakePointerType();

            return type;
        }

        public override StringBuilder Append(StringBuilder sb)
        {
            return sb.Append('*', _pointerLevel);
        }

        public override string ToString()
        {
            return Append(new StringBuilder()).ToString();
        }

        public int PointerLevel => _pointerLevel;
    }

    #endregion

    // A TypeName is awrapper around type names in display form
    // (that is, with special characters escaped).
    //
    // Note that in general if you unescape a type name, you will
    // lose information: If the type name's DisplayName is
    // Foo\+Bar+Baz (outer class ``Foo+Bar``, inner class Baz)
    // unescaping the first plus will give you (outer class Foo,
    // inner class Bar, innermost class Baz).
    //
    // The correct way to take a TypeName apart is to feed its
    // DisplayName to TypeSpec.Parse()
    public class TypeName : IEquatable<TypeName>
    {
        internal TypeName(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; }

        public TypeName GetNested(TypeIdentifier innerName)
        {
            return new TypeName(DisplayName + "+" + innerName.DisplayName);
        }

        public bool Equals(TypeName other)
        {
            throw new NotImplementedException();
        }

        public static TypeName FromDisplayName(string displayName)
        {
            return new TypeName(displayName);
        }

        public static TypeName FromTypeSpecification(TypeSpecification typeSpecifications, bool includeModifiers)
        {
            if (includeModifiers)
            {
                return new TypeName(typeSpecifications.DisplayFullName);
            }

            return new TypeName(typeSpecifications.GetDisplayFullName(DisplayNameFormat.NoModifiers));
        }
    }

    // A type identifier is a single component of a type name.
    // Unlike a general typename, a type identifier can be
    // converted to internal form without loss of information.
    public class TypeIdentifier : TypeName, IEquatable<TypeIdentifier>
    {
        private TypeIdentifier(string displayName, string internalName) : base(displayName)
        {
            InternalName = internalName;
        }

        public string InternalName { get; }

        public bool Equals(TypeIdentifier other)
        {
            throw new NotImplementedException();
        }

        public static TypeIdentifier FromDisplay(string displayName)
        {
            return new TypeIdentifier(displayName, UnescapeInternalName(displayName));
        }

        public static TypeIdentifier FromInternal(string internalNamespace, TypeIdentifier typeName)
        {
            var internalName = internalNamespace + "." + typeName.InternalName;

            return new TypeIdentifier(EscapeDisplayName(internalName), internalName);
        }

        public static TypeIdentifier FromInternal(string internalName)
        {
            return new TypeIdentifier(EscapeDisplayName(internalName), internalName);
        }

        // Only use if simpleName is certain not to contain
        // unexpected characters that ordinarily require
        // escaping: ,+*&[]\
        public static TypeIdentifier WithoutEscape(string name)
        {
#if DEBUG
            CheckNoBadChars(name);
#endif

            return new TypeIdentifier(name, name);
        }

#if DEBUG
        static private void CheckNoBadChars(string name)
        {
            if (NeedsEscaping(name))
                throw new ArgumentException(nameof(name));
        }
#endif

        private static string EscapeDisplayName(string internalName)
        {
            // initial capacity = length of internalName.
            // Maybe we won't have to escape anything.
            var res = new StringBuilder(internalName.Length);
            foreach (var c in internalName)
            {
                switch (c)
                {
                    case '+':
                    case ',':
                    case '[':
                    case ']':
                    case '*':
                    case '&':
                    case '\\':
                        res.Append('\\').Append(c);
                        break;

                    default:
                        res.Append(c);
                        break;
                }
            }
            return res.ToString();
        }

        private static string UnescapeInternalName(string displayName)
        {
            var res = new StringBuilder(displayName.Length);
            for (var i = 0; i < displayName.Length; ++i)
            {
                var c = displayName[i];
                if (c == '\\' && ++i < displayName.Length)
                {
                    c = displayName[i];
                }

                res.Append(c);
            }
            return res.ToString();
        }

        private static bool NeedsEscaping(string internalName)
        {
            foreach (var c in internalName)
            {
                switch (c)
                {
                    case ',':
                    case '+':
                    case '*':
                    case '&':
                    case '[':
                    case ']':
                    case '\\':
                        return true;
                }
            }
            return false;
        }
    }
}
