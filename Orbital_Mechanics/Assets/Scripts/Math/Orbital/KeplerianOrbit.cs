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

        public Elements elements;

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
                    orbit = new EllipticOrbit(centralBody);
                    break;
                case OrbitType.HYPERBOLIC:
                    orbit = new HyperbolicOrbit(centralBody);
                    break;
            }
            orbitType = type;
        }

        public void CheckOrbitType(float eccentricity)
        {
            if (eccentricity >= 0 && eccentricity < 1)
            {
                if (orbitType != OrbitType.ELLIPTIC)
                    ChangeOrbitType(OrbitType.ELLIPTIC, this.orbit.centralBody);
            }
            else if (eccentricity >= 1)
            {
                if (orbitType != OrbitType.HYPERBOLIC)
                    ChangeOrbitType(OrbitType.HYPERBOLIC, this.orbit.centralBody);
            }
            else {
                Debug.LogError("Wrong eccentricity value");
            }
        }

        public static Orbit CreateOrbit(float eccentricity, Celestial body) {
            if (eccentricity >= 0 && eccentricity < 1) {
                return new EllipticOrbit(body);
            }
            else if (eccentricity >= 1) {
                return new HyperbolicOrbit(body);
            }
            else return null;
        }

        public void ApplyElementsFromStruct(Elements elements)
        {
            this.elements.semimajorAxis = elements.semimajorAxis;
            this.elements.eccentricity = elements.eccentricity;
            this.elements.inclination = elements.inclination;
            this.elements.lonAscNode = elements.lonAscNode;
            this.elements.argPeriapsis = elements.argPeriapsis;
            this.elements.trueAnomaly = elements.trueAnomaly;

            CheckOrbitType(elements.eccentricity);

            elements.angMomentum = Quaternion.AngleAxis(elements.inclination * Mathf.Rad2Deg, Vector3.right) * Vector3.forward;
            elements.angMomentum = Quaternion.AngleAxis(elements.lonAscNode * Mathf.Rad2Deg, Vector3.forward) * elements.angMomentum;
            elements.angMomentum = elements.angMomentum.normalized * MathLib.Sqrt(GM * elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity));

            elements.eccVec = Quaternion.AngleAxis(elements.lonAscNode * Mathf.Rad2Deg, Vector3.forward) * Vector3.right;
            elements.eccVec = Quaternion.AngleAxis(elements.argPeriapsis * Mathf.Rad2Deg, elements.angMomentum) * elements.eccVec;
            elements.eccVec = elements.eccVec.normalized * elements.eccentricity;

            orbit.CalculateOtherElements(elements);
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

            public float meanAnomaly { get; set; }
            public float anomaly { get; set; }
            public float semiminorAxis { get; set; }
            public float trueAnomalyConstant { get; set; }
            public float meanMotion { get; set; }
            public float semiLatusRectum { get; set; }

            public Vector3 angMomentum { get; set; }
            public Vector3 eccVec { get; set; }
        }
    }

    public enum OrbitType
    {
        ELLIPTIC,
        HYPERBOLIC
    }

    public struct StateVectors {
        public Vector3 position;
        public Vector3 velocity;
    }
}
