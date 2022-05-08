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
            public float sinArgPeriapsis;
            public float cosArgPeriapsis;

            [Space]
            public float semiminorAxis;
            public float trueAnomalyConstant;
            public float meanMotion;
            public float semiLatusRectum;
            public float meanAnomalyAtZero;
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

            [Space]
            public float trueAnomaly;     

            public MetaElements meta;
        }

        public const float G = 6.67f;

        // CONVERTER https://janus.astro.umd.edu/orbits/elements/convertframe.html
        // https://en.wikipedia.org/wiki/Hyperbolic_trajectory
        // source: https://phys.libretexts.org/Bookshelves/Astronomy__Cosmology/Celestial_Mechanics_(Tatum)/09%3A_The_Two_Body_Problem_in_Two_Dimensions/9.08%3A_Orbital_Elements_and_Velocity_Vector
        public static Elements CalculateOrbitElements(Vector3 relativePosition, Vector3 velocity, float celestialMass, bool deg = false)
        {
            float PI2 = 2 * Mathf.PI;
            float GM = G * celestialMass;
            float posMagnitude = relativePosition.magnitude;
            float velMagnitude = velocity.magnitude;

            // Semi-major axis
            // source: wyprowadzenie_semimajor.png
            float semimajorAxis = (GM * posMagnitude) / (2 * GM - velMagnitude * velMagnitude * posMagnitude);

            // Eccentricity
            // source: https://en.wikipedia.org/wiki/Eccentricity_vector
            Vector3 angMomentum = Vector3.Cross(relativePosition, velocity);
            float angMomMag = angMomentum.magnitude;
            if (angMomMag == 0) angMomMag = .0001f;
            Vector3 eccVec = (Vector3.Cross(velocity, angMomentum) / GM) - (relativePosition / posMagnitude);
            float eccentricity = eccVec.magnitude;

            // Inclination
            // source: https://en.wikipedia.org/wiki/Orbital_inclination
            float inclination = Mathf.Acos(angMomentum.z / angMomMag);
            if (deg) inclination *= Mathf.Rad2Deg;

            // Longitude of the ascending node
            // source: https://en.wikipedia.org/wiki/Longitude_of_the_ascending_node
            Vector3 nodeVector = Vector3.Cross(Vector3.forward, angMomentum);
            float nodeMag = nodeVector.magnitude;
            if (nodeMag == 0) nodeMag = .0001f;
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
            float trueAnomaly = Mathf.Acos(Vector3.Dot(eccVec, relativePosition) / (eccentricity * posMagnitude));
            if (Vector3.Dot(relativePosition, velocity) < 0)
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

            MetaElements meta = CalculateMetaElements(elements, celestialMass);
            elements.meta = meta;

            return elements;
        }

        public static MetaElements CalculateMetaElements(Elements elements, float celestialMass)  {
            MetaElements meta = new MetaElements();
            meta.sinLonAcsNode = Mathf.Sin(elements.lonAscNode);
            meta.cosLonAcsNode = Mathf.Cos(elements.lonAscNode);
            meta.sinInclination = Mathf.Sin(elements.inclination);
            meta.cosInclination = Mathf.Cos(elements.inclination);
            meta.sinArgPeriapsis = Mathf.Sin(elements.argPeriapsis);
            meta.cosArgPeriapsis = Mathf.Cos(elements.argPeriapsis);
            meta.semiminorAxis = elements.semimajorAxis * Mathf.Sqrt(1 - elements.eccentricity * elements.eccentricity);
            meta.trueAnomalyConstant = Mathf.Sqrt((1 + elements.eccentricity) / (1 -elements. eccentricity));
            meta.meanMotion = Mathf.Sqrt(G * celestialMass / Mathf.Pow(elements.semimajorAxis, 3));
            meta.semiLatusRectum = elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity);
            meta.meanAnomalyAtZero = elements.meanAnomaly;
            return meta;
        }

        // source: https://phas.ubc.ca/~newhouse/p210/orbits/cometreport.pdf
        public static Vector3 CalculateOrbitalPosition(Elements elements, float eccentricAnomaly, out float trueAnomaly)
        {
            trueAnomaly = 2 * Mathf.Atan(elements.meta.trueAnomalyConstant * Mathf.Tan(eccentricAnomaly / 2));
            if (trueAnomaly < 0) trueAnomaly += 2 * Mathf.PI;
            float distance = elements.semimajorAxis * (1 - elements.eccentricity * Mathf.Cos(eccentricAnomaly));

            float cosArgTrue = Mathf.Cos(elements.argPeriapsis + trueAnomaly);
            float sinArgTrue = Mathf.Sin(elements.argPeriapsis + trueAnomaly);

            float x = distance * ((elements.meta.cosLonAcsNode * cosArgTrue) - (elements.meta.sinLonAcsNode * sinArgTrue * elements.meta.cosInclination));
            float y = distance * ((elements.meta.sinLonAcsNode * cosArgTrue) + (elements.meta.cosLonAcsNode * sinArgTrue * elements.meta.cosInclination));
            float z = distance * (elements.meta.sinInclination * sinArgTrue);

            return new Vector3(x, y, z);
        }
        public static Vector3 CalculateOrbitalPosition(Elements elements, float trueAnomaly)
        {
            float distance = (elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity)) / (1 + elements.eccentricity * Mathf.Cos(trueAnomaly));

            float cosArgTrue = Mathf.Cos(elements.argPeriapsis + trueAnomaly);
            float sinArgTrue = Mathf.Sin(elements.argPeriapsis + trueAnomaly);

            float x = distance * ((elements.meta.cosLonAcsNode * cosArgTrue) - (elements.meta.sinLonAcsNode * sinArgTrue * elements.meta.cosInclination));
            float y = distance * ((elements.meta.sinLonAcsNode * cosArgTrue) + (elements.meta.cosLonAcsNode * sinArgTrue * elements.meta.cosInclination));
            float z = distance * (elements.meta.sinInclination * sinArgTrue);

            return new Vector3(x, y, z);
        }

        // source: https://www.orbiter-forum.com/threads/calculate-not-derive-orbital-velocity-vector.22367/
        public static Vector3 CalculateVelocity(Elements elements, Vector3 relativePosition, Vector3 orbitNormal, float celestialMass, out float speed)
        {
            float posDst = relativePosition.magnitude;
            speed = Mathf.Sqrt(KeplerianOrbit.G * celestialMass * (2 / posDst - 1 / elements.semimajorAxis));
            
            float e = elements.eccentricity;
            float k = posDst / elements.semimajorAxis;
            float alpha = Mathf.Acos((2 - 2 * e * e) / (k * (2 - k)) - 1);
            float angle = alpha + ((Mathf.PI - alpha) / 2);         
            if (elements.trueAnomaly < Mathf.PI)
                angle = Mathf.PI - angle;
            Vector3 velocity = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, orbitNormal) * (relativePosition / posDst) * speed;
            return velocity;
        }

        // source: https://en.wikipedia.org/wiki/Eccentric_anomaly
        // numerical method: https://en.wikipedia.org/wiki/Newton%27s_method
        public static float CalculateEccentricAnomalyFromMean(Elements elements, float meanAnomaly)
        {
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
        public static float CalculateEccentricAnomaly(Elements elements, float time)
        {
            float meanAnomaly = elements.meta.meanAnomalyAtZero + elements.meta.meanMotion * time;

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
