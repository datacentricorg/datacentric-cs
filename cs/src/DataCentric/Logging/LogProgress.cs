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

namespace DataCentric
{
    /// <summary>Progress data is recorded in Context.Log under
    /// Progress.Ratio and Progress.Message entry types.</summary>
    public class LogProgress : Progress
    {
        private double ratio_;
        private string message_;

        //--- PROPERTIES

        /// <summary>Get or set progress ratio from 0 to 1 (0 if not set).</summary>
        public override double Ratio
        {
            get { return ratio_; }
            set
            {
                ratio_ = value;
                Context.Log.Publish(LogVerbosityEnum.Info, $"ProgressRatio={ratio_}");
            }
        }

        /// <summary>Get or set message displayed next to the progress ratio (null if not set).</summary>
        public override string Message
        {
            get { return message_; }
            set
            {
                message_ = value;
                Context.Log.Publish(LogVerbosityEnum.Info, $"ProgressMessage={message_}");
            }
        }

        //--- METHODS

        /// <summary>Flush data to permanent storage.</summary>
        public override void Flush()
        {
            Context.Log.Flush();
        }
    }
}
