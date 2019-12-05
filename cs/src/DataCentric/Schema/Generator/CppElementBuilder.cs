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
using Humanizer;

namespace DataCentric
{
    public static class CppElementBuilder
    {
        public static void WriteElements(List<TypeElementDecl> elements, CodeWriter writer)
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

        public static string GetType(TypeElementDecl element)
        {
            string type = element.Value != null ? GetValue(element.Value) :
                          element.Data != null  ? $"{element.Data.Name.Underscore()}_data" :
                          element.Key != null   ? $"{element.Key.Name.Underscore()}_key" :
                          element.Enum != null  ? element.Enum.Name.Underscore() :
                                                  throw new ArgumentException("Can't deduct type");

            return element.Vector == YesNo.Y ? $"dot::list<{type}>" : type;
        }

        public static string GetValue(ValueDecl valueDecl)
        {
            var atomicType = valueDecl.Type;
            return
                atomicType == AtomicType.String           ? "dot::string" :
                atomicType == AtomicType.Bool             ? "bool" :
                atomicType == AtomicType.DateTime         ? "dot::local_date_time" :
                atomicType == AtomicType.Double           ? "double" :
                atomicType == AtomicType.Int              ? "int" :
                atomicType == AtomicType.Long             ? "long" :
                atomicType == AtomicType.NullableBool     ? "dot::nullable<bool>" :
                atomicType == AtomicType.NullableDateTime ? "dot::nullable<dot::local_date_time>" :
                atomicType == AtomicType.NullableDouble   ? "dot::nullable<double>" :
                atomicType == AtomicType.NullableInt      ? "dot::nullable<int>" :
                atomicType == AtomicType.NullableLong     ? "dot::nullable<long>" :
                atomicType == AtomicType.DateTime         ? "dot::local_date_time" :
                atomicType == AtomicType.Date             ? "dot::local_date" :
                atomicType == AtomicType.Time             ? "dot::local_time" :
                atomicType == AtomicType.Minute           ? "dot::local_minute" :
                atomicType == AtomicType.NullableDateTime ? "dot::nullable<dot::local_date_time>" :
                atomicType == AtomicType.NullableDate     ? "dot::nullable<dot::local_date>" :
                atomicType == AtomicType.NullableTime     ? "dot::nullable<dot::local_time>" :
                atomicType == AtomicType.NullableMinute   ? "dot::nullable<dot::local_minute>" :
                atomicType == AtomicType.TemporalId         ? "dot::object_id" :
                atomicType == AtomicType.NullableTemporalId ? "dot::nullable<dot::object_id>" :
                                                            throw new
                                                                ArgumentException($"Unknown value type: {atomicType.ToString()}");
        }
    }
}