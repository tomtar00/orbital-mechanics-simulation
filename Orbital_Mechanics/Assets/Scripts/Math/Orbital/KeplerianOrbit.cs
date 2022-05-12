using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public abstract class KeplerianOrbit
    {
        public const float G = 6.67f;
        public const float PI2 = 6.28318531f;

        protected float GM;
        protected Celestial centralBody;
        protected float posMagnitude;
        protected float velMagnitude;
        protected Vector3 angMomentum;
        protected float angMomMag;
        protected Vector3 eccVec;
        protected Vector3 nodeVector;      
        protected float nodeMag;

        // Main keplerian orbital elements
        public float semimajorAxis { get; protected set; }
        public float eccentricity { get; protected set; }
        public float inclination { get; protected set; }
        public float lonAscNode { get; protected set; }
        public float argPeriapsis { get; protected set; }
        public float meanAnomaly { get; protected set; }

        // True anomaly / True longitude / Argument of latitude
        public float trueAnomaly { get; protected set; }
        // Eccentric anomaly / Parabolic anomaly / Hyperbolic anomaly
        public float anomaly { get; protected set; }      

        // Other orbital elements
        public float semiminorAxis { get; protected set; }
        public float trueAnomalyConstant { get; protected set; }
        public float meanMotion { get; protected set; }
        public float semiLatusRectum { get; protected set; }
        public float meanAnomalyAtZero { get; protected set; }

        // Supporting variables
        public float sinLonAcsNode { get; protected set; }
        public float cosLonAcsNode { get; protected set; }
        public float sinInclination { get; protected set; }
        public float cosInclination { get; protected set; }
        public float sinArgPeriapsis { get; protected set; }
        public float cosArgPeriapsis { get; protected set; }    

        public KeplerianOrbit(Celestial centralBody) 
        {
            this.centralBody = centralBody;
            this.GM = G * centralBody.Data.Mass;
        }  

        public abstract void CalculateTrueAnomaly(float anomaly);
        public abstract Vector3 CalculateOrbitalPositionTrue(float trueAnomaly);

        // CONVERTER https://janus.astro.umd.edu/orbits/elements/convertframe.html
        // https://en.wikipedia.org/wiki/Hyperbolic_trajectory
        // source: https://phys.libretexts.org/Bookshelves/Astronomy__Cosmology/Celestial_Mechanics_(Tatum)/09%3A_The_Two_Body_Problem_in_Two_Dimensions/9.08%3A_Orbital_Elements_and_Velocity_Vector
        public virtual void CalculateMainOrbitElements(Vector3 relativePosition, Vector3 velocity)
        {
            posMagnitude = relativePosition.magnitude;
            velMagnitude = velocity.magnitude;

            // Semi-major axis
            // source: wyprowadzenie_semimajor.png
            // source2: https://en.wikipedia.org/wiki/Vis-viva_equation
            semimajorAxis = (GM * posMagnitude).SafeDivision((2 * GM - velMagnitude * velMagnitude * posMagnitude));

            // Eccentricity
            // source: https://en.wikipedia.org/wiki/Eccentricity_vector
            angMomentum = Vector3.Cross(relativePosition, velocity);
            angMomMag = angMomentum.magnitude;
            eccVec = (Vector3.Cross(velocity, angMomentum) / GM) - (relativePosition.SafeDivision(posMagnitude));
            eccentricity = eccVec.magnitude;

            // Inclination
            // source: https://en.wikipedia.org/wiki/Orbital_inclination
            inclination = Mathf.Acos(angMomentum.z.SafeDivision(angMomMag));

            // Longitude of the ascending node
            // source: https://en.wikipedia.org/wiki/Longitude_of_the_ascending_node
            nodeVector = Vector3.Cross(Vector3.forward, angMomentum);
            nodeMag = nodeVector.magnitude;
            lonAscNode = Mathf.Acos(nodeVector.x.SafeDivision(nodeMag));
            if (nodeVector.y < 0)
                lonAscNode = PI2 - lonAscNode;

            // Argument of periapsis
            // source: https://en.wikipedia.org/wiki/Argument_of_periapsis
            argPeriapsis = Mathf.Acos(Vector3.Dot(nodeVector, eccVec).SafeDivision(nodeMag * eccentricity));
            if (eccVec.z < 0)
                argPeriapsis = PI2 - argPeriapsis;
        }

        public virtual void CalculateOtherElements()
        {
            sinLonAcsNode = Mathf.Sin(lonAscNode);
            cosLonAcsNode = Mathf.Cos(lonAscNode);
            sinInclination = Mathf.Sin(inclination);
            cosInclination = Mathf.Cos(inclination);
            sinArgPeriapsis = Mathf.Sin(argPeriapsis);
            cosArgPeriapsis = Mathf.Cos(argPeriapsis);

            meanAnomalyAtZero = meanAnomaly;      
        }  
     
        public Vector3 CalculateOrbitalPosition(float anomaly)
        {
            CalculateTrueAnomaly(anomaly);
            return CalculateOrbitalPositionTrue(trueAnomaly);
        }

        // source: https://www.orbiter-forum.com/threads/calculate-not-derive-orbital-velocity-vector.22367/
        public Vector3 CalculateVelocity(Vector3 relativePosition, Vector3 orbitNormal, out float speed)
        {
            float posDst = relativePosition.magnitude;
            speed = Mathf.Sqrt(GM * ((2f).SafeDivision(posDst) - (1f).SafeDivision(semimajorAxis)));

            float e = eccentricity;
            float k = posDst.SafeDivision(semimajorAxis);
            float alpha = Mathf.Acos((2 - 2 * e * e).SafeDivision(k * (2 - k)) - 1);
            float angle = alpha + ((Mathf.PI - alpha) / 2);
            if (trueAnomaly < Mathf.PI)
                angle = Mathf.PI - angle;
            Vector3 velocity = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, orbitNormal) * (relativePosition.SafeDivision(posDst)) * speed;
            return velocity;
        }

        // source: https://en.wikipedia.org/wiki/Eccentric_anomaly
        // numerical method: https://en.wikipedia.org/wiki/Newton%27s_method
        public float CalculateEccentricAnomalyFromMean(float meanAnomaly)
        {
            float E1 = meanAnomaly;
            float difference = float.MaxValue;
            float sigma = 1e-06f;
            int maxIterations = 5;

            for (int i = 0; difference > sigma && i < maxIterations; i++)
            {
                float E0 = E1;
                E1 = E0 - Kepler(E0, eccentricity, meanAnomaly).SafeDivision(d_Kepler(E0, eccentricity));
                difference = Mathf.Abs(E1 - E0);
            }

            return E1;
        }
        public float CalculateEccentricAnomaly(float time, out float meanAnomaly)
        {
            meanAnomaly = meanAnomalyAtZero + meanMotion * time;
            if (meanAnomaly > PI2) meanAnomaly -= PI2;

            float E1 = meanAnomaly;
            float difference = float.MaxValue;
            float sigma = 1e-06f;
            int maxIterations = 5;

            for (int i = 0; difference > sigma && i < maxIterations; i++)
            {
                float E0 = E1;
                E1 = E0 - Kepler(E0, eccentricity, meanAnomaly).SafeDivision(d_Kepler(E0, eccentricity));
                difference = Mathf.Abs(E1 - E0);
            }

            return E1;
        }

        // Kepler equation f(x) = 0 
        protected float Kepler(float E, float e, float M)
        {
            return M - E + e * Mathf.Sin(E);
        }
        // Derivative of the Kepler equation
        protected float d_Kepler(float E, float e)
        {
            return e * Mathf.Cos(E) - 1f;
        }

        // source: https://en.wikipedia.org/wiki/Mean_anomaly
        public static float ConvertTrueToMeanAnomaly(float trueAnomaly, float eccentricity)
        {
            float sinTrue = Mathf.Sin(trueAnomaly);
            float cosTrue = Mathf.Cos(trueAnomaly);
            float e = eccentricity;

            float y = (Mathf.Sqrt(1 - e * e) * sinTrue).SafeDivision(1 + e * cosTrue);
            float x = (e + cosTrue).SafeDivision(1 + e * cosTrue);
            float mean = Mathf.Atan2(y, x) - e * y;
            float PI2 = 2 * Mathf.PI;

            if (mean < 0)
            {
                mean += PI2;
            }
            else if (mean > PI2)
            {
                mean -= PI2;
            }

            return mean;
        }
    }
}
