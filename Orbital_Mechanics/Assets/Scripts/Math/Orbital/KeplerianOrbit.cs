using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class KeplerianOrbit
    {
        public const float G = 6.67f;

        public Orbit orbit { get; set; }
        public OrbitType orbitType { get; set; } = OrbitType.NONE;

        public KeplerianOrbit(Celestial centralBody)
        {
            // if (centralBody != null)
            // {
            //     this.orbit = new EllipticOrbit()
            // }
        }

        public void CheckOrbitType(StateVectors stateVectors, Celestial centralBody)
        {
            float centralMass = centralBody.Data.Mass;
            float eccentricity = Orbit.CalculateEccentricity(stateVectors, centralMass);

            if (eccentricity >= 0 && eccentricity < 1 && orbitType != OrbitType.ELLIPTIC)
            {
                this.orbit = new EllipticOrbit(stateVectors, centralBody);
                orbitType = OrbitType.ELLIPTIC;
            }
            else if (eccentricity >= 1 && orbitType != OrbitType.HYPERBOLIC)
            {
                this.orbit = new HyperbolicOrbit(stateVectors, centralBody);
                orbitType = OrbitType.HYPERBOLIC;
            }
        }

        public static Orbit CreateOrbit(StateVectors stateVectors, Celestial body, out OrbitType type)
        {
            float centralMass = body.Data.Mass;
            float eccentricity = Orbit.CalculateEccentricity(stateVectors, centralMass);

            if (eccentricity >= 0 && eccentricity < 1)
            {
                type = OrbitType.ELLIPTIC;
                return new EllipticOrbit(stateVectors, body);
            }
            else if (eccentricity >= 1)
            {
                type = OrbitType.HYPERBOLIC;
                return new HyperbolicOrbit(stateVectors, body);
            }
            else
            {
                type = OrbitType.NONE;
                return null;
            }
        }
        public static Orbit CreateOrbit(OrbitElements elements, Celestial body, out OrbitType type)
        {
            float centralMass = body.Data.Mass;
            float eccentricity = elements.eccentricity;

            if (eccentricity >= 0 && eccentricity < 1)
            {
                type = OrbitType.ELLIPTIC;
                return new EllipticOrbit(elements, body);
            }
            else if (eccentricity >= 1)
            {
                type = OrbitType.HYPERBOLIC;
                return new HyperbolicOrbit(elements, body);
            }
            else
            {
                type = OrbitType.NONE;
                return null;
            }
        }

        public (float, float, float) UpdateAnomalies(float time)
        {
            (float, float, float) mat = orbit.GetFutureAnomalies(time);
            orbit.elements.meanAnomaly = mat.Item1;
            orbit.elements.anomaly = mat.Item2;
            orbit.elements.trueAnomaly = mat.Item3;

            return (
                orbit.elements.meanAnomaly,
                orbit.elements.anomaly,
                orbit.elements.trueAnomaly
            );
        }
        public StateVectors UpdateStateVectors(float trueAnomaly)
        {
            return orbit.ConvertOrbitElementsToStateVectors(trueAnomaly);
        }

        public void ApplyElementsFromStruct(OrbitElements elements, Celestial centralBody)
        {
            float GM = G * centralBody.Data.Mass;

            elements.angMomentum = Quaternion.AngleAxis(elements.inclination * Mathf.Rad2Deg, Vector3.right) * Vector3.forward;
            elements.angMomentum = Quaternion.AngleAxis(elements.lonAscNode * Mathf.Rad2Deg, Vector3.forward) * elements.angMomentum;
            elements.angMomentum = elements.angMomentum.normalized * MathLib.Sqrt(GM * elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity));

            elements.eccVec = Quaternion.AngleAxis(elements.lonAscNode * Mathf.Rad2Deg, Vector3.forward) * Vector3.right;
            elements.eccVec = Quaternion.AngleAxis(elements.argPeriapsis * Mathf.Rad2Deg, elements.angMomentum) * elements.eccVec;
            elements.eccVec = elements.eccVec.normalized * elements.eccentricity;

            this.orbit = CreateOrbit(elements, centralBody, out _);
        }

    }

    public enum OrbitType
    {
        ELLIPTIC,
        HYPERBOLIC,
        NONE
    }

    public class StateVectors
    {
        public Vector3 position;
        public Vector3 velocity;

        public StateVectors(Vector3 pos, Vector3 vel)
        {
            this.position = pos;
            this.velocity = vel;
        }
    }

    [System.Serializable]
    public struct OrbitElements
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
