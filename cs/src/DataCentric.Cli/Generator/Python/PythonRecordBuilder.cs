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

            bool hasKey = decl.Keys.Any();
            bool isDerived = decl.Inherit != null;
            string name = decl.Name;

            if (hasKey)
                writer.AppendLine("from datacentric.types.record import TypedRecord, TypedKey");
            else if (isDerived)
                writer.AppendLine("");
            else
                writer.AppendLine("from datacentric.types.record import Data");

            writer.AppendNewLineWithoutIndent();
            writer.AppendNewLineWithoutIndent();

            if (decl.Keys.Any())
            {
                var keyElements = decl.Elements.Where(e => decl.Keys.Contains(e.Name)).ToList();

                writer.AppendLine($"class {name}Key(TypedKey['{name}']):");
                writer.PushIndent();
                writer.AppendLine($"\"\"\"Key for {name}.\"\"\"");
                writer.AppendNewLineWithoutIndent();

                var keySlots = string.Join(", ", decl.Keys.Select(t => $"'{t.Underscore()}'"));
                writer.AppendLine($"__slots__ = ({string.Join(", ", keySlots)})");
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

            if (hasKey)
                writer.AppendLine($"class {name}(TypedRecord[{name}Key]{abstractBase}):");
            else if (isDerived)
                writer.AppendLine($"class {name}({decl.Inherit.Name}{abstractBase}):");
            else
                writer.AppendLine($"class {name}(Data{abstractBase}):");

            writer.PushIndent();
            writer.AppendLines(CommentHelper.PyComment(decl.Comment));
            writer.AppendNewLineWithoutIndent();

            var slots = string.Join(", ", decl.Elements.Select(t => $"'{t.Name.Underscore()}'"));
            writer.AppendLine($"__slots__ = ({string.Join(", ", slots)})");
            writer.AppendNewLineWithoutIndent();

            foreach (var element in decl.Elements) writer.AppendLine($"{element.Name.Underscore()}: {GetTypeHint(element)}");
            writer.AppendNewLineWithoutIndent();

            // Init start
            writer.AppendLine(decl.Kind == TypeKind.Element ? "def __init__(self):" : "def __init__(self, context: Context):");
            writer.PushIndent();

            writer.AppendLine(decl.Kind == TypeKind.Element ? "super().__init__()" : "super().__init__(context)");
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
                atomicType == AtomicType.String             ? "Optional[str]" :
                atomicType == AtomicType.Bool               ? "bool" :
                atomicType == AtomicType.DateTime           ? "dt.datetime" :
                atomicType == AtomicType.Double             ? "float" :
                atomicType == AtomicType.Int                ? "int" :
                atomicType == AtomicType.Long               ? "int" :
                atomicType == AtomicType.NullableBool       ? "Optional[bool]" :
                atomicType == AtomicType.NullableDateTime   ? "Optional[dt.datetime]" :
                atomicType == AtomicType.NullableDouble     ? "Optional[float]" :
                atomicType == AtomicType.NullableInt        ? "Optional[int]" :
                atomicType == AtomicType.NullableLong       ? "Optional[int]" :
                atomicType == AtomicType.DateTime           ? "dt.datetime" :
                atomicType == AtomicType.Date               ? "dt.date" :
                atomicType == AtomicType.Time               ? "dt.time" :
                atomicType == AtomicType.Minute             ? "LocalMinute" :
                atomicType == AtomicType.NullableDateTime   ? "Optional[dt.datetime]" :
                atomicType == AtomicType.NullableDate       ? "Optional[dt.date]" :
                atomicType == AtomicType.NullableTime       ? "Optional[dt.time]" :
                atomicType == AtomicType.NullableMinute     ? "Optional[LocalMinute]" :
                atomicType == AtomicType.TemporalId         ? "ObjectId" :
                atomicType == AtomicType.NullableTemporalId ? "Optional[ObjectId]" :
                                                              throw new
                                                                  ArgumentException($"Unknown value type: {atomicType.ToString()}");
        }
    }
}