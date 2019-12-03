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

using System.Collections.Generic;
using DataCentric.Schema.Declaration.Type;
using Humanizer;

namespace DataCentric.Schema.Generator
{
    public static class CppEnumBuilder
    {
        public static string BuildEnumHeader(EnumDecl decl, Dictionary<string, string> includePath)
        {
            var writer = new CodeWriter();

            var settings = GeneratorSettingsProvider.Get(decl.Module.ModuleName);

            writer.AppendLines(settings.Copyright);
            writer.AppendNewLineWithoutIndent();

            writer.AppendLine("#pragma once");
            writer.AppendNewLineWithoutIndent();

            // includes
            writer.AppendLine("#include <dot/system/enum.hpp>");
            // pure data

            writer.AppendNewLineWithoutIndent();

            writer.AppendLine($"namespace {settings.Namespace}");
            writer.AppendLine("{");
            writer.PushIndent();
            BuildEnumDeclaration(decl, writer);
            writer.PopIndent();
            writer.AppendLine("}");

            return writer.ToString();
        }

        public static string BuildEnumSource(EnumDecl decl, Dictionary<string, string> includePath)
        {
            var writer = new CodeWriter();
            var settings = GeneratorSettingsProvider.Get(decl.Module.ModuleName);

            writer.AppendLines(settings.Copyright);
            writer.AppendNewLineWithoutIndent();

            writer.AppendLine($"#include <{settings.Namespace}/precompiled.hpp>");
            writer.AppendLine($"#include <{settings.Namespace}/implement.hpp>");
            writer.AppendLine($"#include <{includePath[decl.Name]}/{decl.Name.Underscore()}.hpp>");
            writer.AppendNewLineWithoutIndent();

            writer.AppendLine($"namespace {settings.Namespace}");
            writer.AppendLine("{");
            writer.PushIndent();
            BuildEnumImplementation(decl, writer);
            writer.PopIndent();
            writer.AppendLine("}");

            return writer.ToString();
        }

        private static void BuildEnumDeclaration(EnumDecl decl, CodeWriter writer)
        {
            var settings = GeneratorSettingsProvider.Get(decl.Module.ModuleName);
            var type = decl.Name.Underscore();

            writer.AppendNewLineWithoutIndent();

            writer.AppendLines(CommentHelper.FormatComment(decl.Comment));

            writer.AppendLine($"class {settings.DeclSpec} {type} : public dot::enum_base");
            writer.AppendLine("{");

            writer.PushIndent();
            writer.AppendLine($"typedef {type} self;");
            writer.PopIndent();
            writer.AppendNewLineWithoutIndent();

            var elements = decl.Items;

            writer.AppendLine("public:");
            writer.AppendNewLineWithoutIndent();
            writer.PushIndent();

            writer.AppendLine("enum enum_type {");
            writer.PushIndent();
            foreach (EnumItemDecl item in elements)
            {
                writer.AppendLines(CommentHelper.FormatComment(item.Comment));
                writer.AppendLine($"{item.Name.Underscore()},");
                // Do not add new line after last item
                if (elements.IndexOf(item) != elements.Count - 1)
                    writer.AppendNewLineWithoutIndent();
            }

            writer.PopIndent();
            writer.AppendLine("};");
            writer.AppendNewLineWithoutIndent();
            writer.PopIndent();

            writer.AppendLines(@"private:
    static dot::object make_self() { return self(); }

public:
    typedef self element_type;
    typedef dot::struct_wrapper_impl<self>* pointer_type;
    using dot::enum_base::enum_base;

    operator dot::object();
    operator int() const;
    self& operator=(int rhs);
    self& operator=(const self& other);
    virtual dot::type_t type();
    static dot::type_t typeof();

protected:
    virtual dot::dictionary<dot::string, int> get_enum_map() override;");

            writer.AppendLine("};");
        }

        private static void BuildEnumImplementation(EnumDecl decl, CodeWriter writer)
        {
            var settings = GeneratorSettingsProvider.Get(decl.Module.ModuleName);
            var type = decl.Name.Underscore();

            writer.AppendLines($@"{type}::operator dot::object() {{ return new dot::struct_wrapper_impl<self>(*this); }}
{type}::operator int() const {{ return value_; }}
{type}& {type}::operator=(int rhs) {{ value_ = rhs; return *this; }}
{type}& {type}::operator=(const self& other) {{ value_ = other.value_; return *this; }}
dot::type_t {type}::type() {{ return typeof(); }}");

            writer.AppendNewLineWithoutIndent();

            writer.AppendLines($@"dot::type_t {type}::typeof()
{{
    static dot::type_t type_ =
        dot::make_type_builder<self>(""{settings.Namespace}"", ""{type}"")
        ->is_enum()
        ->with_constructor(&self::make_self, {{}})
        ->with_base<enum_base>()
        ->build();
    return type_;
}}");

            writer.AppendNewLineWithoutIndent();

            writer.AppendLines($@"dot::dictionary<dot::string, int> {type}::get_enum_map()
{{
    static dot::dictionary<dot::string, int> enum_map_ = []()
    {{
        auto map_ = dot::make_dictionary<dot::string, int>();");
            // catching indents from prev verbatim get_enum_map block
            writer.PushIndent();
            writer.PushIndent();

            foreach (EnumItemDecl item in decl.Items)
            {
                writer.AppendLine($"map_[\"{item.Name.Underscore()}\"] = {item.Name.Underscore()};");
            }

            writer.AppendLine("return map_;");
            writer.PopIndent();
            writer.AppendLine("}();");
            writer.AppendLine("return enum_map_;");
            writer.PopIndent();
            writer.AppendLine("}");
        }
    }
}