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

        public static string ToTimeSpan(this float seconds) {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return string.Format("{0:D2}d:{1:D2}h:{2:D2}m:{3:D2}s", 
                t.Days,
                t.Hours, 
                t.Minutes, 
                t.Seconds);
        }

        public static Vector3 ScaleWithDistance(Vector3 pos1, Vector3 pos2, float multiplier, float minScale, float maxScale) {
            float distance = Vector3.Distance(pos1, pos2);
            distance = Mathf.Clamp(distance, minScale, maxScale);
            return Vector3.one * distance * multiplier;
        }
        public static float FitBetween0And2PI(float number) {
            float result = number;
            float PI2 = 2 * Mathf.PI;

            while(result > PI2)
                result -= PI2;
            while(result < 0)
                result += PI2;

            return result;
        }
    }

}