using UnityEngine;
using System;

namespace Sim.Math
{
    public static class MathLib
    {
        public const float Rad2Deg = 57.29578f;
        public const float PI = 3.14159274f;

        public static float EnsureFunctionConditions(Func<float, float> func, float param, bool clampInDomain = false, float domainStart = float.MinValue, float domainEnd = float.MaxValue)
        {
            if (float.IsNaN(param))
                throw new ArgumentException("Parameter is NaN!");

            if (param < domainStart || param > domainEnd)
            {
                if (clampInDomain)
                {
                    var clamp = Mathf.Clamp(param, domainStart, domainEnd);
                    Debug.Log($"Need to clamp param in domain: {param} -> {clamp}");
                    param = clamp;
                }
                else throw new ArgumentOutOfRangeException($"Parameter out of function domain! Passed: {param}");
            }

            float result = func(param);
            if (float.IsNaN(result))
                throw new ArithmeticException("Result of function is NaN!");
            return result;
        }

        public static float EnsureFunctionConditions2(Func<float, float, float> func, float param1, float param2, bool clampInDomain = false, float domainStart = float.MinValue, float domainEnd = float.MaxValue)
        {
            if (float.IsNaN(param1))
                throw new ArgumentException("Parameter 1 is NaN!");
            if (float.IsNaN(param2))
                throw new ArgumentException("Parameter 2 is NaN!");

            if (param1 < domainStart || param1 > domainEnd)
            {
                if (clampInDomain)
                {
                    var clamp = Mathf.Clamp(param1, domainStart, domainEnd);
                    Debug.Log($"Need to clamp param 1 in domain: {param1} -> {clamp}");
                    param1 = clamp;
                }
                else throw new ArgumentOutOfRangeException($"Parameter 1 out of function domain! Passed: {param1}");
            }
            if (param2 < domainStart || param2 > domainEnd)
            {
                if (clampInDomain)
                {
                    var clamp = Mathf.Clamp(param2, domainStart, domainEnd);
                    Debug.Log($"Need to clamp param 2 in domain: {param2} -> {clamp}");
                    param2 = clamp;
                }
                else throw new ArgumentOutOfRangeException($"Parameter 2 out of function domain! Passed: {param2}");
            }

            float result = func(param1, param2);
            if (float.IsNaN(result))
                throw new ArithmeticException("Result of function is NaN!");
            return result;
        }

        public static float Sin(float x)
        {
            return EnsureFunctionConditions(Mathf.Sin, x);
        }
        public static float Cos(float x)
        {
            return EnsureFunctionConditions(Mathf.Cos, x);
        }
        public static float Tan(float x)
        {
            return EnsureFunctionConditions(Mathf.Tan, x);
        }
        public static float Sinh(float x)
        {
            return EnsureFunctionConditions((float a) => (float)System.Math.Sinh(a), x);
        }
        public static float Cosh(float x)
        {
            return EnsureFunctionConditions((float a) => (float)System.Math.Cosh(a), x);
        }
        public static float Tanh(float x)
        {
            return EnsureFunctionConditions((float a) => (float)System.Math.Tanh(a), x);
        }

        public static float Asin(float x)
        {
            return EnsureFunctionConditions(Mathf.Asin, x, true, -1f, 1f);
        }
        public static float Acos(float x)
        {
            return EnsureFunctionConditions(Mathf.Acos, x, true, -1f, 1f);
        }
        public static float Atan(float x)
        {
            return EnsureFunctionConditions(Mathf.Atan, x);
        }
        public static float Atan2(float y, float x)
        {
            return EnsureFunctionConditions2(Mathf.Atan2, y, x);
        }
        public static float Atanh(float x)
        {
            Func<float, float> Atanh = (float a) => (Mathf.Log(1 + a) - Mathf.Log(1 - a)) / 2;
            return EnsureFunctionConditions(Atanh, x, true, -1f, 1f);
        }

        public static float Sqrt(float x)
        {
            return EnsureFunctionConditions(Mathf.Sqrt, x, true, 0);
        }
        public static float Pow(float x, float y)
        {
            return EnsureFunctionConditions2(Mathf.Pow, x, y);
        }
        public static float Abs(float x)
        {
            return EnsureFunctionConditions(Mathf.Abs, x);
        }

    }
}
