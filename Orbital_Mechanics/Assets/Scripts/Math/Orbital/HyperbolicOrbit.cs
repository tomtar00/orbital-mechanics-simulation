using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class HyperbolicOrbit : Orbit
    {
        public HyperbolicOrbit(Celestial centralBody) : base(centralBody) { }

        public override KeplerianOrbit.Elements CalculateOtherElements(KeplerianOrbit.Elements elements)
        {
            float sqrt = MathLib.Sqrt((elements.eccentricity - 1).SafeDivision(elements.eccentricity + 1));
            elements.anomaly = 2f * MathLib.Atanh(sqrt * MathLib.Tan(elements.trueAnomaly / 2f));
            elements.meanAnomaly = elements.eccentricity * MathLib.Sinh(elements.anomaly) - elements.anomaly; 

            elements.meanMotion = MathLib.Sqrt((GM).SafeDivision(MathLib.Pow(-elements.semimajorAxis, 3)));
            elements.semiminorAxis = -elements.semimajorAxis * MathLib.Sqrt(elements.eccentricity * elements.eccentricity - 1f);
            elements.semiLatusRectum = elements.semimajorAxis * (elements.eccentricity * elements.eccentricity - 1f);
            elements.trueAnomalyConstant = MathLib.Sqrt((elements.eccentricity + 1f).SafeDivision(elements.eccentricity - 1f));
        
            return elements;
        }

        // https://www.orbiter-forum.com/threads/plotting-a-hyperbolic-trajectory.22004/
        public override Vector3 CalculateOrbitalPosition(KeplerianOrbit.Elements elements)
        {      
            float distance = -elements.semiLatusRectum / (1f + elements.eccentricity * MathLib.Cos(elements.trueAnomaly));

            Vector3 periapsisDir = elements.eccVec.normalized;
            Vector3 pos = Quaternion.AngleAxis(elements.trueAnomaly * Mathf.Rad2Deg, elements.angMomentum) * periapsisDir;

            return pos * distance;
        }
        

        public override float CalculateMeanAnomaly(float currentMean, float meanMotion, float time)
        {
            float meanAnomaly = currentMean;
            meanAnomaly += meanMotion * time;
            return meanAnomaly;
        }
        public override float CalculateAnomaly(float eccentricity, float meanAnomaly, float anomaly)
        {
            if (MathLib.Abs(meanAnomaly) > Mathf.PI) {
                return MathLib.Asinh((meanAnomaly + anomaly) / eccentricity);
            }
            else {
                return base.CalculateAnomaly(eccentricity, meanAnomaly, anomaly);
            }
        }
        public override float CalculateTrueAnomaly(float constant, float anomaly)
        {
            return 2f * MathLib.Atan(constant * MathLib.Tanh(anomaly / 2f));
        }

        public override float MeanAnomalyEquation(float H, float e, float M)
        {
            return M - e * MathLib.Sinh(H) + H; // M - e*sinh(H) + H = 0
        }
        public override float d_MeanAnomalyEquation(float H, float e)
        {
            return -e * MathLib.Cosh(H) + 1f; //  -e*cosh(H) + 1 = 0
        }
    }
}
