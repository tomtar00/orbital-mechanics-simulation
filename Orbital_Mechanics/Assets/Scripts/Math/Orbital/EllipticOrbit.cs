using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class EllipticOrbit : Orbit
    {
        public EllipticOrbit(KeplerianOrbit trajectory, Celestial centralBody) : base(trajectory, centralBody) { }

        // CONVERTER https://janus.astro.umd.edu/orbits/elements/convertframe.html
        public override void CalculateMainOrbitElements(Vector3 relativePosition, Vector3 velocity)
        {
            base.CalculateMainOrbitElements(relativePosition, velocity);

            // True anomaly
            // source: https://en.wikipedia.org/wiki/True_anomaly                
            float eccPosDot = Vector3.Dot(eccVec, relativePosition);
            parent.trueAnomaly = MathLib.Acos(eccPosDot.SafeDivision(parent.eccentricity * posMagnitude));
            if (Vector3.Dot(relativePosition, velocity) < 0)
                parent.trueAnomaly = PI2 - parent.trueAnomaly;

            parent.meanAnomaly = ConvertTrueToMeanAnomaly(parent.trueAnomaly, parent.eccentricity);
            CalculateOtherElements();
        }
        public override void CalculateOtherElements()
        {
            base.CalculateOtherElements();

            parent.semiminorAxis = parent.semimajorAxis * MathLib.Sqrt(1 - parent.eccentricity * parent.eccentricity);
            parent.trueAnomalyConstant = MathLib.Sqrt((1 + parent.eccentricity).SafeDivision(1 - parent.eccentricity));
            parent.meanMotion = MathLib.Sqrt((GM).SafeDivision(MathLib.Pow(parent.semimajorAxis, 3)));
            parent.semiLatusRectum = parent.semimajorAxis * (1 - parent.eccentricity * parent.eccentricity);
        }
        public override void CalculateTrueAnomaly(float eccentricAnomaly) {
            parent.trueAnomaly = 2 * MathLib.Atan(parent.trueAnomalyConstant * MathLib.Tan(eccentricAnomaly / 2));
            if (parent.trueAnomaly < 0) parent.trueAnomaly += 2 * MathLib.PI;
        }
        // source: https://phas.ubc.ca/~newhouse/p210/orbits/cometreport.pdf
        public override Vector3 CalculateOrbitalPositionTrue(float trueAnomaly)
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
        // source: https://www.orbiter-forum.com/threads/calculate-not-derive-orbital-velocity-vector.22367/
        public override Vector3 CalculateVelocity(Vector3 relativePosition, Vector3 orbitNormal, out float speed)
        {
            float posDst = relativePosition.magnitude;
            speed = MathLib.Sqrt(GM * ((2f).SafeDivision(posDst) - (1f).SafeDivision(parent.semimajorAxis)));

            float e = parent.eccentricity;
            float k = posDst.SafeDivision(parent.semimajorAxis);
            float alpha = MathLib.Acos((2 - 2 * e * e).SafeDivision(k * (2 - k)) - 1);
            float angle = alpha + ((MathLib.PI - alpha) / 2);
            if (parent.trueAnomaly < MathLib.PI)
                angle = MathLib.PI - angle;
            return Quaternion.AngleAxis(angle * MathLib.Rad2Deg, orbitNormal) * (relativePosition.SafeDivision(posDst)) * speed;
        } 

        public override float MeanAnomalyEquation(float E, float e, float M)
        {
            return M - E + e * MathLib.Sin(E);
        }
        public override float d_MeanAnomalyEquation(float E, float e)
        {
            return e * MathLib.Cos(E) - 1f;
        }

        // source: https://en.wikipedia.org/wiki/Mean_anomaly
        public static float ConvertTrueToMeanAnomaly(float trueAnomaly, float eccentricity)
        {
            float sinTrue = MathLib.Sin(trueAnomaly);
            float cosTrue = MathLib.Cos(trueAnomaly);
            float e = eccentricity;

            float y = (MathLib.Sqrt(1 - e * e) * sinTrue).SafeDivision(1 + e * cosTrue);
            float x = (e + cosTrue).SafeDivision(1 + e * cosTrue);
            float mean = MathLib.Atan2(y, x) - e * y;
            float PI2 = 2 * MathLib.PI;

            if (mean < 0)
            {
                mean += PI2;
            }
            else if (mean > PI2)
            {
                mean -= PI2;
            }

            return mean;
        }
    }
}


