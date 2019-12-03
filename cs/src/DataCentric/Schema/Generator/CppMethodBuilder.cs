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
using DataCentric.Schema.Declaration.Type;
using Humanizer;

namespace DataCentric.Schema.Generator
{
    public static class CppMethodBuilder
    {
        public static void WriteElements(TypeDecl decl, CodeWriter writer)
        {
            foreach (var declare in decl.Declare.Handlers)
            {
                var @return = declare.Return != null ? GetType(declare.Return) : "void";
                var @params = string.Join(", ", declare.Params.Select(param => $"{GetType(param)} {param.Name.Underscore()}"));
                var function = $"{@return} {declare.Name.Underscore()}({@params})";

                var comment = CommentHelper.FormatComment(declare.Comment);
                writer.AppendLines(comment);

                var implement = decl.Implement?.Handlers.FirstOrDefault(i => i.Name == declare.Name);
                // Abstract
                if (implement == null)
                    writer.AppendLine($"virtual {function} = 0;");
                // Override
                else if (implement.Override == YesNo.Y)
                {
                    writer.AppendLine($"virtual {function} override;");
                }
                // No modifiers.
                else
                {
                    writer.AppendLine($"{function};");
                }

                writer.AppendNewLineWithoutIndent();
            }
        }

        public static string GetType(HandlerVariableDecl element)
        {
            string type = element.Value != null ? CppElementBuilder.GetValue(element.Value) :
                          element.Data != null  ? $"{element.Data.Name.Underscore()}_data" :
                          element.Key != null   ? $"{element.Key.Name.Underscore()}_key" :
                          element.Enum != null  ? element.Enum.Name.Underscore() :
                                                  throw new ArgumentException("Can't deduct type");

            return element.Vector == YesNo.Y ? $"dot::list<{type}>" : type;
        }
    }
}