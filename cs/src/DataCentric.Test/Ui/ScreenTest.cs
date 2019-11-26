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
using Xunit;
using DataCentric;
using System.Collections.Generic;

namespace DataCentric.Test
{
    /// <summary>Unit tests for Screen.</summary>
    public class ScreenTest : UnitTest
    {
        [Fact]
        public void Smoke()
        {
            using (var context = CreateMethodContext())
            {
                context.KeepTestData();

                // Create screen instance
                Screen screenSample = GetScreenSample();
                screenSample.ScreenName = "CustomScreen";
                context.SaveOne(screenSample);

                // Check for the results
                var loadedScreenSample = context
                    .Load(new ScreenKey { ScreenName = screenSample.ScreenName })
                    .CastTo<ScreenSample>();

                context.Log.Verify(loadedScreenSample.SampleScreenString);
            }
        }

        public Screen GetScreenSample()
        {
            return new ScreenSample()
            {
                Content = new List<UiItem>() {
                    new UiItem()
                    {
                        Type = UiItemType.Row,
                        Height = null,
                        Width = null,
                        Resizable = true,
                        Content = new List<UiItem>()
                        {
                            new UiItem()
                            {
                                Type = UiItemType.Column,
                                Height = null,
                                Width = 61.575178997613364,
                                Resizable = true,
                                Content = new List<UiItem>()
                                {
                                    new UiItem()
                                    {
                                        Type = UiItemType.Container,
                                        Height = 37.127071823204425,
                                        Width = null,
                                        Resizable = true
                                    },
                                    new UiItem()
                                    {
                                        Type = UiItemType.Container,
                                        Height = 62.872928176795575,
                                        Width = null,
                                        Resizable = true
                                    }
                                }
                            },
                            new UiItem()
                            {
                                Type = UiItemType.Container,
                                Height = null,
                                Width = 38.424821002386636,
                                Resizable = true
                            }
                        }
                    }
                }
            };
        }
    }
}
