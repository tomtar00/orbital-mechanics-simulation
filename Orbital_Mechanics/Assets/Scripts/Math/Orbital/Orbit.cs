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
        public Vector3 angMomentum { get; set; }
        public float angMomMag { get; private set; }
        public Vector3 eccVec { get; set; }
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
            if (centralBody != null)
                this.GM = KeplerianOrbit.G * centralBody.Data.Mass;
        }  

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

            // True anomaly
            // source: https://en.wikipedia.org/wiki/True_anomaly                
            float eccPosDot = Vector3.Dot(eccVec, relativePosition);
            parent.trueAnomaly = MathLib.Acos(eccPosDot.SafeDivision(parent.eccentricity * posMagnitude));
            if (Vector3.Dot(relativePosition, velocity) < 0)
                parent.trueAnomaly = PI2 - parent.trueAnomaly; 

            CalculateOtherElements();
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
            
        // source: https://en.wikipedia.org/wiki/Elliptic_orbit#Flight_path_angle
        public virtual Vector3 CalculateVelocity(Vector3 relativePosition, out float speed)
        {
            float posDst = relativePosition.magnitude;
            speed = MathLib.Sqrt(GM * ((2f).SafeDivision(posDst) - (1f).SafeDivision(parent.semimajorAxis)));
        
            float e = parent.eccentricity;
            float pathAngle = MathLib.Atan((e * MathLib.Sin(parent.trueAnomaly)) / (1 + e * MathLib.Cos(parent.trueAnomaly)));
            Vector3 radDir = Quaternion.AngleAxis(90, parent.orbit.angMomentum) * relativePosition.normalized;
            Vector3 dir = Quaternion.AngleAxis(-pathAngle * MathLib.Rad2Deg, parent.orbit.angMomentum) * radDir;

            return dir * speed;
        } 
        public abstract Vector3 CalculateOrbitalPosition(float trueAnomaly);
        
        public abstract float CalculateMeanAnomaly(float time);
        public virtual float CalculateAnomaly(float meanAnomaly)
        {
            // source: https://en.wikipedia.org/wiki/Eccentric_anomaly
            // numerical method: https://en.wikipedia.org/wiki/Newton%27s_method

            float a1 = meanAnomaly;
            float a0 = float.MaxValue;

            while (MathLib.Abs(a1 - a0) > 0.0001f)
            {
                a0 = a1;
                float eq = MeanAnomalyEquation(a0, parent.eccentricity, meanAnomaly); 
                float deq = d_MeanAnomalyEquation(a0, parent.eccentricity);
                a1 = a0 - eq.SafeDivision(deq);
            }

            return a1;
        }
        public abstract float CalculateTrueAnomaly(float anomaly);

        public abstract float MeanAnomalyEquation(float anomaly, float e, float M);
        public abstract float d_MeanAnomalyEquation(float anomaly, float e);
    }
}