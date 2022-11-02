using UnityEngine;
using Sim.Objects;
using Sim.Math;

namespace Sim.Orbits
{
    public class KeplerianOrbit
    {
        public static double G { get; private set; }

        public Orbit orbit { get; set; }
        public OrbitType orbitType { get; set; } = OrbitType.NONE;

        public KeplerianOrbit() {
            G = SimulationSettings.Instance.G;
        }

        private (double, double, double) mat;

        public void CheckOrbitType(StateVectors stateVectors, Celestial centralBody)
        {
            double centralMass = centralBody.Data.Mass;
            double eccentricity = Orbit.CalculateEccentricity(stateVectors, centralMass);

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
            double centralMass = body.Data.Mass;
            double eccentricity = Orbit.CalculateEccentricity(stateVectors, centralMass);

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
        public static Orbit CreateOrbit(OrbitalElements elements, Celestial body, out OrbitType type)
        {
            double centralMass = body.Data.Mass;
            double eccentricity = elements.eccentricity;

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

        public (double, double, double) UpdateAnomalies(double time)
        {
            mat = orbit.GetFutureAnomalies(time);
            orbit.elements.meanAnomaly = mat.Item1;
            orbit.elements.anomaly = mat.Item2;
            orbit.elements.trueAnomaly = mat.Item3;

            return mat;
        }
        public StateVectors UpdateStateVectors(double trueAnomaly)
        {
            return orbit.ConvertOrbitElementsToStateVectors(trueAnomaly);
        }
        public void UpdateTimeToPeriapsis() {
            orbit.elements.timeToPeriapsis = orbit.CalculateTimeToPeriapsis(orbit.elements.meanAnomaly);
        }

        public void ApplyElementsFromStruct(OrbitalElements elements, Celestial centralBody)
        {
            double GM = G * centralBody.Data.Mass;

            elements.angMomentum = (Quaternion.AngleAxis((float)(elements.inclination * MathLib.Rad2Deg), Vector3.right) * Vector3.up);
            elements.angMomentum = (Quaternion.AngleAxis((float)(elements.lonAscNode * MathLib.Rad2Deg), Vector3.up) * elements.angMomentum);
            elements.angMomentum = elements.angMomentum.normalized * (double)MathLib.Sqrt(GM * elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity));

            elements.eccVec = (Quaternion.AngleAxis((float)(-elements.lonAscNode * MathLib.Rad2Deg), Vector3.up) * Vector3.right);
            elements.eccVec = (Quaternion.AngleAxis((float)(-elements.argPeriapsis * MathLib.Rad2Deg), elements.angMomentum) * elements.eccVec);
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
}
