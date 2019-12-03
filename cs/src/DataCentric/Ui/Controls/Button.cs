﻿/*
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
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>
    /// Data associated with a button UI control.
    ///
    /// The method Action() is invoked when the
    /// button is pressed.
    /// </summary>
    public class Button : Control
    {
        /// <summary>
        /// Text is displayed on the button.
        /// </summary>
        [BsonRequired]
        public string Text { get; set; }

        /// <summary>
        /// Method invoked when the button is pressed.
        /// </summary>
        [HandlerMethod]
        public void Action()
        {
            // Base action does nothing. This functionality should be implemented in derived classes.
        }
    }
}
