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
    /// <summary>Base class of sample data for data source testing.</summary>
    [IndexElements("DoubleElement, LocalDateElement, EnumValue")]
    [IndexElements("LocalDateElement")]
    [IndexElements("RecordName, -Version", "CustomIndexName")]
    [IndexElements("-RecordIndex")]
    public class BaseSample : TypedRecord<BaseSampleKey, BaseSample>
    {
        /// <summary>Sample element.</summary>
        public string RecordName { get; set; }

        /// <summary>Sample element.</summary>
        public int? RecordIndex { get; set; }

        /// <summary>Sample element.</summary>
        public double? DoubleElement { get; set; }

        /// <summary>Sample element.</summary>
        public LocalDate? LocalDateElement { get; set; }

        /// <summary>Sample element.</summary>
        public LocalTime? LocalTimeElement { get; set; }

        /// <summary>Sample element.</summary>
        public LocalMinute? LocalMinuteElement { get; set; }

        /// <summary>Sample element.</summary>
        public LocalDateTime? LocalDateTimeElement { get; set; }

        /// <summary>Sample element.</summary>
        public Instant? InstantElement { get; set; }

        /// <summary>Sample element.</summary>
        public SampleEnum EnumValue { get; set; }

        /// <summary>Sample element.</summary>
        public int? Version { get; set; }

        //--- METHODS

        /// <summary>Non-virtual handler defined in base type.</summary>
        [HandlerMethod]
        public void NonVirtualBaseHandler()
        {
            Context.Log.Verify("NonVirtualBaseHandler");
        }

        /// <summary>Virtual handler defined in base type.</summary>
        [HandlerMethod]
        public virtual void VirtualBaseHandler()
        {
            Context.Log.Verify("VirtualBaseHandler implemented in Base");
        }

        /// <summary>Viewer with default view name.</summary>
        [ViewerMethod]
        public void DefaultNamedViewer()
        {
            var viewData = new ViewSample();
            viewData.RecordId = Id;
            viewData.ViewName = "DefaultNamedViewer";
            viewData.SampleViewString = "Default named viewer";
            Context.SaveOne(viewData);
        }

        /// <summary>Viewer with a custom view name.</summary>
        [ViewerMethod("CustomName")]
        public void CustomNamedViewer()
        {
            var viewData = new ViewSample();
            viewData.RecordId = Id;
            viewData.ViewName = "CustomName";
            viewData.SampleViewString = "Custom named viewer";
            Context.SaveOne(viewData);
        }
    }
}
