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
using System.Linq;
using DataCentric;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Xunit;

namespace DataCentric.Test
{
    /// <summary>Sample derived data class.</summary>
    [IndexElements("DoubleElement2, -DoubleElement")]
    public class DerivedSample : BaseSample
    {
        /// <summary>Sample element.</summary>
        public double? DoubleElement2 { get; set; }

        /// <summary>Sample element.</summary>
        public string StringElement2 { get; set; }

        /// <summary>Sample element.</summary>
        public string[] ArrayOfString { get; set; }

        /// <summary>Sample element.</summary>
        public List<string> ListOfString { get; set; }

        /// <summary>Sample element.</summary>
        public double[] ArrayOfDouble { get; set; }

        /// <summary>Sample element.</summary>
        public double?[] ArrayOfNullableDouble { get; set; }

        /// <summary>Sample element.</summary>
        public List<double> ListOfDouble { get; set; }

        /// <summary>Sample element.</summary>
        public List<double?> ListOfNullableDouble { get; set; }

        /// <summary>Sample element.</summary>
        public ElementSample DataElement { get; set; }

        /// <summary>Sample element.</summary>
        public List<ElementSample> DataElementList { get; set; }

        /// <summary>Sample element.</summary>
        public BaseSampleKey KeyElement { get; set; }

        /// <summary>Sample element.</summary>
        public List<BaseSampleKey> KeyElementList { get; set; }

        //--- METHODS

        /// <summary>Non-virtual handler defined in base type.</summary>
        [HandlerMethod]
        public void NonVirtualDerivedHandler()
        {
            Context.Log.Verify("NonVirtualDerivedHandler");
        }

        /// <summary>Override of the virtual handler defined in base type.</summary>
        [HandlerMethod]
        public override void VirtualBaseHandler()
        {
            Context.Log.Verify("VirtualBaseHandler implemented in Derived");
        }
    }
}
