using UnityEngine;

namespace Sim.Math
{
    public class Vector3Double
    {
        public double x { get; private set; }
        public double y { get; private set; }
        public double z { get; private set; }

        public double sqrMagnitude {
            get => x*x + y*y + z*z;
        }
        public double magnitude {
            get => MathLib.Sqrt(sqrMagnitude);
        }
        public Vector3Double normalized {
            get => this / magnitude;
        }

        public static Vector3Double zero {
            get => new Vector3Double(0, 0, 0);
        }
        public static Vector3Double one {
            get => new Vector3Double(1, 1, 1);
        }
        public static Vector3Double right {
            get => new Vector3Double(1, 0, 0);
        }
        public static Vector3Double up {
            get => new Vector3Double(0, 1, 0);
        }
        public static Vector3Double forward {
            get => new Vector3Double(0, 0, 1);
        }

        public Vector3Double() {
            this.x = this.y = this.z = 0;
        }

        public Vector3Double(double x, double y, double z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static double Dot(Vector3Double a, Vector3Double b) {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static Vector3Double Cross(Vector3Double a, Vector3Double b) {
            return Vector3Double.right * (a.y*b.z-a.z*b.y) - Vector3Double.up * (a.x*b.z-a.z*b.x) + Vector3Double.forward * (a.x*b.y-a.y*b.x);
        }

        public static double Distance(Vector3Double a, Vector3Double b) {
            return MathLib.Sqrt(MathLib.Pow(b.x - a.x, 2) + MathLib.Pow(b.y - a.y, 2) + MathLib.Pow(b.z - a.z, 2));
        }

        public static double Angle(Vector3Double a, Vector3Double b) {
            return MathLib.Acos(Vector3Double.Dot(a, b) / (a.magnitude * b.magnitude)) * MathLib.Rad2Deg;
        }
        public static double SignedAngle(Vector3Double a, Vector3Double b, Vector3Double axis) {
            axis = axis.normalized;
            var det = a.x*b.y*axis.z + b.x*axis.y*a.z + axis.x*a.y*b.z - a.z*b.y*axis.x - b.z*axis.y*a.x - axis.z*a.y*b.x;
            return MathLib.Atan2(det, Vector3Double.Dot(a, b)) * MathLib.Rad2Deg;
        }

        public static Vector3Double operator / (Vector3Double vec, double value) {
            return new Vector3Double(vec.x / value, vec.y / value, vec.z / value);
        }
        public static Vector3Double operator * (Vector3Double vec, double value) {
            return new Vector3Double(vec.x * value, vec.y * value, vec.z * value);
        }
        public static Vector3Double operator + (Vector3Double vec1, Vector3Double vec2) {
            return new Vector3Double(vec1.x + vec2.x, vec1.y + vec2.y, vec1.z + vec2.z);
        }
        public static Vector3Double operator - (Vector3Double vec1, Vector3Double vec2) {
            return new Vector3Double(vec1.x - vec2.x, vec1.y - vec2.y, vec1.z - vec2.z);
        }

        public static Vector3Double operator + (Vector3Double vec1, Vector3 vec2) {
            return new Vector3Double(vec1.x + vec2.x, vec1.y + vec2.y, vec1.z + vec2.z);
        }
        public static Vector3Double operator + (Vector3 vec1, Vector3Double vec2) {
            return new Vector3Double(vec1.x + vec2.x, vec1.y + vec2.y, vec1.z + vec2.z);
        }
        public static Vector3Double operator - (Vector3Double vec1, Vector3 vec2) {
            return new Vector3Double(vec1.x - vec2.x, vec1.y - vec2.y, vec1.z - vec2.z);
        }
        public static Vector3Double operator - (Vector3 vec1, Vector3Double vec2) {
            return new Vector3Double(vec1.x - vec2.x, vec1.y - vec2.y, vec1.z - vec2.z);
        }

        public static Vector3Double operator - (Vector3Double vec) {
            return new Vector3Double(-vec.x, -vec.y, -vec.z);
        }

        public static implicit operator Vector3(Vector3Double vec) => new Vector3((float)vec.x, (float)vec.y, (float)vec.z);
        public static implicit operator Vector3Double(Vector3 vec) => new Vector3Double(vec.x, vec.y, vec.z);
    }
}
