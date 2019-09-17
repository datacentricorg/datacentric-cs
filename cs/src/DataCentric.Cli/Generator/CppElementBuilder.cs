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
using Humanizer;

namespace DataCentric.Cli
{
    public static class CppElementBuilder
    {
        public static void WriteElements(List<TypeElementDeclData> elements, CppCodeWriter writer)
        {
            foreach (var element in elements)
            {
                var comment = CommentHelper.FormatComment(element.Comment);
                writer.AppendLines(comment);

                var type = GetType(element);
                writer.AppendLine($"{type} {element.Name.Underscore()};");
                writer.AppendNewLineWithoutIndent();
            }
        }

        private static string GetType(TypeElementDeclData element)
        {
            string type = element.Value != null ? GetValue(element.Value) :
                          element.Data != null  ? $"{element.Data.Name.Underscore()}_data" :
                          element.Key != null   ? $"{element.Key.Name.Underscore()}_key" :
                          element.Enum != null  ? element.Enum.Name.Underscore() :
                                                  throw new ArgumentException("Can't deduct type");

            return element.Vector == YesNo.Y ? $"dot::list<{type}>" : type;
        }

        private static string GetValue(ValueDeclData valueDecl)
        {
            var atomicType = valueDecl.Type;
            return atomicType == AtomicType.Bool     ? "bool" :
                   atomicType == AtomicType.DateTime ? "dot::local_date_time" :
                   atomicType == AtomicType.Double   ? "double" :
                   atomicType == AtomicType.String   ? "dot::string" :
                   atomicType == AtomicType.Int      ? "int" :
                   atomicType == AtomicType.Long     ? "long" :
                   atomicType == AtomicType.Date     ? "dot::local_date" :
                   atomicType == AtomicType.Time     ? "dot::local_time" :
                   // Minute is mapped to int
                   //atomicType == AtomicType.Minute   ? "LocalMinute" :
                                                         throw new ArgumentException($"Unknown value type: {atomicType.ToString()}");
        }
    }
}