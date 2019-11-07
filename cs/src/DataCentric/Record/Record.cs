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
using CsvHelper.Configuration.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>
    /// Base class of records stored in data source.
    /// </summary>
    [BsonDiscriminator(RootClass = true)]
    public abstract class Record : Data
    {
        /// <summary>
        /// Execution context provides access to key resources including:
        ///
        /// * Logging and error reporting
        /// * Cloud calculation service
        /// * Data sources
        /// * Filesystem
        /// * Progress reporting
        /// </summary>
        [BsonIgnore]
        [Ignore]
        public IContext Context { get; private set; }

        //--- ELEMENTS

        /// <summary>
        /// TemporalId of the record is specific to its version.
        ///
        /// For the record's history to be captured correctly, all
        /// update operations must assign a new TemporalId with the
        /// timestamp that matches update time.
        /// </summary>
        [BsonId]
        [BsonRequired]
        public TemporalId Id { get; set; }

        /// <summary>
        /// TemporalId of the dataset where the record is stored.
        ///
        /// For records stored in root dataset, the value of
        /// DataSet element should be TemporalId.Empty.
        /// </summary>
        [BsonElement("_dataset")]
        public TemporalId DataSet { get; set; }

        /// <summary>
        /// String key consists of semicolon delimited primary key elements:
        ///
        /// KeyElement1;KeyElement2
        ///
        /// To avoid serialization format uncertainty, key elements
        /// can have any atomic type except Double.
        /// </summary>
        [BsonElement("_key")]
        [BsonRequired]
        public abstract string Key { get; set; }

        //--- METHODS

        /// <summary>
        /// Set Context property and perform validation of the record's data,
        /// then initialize any fields or properties that depend on that data.
        ///
        /// This method may be called multiple times for the same instance,
        /// possibly with a different context parameter for each subsequent call.
        ///
        /// IMPORTANT - Every override of this method must call base.Init()
        /// first, and only then execute the rest of the override method's code.
        /// </summary>
        public virtual void Init(IContext context)
        {
            // Uncomment except in root class of the hierarchy
            // base.Init(context);

            // Check that argument is not null and assign to the Context property
            if (context == null) throw new Exception($"Null context is passed to the Init(...) method for {GetType().Name}.");
            Context = context;
        }

        /// <summary>Return string representation of the record's key.</summary>
        public override string ToString() { return Key; }
    }

    /// <summary>Extension methods for Record.</summary>
    public static class RecordExtensions
    {
        /// <summary>Deserialize record from XML using short
        /// class name without namespace for the root XML element.</summary>
        public static void ParseXml(this Record obj, string xmlString)
        {
            ITreeReader reader = new XmlTreeReader(xmlString);

            // Root node of serialized XML must be the same as mapped class name without namespace
            var mappedFullName = obj.GetType().Name;
            ITreeReader recordNodes = reader.ReadElement(mappedFullName);

            // Deserialize from XML nodes inside the root node
            obj.DeserializeFrom(recordNodes);
        }

        /// <summary>Serialize record to XML using short
        /// class name without namespace for the root XML element.</summary>
        public static string ToXml(this Record obj)
        {
            // Get root XML element name using mapped final type of the object
            string rootName = obj.GetType().Name;

            // Serialize to XML
            ITreeWriter writer = new XmlTreeWriter();
            writer.WriteStartDocument(rootName);
            obj.SerializeTo(writer);
            writer.WriteEndDocument(rootName);

            // Convert to string
            string result = writer.ToString();
            return result;
        }
    }
}
