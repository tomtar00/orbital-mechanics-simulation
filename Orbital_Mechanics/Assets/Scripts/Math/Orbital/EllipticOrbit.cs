using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class EllipticOrbit : KeplerianOrbit
    {
        public EllipticOrbit(Celestial centralBody) : base(centralBody) { }

        public override void CalculateMainOrbitElements(Vector3 relativePosition, Vector3 velocity)
        {
            base.CalculateMainOrbitElements(relativePosition, velocity);

            // True anomaly
            // source: https://en.wikipedia.org/wiki/True_anomaly                
            float eccPosDot = Vector3.Dot(eccVec, relativePosition);
            trueAnomaly = Mathf.Acos(Mathf.Clamp(eccPosDot.SafeDivision(eccentricity * posMagnitude), -1f, 1f));
            if (Vector3.Dot(relativePosition, velocity) < 0)
                trueAnomaly = PI2 - trueAnomaly;
        }

        public override void CalculateOtherElements()
        {
            base.CalculateOtherElements();

            semiminorAxis = semimajorAxis * Mathf.Sqrt(1 - eccentricity * eccentricity);
            trueAnomalyConstant = Mathf.Sqrt((1 + eccentricity).SafeDivision(1 - eccentricity));
            meanMotion = Mathf.Sqrt((GM).SafeDivision(Mathf.Pow(semimajorAxis, 3)));
            semiLatusRectum = semimajorAxis * (1 - eccentricity * eccentricity);
        }

        public override void CalculateTrueAnomaly(float eccentricAnomaly) {
            trueAnomaly = 2 * Mathf.Atan(trueAnomalyConstant * Mathf.Tan(eccentricAnomaly / 2));
            if (trueAnomaly < 0) trueAnomaly += 2 * Mathf.PI;
        }

        // source: https://phas.ubc.ca/~newhouse/p210/orbits/cometreport.pdf
        public override Vector3 CalculateOrbitalPositionTrue(float trueAnomaly)
        {
            float distance = (semimajorAxis * (1 - eccentricity * eccentricity)).SafeDivision(1 + eccentricity * Mathf.Cos(trueAnomaly));

            float cosArgTrue = Mathf.Cos(argPeriapsis + trueAnomaly);
            float sinArgTrue = Mathf.Sin(argPeriapsis + trueAnomaly);

            float x = distance * ((cosLonAcsNode * cosArgTrue) - (sinLonAcsNode * sinArgTrue * cosInclination));
            float y = distance * ((sinLonAcsNode * cosArgTrue) + (cosLonAcsNode * sinArgTrue * cosInclination));
            float z = distance * (sinInclination * sinArgTrue);

            return new Vector3(x, y, z);
        }
    }
}


