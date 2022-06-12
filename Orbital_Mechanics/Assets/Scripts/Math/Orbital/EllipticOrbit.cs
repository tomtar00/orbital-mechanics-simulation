using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class EllipticOrbit : Orbit
    {
        public EllipticOrbit(KeplerianOrbit trajectory, Celestial centralBody) : base(trajectory, centralBody) { }

        public override void CalculateOtherElements()
        {
            base.CalculateOtherElements();

            float sqrt = MathLib.Sqrt((1 - parent.eccentricity).SafeDivision(1 + parent.eccentricity));
            parent.anomaly = 2 * MathLib.Atan(sqrt * MathLib.Tan(parent.trueAnomaly / 2));
            parent.meanAnomaly = parent.anomaly - parent.eccentricity * MathLib.Sin(parent.anomaly);

            parent.semiminorAxis = parent.semimajorAxis * MathLib.Sqrt(1 - parent.eccentricity * parent.eccentricity);
            parent.trueAnomalyConstant = MathLib.Sqrt((1 + parent.eccentricity).SafeDivision(1 - parent.eccentricity));
            parent.meanMotion = MathLib.Sqrt((GM).SafeDivision(MathLib.Pow(parent.semimajorAxis, 3)));
            parent.semiLatusRectum = parent.semimajorAxis * (1 - parent.eccentricity * parent.eccentricity);
        }
        
        // source: https://phas.ubc.ca/~newhouse/p210/orbits/cometreport.pdf
        public override Vector3 CalculateOrbitalPosition(float trueAnomaly)
        {
            float distance = (parent.semimajorAxis * (1 - parent.eccentricity * parent.eccentricity))
                            .SafeDivision(1 + parent.eccentricity * MathLib.Cos(trueAnomaly));

            float cosArgTrue = MathLib.Cos(parent.argPeriapsis + trueAnomaly);
            float sinArgTrue = MathLib.Sin(parent.argPeriapsis + trueAnomaly);

            float x = distance * ((parent.cosLonAcsNode * cosArgTrue) - (parent.sinLonAcsNode * sinArgTrue * parent.cosInclination));
            float y = distance * ((parent.sinLonAcsNode * cosArgTrue) + (parent.cosLonAcsNode * sinArgTrue * parent.cosInclination));
            float z = distance * (parent.sinInclination * sinArgTrue);

            return new Vector3(x, y, z);
        }

        public override float CalculateMeanAnomaly(float time)
        {
            float meanAnomaly = parent.meanAnomaly;
            meanAnomaly += parent.meanMotion * time;
            if (meanAnomaly > PI2) meanAnomaly -= PI2;
            return meanAnomaly;
        }
        public override float CalculateTrueAnomaly(float eccentricAnomaly) {
            float trueAnomaly = 2f * MathLib.Atan(parent.trueAnomalyConstant * MathLib.Tan(eccentricAnomaly / 2f));
            if (trueAnomaly < 0) trueAnomaly += 2f * MathLib.PI;
            return trueAnomaly;
        }

        public override float MeanAnomalyEquation(float E, float e, float M)
        {
            return M - E + e * MathLib.Sin(E); // M - E + e*sin(E) = 0
        }
        public override float d_MeanAnomalyEquation(float E, float e)
        {
            return -1f + e * MathLib.Cos(E); //  -1 + e*cos(E) = 0
        }
    }
}


