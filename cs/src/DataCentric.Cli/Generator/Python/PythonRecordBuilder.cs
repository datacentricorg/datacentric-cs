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
    public static class PythonRecordBuilder
    {
        public static string Build(TypeDecl decl, Dictionary<string, string> declPathDict)
        {
            var writer = new CodeWriter();

            // Determine if we are inside datacentric package
            // based on module name. This affects the imports
            // and namespace use.
            string name = decl.Name;
            bool insideDc = decl.Module.ModuleName == "DataCentric";

            // If not generating for DataCentric package, use dc. namespace
            // in front of datacentric types, otherwise use no prefix
            string dcNamespacePrefix = insideDc ? "" : "dc.";

            // Full package name and short namespace of the parent class,
            // or null if there is no parent
            bool parentClassInDifferentModule =
                decl.Inherit != null && decl.Inherit.Module.ModuleName != decl.Module.ModuleName;
            string parentClassNamespace = parentClassInDifferentModule ?
                PythonImportsBuilder.GetPythonNamespace(decl.Inherit.Module.ModuleName) : null;
            string parentClassNamespacePrefix = parentClassInDifferentModule ?
                parentClassNamespace + "." : "";

            PythonImportsBuilder.WriteImports(decl, declPathDict, writer);

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
                writer.AppendLine($"__slots__ = ({keySlots})");
                writer.AppendNewLineWithoutIndent();

                foreach (var element in keyElements) writer.AppendLine($"{element.Name.Underscore()}: {GetTypeHint(element)}");
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

            string abstractBase = "";
            if (decl.Kind == TypeKind.Abstract)
                abstractBase = ", ABC";

            if (decl.Keys.Any())
                writer.AppendLine($"class {name}({dcNamespacePrefix}TypedRecord[{name}Key]{abstractBase}):");
            else if (decl.Inherit != null)
                writer.AppendLine($"class {name}({parentClassNamespacePrefix}{decl.Inherit.Name}{abstractBase}):");
            else
                writer.AppendLine($"class {name}({dcNamespacePrefix}Data{abstractBase}):");

            // Same comment for the key and for the record
            writer.PushIndent();
            writer.AppendLines(CommentHelper.PyComment(decl.Comment));
            writer.AppendNewLineWithoutIndent();

            var slots = string.Join(", ", decl.Elements.Select(t => $"'{t.Name.Underscore()}'"));
            writer.AppendLine($"__slots__ = ({slots})");
            writer.AppendNewLineWithoutIndent();

            foreach (var element in decl.Elements) writer.AppendLine($"{element.Name.Underscore()}: {GetTypeHint(element)}");
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
                return decl.Implement?.Handlers.FirstOrDefault(t=>t.Name == declare.Name) == null;
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
                var result = GetValue(parameter.Value);
                if (parameter.Vector == YesNo.Y) result = $"List[{result}]";
                return result;
            }
            else if (parameter.Data != null)
            {
                string paramNamespace = parameter.Data.Module.ModuleName != decl.Module.ModuleName
                    ? PythonImportsBuilder.GetPythonNamespace(parameter.Data.Module.ModuleName) + "."
                    : "";
                var result = $"{paramNamespace}{parameter.Data.Name}";
                if (parameter.Vector == YesNo.Y) result = $"List[{result}]";
                return result;
            }
            else if (parameter.Key != null)
            {
                string paramNamespace = parameter.Key.Module.ModuleName != decl.Module.ModuleName
                    ? PythonImportsBuilder.GetPythonNamespace(parameter.Key.Module.ModuleName) + "."
                    : "";
                var result = $"{paramNamespace}{parameter.Key.Name}Key";
                if (parameter.Vector == YesNo.Y) result = $"List[{result}]";
                return result;
            }
            else if (parameter.Enum != null)
            {
                string paramNamespace = parameter.Enum.Module.ModuleName != decl.Module.ModuleName
                    ? PythonImportsBuilder.GetPythonNamespace(parameter.Enum.Module.ModuleName) + "."
                    : "";
                var result = $"{paramNamespace}{parameter.Enum.Name}";
                if (parameter.Vector == YesNo.Y) result = $"List[{result}]";
                return result;
            }
            else throw new ArgumentException("Can't deduct type");
        }

        private static string GetTypeHint(TypeElementDecl element)
        {
            string type = element.Value != null ? GetValue(element.Value) :
                          element.Data != null  ? $"{element.Data.Name}" :
                          element.Key != null   ? $"{element.Key.Name}Key" :
                          element.Enum != null  ? element.Enum.Name :
                                                  throw new ArgumentException("Can't deduct type");

            return element.Vector == YesNo.Y ? $"List[{type}]" : type;
        }

        private static string GetValue(ValueDecl valueDecl)
        {
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
                atomicType == AtomicType.DateTime ? "LocalDateTime" :
                atomicType == AtomicType.Date ? "LocalDate" :
                atomicType == AtomicType.Time ? "LocalTime" :
                atomicType == AtomicType.Minute ? "LocalMinute" :
                atomicType == AtomicType.NullableDateTime ? "Optional[LocalDateTime]" :
                atomicType == AtomicType.NullableDate ? "Optional[LocalDate]" :
                atomicType == AtomicType.NullableTime ? "Optional[LocalTime]" :
                atomicType == AtomicType.NullableMinute ? "Optional[LocalMinute]" :
                atomicType == AtomicType.TemporalId ? "ObjectId" :
                atomicType == AtomicType.NullableTemporalId ? "Optional[ObjectId]" :
                throw new
                    ArgumentException($"Unknown value type: {atomicType.ToString()}");
        }

    }
}