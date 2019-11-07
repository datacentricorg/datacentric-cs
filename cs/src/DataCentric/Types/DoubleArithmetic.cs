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
    /// <summary>Function of one double argument returning one double value.</summary>
    public delegate double DoubleUnaryOperator(double arg);

    /// <summary>Function of two double arguments returning one double value.</summary>
    public delegate double DoubleBinaryOperator(double firstArg, double secondArg);

    /// <summary>
    /// Abstract base class providing support for arithmetic operations
    /// for objects of type T and their combination with double values.
    /// </summary>
    public abstract class DoubleArithmetic<T>
        where T : DoubleArithmetic<T>
    {
        /// <summary>Create in locked state.</summary>
        protected DoubleArithmetic() { }

        /// <summary>Return self as reference to generic parameter type T.</summary>
        public abstract T Self();

        /// <summary>Return object of type T representing zero
        /// and suitable for arithmetic operations with self.</summary>
        public abstract T Zero();

        /// <summary>Returns the result of applying the
        /// specified function of one variable (unary operator) to self.</summary>
        public abstract T Apply(DoubleUnaryOperator unaryFunc);

        /// <summary>Returns the result of applying the specified
        /// function of two variables (binary operator) to two objects of type T.
        /// The first argument is self, the second is passed to the function.</summary>
        public abstract T Apply(DoubleBinaryOperator binaryFunc, T secondArg);

        /// <summary>Returns the result of applying the specified
        /// function of two variables (binary operator) to an object of type T and a double.
        /// The first argument is self, the second (double) is passed to the function.</summary>
        public abstract T Apply(DoubleBinaryOperator binaryFunc, double secondArg);

        /// <summary>Returns the result of applying the specified
        /// function of two variables (binary operator) to a double and an object of type T.
        /// The first argument is passed to the function, the second argument is self.</summary>
        public abstract T Apply(double firstArg, DoubleBinaryOperator binaryFunc);

        /* Member functions with type T argument */

        /// <summary>Returns the result of adding the argument to self (null rhs is treated as zero).</summary>
        public T Add(T rhs) { return rhs == null ? Self() : Apply((x, y) => x + y, rhs); }

        /// <summary>Returns the result of subtracting the argument from self  (null rhs is treated as zero).</summary>
        public T Subtract(T rhs) { return rhs == null ? Self() : Apply((x, y) => x - y, rhs); }

        /// <summary>Returns the result of multiplying self by the argument (null rhs is treated as zero).</summary>
        public T MultiplyBy(T rhs) { return rhs == null ? null : Apply((x, y) => x * y, rhs); }

        /// <summary>Returns the result of dividing self by the argument (null rhs is treated as zero).</summary>
        public T DivideBy(T rhs)
        {
            if(rhs == null) throw new Exception("Division by null object which represents zero.");
            return Apply((x, y) => x / y, rhs);
        }

        /// <summary>Returns the result of taking the maximum of argument and self (null rhs is treated as zero).</summary>
        public T MaxWith(T rhs) { return Apply(Math.Max, rhs ?? Zero()); }

        /// <summary>Returns the result of taking the minimum of argument and self (null rhs is treated as zero).</summary>
        public T MinWith(T rhs) { return Apply(Math.Min, rhs ?? Zero()); }

        /* Member functions with double argument */

        /// <summary>Returns the result of adding the argument to self.</summary>
        public T Add(double rhs) { return Apply((x, y) => x + y, rhs); }

        /// <summary>Returns the result of subtracting the argument from self.</summary>
        public T Subtract(double rhs) { return Apply((x, y) => x - y, rhs); }

        /// <summary>Returns the result of multiplying self by the argument.</summary>
        public T MultiplyBy(double rhs) { return Apply((x, y) => x * y, rhs); }

        /// <summary>Returns the result of dividing self by the argument.</summary>
        public T DivideBy(double rhs) { return Apply((x, y) => x / y, rhs); }

        /// <summary>Returns the result of taking the maximum of argument and self.</summary>
        public T MaxWith(double rhs) { return Apply(Math.Max, rhs); }

        /// <summary>Returns the result of taking the minimum of argument and self.</summary>
        public T MinWith(double rhs) { return Apply(Math.Min, rhs); }

        /// <summary>Returns the result of taking the specified power of self.</summary>
        public T Pow(double rhs) { return Apply(Math.Pow, rhs); }

        /* Static functions with type T argument */

        /// <summary>Returns the result of taking the exponent of object.</summary>
        public static T Exp(T arg)
        {
            if (arg == null) throw new Exception("Cannot accept null argument as it contains no information to create the result.");
            return arg.Apply(Math.Exp);
        }

        /// <summary>Returns the result of taking the natural log of object.</summary>
        public static T Log(T arg)
        {
            if (arg == null) throw new Exception("Attempt to take log of zero represented as null object.");
            return arg.Apply(Math.Log);
        }

        /// <summary>Returns the result of taking the maximum of two objects (null is treated as zero).</summary>
        public static T Max(T lhs, T rhs)
        {
            if (lhs == null) return rhs == null ? null : rhs.MaxWith(rhs.Zero());
            else return lhs.MaxWith(rhs);
        }

        /// <summary>Returns the result of taking the minimum of two objects (null is treated as zero).</summary>
        public static T Min(T lhs, T rhs)
        {
            if (lhs == null) return rhs == null ? null : rhs.MinWith(rhs.Zero());
            else return lhs.MinWith(rhs);
        }

        /* Static functions with double argument */

        /// <summary>Returns the result of taking the maximum of object and double (null is treated as zero).</summary>
        public static T Max(T lhs, double rhs)
        {
            if (lhs == null) throw new Exception("Cannot accept null argument as it contains no information to create the result.");
            return lhs.MaxWith(rhs);
        }

        /// <summary>Returns the result of taking the minimum of object and double (null is treated as zero).</summary>
        public static T Min(T lhs, double rhs)
        {
            if (lhs == null) throw new Exception("Cannot accept null argument as it contains no information to create the result.");
            return lhs.MinWith(rhs);
        }

        /// <summary>Returns the result of taking the maximum of double and object (null is treated as zero).</summary>
        public static T Max(double lhs, T rhs)
        {
            if (rhs == null) throw new Exception("Cannot accept null argument as it contains no information to create the result.");
            return rhs.Apply(lhs, Math.Max);
        }

        /// <summary>Returns the result of taking the minimum of double and object (null is treated as zero).</summary>
        public static T Min(double lhs, T rhs)
        {
            if (rhs == null) throw new Exception("Cannot accept null argument as it contains no information to create the result.");
            return rhs.Apply(lhs, Math.Min);
        }

        /* Arithmetic operators with type T argument */

        /// <summary>Returns the result of changing the sign of the object (null is treated as zero).</summary>
        public static T operator -(DoubleArithmetic<T> arg) { return arg == null ? null : arg.MultiplyBy(-1.0); }

        /// <summary>Returns the result of addition of two objects (null is treated as zero).</summary>
        public static T operator +(DoubleArithmetic<T> lhs, T rhs)
        {
            if (lhs == null) return rhs;
            else if (rhs == null) return lhs.Self();
            else return lhs.Add(rhs);
        }

        /// <summary>Returns the result of subtraction of two objects (null is treated as zero).</summary>
        public static T operator -(DoubleArithmetic<T> lhs, T rhs)
        {
            if (lhs == null) return -rhs;
            else if (rhs == null) return lhs.Self();
            else return lhs.Subtract(rhs);
        }

        /// <summary>Returns the result of multiplication of two objects (null is treated as zero).</summary>
        public static T operator *(DoubleArithmetic<T> lhs, T rhs)
        {
            if (lhs == null || rhs == null) return null;
            else return lhs.MultiplyBy(rhs);
        }

        /// <summary>Returns the result of division of two objects (null is treated as zero).</summary>
        public static T operator /(DoubleArithmetic<T> lhs, T rhs)
        {
            if (rhs == null) throw new Exception("Division by null object which represents zero.");
            return lhs.DivideBy(rhs);
        }

        /* Arithmetic operators with double argument */

        /// <summary>Returns the result of addition of object and double.</summary>
        public static T operator +(DoubleArithmetic<T> lhs, double rhs)
        {
            if (lhs == null && DoubleUtil.Equal(rhs, 0.0)) return (T)lhs;
            else if (lhs == null) throw new Exception("Cannot accept null argument as it contains no information to create the result.");
            return lhs.Add(rhs);
        }

        /// <summary>Returns the result of subtraction of object and double.</summary>
        public static T operator -(DoubleArithmetic<T> lhs, double rhs)
        {
            if (lhs == null && DoubleUtil.Equal(rhs, 0.0)) return (T)lhs;
            else if (lhs == null) throw new Exception("Cannot accept null argument as it contains no information to create the result.");
            return lhs.Subtract(rhs);
        }

        /// <summary>Returns the result of multiplication of object and double.</summary>
        public static T operator *(DoubleArithmetic<T> lhs, double rhs)
        {
            if (lhs == null) return null;
            return lhs.MultiplyBy(rhs);
        }

        /// <summary>Returns the result of division of object and double.</summary>
        public static T operator /(DoubleArithmetic<T> lhs, double rhs)
        {
            if (DoubleUtil.Equal(rhs, 0.0)) throw new Exception("Division of array by zero.");
            else if (lhs == null) return null;
            return lhs.DivideBy(rhs);
        }

        /// <summary>Returns the result of addition of double and object.</summary>
        public static T operator +(double lhs, DoubleArithmetic<T> rhs)
        {
            if (rhs == null && DoubleUtil.Equal(lhs, 0.0)) return (T)rhs;
            else if (rhs == null) throw new Exception("Cannot accept null argument as it contains no information to create the result.");
            return rhs.Apply(lhs, (x, y) => x + y);
        }

        /// <summary>Returns the result of subtraction of double and object.</summary>
        public static T operator -(double lhs, DoubleArithmetic<T> rhs)
        {
            if (rhs == null && DoubleUtil.Equal(lhs, 0.0)) return rhs.MultiplyBy(-1.0);
            else if (rhs == null) throw new Exception("Cannot accept null argument as it contains no information to create the result.");
            return rhs.Apply(lhs, (x, y) => x - y);
        }

        /// <summary>Returns the result of multiplication of double and object.</summary>
        public static T operator *(double lhs, DoubleArithmetic<T> rhs)
        {
            if (rhs == null) return null;
            return rhs.Apply(lhs, (x, y) => x * y);
        }

        /// <summary>Returns the result of division of double and object.</summary>
        public static T operator /(double lhs, DoubleArithmetic<T> rhs)
        {
            if (rhs == null && DoubleUtil.Equal(lhs, 0.0)) return null;
            else if (rhs == null) throw new Exception("Cannot accept null argument as it contains no information to create the result.");
            return rhs.Apply(lhs, (x, y) => x / y);
        }
    }
}
