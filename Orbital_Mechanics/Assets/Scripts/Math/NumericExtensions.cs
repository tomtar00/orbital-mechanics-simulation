using System;
using UnityEngine;

namespace Sim.Math
{
    public static class NumericExtensions
    {
        public static float SafeDivision(this float Numerator, float Denominator)
        {
            return (Denominator == 0) ? 0 : Numerator / Denominator;
        }
        public static double SafeDivision(this double Numerator, double Denominator)
        {
            return (Denominator == 0) ? 0 : Numerator / Denominator;
        }

        public static Vector3 SafeDivision(this Vector3 Numerator, float Denominator)
        {
            return (Denominator == 0) ? Vector3.zero : Numerator / Denominator;
        }
    }

}