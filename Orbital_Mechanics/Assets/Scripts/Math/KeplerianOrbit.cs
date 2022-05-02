using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class KeplerianOrbit
    {
        [System.Serializable]
        public struct MetaElements {
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
            // public float trueAnomaly;

            public MetaElements meta;
        }

        public const float G = 6.67f;

        // CONVERTER https://janus.astro.umd.edu/orbits/elements/convertframe.html
        // https://en.wikipedia.org/wiki/Hyperbolic_trajectory
        // https://phys.libretexts.org/Bookshelves/Astronomy__Cosmology/Celestial_Mechanics_(Tatum)/09%3A_The_Two_Body_Problem_in_Two_Dimensions/9.08%3A_Orbital_Elements_and_Velocity_Vector
        public static Elements CalculateOrbitElements(Vector3 position, Vector3 velocity, Celestial body, bool deg=false)
        {
            float PI2 = 2 * Mathf.PI;
            float GM = G * body.Mass;
            float posMagnitude = position.magnitude;
            float velMagnitude = velocity.magnitude;

            // Semi-major axis
            float centerMassDst = Vector3.Distance(position, body.transform.position);
            float semimajorAxis = (GM * centerMassDst) / (2*GM - velMagnitude*velMagnitude*centerMassDst);

            // Eccentricity
            Vector3 angMomentum = Vector3.Cross(position, velocity);
            float angMomMag = angMomentum.magnitude; 
            Vector3 eccVec = (Vector3.Cross(velocity, angMomentum) / GM) - (position / posMagnitude);
            float eccentricity = eccVec.magnitude;

            // Inclination
            float inclination = Mathf.Acos(angMomentum.z / angMomMag);
            if (deg) inclination *= Mathf.Rad2Deg;   

            // Longitude of the ascending node
            Vector3 nodeVector = Vector3.Cross(Vector3.forward, angMomentum);
            float nodeMag = nodeVector.magnitude;
            float lonAscNode = Mathf.Acos(nodeVector.x / nodeMag);
            if (nodeVector.y < 0)
                lonAscNode = PI2 - lonAscNode;
            if (deg) lonAscNode *= Mathf.Rad2Deg;

            // Argument of periapsis
            float argPeriapsis = Mathf.Acos(Vector3.Dot(nodeVector, eccVec) / (nodeMag * eccentricity));
            if (eccVec.z < 0)
                argPeriapsis = PI2 - argPeriapsis;
            if (deg) argPeriapsis *= Mathf.Rad2Deg;

            // True anomaly
            // float trueAnomaly = Mathf.Acos(Vector3.Dot(eccVec, position) / (eccentricity * posMagnitude));
            // if (Vector3.Dot(position, velocity) < 0)
            //     trueAnomaly = PI2 - trueAnomaly;
            // if (deg) trueAnomaly *= Mathf.Rad2Deg;

            Elements elements = new Elements();
            elements.semimajorAxis = semimajorAxis;
            elements.eccentricity = eccentricity;
            elements.inclination = inclination;
            elements.lonAscNode = lonAscNode;
            elements.argPeriapsis = argPeriapsis;
            // elements.trueAnomaly = trueAnomaly;

            MetaElements meta = new MetaElements();
            meta.sinLonAcsNode = Mathf.Sin(lonAscNode);
            meta.cosLonAcsNode = Mathf.Cos(lonAscNode);
            meta.sinInclination = Mathf.Sin(inclination);
            meta.cosInclination = Mathf.Cos(inclination);
            meta.trueAnomalyConstant = Mathf.Sqrt((1 + eccentricity) / (1 - eccentricity));
            Debug.Log(meta.trueAnomalyConstant + "  " + eccentricity);
            meta.meanMotion = Mathf.Sqrt(GM / Mathf.Pow(semimajorAxis, 3));

            elements.meta = meta;

            return elements;
        }
    }
}
