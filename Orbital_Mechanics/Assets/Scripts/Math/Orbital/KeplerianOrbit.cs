using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    // Mostly based on: http://control.asu.edu/Classes/MAE462/462Lecture05.pdf
    public class KeplerianOrbit
    {
        public const float G = 6.67f;

        public Orbit orbit { get; set; }

        // Main keplerian orbital elements
        public float semimajorAxis { get; set; }
        public float eccentricity { get; set; }
        public float inclination { get; set; }
        public float lonAscNode { get; set; }
        public float argPeriapsis { get; set; }
        public float meanAnomaly { get; set; }

        // True anomaly / True longitude / Argument of latitude
        public float trueAnomaly { get; set; }
        // Eccentric anomaly / Parabolic anomaly / Hyperbolic anomaly
        public float anomaly { get; set; }

        // Other orbital elements
        public float semiminorAxis { get; set; }
        public float trueAnomalyConstant { get; set; }
        public float meanMotion { get; set; }
        public float semiLatusRectum { get; set; }

        // Supporting variables
        public float sinLonAcsNode { get; set; }
        public float cosLonAcsNode { get; set; }
        public float sinInclination { get; set; }
        public float cosInclination { get; set; }
        public float sinArgPeriapsis { get; set; }
        public float cosArgPeriapsis { get; set; }

        public KeplerianOrbit(OrbitType type, Celestial centralBody)
        {
            ChangeOrbitType(type, centralBody);
        }

        public void ChangeOrbitType(OrbitType type, Celestial centralBody)
        {
            switch (type)
            {
                case OrbitType.CIRCULAR:
                    orbit = new CircularOrbit(this, centralBody);
                    break;
                case OrbitType.ELLIPTIC:
                    orbit = new EllipticOrbit(this, centralBody);
                    break;
                case OrbitType.PARABOLIC:
                    orbit = new ParabolicOrbit(this, centralBody);
                    break;
                case OrbitType.HYPERBOLIC:
                    orbit = new HyperbolicOrbit(this, centralBody);
                    break;
            }
        }

        public void ChangeOrbitType(OrbitType type)
        {
            ChangeOrbitType(type, this.orbit.centralBody);
        }

        public void ApplyElementsFromStruct(Elements elements)
        {
            semimajorAxis = elements.semimajorAxis;
            eccentricity = elements.eccentricity;
            inclination = elements.inclination;
            lonAscNode = elements.lonAscNode;
            argPeriapsis = elements.argPeriapsis;
            meanAnomaly = elements.meanAnomaly;

            orbit.CalculateOtherElements();
        }
        [System.Serializable]
        public struct Elements
        {
            public float semimajorAxis;
            public float eccentricity;
            public float inclination;
            public float lonAscNode;
            public float argPeriapsis;
            public float meanAnomaly;
        }

    }

    public enum OrbitType
    {
        CIRCULAR,
        ELLIPTIC,
        PARABOLIC,
        HYPERBOLIC
    }
}
