﻿/*
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

                writer.AppendLine($"class {name}Key({dcNamespacePrefix}TypedKey['{name}']):");

                // Same comment for the key and for the record
                writer.PushIndent();
                writer.AppendLines(CommentHelper.PyComment(decl.Comment));
                writer.AppendNewLineWithoutIndent();

                var keySlots = string.Join(", ", decl.Keys.Select(t => $"'{t.Underscore()}'"));
                if (decl.Keys.Count == 1)
                    keySlots = keySlots + ",";

                writer.AppendLine($"__slots__ = ({keySlots})");
                writer.AppendNewLineWithoutIndent();

                foreach (var element in keyElements)
                    writer.AppendLine($"{element.Name.Underscore()}: {GetTypeHint(decl, element)}");
                writer.AppendNewLineWithoutIndent();

                writer.AppendLine("def __init__(self):");
                writer.PushIndent();

                writer.AppendLine("super().__init__()");
                writer.AppendNewLineWithoutIndent();
                foreach (var element in keyElements)
                {
                    writer.AppendLine($"self.{element.Name.Underscore()} = None");
                    writer.AppendLines(CommentHelper.PyComment(element.Comment));
                    if (keyElements.IndexOf(element) != keyElements.Count - 1)
                        writer.AppendNewLineWithoutIndent();
                }

                writer.PopIndent();
                writer.PopIndent();

                writer.AppendNewLineWithoutIndent();
                writer.AppendNewLineWithoutIndent();
            }

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

            var slots = string.Join(", ", decl.Elements.Select(t => $"'{t.Name.Underscore()}'"));
            if (decl.Elements.Count == 1)
                slots = slots + ",";
            writer.AppendLine($"__slots__ = ({slots})");
            writer.AppendNewLineWithoutIndent();

            foreach (var element in decl.Elements)
                writer.AppendLine($"{element.Name.Underscore()}: {GetTypeHint(decl, element)}");
            writer.AppendNewLineWithoutIndent();

            // Init start
            writer.AppendLine("def __init__(self):");
            writer.PushIndent();

            writer.AppendLine("super().__init__()");
            writer.AppendNewLineWithoutIndent();

            foreach (var element in decl.Elements)
            {
                writer.AppendLine($"self.{element.Name.Underscore()} = None");
                writer.AppendLines(CommentHelper.PyComment(element.Comment));
                if (decl.Elements.IndexOf(element) != decl.Elements.Count - 1)
                    writer.AppendNewLineWithoutIndent();
            }

            // Init end
            writer.PopIndent();

            if (decl.Declare != null)
            {
                writer.AppendNewLineWithoutIndent();
                WriteMethods(decl, writer);
            }

            // Class end
            writer.PopIndent();

            return writer.ToString();
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
                string result = $"Optional[{paramNamespace}{parameter.Data.Name}]";
                return parameter.Vector == YesNo.Y ? $"List[{result}]" : result;
            }
            else if (parameter.Key != null)
            {
                var paramNamespace = !PyExtensions.IsPackageEquals(decl, parameter.Key)
                    ? PyExtensions.GetAlias(parameter.Key) + "."
                    : "";
                var result = $"Optional[{paramNamespace}{parameter.Key.Name}Key]";
                return parameter.Vector == YesNo.Y ? $"List[{result}]" : result;
            }
            else if (parameter.Enum != null)
            {
                string paramNamespace = !PyExtensions.IsPackageEquals(decl, parameter.Enum)
                    ? PyExtensions.GetAlias(parameter.Enum) + "."
                    : "";
                string result = $"Optional[{paramNamespace}{parameter.Enum.Name}]";
                return parameter.Vector == YesNo.Y ? $"List[{result}]" : result;
            }
            else throw new ArgumentException("Can't deduct type");
        }

        private static string GetTypeHint(TypeDecl decl, TypeElementDecl element)
        {
            if (element.Value != null)
            {
                bool insideDc = PyExtensions.GetPackage(decl) == "datacentric";
                string result = GetValue(insideDc, element.Value);
                return element.Vector == YesNo.Y ? $"List[{result}]" : result;
            }
            else if (element.Data != null)
            {
                string paramNamespace = !PyExtensions.IsPackageEquals(decl, element.Data)
                    ? PyExtensions.GetAlias(element.Data) + "."
                    : "";
                string result = $"Optional[{paramNamespace}{element.Data.Name}]";
                return element.Vector == YesNo.Y ? $"List[{result}]" : result;
            }
            else if (element.Key != null)
            {
                var paramNamespace = !PyExtensions.IsPackageEquals(decl, element.Key)
                    ? PyExtensions.GetAlias(element.Key) + "."
                    : "";
                var result = $"Optional[{paramNamespace}{element.Key.Name}Key]";
                return element.Vector == YesNo.Y ? $"List[{result}]" : result;
            }
            else if (element.Enum != null)
            {
                string paramNamespace = !PyExtensions.IsPackageEquals(decl, element.Enum)
                    ? PyExtensions.GetAlias(element.Enum) + "."
                    : "";
                string result = $"Optional[{paramNamespace}{element.Enum.Name}]";
                return element.Vector == YesNo.Y ? $"List[{result}]" : result;
            }
            else throw new ArgumentException("Can't deduct type");
        }

        private static string GetValue(bool insideDc, ValueDecl valueDecl)
        {
            string prefix = insideDc ? "" : "dc.";
            var atomicType = valueDecl.Type;
            return
                atomicType == AtomicType.String ? "Optional[str]" :
                atomicType == AtomicType.Bool ? "bool" :
                atomicType == AtomicType.Double ? "float" :
                atomicType == AtomicType.Int ? "int" :
                atomicType == AtomicType.Long ? "int" :
                atomicType == AtomicType.NullableBool ? "Optional[bool]" :
                atomicType == AtomicType.NullableDouble ? "Optional[float]" :
                atomicType == AtomicType.NullableInt ? "Optional[int]" :
                atomicType == AtomicType.NullableLong ? "Optional[int]" :
                atomicType == AtomicType.DateTime ? $"LocalDateTime" :
                atomicType == AtomicType.Date ? $"{prefix}LocalDate" :
                atomicType == AtomicType.Time ? $"{prefix}LocalTime" :
                atomicType == AtomicType.Minute ? $"{prefix}LocalMinute" :
                atomicType == AtomicType.Instant ? $"{prefix}Instant" :
                atomicType == AtomicType.NullableDateTime ? $"Optional[{prefix}LocalDateTime]" :
                atomicType == AtomicType.NullableDate ? $"Optional[{prefix}LocalDate]" :
                atomicType == AtomicType.NullableTime ? $"Optional[{prefix}LocalTime]" :
                atomicType == AtomicType.NullableMinute ? $"Optional[{prefix}LocalMinute]" :
                atomicType == AtomicType.NullableInstant ? $"Optional[{prefix}Instant]" :
                atomicType == AtomicType.TemporalId ? "ObjectId" :
                atomicType == AtomicType.NullableTemporalId ? "Optional[ObjectId]" :
                throw new
                    ArgumentException($"Unknown value type: {atomicType.ToString()}");
        }
    }
}