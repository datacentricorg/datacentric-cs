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
using DataCentric.Schema.Declaration.Type;
using Humanizer;

namespace DataCentric.Schema.Generator
{
    public static class IncludesProvider
    {
        public static IEnumerable<string> ForDataHeader(TypeDecl decl, Dictionary<string, string> declSet)
        {
            var settings = GeneratorSettingsProvider.Get(decl.Module.ModuleName);
            var includes = new List<string>
            {
                $"#include <{settings.Namespace}/declare.hpp>",
                "#include <dot/system/ptr.hpp>"
            };

            // Base type include
            if (decl.Keys.Any())
            {
                includes.Add($"#include <dc/types/record/record.hpp>");
                includes.Add($"#include <{declSet[decl.Name]}/{decl.Name.Underscore()}_key.hpp>");
            }
            else if (decl.Inherit != null)
            {
                var baseName = decl.Inherit.Name;
                if (declSet.ContainsKey(baseName))
                {
                    includes.Add($"#include <{declSet[baseName]}/{baseName.Underscore()}_data.hpp>");
                }
            }
            else
            {
                includes.Add($"#include <dc/types/record/data.hpp>");
            }

            // Include from field types
            includes.AddRange(decl.Elements.Where(t => t.Data != null)
                                  .Select(t => t.Data.Name).Distinct()
                                  .Select(t => $"#include <{declSet[t]}/{t.Underscore()}_data.hpp>"));
            includes.AddRange(decl.Elements.Where(t => t.Key != null)
                                  .Select(t => t.Key.Name).Distinct()
                                  .Select(t => $"#include <{declSet[t]}/{t.Underscore()}_key.hpp>"));

            var knownModules = GeneratorSettingsProvider.KnownModules();
            includes.AddRange(decl.Elements.Where(t => t.Enum != null)
                                   // Skip external enum
                                  .Where(t => knownModules.Contains(t.Enum.Module.ModuleName))
                                  .Select(t => t.Enum.Name).Distinct()
                                  .Select(t => $"#include <{declSet[t]}/{t.Underscore()}.hpp>"));

            return includes;
        }

        public static IEnumerable<string> ForKeyHeader(TypeDecl decl, Dictionary<string, string> declSet)
        {
            var settings = GeneratorSettingsProvider.Get(decl.Module.ModuleName);
            var includes = new List<string>
            {
                $"#include <{settings.Namespace}/declare.hpp>",
                "#include <dot/system/ptr.hpp>",
                "#include <dc/types/record/record.hpp>",
                $"#include <{declSet[decl.Name]}/{decl.Name.Underscore()}_data.hpp>"
            };

            // Include from field types
            includes.AddRange(decl.Elements.Where(t => t.Data != null)
                                  .Select(t => t.Data.Name).Distinct()
                                  .Select(t => $"#include <{declSet[t]}/{t.Underscore()}_data.hpp>"));
            includes.AddRange(decl.Elements.Where(t => t.Key != null)
                                  .Select(t => t.Key.Name).Distinct()
                                  .Select(t => $"#include <{declSet[t]}/{t.Underscore()}_key.hpp>"));

            var knownModules = GeneratorSettingsProvider.KnownModules();
            includes.AddRange(decl.Elements.Where(t => t.Enum != null)
                                   // Skip external enum
                                  .Where(t => knownModules.Contains(t.Enum.Module.ModuleName))
                                  .Select(t => t.Enum.Name).Distinct()
                                  .Select(t => $"#include <{declSet[t]}/{t.Underscore()}.hpp>"));

            return includes;
        }
    }
}