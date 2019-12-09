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
        public static void WriteElements(List<ElementDecl> elements, CodeWriter writer)
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

        public static string GetType(ElementDecl element)
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
                atomicType == ValueParamType.String           ? "dot::string" :
                atomicType == ValueParamType.Bool             ? "bool" :
                atomicType == ValueParamType.DateTime         ? "dot::local_date_time" :
                atomicType == ValueParamType.Double           ? "double" :
                atomicType == ValueParamType.Int              ? "int" :
                atomicType == ValueParamType.Long             ? "long" :
                atomicType == ValueParamType.NullableBool     ? "dot::nullable<bool>" :
                atomicType == ValueParamType.NullableDateTime ? "dot::nullable<dot::local_date_time>" :
                atomicType == ValueParamType.NullableDouble   ? "dot::nullable<double>" :
                atomicType == ValueParamType.NullableInt      ? "dot::nullable<int>" :
                atomicType == ValueParamType.NullableLong     ? "dot::nullable<long>" :
                atomicType == ValueParamType.DateTime         ? "dot::local_date_time" :
                atomicType == ValueParamType.Date             ? "dot::local_date" :
                atomicType == ValueParamType.Time             ? "dot::local_time" :
                atomicType == ValueParamType.Minute           ? "dot::local_minute" :
                atomicType == ValueParamType.NullableDateTime ? "dot::nullable<dot::local_date_time>" :
                atomicType == ValueParamType.NullableDate     ? "dot::nullable<dot::local_date>" :
                atomicType == ValueParamType.NullableTime     ? "dot::nullable<dot::local_time>" :
                atomicType == ValueParamType.NullableMinute   ? "dot::nullable<dot::local_minute>" :
                atomicType == ValueParamType.TemporalId         ? "dot::object_id" :
                atomicType == ValueParamType.NullableTemporalId ? "dot::nullable<dot::object_id>" :
                                                            throw new
                                                                ArgumentException($"Unknown value type: {atomicType.ToString()}");
        }
    }
}