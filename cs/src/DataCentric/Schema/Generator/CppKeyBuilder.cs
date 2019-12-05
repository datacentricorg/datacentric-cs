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
using System.Linq;
using Humanizer;

namespace DataCentric
{
    public static class CppKeyBuilder
    {
        public static string BuildKeyHeader(TypeDecl decl, Dictionary<string, string> declSet)
        {
            var writer = new CodeWriter();
            var settings = GeneratorSettingsProvider.Get(decl.Module.ModuleName);

            writer.AppendLines(settings.Copyright);
            writer.AppendNewLineWithoutIndent();

            writer.AppendLine("#pragma once");
            writer.AppendNewLineWithoutIndent();

            // includes
            var includes = IncludesProvider.ForKeyHeader(decl, declSet);
            foreach (string include in includes)
                writer.AppendLine(include);
            writer.AppendNewLineWithoutIndent();

            writer.AppendLine($"namespace {settings.Namespace}");
            writer.AppendLine("{");
            writer.PushIndent();
            BuildClassDeclaration(decl, writer);
            writer.PopIndent();
            writer.AppendLine("}");

            return writer.ToString();
        }

        public static string BuildKeySource(TypeDecl decl, Dictionary<string, string> declSet)
        {
            var writer = new CodeWriter();
            var settings = GeneratorSettingsProvider.Get(decl.Module.ModuleName);

            writer.AppendLines(settings.Copyright);
            writer.AppendNewLineWithoutIndent();

            // includes
            writer.AppendLine($"#include <{settings.Namespace}/precompiled.hpp>");
            writer.AppendLine($"#include <{settings.Namespace}/implement.hpp>");
            writer.AppendLine($"#include <{declSet[decl.Name]}/{decl.Name.Underscore()}_key.hpp>");
            writer.AppendLine($"#include <dc/platform/context/context_base.hpp>");
            writer.AppendNewLineWithoutIndent();

            writer.AppendLine($"namespace {settings.Namespace}");
            writer.AppendLine("{");
            writer.PushIndent();
            BuildClassImplementation(decl, writer);
            writer.PopIndent();
            writer.AppendLine("}");

            return writer.ToString();
        }

        private static void BuildClassDeclaration(TypeDecl decl, CodeWriter writer)
        {
            var settings = GeneratorSettingsProvider.Get(decl.Module.ModuleName);
            var type = decl.Name.Underscore();

            // forwards
            writer.AppendLine($"class {type}_key_impl; using {type}_key = dot::ptr<{type}_key_impl>;");
            writer.AppendLine($"class {type}_data_impl; using {type}_data = dot::ptr<{type}_data_impl>;");

            // Get unique keys and data from elements
            var keyElements = decl.Elements.Where(e => decl.Keys.Contains(e.Name)).ToList();
            var dataForwards = keyElements.Where(e => e.Data != null)
                                          .Where(e => e.Data.Module.ModuleName == decl.Module.ModuleName)
                                          .Select(e => $"{e.Data.Name.Underscore()}_data").ToList();
            var keysForwards = keyElements.Where(e => e.Key != null)
                                          .Where(e => e.Key.Module.ModuleName == decl.Module.ModuleName)
                                          .Select(e => $"{e.Key.Name.Underscore()}_key").ToList();
            var forwards = keysForwards.Union(dataForwards).Distinct();
            // Appends forwards
            foreach (var f in forwards)
                writer.AppendLine($"class {f}_impl; using {f} = dot::ptr<{f}_impl>;");
            writer.AppendNewLineWithoutIndent();

            writer.AppendLine($"inline {type}_key make_{type}_key();");
            writer.AppendNewLineWithoutIndent();

            writer.AppendLines(CommentHelper.FormatComment(decl.Comment));

            writer.AppendLine($"class {settings.DeclSpec} {type}_key_impl : public key_impl<{type}_key_impl,{type}_data_impl>");
            writer.AppendLine("{");

            writer.PushIndent();
            writer.AppendLine($"typedef {type}_key_impl self;");
            writer.AppendLine($"friend {type}_key make_{type}_key();");
            writer.PopIndent();
            writer.AppendNewLineWithoutIndent();

            writer.AppendLine("public: // FIELDS");
            writer.AppendNewLineWithoutIndent();

            if (keyElements.Any())
            {
                writer.PushIndent();
                CppElementBuilder.WriteElements(keyElements, writer);
                writer.PopIndent();
            }

            writer.AppendLine("public:");
            writer.PushIndent();
            writer.AppendLine("virtual dot::type_t type();");
            writer.AppendLine("static dot::type_t typeof();");
            writer.PopIndent();

            writer.AppendLine("};");
            writer.AppendNewLineWithoutIndent();

            writer.AppendLine("/// Create an empty instance.");
            writer.AppendLine($"inline {type}_key make_{type}_key() {{ return new {type}_key_impl; }}");
        }

        private static void BuildClassImplementation(TypeDecl decl, CodeWriter writer)
        {
            var settings = GeneratorSettingsProvider.Get(decl.Module.ModuleName);
            var type = decl.Name.Underscore();

            writer.AppendLine($"dot::type_t {type}_key_impl::type() {{ return typeof(); }}");
            writer.AppendLine($"dot::type_t {type}_key_impl::typeof()");

            writer.AppendLine("{");
            writer.PushIndent();

            writer.AppendLine("static dot::type_t type_ =");
            writer.PushIndent();

            writer.AppendLine($"dot::make_type_builder<self>(\"{settings.Namespace}\", \"{type}\")");
            var keyElements = decl.Elements.Where(e => decl.Keys.Contains(e.Name)).ToList();
            foreach (var element in keyElements.Where(e => e.BsonIgnore != YesNo.Y))
            {
                var name = element.Name.Underscore();
                writer.AppendLine($"->with_field(\"{name}\", &self::{name})");
            }

            writer.AppendLine($"->template with_base<key<{type}_key_impl, {type}_data_impl>>()");
            writer.AppendLine($"->with_constructor(&make_{type}_key, {{  }})");
            writer.AppendLine("->build();");

            writer.PopIndent();
            writer.AppendLine("return type_;");

            writer.PopIndent();
            writer.AppendLine("}");
        }
    }
}