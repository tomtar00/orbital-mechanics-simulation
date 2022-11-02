using System;
using UnityEngine;

namespace Sim.Math
{
    public static class NumericExtensions
    {
        public static double SafeDivision(this double Numerator, double Denominator)
        {
            return (Denominator == 0) ? 0 : Numerator / Denominator;
        }

        public static Vector3Double SafeDivision(this Vector3Double Numerator, double Denominator)
        {
            return (Denominator == 0) ? Vector3Double.zero : Numerator / Denominator;
        }

        public static string ToTimeSpan(this double seconds) {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return string.Format("{0:D2}d:{1:D2}h:{2:D2}m:{3:D2}s", 
                t.Days,
                t.Hours, 
                t.Minutes, 
                t.Seconds);
        }

        public static string Precise(this Vector3 vec) {
            return $"({vec.x}, {vec.y}, {vec.z})";
        }

        public static Vector3 ScaleWithDistance(Vector3 pos1, Vector3 pos2, float multiplier, float minScale, float maxScale) {
            float distance = Vector3.Distance(pos1, pos2);
            distance = Mathf.Clamp(distance, minScale, maxScale);
            return Vector3.one * distance * multiplier;
        }
    }

}