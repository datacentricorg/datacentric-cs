/*
Copyright (C) 2013-present The DataCentric Authors.
Copyright (C) 2010-present MongoDB Inc.

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
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using MongoDB.Bson; // TODO - remove the remaining use of MongoDB so TemporalId is fully portable
using NodaTime;

namespace DataCentric
{
    /// <summary>
    /// A 16-byte, unique, ordered identifier that consists of two Int64
    /// elements:
    ///
    /// * First element representing creation time recorded as the number
    ///   of ticks since the Unix epoch (1970).
    /// * Second element is randomized with the inclusion of machine-specific
    ///   information to make the identifier unique in combination with the
    ///   first element.
    ///
    /// TemporalId has the same size as GUID and can be stored in a
    /// data type designed for GUID.
    ///
    /// TemporalId is unique in the absence of collision for its randomized
    /// part, which by design has extremely low probability (similar to GUID).
    /// It has the following ordering guarantees:
    ///
    /// * When generated in the same process, TemporalIds are strictly ordered
    ///   irrespective of how fast they are generated.
    /// * When generated independently, TemporalIds are ordered if generated
    ///   more than one operating system clock event apart. While the
    ///   underlying tick data type has 100ns resolution, the operating
    ///   system clock more typically has one event per 10-20 ms.
    ///
    /// Because TemporalId does not rely on auto-incremented database field,
    /// it can be used with distributed, web scale databases where getting
    /// a strictly increasing auto-incremented identifier would cause a
    /// performance hit.
    /// </summary>
    public struct TemporalId : IComparable<TemporalId>, IEquatable<TemporalId>
    {
        private static readonly int __staticMachine = (GetMachineHash() + GetAppDomainId()) & 0x00ffffff;
        private static readonly short __staticPid = GetPid();
        private static int __staticIncrement = (new Random()).Next();

        private readonly int _a;
        private readonly int _b;
        private readonly int _c;

        //--- PROPERTIES

        /// <summary>Empty value.</summary>
        public static TemporalId Empty { get; } = default(TemporalId);

        /// <summary>Creation time of the current object.</summary>
        public Instant CreatedTime
        {
            get => Instant.FromUnixTimeSeconds(_a);
        }

        //--- CONSTRUCTORS

        /// <summary>Create from a byte array of size 12.</summary>
        public TemporalId(byte[] bytes)
        {
            if (bytes == null || bytes.Length != 12)
                throw new Exception($"Bytes array passed to TemporalId ctor must be 12 bytes long.");

            _a = (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
            _b = (bytes[4] << 24) | (bytes[5] << 16) | (bytes[6] << 8) | bytes[7];
            _c = (bytes[8] << 24) | (bytes[9] << 16) | (bytes[10] << 8) | bytes[11];
        }

        /// <summary>Create from datetime in UTC and the remaining bytes.</summary>
        public TemporalId(Instant createdTime, byte[] remainingBytes)
        {
            long secondsSinceEpoch = createdTime.ToUnixTimeSeconds();
            if (secondsSinceEpoch < int.MinValue || secondsSinceEpoch > int.MaxValue)
                throw new Exception(
                    $"CreatedTime={createdTime} is out of range that can be represented by TemporalId.");

            if (remainingBytes == null || remainingBytes.Length != 8)
                throw new Exception(
                    $"Remaining bytes array passed to TemporalId ctor must be 8 bytes long.");

            _a = (int) secondsSinceEpoch;
            _b = (remainingBytes[0] << 24) | (remainingBytes[1] << 16) | (remainingBytes[2] << 8) | remainingBytes[3];
            _c = (remainingBytes[4] << 24) | (remainingBytes[5] << 16) | (remainingBytes[6] << 8) | remainingBytes[7];
        }

        /// <summary>Create from datetime in UTC and randomization parameters.</summary>
        public TemporalId(Instant createdTime, int machine, short pid, int increment)
        {
            long secondsSinceEpoch = createdTime.ToUnixTimeSeconds();
            if (secondsSinceEpoch < int.MinValue || secondsSinceEpoch > int.MaxValue)
                throw new Exception(
                    $"CreatedTime={createdTime} is out of range that can be represented by TemporalId.");

            if ((machine & 0xff000000) != 0) throw new Exception($"The machine value {machine} must be between 0 and 16777215 (it must fit in 3 bytes).");
            if ((increment & 0xff000000) != 0) throw new Exception($"The increment value {increment} must be between 0 and 16777215 (it must fit in 3 bytes).");

            _a = (int) secondsSinceEpoch;
            _b = (machine << 8) | (((int)pid >> 8) & 0xff);
            _c = ((int)pid << 24) | increment;
        }

        /// <summary>Compares this TemporalId to another TemporalId.</summary>
        public int CompareTo(TemporalId other)
        {
            int result = ((uint)_a).CompareTo((uint)other._a);
            if (result != 0) { return result; }
            result = ((uint)_b).CompareTo((uint)other._b);
            if (result != 0) { return result; }
            return ((uint)_c).CompareTo((uint)other._c);
        }

        /// <summary>True if the two TemporalIds are equal.</summary>
        public bool Equals(TemporalId rhs)
        {
            return
                _a == rhs._a &&
                _b == rhs._b &&
                _c == rhs._c;
        }

        /// <summary>True if the other object is an TemporalId and equal to this one.</summary>
        public override bool Equals(object obj)
        {
            if (obj is TemporalId)
            {
                return Equals((TemporalId)obj);
            }
            else
            {
                return false;
            }
        }

        /// <summary>Gets the hash code.</summary>
        public override int GetHashCode()
        {
            int hash = 17;
            hash = 37 * hash + _a.GetHashCode();
            hash = 37 * hash + _b.GetHashCode();
            hash = 37 * hash + _c.GetHashCode();
            return hash;
        }

        /// <summary>Converts the TemporalId to a byte array of length 12.</summary>
        public byte[] ToByteArray()
        {
            var bytes = new byte[12];
            bytes[0] = (byte)(_a >> 24);
            bytes[1] = (byte)(_a >> 16);
            bytes[2] = (byte)(_a >> 8);
            bytes[3] = (byte)(_a);
            bytes[4] = (byte)(_b >> 24);
            bytes[5] = (byte)(_b >> 16);
            bytes[6] = (byte)(_b >> 8);
            bytes[7] = (byte)(_b);
            bytes[8] = (byte)(_c >> 24);
            bytes[9] = (byte)(_c >> 16);
            bytes[10] = (byte)(_c >> 8);
            bytes[11] = (byte)(_c);
            return bytes;
        }

        /// <summary>
        /// Returns a string representation of the value using the following format:
        ///
        /// yyyy-mm-dd hh:mm:ss bytes[16]
        /// </summary>
        public override string ToString()
        {
            // First part of the serialized value is the timestamp
            // serialized using fixed width method to millisecond precision
            // in UTC timezone, where milliseconds are included even if the
            // time falls on a second.
            string createdTimeString = this.CreatedTime.ToFixedWidthIsoString();

            // Second part of the serialized value is 16 byte hexadecimal string
            var c = new char[16];
            c[0] = BsonUtils.ToHexChar((_b >> 28) & 0x0f);
            c[1] = BsonUtils.ToHexChar((_b >> 24) & 0x0f);
            c[2] = BsonUtils.ToHexChar((_b >> 20) & 0x0f);
            c[3] = BsonUtils.ToHexChar((_b >> 16) & 0x0f);
            c[4] = BsonUtils.ToHexChar((_b >> 12) & 0x0f);
            c[5] = BsonUtils.ToHexChar((_b >> 8) & 0x0f);
            c[6] = BsonUtils.ToHexChar((_b >> 4) & 0x0f);
            c[7] = BsonUtils.ToHexChar(_b & 0x0f);
            c[8] = BsonUtils.ToHexChar((_c >> 28) & 0x0f);
            c[9] = BsonUtils.ToHexChar((_c >> 24) & 0x0f);
            c[10] = BsonUtils.ToHexChar((_c >> 20) & 0x0f);
            c[11] = BsonUtils.ToHexChar((_c >> 16) & 0x0f);
            c[12] = BsonUtils.ToHexChar((_c >> 12) & 0x0f);
            c[13] = BsonUtils.ToHexChar((_c >> 8) & 0x0f);
            c[14] = BsonUtils.ToHexChar((_c >> 4) & 0x0f);
            c[15] = BsonUtils.ToHexChar(_c & 0x0f);
            string hex = new string(c);

            // TemporalId is serialized using the following format:
            //
            // yyyy-mm-ddThh:mm:ss.fffZhhhhhhhhhhhhhhhh
            //
            // where h represents a hexadecimal digit (total of 16)
            string result = createdTimeString + hex;
            return result;
        }

        //--- STATIC

        /// <summary>Generates a new TemporalId with a unique value.</summary>
        public static TemporalId Next()
        {
            Instant createdTime = DateTime.UtcNow.ToInstant();

            // Only use low order 3 bytes
            int increment = Interlocked.Increment(ref __staticIncrement) & 0x00ffffff;
            return new TemporalId(createdTime, __staticMachine, __staticPid, increment);
        }

        /// <summary>
        /// Parses a string and creates a new TemporalId.
        ///
        /// TemporalId is serialized using the following format:
        ///
        /// yyyy-mm-ddThh:mm:ss.fffZhhhhhhhhhhhhhhhh
        ///
        /// where each h represents a hexadecimal digit (total of 16).
        /// </summary>
        public static TemporalId Parse(string value)
        {
            TemporalId result;
            if (TryParse(value, out result))
            {
                return result;
            }
            else
            { 
                throw new Exception(
                    $"TemporalId={value} does not consist of ISO timestamp in UTC (Z) " +
                    $"timezone followed by hexadecimal string of length 16.");
            }
        }

        /// <summary>
        /// Tries to parse a string and create a new TemporalId.
        ///
        /// TemporalId is serialized using the following format:
        ///
        /// yyyy-mm-ddThh:mm:ss.fffZhhhhhhhhhhhhhhhh
        ///
        /// where each h represents a hexadecimal digit (total of 16).
        /// </summary>
        public static bool TryParse(string value, out TemporalId result)
        {
            // Return empty TemporalId for null or empty string
            if (string.IsNullOrEmpty(value))
            {
                result = TemporalId.Empty;
                return true;
            }

            // Set to empty value in case the method exits early
            result = default(TemporalId);

            // TemporalId is serialized using the following format:
            //
            // yyyy-mm-ddThh:mm:ss.fffZhhhhhhhhhhhhhhhh
            //
            // where each h represents a hexadecimal digit (total of 16)
            //
            // Exit if the string length is not 37
            if (value.Length != 40) return false;

            // Parse creation time using strict format yyyy-mm-ddThh:mm:ss
            Instant createdTime;
            string createdTimeString = value.Substring(0, 24);
            if (!InstantUtil.TryParse(createdTimeString, out createdTime))
            {
                return false;
            }

            // Try to parse the third token to a byte array
            byte[] bytes;
            string bytesString = value.Substring(24, 16);
            if (!BsonUtils.TryParseHexString(bytesString, out bytes)) return false;

            // Populate the first integer from timestamp
            // and the two remaining integers from the byte array
            result = new TemporalId(createdTime, bytes);
            return true;
        }

        /// <summary>
        /// The smallest value of TemporalId possible for
        /// a given creation time.
        ///
        /// Any TemporalId with the same creation time will
        /// be greater than this value.
        /// </summary>
        public static TemporalId FromCreatedTime(Instant createdTime)
        {
            return new TemporalId(createdTime, 0, 0, 0);
        }

        /// <summary>
        /// Returns the minimum of two arguments.
        ///
        /// This method does not consider Record.Empty and null
        /// to be equivalent:
        ///
        /// * Null is treated as missing value, the method returns
        ///   the other value.
        /// * TemporalId.Empty is treated as being less than any
        ///   other argument.
        ///
        /// Returns null if both arguments are null.
        /// </summary>
        public static TemporalId? Min(TemporalId? arg1, TemporalId? arg2)
        {
            if (arg1 != null && arg2 != null)
            {
                // Neither is null, returns the smaller value
                if (arg1 < arg2) return arg1;
                else return arg2;
            }
            else if (arg1 == null)
            {
                // Also covers the case when both are null
                return arg2;
            }
            else
            {
                return arg1;
            }
        }

        /// <summary>
        /// Returns the maximum of two arguments.
        ///
        /// This method does not consider Record.Empty and null
        /// to be equivalent:
        ///
        /// * Null is treated as missing value, the method returns
        ///   the other value.
        /// * TemporalId.Empty is treated as being less than any
        ///   other argument.
        ///
        /// Returns null if both arguments are null.
        /// </summary>
        public static TemporalId? Max(TemporalId? arg1, TemporalId? arg2)
        {
            if (arg1 != null && arg2 != null)
            {
                // Neither is null, returns the smaller value
                if (arg1 > arg2) return arg1;
                else return arg2;
            }
            else if (arg1 == null)
            {
                // Also covers the case when both are null
                return arg2;
            }
            else
            {
                return arg1;
            }
        }

        //--- OPERATORS

        /// <summary>True if the first TemporalId is less than the second TemporalId.</summary>
        public static bool operator <(TemporalId lhs, TemporalId rhs)
        {
            return lhs.CompareTo(rhs) < 0;
        }

        /// <summary>True if the first TemporalId is less than or equal to the second TemporalId.</summary>
        public static bool operator <=(TemporalId lhs, TemporalId rhs)
        {
            return lhs.CompareTo(rhs) <= 0;
        }

        /// <summary>True if the two TemporalIds are equal.</summary>
        public static bool operator ==(TemporalId lhs, TemporalId rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>True if the two TemporalIds are not equal.</summary>
        public static bool operator !=(TemporalId lhs, TemporalId rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>True if the first TemporalId is greater than or equal to the second TemporalId.</summary>
        public static bool operator >=(TemporalId lhs, TemporalId rhs)
        {
            return lhs.CompareTo(rhs) >= 0;
        }

        /// <summary>True if the first TemporalId is greater than the second TemporalId.</summary>
        public static bool operator >(TemporalId lhs, TemporalId rhs)
        {
            return lhs.CompareTo(rhs) > 0;
        }

        //--- PRIVATE

        // private static methods
        private static int GetAppDomainId()
        {
#if NETSTANDARD1_5 || NETSTANDARD1_6
            return 1;
#else
            return AppDomain.CurrentDomain.Id;
#endif
        }

        /// <summary>
        /// Gets the current process id.  This method exists because of how CAS operates on the call stack, checking
        /// for permissions before executing the method.  Hence, if we inlined this call, the calling method would not execute
        /// before throwing an exception requiring the try/catch at an even higher level that we don't necessarily control.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetCurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }

        private static int GetMachineHash()
        {
            // Use instead of Dns.HostName so it will work offline.
            // Use first 3 bytes of hash.
            return 0x00ffffff & Environment.MachineName.GetHashCode(); 
        }

        private static short GetPid()
        {
            try
            {
                // Use low order two bytes only
                return (short)GetCurrentProcessId();
            }
            catch (SecurityException)
            {
                return 0;
            }
        }

        private static void FromByteArray(byte[] bytes, int offset, out int a, out int b, out int c)
        {
            a = (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
            b = (bytes[offset + 4] << 24) | (bytes[offset + 5] << 16) | (bytes[offset + 6] << 8) | bytes[offset + 7];
            c = (bytes[offset + 8] << 24) | (bytes[offset + 9] << 16) | (bytes[offset + 10] << 8) | bytes[offset + 11];
        }
    }

    /// <summary>Extension methods for TemporalId.</summary>
    public static class TemporalIdExtensions
    {
        /// <summary>Return false if equal to the default constructed value.</summary>
        public static bool HasValue(this TemporalId value)
        {
            return value != default;
        }

        /// <summary>Return false if null or equal to the default constructed value.</summary>
        public static bool HasValue(this TemporalId? value)
        {
            return value.HasValue && value.HasValue();
        }

        /// <summary>Error message if equal to the default constructed value.</summary>
        public static void CheckHasValue(this TemporalId value)
        {
            if (!value.HasValue()) throw new Exception("Required TemporalId value is not set.");
        }

        /// <summary>Error message if null or equal to the default constructed value.</summary>
        public static void CheckHasValue(this TemporalId? value)
        {
            if (!value.HasValue()) throw new Exception("Required TemporalId value is not set.");
        }
    }
}
