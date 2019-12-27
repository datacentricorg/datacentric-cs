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

namespace DataCentric
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

            // Get base classes for current declaration
            List<string> bases = new List<string>();

            if (decl.Keys.Any())
                bases.Add(dcNamespacePrefix+"Record");
            else if (decl.Inherit != null)
            {
                // Full package name and short namespace of the parent class,
                // or null if there is no parent
                bool parentClassInDifferentModule = !PyExtensions.IsPackageEquals(decl, decl.Inherit);
                string parentPackage = PyExtensions.GetPackage(decl.Inherit);
                string parentClassNamespacePrefix =
                    parentClassInDifferentModule ? PyExtensions.GetAlias(parentPackage) + "." : "";
                bases.Add(parentClassNamespacePrefix + decl.Inherit.Name);
            }
            else
                bases.Add(dcNamespacePrefix+"Data");

            if (decl.Kind == TypeKind.Abstract)
                bases.Add("ABC");

            // Python 3.8:
            // if (decl.Kind == TypeKind.Final)
            // writer.AppendLine("@final");
            writer.AppendLine("@attr.s(slots=True, auto_attribs=True)");
            writer.AppendLine($"class {name}({string.Join(", ", bases)}):");
            writer.PushIndent();
            writer.AppendLines(CommentHelper.PyComment(decl.Comment));

            writer.AppendNewLineWithoutIndent();
            if (!decl.Elements.Any()) writer.AppendLine("pass");

            foreach (var element in decl.Elements)
            {
                // TODO: Should be replaced with callable with specific format instead of skipping
                string skipRepresentation = element.Vector == YesNo.Y ? ", repr=False" : "";

                writer.AppendLine($"{element.Name.Underscore()}: {GetTypeHint(decl, element)} = attr.ib(default=None, kw_only=True{skipRepresentation}{GetMetaData(element)})");
                writer.AppendLines(CommentHelper.PyComment(element.Comment));
                if (element != decl.Elements.Last())
                    writer.AppendNewLineWithoutIndent();
            }

            // Add to_key and create_key() methods
            if (decl.Keys.Any())
            {
                var keyElements = decl.Elements.Where(e => decl.Keys.Contains(e.Name)).ToList();

                writer.AppendNewLineWithoutIndent();
                writer.AppendLine("def to_key(self) -> str:");
                writer.PushIndent();
                writer.AppendLine(CommentHelper.PyComment($"Get {decl.Name} key."));
                writer.AppendLines($"return '{decl.Name}='{GetToKeyArgs(decl.Name, keyElements, true)}");
                writer.PopIndent();

                writer.AppendNewLineWithoutIndent();

                var namedParams = keyElements.Select(e=>$"{e.Name.Underscore()}: {GetTypeHint(decl, e)}").ToList();
                var joinedNamedParams = string.Join(", ", namedParams);
                string start = "def create_key(";

                // Check if tokens should be separated by new line
                if (4 + start.Length + joinedNamedParams.Length > 120)
                {
                    var indent = new string(' ', start.Length);
                    joinedNamedParams = string.Join("," + Environment.NewLine + indent, namedParams);
                }

                writer.AppendLine("@classmethod");
                writer.AppendLines($"def create_key(cls, *, {joinedNamedParams}) -> str:");

                writer.PushIndent();
                writer.AppendLine(CommentHelper.PyComment($"Create {decl.Name} key."));
                writer.AppendLines($"return '{decl.Name}='{GetToKeyArgs(decl.Name, keyElements, false)}");
                writer.PopIndent();
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

        private static string GetToKeyArgs(string declName, IEnumerable<ElementDecl> keyElements, bool withSelf)
        {
            List<string> tokens = new List<string>();
            string self = withSelf ? "self." : "";

            List<ValueParamType> toStr = new List<ValueParamType>()
            {
                ValueParamType.Int, ValueParamType.Long, ValueParamType.NullableInt, ValueParamType.NullableLong,
                ValueParamType.DateTime, ValueParamType.Date, ValueParamType.Time, ValueParamType.Minute, ValueParamType.Instant,
                ValueParamType.NullableDateTime, ValueParamType.NullableDate, ValueParamType.NullableTime,
                ValueParamType.NullableMinute, ValueParamType.NullableInstant, ValueParamType.TemporalId,
                ValueParamType.NullableTemporalId,
            };

            foreach (var element in keyElements)
            {
                string elementName = element.Name.Underscore();

                if (element.Value?.Type != null && toStr.Contains(element.Value.Type.Value))
                    tokens.Add($"str({self}{elementName})");
                else if (element.Value != null && element.Value.Type == ValueParamType.String)
                    tokens.Add($"{self}{elementName}");
                else if (element.Value != null && (element.Value.Type == ValueParamType.Bool || element.Value.Type == ValueParamType.NullableBool))
                    tokens.Add($"str({self}{elementName}).lower()");
                else if (element.Enum != null)
                    tokens.Add($"{self}{elementName}.name");
                else if (element.Key !=null)
                    tokens.Add($"{self}{elementName}.split('=', 1)[1]");
                else
                    throw new Exception($"Wrong key element type.");
            }

            // Based on timeit test - for one parameter it is faster to concat two strings with +
            if (tokens.Count == 1)
                return $" + {tokens[0]}";
            if (tokens.Count > 1)
            {
                string start = $"return '{declName}=' + ';'.join([";
                string joinedTokens = $" + ';'.join([{string.Join(", ", tokens)}])";

                // Check if tokens should be separated by new line
                if (8 + start.Length + joinedTokens.Length > 120)
                {
                    var indent = new string(' ', start.Length);
                    return $" + ';'.join([{string.Join("," + Environment.NewLine + indent, tokens)}])";
                }

                return joinedTokens;
            }

            return "";
        }

        private static string GetMetaData(ElementDecl element)
        {
            var meta = new List<string>();
            if (element.Optional == YesNo.Y)
                meta.Add("'optional': True");

            if (element.Value != null && (element.Value.Type == ValueParamType.Long ||
                                          element.Value.Type == ValueParamType.NullableLong))
                meta.Add("'type': 'long'");
            if (element.Value != null && (element.Value.Type == ValueParamType.Date ||
                                          element.Value.Type == ValueParamType.NullableDate))
                meta.Add("'type': 'LocalDate'");
            if (element.Value != null && (element.Value.Type == ValueParamType.Minute ||
                                          element.Value.Type == ValueParamType.NullableMinute))
                meta.Add("'type': 'LocalMinute'");
            if (element.Value != null && (element.Value.Type == ValueParamType.Time ||
                                          element.Value.Type == ValueParamType.NullableTime))
                meta.Add("'type': 'LocalTime'");
            if (element.Value != null && (element.Value.Type == ValueParamType.DateTime ||
                                          element.Value.Type == ValueParamType.NullableDateTime))
                meta.Add("'type': 'LocalDateTime'");
            if (element.Value != null && (element.Value.Type == ValueParamType.Instant ||
                                          element.Value.Type == ValueParamType.NullableInstant))
                meta.Add("'type': 'Instant'");

            if (element.Key != null)
                meta.Add($"'key': '{element.Key.Name}'");

            return meta.Any()
                ? $", metadata={{{string.Join(", ", meta)}}}"
                : "";
        }

        private static void WriteMethods(TypeDecl decl, CodeWriter writer)
        {
            bool HasImplement(HandlerDeclareItem declare)
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

        private static string GetTypeHint(TypeDecl decl, ParamDecl parameter)
        {
            if (parameter.Value != null)
            {
                string result = GetValue(parameter.Value);
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
                return parameter.Vector == YesNo.Y ? "List[str]" : "str";
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

        private static string GetTypeHint(TypeDecl decl, ElementDecl element)
        {
            string GetParamNamespace(TypeDecl declaration, IDeclKey key) =>
                !PyExtensions.IsPackageEquals(declaration, key) ? PyExtensions.GetAlias(key) + "." : "";

            string GetFinalHint(string typeHint) =>
                element.Vector == YesNo.Y ? $"List[{typeHint}]" : typeHint;

            if (element.Value != null)
            {
                string hint = GetValue(element.Value);
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
                return GetFinalHint("str");
            }
            else if (element.Enum != null)
            {
                string paramNamespace = GetParamNamespace(decl, element.Enum);
                string hint = $"{paramNamespace}{element.Enum.Name}";
                return GetFinalHint(hint);
            }
            else throw new ArgumentException("Can't deduct type");
        }

        private static string GetValue(ValueDecl valueDecl)
        {
            var atomicType = valueDecl.Type;
            return
                atomicType == ValueParamType.String             ? "str" :
                atomicType == ValueParamType.Bool               ? "bool" :
                atomicType == ValueParamType.Double             ? "float" :
                atomicType == ValueParamType.Int                ? "int" :
                atomicType == ValueParamType.Long               ? "int" :
                atomicType == ValueParamType.NullableBool       ? "bool" :
                atomicType == ValueParamType.NullableDouble     ? "float" :
                atomicType == ValueParamType.NullableInt        ? "int" :
                atomicType == ValueParamType.NullableLong       ? "int" :
                atomicType == ValueParamType.DateTime           ? "int" :
                atomicType == ValueParamType.Date               ? "int" :
                atomicType == ValueParamType.Time               ? "int" :
                atomicType == ValueParamType.Minute             ? "int" :
                atomicType == ValueParamType.Instant            ? "dt.datetime" :
                atomicType == ValueParamType.NullableDateTime   ? "int" :
                atomicType == ValueParamType.NullableDate       ? "int" :
                atomicType == ValueParamType.NullableTime       ? "int" :
                atomicType == ValueParamType.NullableMinute     ? "int" :
                atomicType == ValueParamType.NullableInstant    ? "dt.datetime" :
                atomicType == ValueParamType.TemporalId         ? "ObjectId" :
                atomicType == ValueParamType.NullableTemporalId ? "ObjectId" :
                    throw new ArgumentException($"Unknown value type: {atomicType.ToString()}");
        }
    }
}