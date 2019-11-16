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
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DataCentric.Cli
{
    /// <summary>
    /// Serializer for declarations based in XmlSerializer implementation.
    /// </summary>
    public static class DeclarationSerializer
    {
        /// <summary>
        /// Deserializes provided input into declaration.
        /// </summary>
        public static T Deserialize<T>(string input) where T : IDecl
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (StringReader stringReader = new StringReader(input))
            {
                return (T)serializer.Deserialize(stringReader);
            }
        }

        /// <summary>
        /// Serializes given declaration into UTF8 encoded and formatted string.
        /// </summary>
        public static string Serialize<T>(T decl) where T : IDecl
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            using (var ms = new MemoryStream())
            {
                using (XmlTextWriter xmlWriter = new XmlTextWriter(ms, new UTF8Encoding(false)) { Formatting = Formatting.Indented })
                    serializer.Serialize(xmlWriter, decl, ns);

                // Remove xml:nil and save to string
                return Encoding.UTF8.GetString(ms.ToArray()).Map(RemoveNilElements);
            }
        }

        /// <summary>
        /// Removes xml:nil nodes from given xml string.
        /// </summary>
        private static string RemoveNilElements(string content)
        {
            XDocument document = XDocument.Parse(content);

            // Do not inline! Xml doesn't support remove operations during enumeration of the document.
            List<XElement> nils = document.Descendants().Where(IsNilElement).ToList();
            foreach (var element in nils) element.Remove();

            return string.Join(StringUtil.Eol, document.Declaration, document.ToString());
        }

        /// <summary>
        /// Checks if given xml elements is nil element.
        /// </summary>
        private static bool IsNilElement(XElement x)
        {
            return x.Attributes()
                    .Where(atr => atr.Name.ToString().Contains("nil"))
                    .Any(atr => (bool?)atr == true);
        }
    }
}