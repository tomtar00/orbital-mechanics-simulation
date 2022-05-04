using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class KeplerianOrbit
    {
        [System.Serializable]
        public struct MetaElements
        {
            public float sinLonAcsNode;
            public float cosLonAcsNode;
            public float sinInclination;
            public float cosInclination;
            public float trueAnomalyConstant;
            public float meanMotion;
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
            public float meanAnomaly;

            public MetaElements meta;
        }

        public const float G = 6.67f;

        // CONVERTER https://janus.astro.umd.edu/orbits/elements/convertframe.html
        // https://en.wikipedia.org/wiki/Hyperbolic_trajectory
        // source: https://phys.libretexts.org/Bookshelves/Astronomy__Cosmology/Celestial_Mechanics_(Tatum)/09%3A_The_Two_Body_Problem_in_Two_Dimensions/9.08%3A_Orbital_Elements_and_Velocity_Vector
        public static Elements CalculateOrbitElements(Vector3 position, Vector3 velocity, Celestial body, bool deg = false)
        {
            float PI2 = 2 * Mathf.PI;
            float GM = G * body.Mass;
            float posMagnitude = position.magnitude;
            float velMagnitude = velocity.magnitude;

            // Semi-major axis
            // source: wyprowadzenie_semimajor.png
            float centerMassDst = Vector3.Distance(position, body.transform.position);
            float semimajorAxis = (GM * centerMassDst) / (2 * GM - velMagnitude * velMagnitude * centerMassDst);

            // Eccentricity
            // source: https://en.wikipedia.org/wiki/Eccentricity_vector
            Vector3 angMomentum = Vector3.Cross(position, velocity);
            float angMomMag = angMomentum.magnitude;
            Vector3 eccVec = (Vector3.Cross(velocity, angMomentum) / GM) - (position / posMagnitude);
            float eccentricity = eccVec.magnitude;

            // Inclination
            // source: https://en.wikipedia.org/wiki/Orbital_inclination
            float inclination = Mathf.Acos(angMomentum.z / angMomMag);
            if (deg) inclination *= Mathf.Rad2Deg;

            // Longitude of the ascending node
            // source: https://en.wikipedia.org/wiki/Longitude_of_the_ascending_node
            Vector3 nodeVector = Vector3.Cross(Vector3.forward, angMomentum);
            float nodeMag = nodeVector.magnitude;
            float lonAscNode = Mathf.Acos(nodeVector.x / nodeMag);
            if (nodeVector.y < 0)
                lonAscNode = PI2 - lonAscNode;
            if (deg) lonAscNode *= Mathf.Rad2Deg;

            // Argument of periapsis
            // source: https://en.wikipedia.org/wiki/Argument_of_periapsis
            float argPeriapsis = Mathf.Acos(Vector3.Dot(nodeVector, eccVec) / (nodeMag * eccentricity));
            if (eccVec.z < 0)
                argPeriapsis = PI2 - argPeriapsis;
            if (deg) argPeriapsis *= Mathf.Rad2Deg;

            // True anomaly
            // source: https://en.wikipedia.org/wiki/True_anomaly
            float trueAnomaly = Mathf.Acos(Vector3.Dot(eccVec, position) / (eccentricity * posMagnitude));
            if (Vector3.Dot(position, velocity) < 0)
                trueAnomaly = PI2 - trueAnomaly;
            if (deg) trueAnomaly *= Mathf.Rad2Deg;

            Elements elements = new Elements();
            elements.semimajorAxis = semimajorAxis;
            elements.eccentricity = eccentricity;
            elements.inclination = inclination;
            elements.lonAscNode = lonAscNode;
            elements.argPeriapsis = argPeriapsis;
            elements.trueAnomaly = trueAnomaly;
            elements.meanAnomaly = ConvertTrueToMeanAnomaly(trueAnomaly, eccentricity);

            MetaElements meta = new MetaElements();
            meta.sinLonAcsNode = Mathf.Sin(lonAscNode);
            meta.cosLonAcsNode = Mathf.Cos(lonAscNode);
            meta.sinInclination = Mathf.Sin(inclination);
            meta.cosInclination = Mathf.Cos(inclination);
            meta.trueAnomalyConstant = Mathf.Sqrt((1 + eccentricity) / (1 - eccentricity));
            meta.meanMotion = Mathf.Sqrt(GM / Mathf.Pow(semimajorAxis, 3));

            elements.meta = meta;

            return elements;
        }

        // source: https://phas.ubc.ca/~newhouse/p210/orbits/cometreport.pdf
        public static Vector3 CalculateOrbitalPosition(Elements elements, float eccentricAnomaly)
        {
            float trueAnomaly = 2 * Mathf.Atan(elements.meta.trueAnomalyConstant * Mathf.Tan(eccentricAnomaly / 2));
            float distance = elements.semimajorAxis * (1 - elements.eccentricity * Mathf.Cos(eccentricAnomaly));

            float cosArgTrue = Mathf.Cos(elements.argPeriapsis + trueAnomaly);
            float sinArgTrue = Mathf.Sin(elements.argPeriapsis + trueAnomaly);

            float x = distance * ((elements.meta.cosLonAcsNode * cosArgTrue) - (elements.meta.sinLonAcsNode * sinArgTrue * elements.meta.cosInclination));
            float y = distance * ((elements.meta.sinLonAcsNode * cosArgTrue) + (elements.meta.cosLonAcsNode * sinArgTrue * elements.meta.cosInclination));
            float z = distance * (elements.meta.sinInclination * sinArgTrue);

            return new Vector3(x, y, z);
        }

        // source: https://en.wikipedia.org/wiki/Eccentric_anomaly
        // numerical method: https://en.wikipedia.org/wiki/Newton%27s_method
        public static float CalculateEccentricAnomaly(KeplerianOrbit.Elements elements, float time)
        {       
            float meanAnomaly = elements.meanAnomaly + elements.meta.meanMotion * time;

            float E1 = meanAnomaly;
            float difference = float.MaxValue;
            float sigma = 1e-06f;
            int maxIterations = 5;

            for (int i = 0; difference > sigma && i < maxIterations; i++)
            {
                float E0 = E1;
                E1 = E0 - Kepler(E0, elements.eccentricity, meanAnomaly) / d_Kepler(E0, elements.eccentricity);
                difference = Mathf.Abs(E1 - E0);
            }

            return E1;
        }
        // Kepler equation f(x) = 0 
        private static float Kepler(float E, float e, float M)
        {
            return M - E + e * Mathf.Sin(E);
        }
        // Derivative of the Kepler equation
        private static float d_Kepler(float E, float e)
        {
            return e * Mathf.Cos(E) - 1f;
        }

        // source: https://en.wikipedia.org/wiki/Mean_anomaly
        public static float ConvertTrueToMeanAnomaly(float trueAnomaly, float eccentricity)
        {
            float sinTrue = Mathf.Sin(trueAnomaly);
            float cosTrue = Mathf.Cos(trueAnomaly);
            float e = eccentricity;

            float y = (Mathf.Sqrt(1 - e * e) * sinTrue) / (1 + e * cosTrue);
            float x = (e + cosTrue) / (1 + e * cosTrue);
            float mean = Mathf.Atan2(y, x) - e * y;
            float PI2 = 2 * Mathf.PI;

            if (mean < 0) {
                mean += PI2;
            }
            else if (mean > PI2) {
                mean -= PI2;
            }

            return mean;
        }
    }
}
