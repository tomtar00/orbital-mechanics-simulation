using System.Collections.Generic;
using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class EllipticOrbit : Orbit
    {
        public EllipticOrbit(StateVectors stateVectors, Celestial centralBody) : base(stateVectors, centralBody) { }
        public EllipticOrbit(OrbitElements elements, Celestial centralBody) : base(elements, centralBody) { }

        public override OrbitElements CalculateOtherElements(OrbitElements elements)
        {
            float sqrt = MathLib.Sqrt((1 - elements.eccentricity).SafeDivision(1 + elements.eccentricity));
            elements.anomaly = 2 * MathLib.Atan(sqrt * MathLib.Tan(elements.trueAnomaly / 2));
            elements.meanAnomaly = elements.anomaly - elements.eccentricity * MathLib.Sin(elements.anomaly);

            elements.semiminorAxis = elements.semimajorAxis * MathLib.Sqrt(1 - elements.eccentricity * elements.eccentricity);
            elements.meanMotion = MathLib.Sqrt((GM).SafeDivision(MathLib.Pow(elements.semimajorAxis, 3)));
            elements.semiLatusRectum = elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity);

            elements.trueAnomalyConstant = MathLib.Sqrt((1 + elements.eccentricity).SafeDivision(1 - elements.eccentricity));
            elements.periodConstant = MathLib.Sqrt((MathLib.Pow(elements.semimajorAxis, 3) / GM));
            elements.period = 2 * Mathf.PI * elements.periodConstant;

            return elements;
        }

        // source: https://phas.ubc.ca/~newhouse/p210/orbits/cometreport.pdf
        public override Vector3 CalculateOrbitalPosition(float trueAnomaly)
        {
            float distance = (elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity))
                            .SafeDivision(1 + elements.eccentricity * MathLib.Cos(trueAnomaly));

            float cosArgTrue = MathLib.Cos(elements.argPeriapsis + trueAnomaly);
            float sinArgTrue = MathLib.Sin(elements.argPeriapsis + trueAnomaly);

            // TODO: change to polar coordinates

            float sinlon = Mathf.Sin(elements.lonAscNode);
            float coslon = Mathf.Cos(elements.lonAscNode);
            float sininc = Mathf.Sin(elements.inclination);
            float cosinc = Mathf.Cos(elements.inclination);

            float x = distance * ((coslon * cosArgTrue) - (sinlon * sinArgTrue * cosinc));
            float y = distance * ((sinlon * cosArgTrue) + (coslon * sinArgTrue * cosinc));
            float z = distance * (sininc * sinArgTrue);

            return new Vector3(x, y, z);
        }
        public override Vector3 CalculateVelocity(Vector3 relativePosition, float trueAnomaly)
        {
            float distance = (elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity))
                            .SafeDivision(1 + elements.eccentricity * MathLib.Cos(trueAnomaly));
            float speed = MathLib.Sqrt(GM * ((2f).SafeDivision(distance) - (1f).SafeDivision(elements.semimajorAxis)));

            // source: https://en.wikipedia.org/wiki/Elliptic_orbit#Flight_path_angle
            float e = elements.eccentricity;
            float pathAngle = MathLib.Atan((e * MathLib.Sin(trueAnomaly)) / (1 + e * MathLib.Cos(trueAnomaly)));
            Vector3 radDir = Quaternion.AngleAxis(90, elements.angMomentum) * relativePosition.normalized;
            Vector3 dir = Quaternion.AngleAxis(-pathAngle * MathLib.Rad2Deg, elements.angMomentum) * radDir;

            return dir * speed;
        }

        public override float CalculateMeanAnomaly(float time)
        {
            float meanAnomaly = elements.meanAnomaly;
            meanAnomaly += elements.meanMotion * time;
            if (meanAnomaly > PI2) meanAnomaly -= PI2;
            return meanAnomaly;
        }
        public override float CalculateMeanAnomalyFromAnomaly(float anomaly) {
            return anomaly - elements.eccentricity * MathLib.Sin(anomaly);
        }
        public override float CalculateTrueAnomaly(float eccentricAnomaly)
        {
            float trueAnomaly = 2f * MathLib.Atan(elements.trueAnomalyConstant * MathLib.Tan(eccentricAnomaly / 2f));
            if (trueAnomaly < 0) trueAnomaly += 2f * MathLib.PI;
            return trueAnomaly;
        }
        public override float CalculateAnomalyFromTrueAnomaly(float trueAnomaly) {
            float anomaly = 2f * MathLib.Atan(MathLib.Tan(trueAnomaly / 2f) / elements.trueAnomalyConstant);
            if (anomaly < 0) anomaly += 2f * MathLib.PI;
            return anomaly;
        }

        public override double MeanAnomalyEquation(float E, float e, float M)
        {
            return M - E + e * MathLib.Sin(E); // M - E + e*sin(E) = 0
        }
        public override double d_MeanAnomalyEquation(float E, float e)
        {
            return -1f + e * MathLib.Cos(E); //  -1 + e*cos(E) = 0
        }

        public override Vector3 GetPointOnOrbit(int i, float orbitFraction, out float meanAnomaly, out float trueAnomaly)
        {
            float eccentricAnomaly = elements.anomaly + i * orbitFraction * PI2;
            meanAnomaly = eccentricAnomaly - elements.eccentricity * MathLib.Sin(eccentricAnomaly);
            trueAnomaly = CalculateTrueAnomaly(eccentricAnomaly);
            return CalculateOrbitalPosition(trueAnomaly);
        } 
    }
}


