using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class HyperbolicOrbit : Orbit
    {
        public HyperbolicOrbit(Celestial centralBody) : base(centralBody) { }

        public override void CalculateOtherElements()
        {
            base.CalculateOtherElements();

            float sqrt = MathLib.Sqrt((parent.eccentricity - 1).SafeDivision(parent.eccentricity + 1));
            parent.anomaly = 2f * MathLib.Atanh(sqrt * MathLib.Tan(parent.trueAnomaly / 2f));
            parent.meanAnomaly = parent.eccentricity * MathLib.Sinh(parent.anomaly) - parent.anomaly; 

            parent.meanMotion = MathLib.Sqrt((GM).SafeDivision(MathLib.Pow(-parent.semimajorAxis, 3)));
            parent.semiminorAxis = -parent.semimajorAxis * MathLib.Sqrt(parent.eccentricity * parent.eccentricity - 1f);
            parent.semiLatusRectum = parent.semimajorAxis * (parent.eccentricity * parent.eccentricity - 1f);
            parent.trueAnomalyConstant = MathLib.Sqrt((parent.eccentricity + 1f).SafeDivision(parent.eccentricity - 1f));
        }
        
        // https://www.orbiter-forum.com/threads/plotting-a-hyperbolic-trajectory.22004/
        public override Vector3 CalculateOrbitalPosition(float trueAnomaly)
        {      
            float distance = -parent.semiLatusRectum / (1f + parent.eccentricity * MathLib.Cos(trueAnomaly));

            Vector3 periapsisDir = eccVec.normalized;
            Vector3 pos = Quaternion.AngleAxis(trueAnomaly * Mathf.Rad2Deg, angMomentum) * periapsisDir;

            return pos * distance;
        }

        public override float CalculateMeanAnomaly(float time)
        {
            float meanAnomaly = parent.meanAnomaly;
            meanAnomaly += parent.meanMotion * time;
            return meanAnomaly;
        }
        public override float CalculateAnomaly(float meanAnomaly)
        {
            if (MathLib.Abs(meanAnomaly) > Mathf.PI) {
                return MathLib.Asinh((meanAnomaly + parent.anomaly) / parent.eccentricity);
            }
            else {
                return base.CalculateAnomaly(meanAnomaly);
            }
        }
        public override float CalculateTrueAnomaly(float anomaly)
        {
            return 2f * MathLib.Atan(parent.trueAnomalyConstant * MathLib.Tanh(anomaly / 2f));
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
