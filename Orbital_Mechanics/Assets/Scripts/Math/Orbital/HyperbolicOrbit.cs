using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class HyperbolicOrbit : Orbit
    {
        public HyperbolicOrbit(KeplerianOrbit trajectory, Celestial centralBody) : base(trajectory, centralBody) { }

        public override void CalculateMainOrbitElements(Vector3 relativePosition, Vector3 velocity)
        {
            base.CalculateMainOrbitElements(relativePosition, velocity);

            // True anomaly
            // source: https://en.wikipedia.org/wiki/True_anomaly                
            float eccPosDot = Vector3.Dot(eccVec, relativePosition);
            parent.trueAnomaly = MathLib.Acos(eccPosDot.SafeDivision(parent.eccentricity * posMagnitude));
            if (Vector3.Dot(relativePosition, velocity) < 0)
                parent.trueAnomaly = PI2 - parent.trueAnomaly;
                
            CalculateOtherElements();

            float sqrt = MathLib.Sqrt((parent.eccentricity - 1).SafeDivision(parent.eccentricity + 1));
            parent.anomaly = 2 * MathLib.Atanh(sqrt * MathLib.Tan(parent.trueAnomaly / 2));
            parent.meanAnomaly = parent.eccentricity * MathLib.Sinh(parent.anomaly) - parent.anomaly;   
        }

        public override void CalculateOtherElements()
        {
            base.CalculateOtherElements();

            parent.meanMotion = MathLib.Sqrt((GM).SafeDivision(MathLib.Pow(-parent.semimajorAxis, 3)));
            parent.semiminorAxis = -parent.semimajorAxis * MathLib.Sqrt(parent.eccentricity * parent.eccentricity - 1);
            parent.semiLatusRectum = parent.semimajorAxis * (parent.eccentricity * parent.eccentricity - 1);
            parent.trueAnomalyConstant = MathLib.Sqrt((parent.eccentricity + 1).SafeDivision(parent.eccentricity - 1));
        }

        public override void CalculateTrueAnomaly(float anomaly)
        {
            parent.trueAnomaly = 2 * MathLib.Atan(parent.trueAnomalyConstant * MathLib.Tanh(anomaly / 2f));
        }

        // https://www.orbiter-forum.com/threads/plotting-a-hyperbolic-trajectory.22004/
        public override Vector3 CalculateOrbitalPositionTrue(float trueAnomaly)
        {      
            float distance = -parent.semiLatusRectum / (1 + parent.eccentricity * MathLib.Cos(trueAnomaly));

            Vector3 periapsisDir = eccVec.normalized;
            Vector3 pos = Quaternion.AngleAxis(trueAnomaly * Mathf.Rad2Deg, angMomentum) * periapsisDir;

            return pos * distance;
        }
        public override Vector3 CalculateVelocity(Vector3 relativePosition, Vector3 orbitNormal, out float speed)
        {
            float posDst = relativePosition.magnitude;
            speed = MathLib.Sqrt(GM * ((2f).SafeDivision(posDst) - (1f).SafeDivision(parent.semimajorAxis)));

            // https://en.wikipedia.org/wiki/Hyperbolic_trajectory flight path angle
            float e = parent.eccentricity;
            float pathAngle = MathLib.Atan((e * MathLib.Sin(parent.trueAnomaly)) / (1 + e * MathLib.Cos(parent.trueAnomaly)));
            Vector3 radDir = Quaternion.AngleAxis(90, orbitNormal) * relativePosition.normalized;
            Vector3 dir = Quaternion.AngleAxis(-pathAngle * MathLib.Rad2Deg, orbitNormal) * radDir;

            return dir * speed;
        }

        public override float CalculateAnomaly(float time)
        {
            parent.meanAnomaly += parent.meanMotion * time;
            return CalculateAnomalyFromMean(parent.meanAnomaly);
        }
        public override float CalculateAnomalyFromMean(float meanAnomaly)
        {
            if (MathLib.Abs(meanAnomaly) > Mathf.PI) {
                return MathLib.Asinh((meanAnomaly + parent.anomaly) / parent.eccentricity);
            }
            else {
                return base.CalculateAnomalyFromMean(meanAnomaly);
            }
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
