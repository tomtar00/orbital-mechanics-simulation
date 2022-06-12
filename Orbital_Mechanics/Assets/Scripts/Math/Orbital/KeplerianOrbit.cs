using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    // Mostly based on: http://control.asu.edu/Classes/MAE462/462Lecture05.pdf
    public class KeplerianOrbit
    {
        public const float G = 6.67f;
        private float GM;

        public Orbit orbit { get; set; }
        public OrbitType orbitType { get; set; }

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
            if (centralBody != null)
            {
                this.GM = G * centralBody.Data.Mass;
            }
        }

        public void ChangeOrbitType(OrbitType type, Celestial centralBody)
        {
            switch (type)
            {
                case OrbitType.ELLIPTIC:
                    orbit = new EllipticOrbit(this, centralBody);
                    break;
                case OrbitType.HYPERBOLIC:
                    orbit = new HyperbolicOrbit(this, centralBody);
                    break;
            }
            orbitType = type;
        }

        public void ChangeOrbitType(OrbitType type)
        {
            ChangeOrbitType(type, this.orbit.centralBody);
            // Debug.Log("Changed to " + type);
        }

        public void CheckOrbitType(float eccentricity)
        {
            if (eccentricity >= 0 && eccentricity < 1)
            {
                if (orbitType != OrbitType.ELLIPTIC)
                    ChangeOrbitType(OrbitType.ELLIPTIC);
            }

            else if (eccentricity >= 1)
            {
                if (orbitType != OrbitType.HYPERBOLIC)
                    ChangeOrbitType(OrbitType.HYPERBOLIC);
            }
        }

        public void ApplyElementsFromStruct(Elements elements)
        {
            semimajorAxis = elements.semimajorAxis;
            eccentricity = elements.eccentricity;
            inclination = elements.inclination;
            lonAscNode = elements.lonAscNode;
            argPeriapsis = elements.argPeriapsis;
            trueAnomaly = elements.trueAnomaly;

            CheckOrbitType(eccentricity);

            orbit.angMomentum = Quaternion.AngleAxis(inclination * Mathf.Rad2Deg, Vector3.right) * Vector3.forward;
            orbit.angMomentum = Quaternion.AngleAxis(lonAscNode * Mathf.Rad2Deg, Vector3.forward) * orbit.angMomentum;
            orbit.angMomentum = orbit.angMomentum.normalized * MathLib.Sqrt(GM * semimajorAxis * (1 - eccentricity * eccentricity));

            orbit.eccVec = Quaternion.AngleAxis(lonAscNode * Mathf.Rad2Deg, Vector3.forward) * Vector3.right;
            orbit.eccVec = Quaternion.AngleAxis(argPeriapsis * Mathf.Rad2Deg, orbit.angMomentum) * orbit.eccVec;
            orbit.eccVec = orbit.eccVec.normalized * eccentricity;

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
            public float trueAnomaly;
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
