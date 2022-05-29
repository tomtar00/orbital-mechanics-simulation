using Sim.Objects;
using UnityEngine;

namespace Sim.Math
{
    public abstract class Orbit
    {
        public const float PI2 = 6.28318531f;

        protected KeplerianOrbit parent;

        public Celestial centralBody { get; private set; }
        public float GM { get; private set; }
        public float posMagnitude { get; private set; }
        public float velMagnitude { get; private set; }
        public Vector3 angMomentum { get; private set; }
        public float angMomMag { get; private set; }
        public Vector3 eccVec { get; private set; }
        public Vector3 nodeVector { get; private set; }
        public float nodeMag { get; private set; }

        public Orbit(KeplerianOrbit trajectory, Celestial centralBody)
        {
            this.parent = trajectory;
            ChangeCentralBody(centralBody);
        }

        public void ChangeCentralBody(Celestial centralBody)
        {
            this.centralBody = centralBody;
            this.GM = KeplerianOrbit.G * centralBody.Data.Mass;
        }

        public abstract void CalculateTrueAnomaly(float anomaly);
        public abstract Vector3 CalculateOrbitalPositionTrue(float trueAnomaly);
        public abstract Vector3 CalculateVelocity(Vector3 relativePosition, Vector3 orbitNormal, out float speed);
        public abstract float MeanAnomalyEquation(float anomaly, float e, float M);
        public abstract float d_MeanAnomalyEquation(float anomaly, float e);

        public virtual void CalculateMainOrbitElements(Vector3 relativePosition, Vector3 velocity)
        {
            posMagnitude = relativePosition.magnitude;
            velMagnitude = velocity.magnitude;

            // Semi-major axis
            // source: wyprowadzenie_semimajor.png
            // source2: https://en.wikipedia.org/wiki/Vis-viva_equation
            parent.semimajorAxis = (GM * posMagnitude).SafeDivision((2 * GM - velMagnitude * velMagnitude * posMagnitude));

            // Eccentricity
            // source: https://en.wikipedia.org/wiki/Eccentricity_vector
            angMomentum = Vector3.Cross(relativePosition, velocity);
            angMomMag = angMomentum.magnitude;
            eccVec = (Vector3.Cross(velocity, angMomentum) / GM) - (relativePosition.SafeDivision(posMagnitude));
            parent.eccentricity = eccVec.magnitude;

            // Inclination
            // source: https://en.wikipedia.org/wiki/Orbital_inclination
            parent.inclination = MathLib.Acos(angMomentum.z.SafeDivision(angMomMag));

            // Longitude of the ascending node
            // source: https://en.wikipedia.org/wiki/Longitude_of_the_ascending_node
            nodeVector = Vector3.Cross(Vector3.forward, angMomentum);
            nodeMag = nodeVector.magnitude;
            parent.lonAscNode = MathLib.Acos(nodeVector.x.SafeDivision(nodeMag));
            if (nodeVector.y < 0)
                parent.lonAscNode = PI2 - parent.lonAscNode;

            // Argument of periapsis
            // source: https://en.wikipedia.org/wiki/Argument_of_periapsis
            parent.argPeriapsis = MathLib.Acos(Vector3.Dot(nodeVector, eccVec).SafeDivision(nodeMag * parent.eccentricity));
            if (eccVec.z < 0)
                parent.argPeriapsis = PI2 - parent.argPeriapsis;
        }
        public virtual void CalculateOtherElements()
        {
            parent.sinLonAcsNode = MathLib.Sin(parent.lonAscNode);
            parent.cosLonAcsNode = MathLib.Cos(parent.lonAscNode);
            parent.sinInclination = MathLib.Sin(parent.inclination);
            parent.cosInclination = MathLib.Cos(parent.inclination);
            parent.sinArgPeriapsis = MathLib.Sin(parent.argPeriapsis);
            parent.cosArgPeriapsis = MathLib.Cos(parent.argPeriapsis);
        }

        public float CalculateEccentricity(Vector3 relativePosition, Vector3 velocity)
        {
            Vector3 angMomentum = Vector3.Cross(relativePosition, velocity);
            float angMomMag = angMomentum.magnitude;
            Vector3 eccVec = (Vector3.Cross(velocity, angMomentum) / GM) - (relativePosition.SafeDivision(relativePosition.magnitude));
            return eccVec.magnitude;
        }
        public Vector3 CalculateOrbitalPosition(float anomaly)
        {
            CalculateTrueAnomaly(anomaly);
            return CalculateOrbitalPositionTrue(parent.trueAnomaly);
        }
        public virtual float CalculateAnomaly(float time)
        {
            parent.meanAnomaly += parent.meanMotion * time;
            if (parent.meanAnomaly > PI2 / 2) parent.meanAnomaly -= PI2;

            return CalculateAnomalyFromMean(parent.meanAnomaly);
        }

        // source: https://en.wikipedia.org/wiki/Eccentric_anomaly
        // numerical method: https://en.wikipedia.org/wiki/Newton%27s_method
        public virtual float CalculateAnomalyFromMean(float meanAnomaly)
        {
            float a1 = meanAnomaly;
            float a0 = 2 * meanAnomaly;

            // Debug.Log("==============================>");
            while (MathLib.Abs(a1 - a0) > 0.0001f)
            {
                a0 = a1;
                float eq = MeanAnomalyEquation(a0, parent.eccentricity, meanAnomaly); 
                float deq = d_MeanAnomalyEquation(a0, parent.eccentricity);
                float sub = eq.SafeDivision(deq); 
                a1 = a0 - sub;
                // Debug.Log(a0 + " === " + a1 + " === " + eq + " === " + deq + " === " + sub +  " === " + meanAnomaly);
            }
            // Debug.Log("<==============================");

            return a1;
        }
    }
}