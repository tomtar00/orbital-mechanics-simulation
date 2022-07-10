using System.Collections.Generic;
using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class HyperbolicOrbit : Orbit
    {
        public HyperbolicOrbit(StateVectors stateVectors, Celestial centralBody) : base(stateVectors, centralBody) { }
        public HyperbolicOrbit(OrbitElements elements, Celestial centralBody) : base(elements, centralBody) { }

        public override OrbitElements CalculateOtherElements(OrbitElements elements)
        {
            float sqrt = MathLib.Sqrt((elements.eccentricity - 1).SafeDivision(elements.eccentricity + 1));
            elements.anomaly = 2f * MathLib.Atanh(sqrt * MathLib.Tan(elements.trueAnomaly / 2f));
            elements.meanAnomaly = (float)(elements.eccentricity * MathLib.Sinh(elements.anomaly) - elements.anomaly);

            elements.meanMotion = MathLib.Sqrt((GM).SafeDivision(MathLib.Pow(-elements.semimajorAxis, 3)));
            elements.semiminorAxis = -elements.semimajorAxis * MathLib.Sqrt(elements.eccentricity * elements.eccentricity - 1f);
            elements.semiLatusRectum = elements.semimajorAxis * (elements.eccentricity * elements.eccentricity - 1f);
            elements.trueAnomalyConstant = MathLib.Sqrt((elements.eccentricity + 1f).SafeDivision(elements.eccentricity - 1f));

            return elements;
        }

        // https://www.orbiter-forum.com/threads/plotting-a-hyperbolic-trajectory.22004/
        public override Vector3 CalculateOrbitalPosition(float trueAnomaly)
        {
            float distance = -elements.semiLatusRectum / (1f + elements.eccentricity * MathLib.Cos(trueAnomaly));

            Vector3 periapsisDir = elements.eccVec.normalized;
            Vector3 pos = Quaternion.AngleAxis(trueAnomaly * Mathf.Rad2Deg, elements.angMomentum) * periapsisDir;

            return pos * distance;
        }
        public override Vector3 CalculateVelocity(Vector3 relativePosition, float trueAnomaly)
        {
            float distance = -elements.semiLatusRectum / (1f + elements.eccentricity * MathLib.Cos(trueAnomaly));
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
            return meanAnomaly;
        }
        public override float CalculateTrueAnomaly(float anomaly)
        {
            return 2f * MathLib.Atan(elements.trueAnomalyConstant * MathLib.Tanh(anomaly / 2f));
        }

        public override double MeanAnomalyEquation(float H, float e, float M)
        {
            return M - e * MathLib.Sinh(H) + H; // M - e*sinh(H) + H = 0
        }
        public override double d_MeanAnomalyEquation(float H, float e)
        {
            return -e * MathLib.Cosh(H) + 1f; //  -e*cosh(H) + 1 = 0
        }

        public override Vector3 GetPointOnOrbit(int i, float orbitFraction, out float meanAnomaly, out float trueAnomaly)
        {
            float theta = MathLib.Acos(-1.0f / elements.eccentricity) - 0.01f;
            float e = elements.eccentricity;
            trueAnomaly = elements.trueAnomaly + i * orbitFraction * 2 * theta;
            float hyperbolicAnomaly = 2 * MathLib.Atanh(MathLib.Sqrt((e - 1) / (e + 1)) * MathLib.Tan(trueAnomaly / 2));
            meanAnomaly = (float)(e * MathLib.Sinh(hyperbolicAnomaly) - hyperbolicAnomaly);
            return CalculateOrbitalPosition(trueAnomaly);
        }
    }
}
