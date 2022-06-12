using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class EllipticOrbit : Orbit
    {
        public EllipticOrbit(Celestial centralBody) : base(centralBody) { }

        public override KeplerianOrbit.Elements CalculateOtherElements(KeplerianOrbit.Elements elements)
        {
            float sqrt = MathLib.Sqrt((1 - elements.eccentricity).SafeDivision(1 + elements.eccentricity));
            elements.anomaly = 2 * MathLib.Atan(sqrt * MathLib.Tan(elements.trueAnomaly / 2));
            elements.meanAnomaly = elements.anomaly - elements.eccentricity * MathLib.Sin(elements.anomaly);

            elements.semiminorAxis = elements.semimajorAxis * MathLib.Sqrt(1 - elements.eccentricity * elements.eccentricity);
            elements.trueAnomalyConstant = MathLib.Sqrt((1 + elements.eccentricity).SafeDivision(1 - elements.eccentricity));
            elements.meanMotion = MathLib.Sqrt((GM).SafeDivision(MathLib.Pow(elements.semimajorAxis, 3)));
            elements.semiLatusRectum = elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity);

            return elements;
        }
        
        // source: https://phas.ubc.ca/~newhouse/p210/orbits/cometreport.pdf
        public override Vector3 CalculateOrbitalPosition(KeplerianOrbit.Elements elements)
        {
            float distance = (elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity))
                            .SafeDivision(1 + elements.eccentricity * MathLib.Cos(elements.trueAnomaly));

            // float cosArgTrue = MathLib.Cos(elements.argPeriapsis + elements.trueAnomaly);
            // float sinArgTrue = MathLib.Sin(elements.argPeriapsis + elements.trueAnomaly);

            // float x = distance * ((elements.cosLonAcsNode * cosArgTrue) - (elements.sinLonAcsNode * sinArgTrue * elements.cosInclination));
            // float y = distance * ((elements.sinLonAcsNode * cosArgTrue) + (elements.cosLonAcsNode * sinArgTrue * elements.cosInclination));
            // float z = distance * (elements.sinInclination * sinArgTrue);

            // return new Vector3(x, y, z);

            Vector3 periapsisDir = elements.eccVec.normalized;
            Debug.Log(periapsisDir);
            Vector3 pos = Quaternion.AngleAxis(elements.trueAnomaly * Mathf.Rad2Deg, elements.angMomentum) * periapsisDir;

            return pos * distance;
        }

        public override float CalculateMeanAnomaly(float currentMean, float meanMotion, float time)
        {
            float meanAnomaly = currentMean;
            meanAnomaly += meanMotion * time;
            if (meanAnomaly > PI2) meanAnomaly -= PI2;
            return meanAnomaly;
        }
        public override float CalculateTrueAnomaly(float constant, float eccentricAnomaly) {
            float trueAnomaly = 2f * MathLib.Atan(constant * MathLib.Tan(eccentricAnomaly / 2f));
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


