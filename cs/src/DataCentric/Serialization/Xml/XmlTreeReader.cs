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
using System.Xml;
using System.Text;
using System.IO;
using System.Linq;

namespace DataCentric
{
    /// <summary>Implementation of IXmlReader using C# XmlNode.</summary>
    public class XmlTreeReader : ITreeReader
    {
        private XmlNode xmlNode_;
        private bool isRootNode_;

        /// <summary>Create an implementation of IXmlReader from XML document.</summary>
        public XmlTreeReader(string xmlText)
        {
            // Check if the XML string empty
            if (string.IsNullOrEmpty(xmlText)) throw new Exception("Empty XML document is pased to XML reader.");

            // Create XML document and load XML to XmlNode
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlText)))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(stream);
                xmlNode_ = xmlDoc.DocumentElement;
            }

            // Check if XML root node is present and is an element
            if (xmlNode_ == null) throw new Exception("XML root node is missing");
            if (xmlNode_.NodeType != XmlNodeType.Element) throw new Exception("XML root node is not an element.");

            // The node created from text is the root node
            isRootNode_ = true;
        }

        /// <summary>This private constructor creates an implementation of IXmlReader from C# XmlNode.</summary>
        private XmlTreeReader(XmlNode xmlNode)
        {
            if(xmlNode == null) throw new Exception("Attempting to create XML reader from null XmlNode.");
            xmlNode_ = xmlNode;

            // The node created from another node is not the root node
            isRootNode_ = false;
        }

        /// <summary>
        /// Read a single element (returns null if not found).
        /// Error message if more than one element with the specified name is present.
        /// </summary>
        public ITreeReader ReadElement(string elementName)
        {
            if (!isRootNode_)
            {
                // If this is not the root node, select and return a reader for the child node
                XmlNode result = xmlNode_.SelectSingleNode(elementName);
                if (result != null) return new XmlTreeReader(result);
                else return null;
            }
            else
            {
                // If this is the root node, check that the element matches
                // and return a new instance created from the same XmlNode
                if (elementName != xmlNode_.Name)
                    throw new Exception(
                        $"Root element name ({xmlNode_.Name}) does not match the " +
                        $"name passed to ReadElement ({elementName}).");

                return new XmlTreeReader(xmlNode_);
            }
        }

        /// <summary>
        /// Read multiple elements (returns empty list if not found).
        /// If contentOnly flag is true, returned nodes don't have their imports.
        /// </summary>
        public IEnumerable<ITreeReader> ReadElements(string elementName)
        {
            if (!isRootNode_)
            {
                // If this is not the root node, select and return a reader for the child nodes array
                XmlNodeList xmlNodeList = xmlNode_.SelectNodes(elementName);
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    // Return a sequence of readers created from each node
                    yield return new XmlTreeReader(xmlNode);
                }
            }
            else throw new Exception($"XML standard does not permit multiple elements at XML document root.");
        }

        /// <summary>
        /// Read atomic value (returns empty string if not found).
        /// This overload is intended for value elements which also have
        /// attributes, otherwise ReadValueElement can be used.
        /// </summary>
        public string ReadValue()
        {
            if (xmlNode_.NodeType != XmlNodeType.Element) throw new Exception("ReadValue() is called for the outer XML node which is not an element.");
            if (xmlNode_.ChildNodes.Count > 1) throw new Exception("ReadValue() is called for XML node which has multiple child nodes.");

            // Support for empty elements.
            if (xmlNode_.FirstChild == null) return string.Empty;

            if (xmlNode_.FirstChild.NodeType != XmlNodeType.Text) throw new Exception("ReadValue() is called for XML node which is not atomic.");
            if (xmlNode_.FirstChild.HasChildNodes) throw new Exception("ReadValue() is called for XML node which has child nodes.");

            string result = xmlNode_.InnerXml;
            return string.IsNullOrEmpty(result) ? string.Empty : result;
        }

        /// <summary>
        /// Read XML attribute (returns empty string if not found).
        ///
        /// When deserializing from a representation format that does not support
        /// attributes, such as JSON, the attribute is read as element with
        /// underscore prefix before its name.
        /// </summary>
        public string ReadAttribute(string attributeName)
        {
            var attributes = xmlNode_.Attributes;
            if (attributes != null)
            {
                // Return empty string if attribute with the specified name is not present
                string result = attributes.GetNamedItem(attributeName).Value;
                return string.IsNullOrEmpty(result) ? string.Empty : result;
            }
            else
            {
                // Return empty string if Attributes property is null
                return string.Empty;
            }
        }

        /// <summary>
        /// Read a single element containing atomic value (returns empty string if not found).
        /// Error message if more than one element with the specified name is present.
        /// </summary>
        public string ReadValueElement(string elementName)
        {
            ITreeReader elementReader = ReadElement(elementName);

            // Return value if element is present, otherwise null
            if (elementReader != null) return elementReader.ReadValue();
            else return String.Empty;
        }

        /// <summary>Convert to XML string for the purpose of inspecting the node contents.</summary>
        public override string ToString()
        {
            string result = xmlNode_.InnerXml;
            return string.IsNullOrEmpty(result) ? string.Empty : result;
        }
    }
}
