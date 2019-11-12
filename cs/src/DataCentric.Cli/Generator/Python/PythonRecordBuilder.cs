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
using System.Linq;
using Humanizer;

namespace DataCentric.Cli
{
    public static class PythonRecordBuilder
    {
        public static string Build(TypeDecl decl)
        {
            var writer = new CodeWriter();

            // Determine if we are inside datacentric package
            // based on module name. This affects the imports
            // and namespace use.
            string name = decl.Name;
            bool insideDc = decl.Module.ModuleName == "DataCentric";

            // If not generating for DataCentric package, use dc. namespace
            // in front of datacentric types, otherwise use no prefix
            string dcNamespace = insideDc ? "dc." : "";

            // Full package name and short namespace of the parent class,
            // or null if there is no parent
            string parentClassPackage = decl.Inherit != null ?
                GetPythonPackage(decl.Inherit.Module.ModuleName) : null;
            string parentClassNamespace = decl.Inherit != null ?
                GetPythonNamespace(decl.Inherit.Module.ModuleName) : null;

            // Import datacentric package as dc, or if inside datacentric,
            // import individual classes instead
            if (decl.IsRecord)
            {
                if (insideDc)
                {
                    writer.AppendLine("from datacentric.storage.typed_key import TypedKey");
                    writer.AppendLine("from datacentric.storage.typed_record import TypedRecord");
                }
                else
                {
                    writer.AppendLine("import datacentric as dc");
                }
            }
            else
            {
                if (insideDc)
                {
                    writer.AppendLine("from datacentric.storage.data import Data");
                }
                else
                {
                    writer.AppendLine("import datacentric as dc");
                }
            }

            // Import parent class package as its namespace, or if inside datacentric,
            // import individual class instead
            if (decl.Inherit != null)
            {
                if (insideDc)
                {
                    // Import parent package namespace unless it is the same as
                    // the namespace for the class itself
                    if (decl.Module.ModuleName == decl.Inherit.Module.ModuleName)
                    {
                        // Parent class name and filename based on converting
                        // class name to snake case
                        string parentClassName = decl.Inherit.Name;
                        string parentPythonFileName = parentClassName.Underscore();

                        // Import individual parent class if package namespace is
                        // the same as parent class namespace. Use ? as the folder
                        // is unknown, this will be corrected after the generation
                        writer.AppendLine($"from datacentric.?.{parentPythonFileName} import {parentClassName}");
                    }
                    else
                        throw new Exception("When generating code for the datacentric package, " +
                                            "parent packages should not be managed via a declaration.");
                }
                else
                {
                    // Import parent package namespace unless it is the same as
                    // the namespace for the class itself
                    if (decl.Module.ModuleName == decl.Inherit.Module.ModuleName)
                    {
                        // Parent class name and filename based on converting
                        // class name to snake case
                        string parentClassName = decl.Inherit.Name;
                        string parentPythonFileName = parentClassName.Underscore();

                        // Import individual parent class if package namespace is
                        // the same as parent class namespace. Use ? as the folder
                        // is unknown, this will be corrected after the generation
                        writer.AppendLine($"from ?.{parentPythonFileName} import {parentClassName}");
                    }
                    else
                    {
                        // Otherwise import the entire package of the parent class
                        writer.AppendLine($"import {parentClassPackage} as {parentClassNamespace}");
                    }
                }
            }

            writer.AppendNewLineWithoutIndent();
            writer.AppendNewLineWithoutIndent();

            if (decl.Keys.Any())
            {
                var keyElements = decl.Elements.Where(e => decl.Keys.Contains(e.Name)).ToList();

                writer.AppendLine($"class {name}Key({dcNamespace}TypedKey['{name}']):");

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
                writer.AppendLine($"class {name}({dcNamespace}TypedRecord[{name}Key]{abstractBase}):");
            else if (decl.Inherit != null)
                writer.AppendLine($"class {name}({decl.Inherit.Name}{abstractBase}):");
            else
                writer.AppendLine($"class {name}(Data{abstractBase}):");

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
                    parameters += ($", {parameter.Name.Underscore()}: {GetTypeHint(parameter)}");

                writer.AppendLine($"def {declare.Name.Underscore()}(self{parameters}):");
                writer.PushIndent();

                writer.AppendLines(CommentHelper.PyComment(declare.Comment));

                writer.AppendLine(isAbstract ? "pass" : "raise NotImplemented");

                if (declarations.IndexOf(declare) != declarations.Count - 1)
                    writer.AppendNewLineWithoutIndent();

                writer.PopIndent();
            }
        }

        private static string GetTypeHint(HandlerParamDecl parameter)
        {
            string type = parameter.Value != null ? GetValue(parameter.Value) :
                          parameter.Data != null  ? $"{parameter.Data.Name}" :
                          parameter.Key != null   ? $"{parameter.Key.Name}Key" :
                          parameter.Enum != null  ? parameter.Enum.Name :
                                                  throw new ArgumentException("Can't deduct type");

            return parameter.Vector == YesNo.Y ? $"List[{type}]" : type;
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
                atomicType == AtomicType.DateTime ? "dt.datetime" :
                atomicType == AtomicType.Double ? "float" :
                atomicType == AtomicType.Int ? "int" :
                atomicType == AtomicType.Long ? "int" :
                atomicType == AtomicType.NullableBool ? "Optional[bool]" :
                atomicType == AtomicType.NullableDateTime ? "Optional[dt.datetime]" :
                atomicType == AtomicType.NullableDouble ? "Optional[float]" :
                atomicType == AtomicType.NullableInt ? "Optional[int]" :
                atomicType == AtomicType.NullableLong ? "Optional[int]" :
                atomicType == AtomicType.DateTime ? "dt.datetime" :
                atomicType == AtomicType.Date ? "dt.date" :
                atomicType == AtomicType.Time ? "dt.time" :
                atomicType == AtomicType.Minute ? "LocalMinute" :
                atomicType == AtomicType.NullableDateTime ? "Optional[dt.datetime]" :
                atomicType == AtomicType.NullableDate ? "Optional[dt.date]" :
                atomicType == AtomicType.NullableTime ? "Optional[dt.time]" :
                atomicType == AtomicType.NullableMinute ? "Optional[LocalMinute]" :
                atomicType == AtomicType.TemporalId ? "ObjectId" :
                atomicType == AtomicType.NullableTemporalId ? "Optional[ObjectId]" :
                throw new
                    ArgumentException($"Unknown value type: {atomicType.ToString()}");
        }

        private static string GetPythonPackage(string moduleName)
        {
            switch (moduleName)
            {
                case "DataCentric": return "datacentric";
                default: return "unknown_module"; // TODO - resolve all and raise an error if not found
            }
        }

        private static string GetPythonNamespace(string moduleName)
        {
            switch (moduleName)
            {
                case "DataCentric": return "dc";
                default: return "unknown_module"; // TODO - resolve all and raise an error if not found
            }
        }
    }
}