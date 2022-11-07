using UnityEngine;
using System;

namespace Sim.Math
{
    public static class MathLib
    {
        public const double Rad2Deg = 57.29577951308232;
        public const double Deg2Rad = 0.017453292519943295;
        public const double PI = 3.141592653589793;

        public static double EnsureFunctionConditions(Func<double, double> func, double param, bool clampInDomain = false, double domainStart = double.MinValue, double domainEnd = double.MaxValue)
        {
            if (double.IsNaN(param))
                throw new ArgumentException("Parameter is NaN!");

            if (param < domainStart || param > domainEnd)
            {
                if (clampInDomain)
                {
                    var clamp = Clamp(param, domainStart, domainEnd);
                    // Debug.Log($"Need to clamp param in domain: {param} -> {clamp}");
                    param = clamp;
                }
                else throw new ArgumentOutOfRangeException($"Parameter out of function domain! Passed: {param}");
            }

            double result = func(param);
            if (double.IsNaN(result))
                throw new ArithmeticException("Result of function is NaN!");
            return result;
        }

        public static double EnsureFunctionConditions2(Func<double, double, double> func, double param1, double param2, bool clampInDomain = false, double domainStart = double.MinValue, double domainEnd = double.MaxValue)
        {
            if (double.IsNaN(param1))
                throw new ArgumentException("Parameter 1 is NaN!");
            if (double.IsNaN(param2))
                throw new ArgumentException("Parameter 2 is NaN!");

            if (param1 < domainStart || param1 > domainEnd)
            {
                if (clampInDomain)
                {
                    var clamp = Clamp(param1, domainStart, domainEnd);
                    Debug.Log($"Need to clamp param 1 in domain: {param1} -> {clamp}");
                    param1 = clamp;
                }
                else throw new ArgumentOutOfRangeException($"Parameter 1 out of function domain! Passed: {param1}");
            }
            if (param2 < domainStart || param2 > domainEnd)
            {
                if (clampInDomain)
                {
                    var clamp = Clamp(param2, domainStart, domainEnd);
                    Debug.Log($"Need to clamp param 2 in domain: {param2} -> {clamp}");
                    param2 = clamp;
                }
                else throw new ArgumentOutOfRangeException($"Parameter 2 out of function domain! Passed: {param2}");
            }

            double result = func(param1, param2);
            if (double.IsNaN(result))
                throw new ArithmeticException("Result of function is NaN!");
            return result;
        }

        public static double Clamp(double value, double min, double max) {
            if (value > max) value = max;
            else if (value < min) value = min;
            return value;
        }
        public static double Min(double a, double b) {
            return a < b ? a : b;
        }
        public static double Max(double a, double b) {
            return a > b ? a : b;
        }
        public static double Repeat(double value, double length) {
            double result = value % length;
            return result < 0 ? result + length : result;
        }

        public static double Sin(double x)
        {
            return EnsureFunctionConditions((a) => System.Math.Sin(a), x);
        }
        public static double Cos(double x)
        {
            return EnsureFunctionConditions((a) => System.Math.Cos(a), x);
        }
        public static double Tan(double x)
        {
            return EnsureFunctionConditions((a) => System.Math.Tan(a), x);
        }
        public static double Sinh(double x)
        {
            return EnsureFunctionConditions((a) => System.Math.Sinh(a), x);
        }
        public static double Cosh(double x)
        {
            return EnsureFunctionConditions((a) => System.Math.Cosh(a), x);
        }
        public static double Tanh(double x)
        {
            return EnsureFunctionConditions((double a) => (double)System.Math.Tanh(a), x);
        }

        public static double Asin(double x)
        {
            return EnsureFunctionConditions((a) => System.Math.Asin(a), x, true, -1f, 1f);
        }
        public static double Acos(double x)
        {
            return EnsureFunctionConditions((a) => System.Math.Acos(a), x, true, -1f, 1f);
        }
        public static double Atan(double x)
        {
            return EnsureFunctionConditions((a) => System.Math.Atan(a), x);
        }
        public static double Atan2(double y, double x)
        {
            return EnsureFunctionConditions2((a, b) => System.Math.Atan2(a, b), y, x);
        }
        public static double Atanh(double x)
        {
            return EnsureFunctionConditions((a) => (System.Math.Log(1 + a) - System.Math.Log(1 - a)) / 2, x, true, -.99f, .99f);
        }
        public static double Asinh(double x)
        {
            return EnsureFunctionConditions((a) => System.Math.Log(a + System.Math.Sqrt(a * a + 1)), x, true);
        }

        public static double Sqrt(double x)
        {
            return EnsureFunctionConditions((a) => System.Math.Sqrt(a), x, true, 0);
        }
        public static double Pow(double x, double y)
        {
            return EnsureFunctionConditions2((a, b) => System.Math.Pow(a, b), x, y);
        }
        public static double Abs(double x)
        {
            return EnsureFunctionConditions((a) => System.Math.Abs(a), x);
        }

    }
}
