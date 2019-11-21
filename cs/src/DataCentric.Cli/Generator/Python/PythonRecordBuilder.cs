/*
Copyright (C) 2013-present The DataCentric Authors.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;

namespace DataCentric.Cli
{
    /// <summary>
    /// Builder for generated python classes.
    /// </summary>
    public static class PythonRecordBuilder
    {
        /// <summary>
        /// Generate python classes from declaration.
        /// </summary>
        public static string Build(TypeDecl decl, List<IDecl> declarations)
        {
            var writer = new CodeWriter();

            string name = decl.Name;

            // Determine if we are inside datacentric package
            // based on module name. This affects the imports
            // and namespace use.
            bool insideDc = PyExtensions.GetPackage(decl) == "datacentric";

            // If not generating for DataCentric package, use dc. namespace
            // in front of datacentric types, otherwise use no prefix
            string dcNamespacePrefix = insideDc ? "" : "dc.";


            PythonImportsBuilder.WriteImports(decl, declarations, writer);

            writer.AppendNewLineWithoutIndent();
            writer.AppendNewLineWithoutIndent();

            if (decl.Keys.Any())
            {
                var keyElements = decl.Elements.Where(e => decl.Keys.Contains(e.Name)).ToList();

                writer.AppendLine("@attr.s(slots=True, auto_attribs=True)");
                writer.AppendLine($"class {name}Key({dcNamespacePrefix}TypedKey['{name}']):");

                // Same comment for the key and for the record
                writer.PushIndent();
                writer.AppendLines(CommentHelper.PyComment(decl.Comment));

                writer.AppendNewLineWithoutIndent();
                if (!keyElements.Any()) writer.AppendLine("pass");

                foreach (var element in keyElements)
                {
                    writer.AppendLine($"{element.Name.Underscore()}: {GetTypeHint(decl, element)} = attr.ib(default=None, kw_only=True{GetMetaData(element)})");
                    writer.AppendLines(CommentHelper.PyComment(element.Comment));
                    // Do not add new line after last item
                    if (element != keyElements.Last())
                        writer.AppendNewLineWithoutIndent();
                }

                writer.PopIndent();

                writer.AppendNewLineWithoutIndent();
                writer.AppendNewLineWithoutIndent();
            }

            writer.AppendLine("@attr.s(slots=True, auto_attribs=True)");
            string abstractBase = decl.Kind == TypeKind.Abstract ? ", ABC" : "";
            if (decl.Keys.Any())
                writer.AppendLine($"class {name}({dcNamespacePrefix}TypedRecord[{name}Key]{abstractBase}):");
            else if (decl.Inherit != null)
            {
                // Full package name and short namespace of the parent class,
                // or null if there is no parent
                bool parentClassInDifferentModule = !PyExtensions.IsPackageEquals(decl, decl.Inherit);
                string parentPackage = PyExtensions.GetPackage(decl.Inherit);
                string parentClassNamespacePrefix =
                    parentClassInDifferentModule ? PyExtensions.GetAlias(parentPackage) + "." : "";
                writer.AppendLine($"class {name}({parentClassNamespacePrefix}{decl.Inherit.Name}{abstractBase}):");
            }
            else
                writer.AppendLine($"class {name}({dcNamespacePrefix}Data{abstractBase}):");

            // Same comment for the key and for the record
            writer.PushIndent();
            writer.AppendLines(CommentHelper.PyComment(decl.Comment));

            writer.AppendNewLineWithoutIndent();
            if (!decl.Elements.Any()) writer.AppendLine("pass");

            foreach (var element in decl.Elements)
            {
                writer.AppendLine($"{element.Name.Underscore()}: {GetTypeHint(decl, element)} = attr.ib(default=None, kw_only=True{GetMetaData(element)})");
                writer.AppendLines(CommentHelper.PyComment(element.Comment));
                if (element != decl.Elements.Last())
                    writer.AppendNewLineWithoutIndent();
            }

            if (decl.Declare != null)
            {
                writer.AppendNewLineWithoutIndent();
                WriteMethods(decl, writer);
            }

            // Class end
            writer.PopIndent();

            return writer.ToString();
        }

        private static string GetMetaData(TypeElementDecl element)
        {
            var meta = new List<string>();
            if (element.Optional == YesNo.Y)
                meta.Add("'optional': True");
            if (element.Value != null && (element.Value.Type == AtomicType.Long ||
                                          element.Value.Type == AtomicType.NullableLong))
                meta.Add("'type': 'long'");

            return meta.Any()
                ? $", metadata={{{string.Join(", ", meta)}}}"
                : "";
        }

        private static void WriteMethods(TypeDecl decl, CodeWriter writer)
        {
            bool HasImplement(HandlerDeclareDecl declare)
            {
                return decl.Implement?.Handlers.FirstOrDefault(t => t.Name == declare.Name) == null;
            }

            var declarations = decl.Declare.Handlers;
            foreach (var declare in declarations)
            {
                bool isAbstract = !HasImplement(declare);
                if (isAbstract)
                    writer.AppendLine("@abstractmethod");

                var parameters = "";
                foreach (var parameter in declare.Params)
                    parameters += ($", {parameter.Name.Underscore()}: {GetTypeHint(decl, parameter)}");

                writer.AppendLine($"def {declare.Name.Underscore()}(self{parameters}):");
                writer.PushIndent();

                writer.AppendLines(CommentHelper.PyComment(declare.Comment));

                writer.AppendLine(isAbstract ? "pass" : "raise NotImplemented");

                if (declarations.IndexOf(declare) != declarations.Count - 1)
                    writer.AppendNewLineWithoutIndent();

                writer.PopIndent();
            }
        }

        private static string GetTypeHint(TypeDecl decl, HandlerParamDecl parameter)
        {
            if (parameter.Value != null)
            {
                bool insideDc = PyExtensions.GetPackage(decl) == "datacentric";
                string result = GetValue(insideDc, parameter.Value);
                return parameter.Vector == YesNo.Y ? $"List[{result}]" : result;
            }
            else if (parameter.Data != null)
            {
                string paramNamespace = !PyExtensions.IsPackageEquals(decl, parameter.Data)
                    ? PyExtensions.GetAlias(parameter.Data) + "."
                    : "";
                string result = $"{paramNamespace}{parameter.Data.Name}";
                return parameter.Vector == YesNo.Y ? $"List[{result}]" : result;
            }
            else if (parameter.Key != null)
            {
                var paramNamespace = !PyExtensions.IsPackageEquals(decl, parameter.Key)
                    ? PyExtensions.GetAlias(parameter.Key) + "."
                    : "";
                var result = $"{paramNamespace}{parameter.Key.Name}Key";
                return parameter.Vector == YesNo.Y ? $"List[{result}]" : result;
            }
            else if (parameter.Enum != null)
            {
                string paramNamespace = !PyExtensions.IsPackageEquals(decl, parameter.Enum)
                    ? PyExtensions.GetAlias(parameter.Enum) + "."
                    : "";
                string result = $"{paramNamespace}{parameter.Enum.Name}";
                return parameter.Vector == YesNo.Y ? $"List[{result}]" : result;
            }
            else throw new ArgumentException("Can't deduct type");
        }

        private static string GetTypeHint(TypeDecl decl, TypeElementDecl element)
        {
            string GetParamNamespace(TypeDecl declaration, TypeDeclKey key) =>
                !PyExtensions.IsPackageEquals(declaration, key) ? PyExtensions.GetAlias(key) + "." : "";

            string GetFinalHint(string typeHint) =>
                element.Vector == YesNo.Y ? $"List[{typeHint}]" : typeHint;

            if (element.Value != null)
            {
                bool insideDc = PyExtensions.GetPackage(decl) == "datacentric";
                string hint = GetValue(insideDc, element.Value);
                return GetFinalHint(hint);
            }
            else if (element.Data != null)
            {
                string paramNamespace = GetParamNamespace(decl, element.Data);
                string hint = $"{paramNamespace}{element.Data.Name}";
                return GetFinalHint(hint);
            }
            else if (element.Key != null)
            {
                string paramNamespace = GetParamNamespace(decl, element.Key);
                string hint = $"{paramNamespace}{element.Key.Name}Key";
                return GetFinalHint(hint);
            }
            else if (element.Enum != null)
            {
                string paramNamespace = GetParamNamespace(decl, element.Enum);
                string hint = $"{paramNamespace}{element.Enum.Name}";
                return GetFinalHint(hint);
            }
            else throw new ArgumentException("Can't deduct type");
        }

        private static string GetValue(bool insideDc, ValueDecl valueDecl)
        {
            string prefix = insideDc ? "" : "dc.";
            var atomicType = valueDecl.Type;
            return
                atomicType == AtomicType.String ? "str" :
                atomicType == AtomicType.Bool ? "bool" :
                atomicType == AtomicType.Double ? "float" :
                atomicType == AtomicType.Int ? "int" :
                atomicType == AtomicType.Long ? "int" :
                atomicType == AtomicType.NullableBool ? "bool" :
                atomicType == AtomicType.NullableDouble ? "float" :
                atomicType == AtomicType.NullableInt ? "int" :
                atomicType == AtomicType.NullableLong ? "int" :
                atomicType == AtomicType.DateTime ? $"{prefix}LocalDateTime" :
                atomicType == AtomicType.Date ? $"{prefix}LocalDate" :
                atomicType == AtomicType.Time ? $"{prefix}LocalTime" :
                atomicType == AtomicType.Minute ? $"{prefix}LocalMinute" :
                atomicType == AtomicType.Instant ? $"{prefix}Instant" :
                atomicType == AtomicType.NullableDateTime ? $"{prefix}LocalDateTime" :
                atomicType == AtomicType.NullableDate ? $"{prefix}LocalDate" :
                atomicType == AtomicType.NullableTime ? $"{prefix}LocalTime" :
                atomicType == AtomicType.NullableMinute ? $"{prefix}LocalMinute" :
                atomicType == AtomicType.NullableInstant ? $"{prefix}Instant" :
                atomicType == AtomicType.TemporalId ? "ObjectId" :
                atomicType == AtomicType.NullableTemporalId ? "ObjectId" :
                throw new
                    ArgumentException($"Unknown value type: {atomicType.ToString()}");
        }
    }
}