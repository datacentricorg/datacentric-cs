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
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace DataCentric.Schema.Generator
{
    public class GeneratorSettings
    {
        public string Project { get; set; }
        public string Namespace { get; set; }
        public string DeclSpec { get; set; }
        public string Copyright { get; set; }
    }

    public static class GeneratorSettingsProvider
    {
        private static readonly Dictionary<string, GeneratorSettings> Settings = new Dictionary<string, GeneratorSettings>();

        public static void PopulateFromFile(string path)
        {
            string json = File.ReadAllText(path);
            var settings = JsonConvert.DeserializeObject<List<GeneratorSettings>>(json);

            foreach (var setting in settings)
            {
                if (!Settings.ContainsKey(setting.Project))
                    Settings[setting.Project] = setting;
            }

            // Populate with default for DataCentric
            if (!Settings.Any())
            {
                Settings["DataCentric"] = new GeneratorSettings()
                {
                    Namespace = "dc",
                    DeclSpec = "DC_CLASS",
                    Copyright = @"/*
Copyright (C) 2013-present The DataCentric Authors.

Licensed under the Apache License, Version 2.0 (the ""License"");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an ""AS IS"" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/"
                };
            }
        }

        public static GeneratorSettings Get(string module)
        {
            if (Settings.ContainsKey(module))
                return Settings[module];
            throw new ArgumentOutOfRangeException($"No settings for module: {module}");
        }

        public static List<string> KnownModules()
        {
            return Settings.Keys.ToList();
        }
    }
}