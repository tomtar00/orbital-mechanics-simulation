using UnityEngine;
using Sim.Objects;
using Sim.Math;

namespace Sim.Orbits
{
    public class HyperbolicOrbit : Orbit
    {
        public HyperbolicOrbit(StateVectors stateVectors, Celestial centralBody) : base(stateVectors, centralBody) { }
        public HyperbolicOrbit(OrbitalElements elements, Celestial centralBody) : base(elements, centralBody) { }

        public override OrbitalElements CalculateOtherElements(OrbitalElements elements)
        {
            double sqrt = MathLib.Sqrt((elements.eccentricity - 1).SafeDivision(elements.eccentricity + 1));
            elements.anomaly = 2f * MathLib.Atanh(sqrt * MathLib.Tan(elements.trueAnomaly / 2f));
            elements.meanAnomaly = (double)(elements.eccentricity * MathLib.Sinh(elements.anomaly) - elements.anomaly);

            elements.meanMotion = MathLib.Sqrt((GM).SafeDivision(MathLib.Pow(-elements.semimajorAxis, 3)));
            elements.semiminorAxis = -elements.semimajorAxis * MathLib.Sqrt(elements.eccentricity * elements.eccentricity - 1f);
            elements.semiLatusRectum = elements.semimajorAxis * (elements.eccentricity * elements.eccentricity - 1f);

            elements.trueAnomalyConstant = MathLib.Sqrt((elements.eccentricity + 1f).SafeDivision(elements.eccentricity - 1f));

            return elements;
        }

        // https://www.orbiter-forum.com/threads/plotting-a-hyperbolic-trajectory.22004/
        public override Vector3Double CalculateOrbitalPosition(double trueAnomaly)
        {
            double distance = -elements.semiLatusRectum / (1f + elements.eccentricity * MathLib.Cos(trueAnomaly));

            Vector3Double periapsisDir = elements.eccVec.normalized;
            Vector3Double pos = (Quaternion.AngleAxis((float)(-trueAnomaly * MathLib.Rad2Deg), elements.angMomentum) * periapsisDir);

            return pos * distance;
        }
        public override Vector3Double CalculateVelocity(Vector3Double relativePosition, double trueAnomaly)
        {
            this.distance = -elements.semiLatusRectum / (1f + elements.eccentricity * MathLib.Cos(trueAnomaly));
            this.speed = MathLib.Sqrt(GM * ((2.0).SafeDivision(distance) - (1.0).SafeDivision(elements.semimajorAxis)));

            // source: https://en.wikipedia.org/wiki/Elliptic_orbit#Flight_path_angle
            double pathAngle = MathLib.Atan((elements.eccentricity * MathLib.Sin(trueAnomaly)) / (1 + elements.eccentricity * MathLib.Cos(trueAnomaly))) * MathLib.Rad2Deg;
            return (Quaternion.AngleAxis((float)pathAngle, elements.angMomentum) *
                            Quaternion.AngleAxis(-90, elements.angMomentum) * relativePosition.normalized *
                            (float)this.speed);
        }

        public override double CalculateMeanAnomaly(double time)
        {
            double meanAnomaly = elements.meanAnomaly;
            meanAnomaly += elements.meanMotion * time;
            return meanAnomaly;
        }
        public override double CalculateMeanAnomalyFromAnomaly(double anomaly) {
            return (double)((double)elements.eccentricity * MathLib.Sinh(anomaly) - (double)anomaly);
        }
        public override double CalculateTrueAnomaly(double anomaly)
        {
            return 2f * MathLib.Atan(elements.trueAnomalyConstant * MathLib.Tanh(anomaly / 2f));
        }
        public override double CalculateAnomalyFromTrueAnomaly(double trueAnomaly) {
            return 2f * MathLib.Atan(MathLib.Tanh(trueAnomaly / 2f) / elements.trueAnomalyConstant);
        }

        public override double MeanAnomalyEquation(double H, double e, double M)
        {
            return M - e * MathLib.Sinh(H) + H; // M - e*sinh(H) + H = 0
        }
        public override double d_MeanAnomalyEquation(double H, double e)
        {
            return -e * MathLib.Cosh(H) + 1f; //  -e*cosh(H) + 1 = 0
        }

        public override Vector3Double GetPointOnOrbit(int i, double orbitFraction, out double meanAnomaly, out double trueAnomaly)
        {
            double theta = MathLib.Acos(-1.0f / elements.eccentricity) - 0.01f;
            double e = elements.eccentricity;
            trueAnomaly = elements.trueAnomaly + i * orbitFraction * 2 * theta;
            double hyperbolicAnomaly = 2 * MathLib.Atanh(MathLib.Sqrt((e - 1) / (e + 1)) * MathLib.Tan(trueAnomaly / 2));
            meanAnomaly = (double)(e * MathLib.Sinh(hyperbolicAnomaly) - hyperbolicAnomaly);
            return CalculateOrbitalPosition(trueAnomaly);
        }
    }
}
