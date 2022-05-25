using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class CircularOrbit : Orbit
    {
        public CircularOrbit(KeplerianOrbit trajectory, Celestial centralBody) : base(trajectory, centralBody) { }

        public override void CalculateMainOrbitElements(Vector3 relativePosition, Vector3 velocity)
        {
            base.CalculateMainOrbitElements(relativePosition, velocity);

            // source: https://en.wikipedia.org/wiki/True_anomaly  
            if (parent.inclination == 0)
            {
                // True longitude
                parent.trueAnomaly = MathLib.Acos(relativePosition.x / posMagnitude);
                if (velocity.x > 0)
                    parent.trueAnomaly = PI2 - parent.trueAnomaly;
            }
            else
            {
                // Argument of latitude
                parent.trueAnomaly = MathLib.Acos(Vector3.Dot(nodeVector, relativePosition).SafeDivision(nodeMag * posMagnitude));
                if (relativePosition.z < 0)
                    parent.trueAnomaly = PI2 - parent.trueAnomaly;
            }

            CalculateOtherElements();
        }
        public override void CalculateOtherElements()
        {
            base.CalculateOtherElements();

            parent.semiminorAxis = parent.semimajorAxis;
            parent.trueAnomalyConstant = 1;
            parent.meanMotion = MathLib.Sqrt((GM).SafeDivision(MathLib.Pow(parent.semimajorAxis, 3)));
            parent.semiLatusRectum = parent.semimajorAxis;
        }
        
        public override void CalculateTrueAnomaly(float anomaly)
        {
            parent.trueAnomaly = parent.meanAnomaly;
        }
        public override Vector3 CalculateOrbitalPositionTrue(float trueAnomaly)
        {
            float distance = parent.semimajorAxis; // OK?

            float cosArgTrue = MathLib.Cos(parent.argPeriapsis + trueAnomaly);
            float sinArgTrue = MathLib.Sin(parent.argPeriapsis + trueAnomaly);

            float x = distance * ((parent.cosLonAcsNode * cosArgTrue) - (parent.sinLonAcsNode * sinArgTrue * parent.cosInclination));
            float y = distance * ((parent.sinLonAcsNode * cosArgTrue) + (parent.cosLonAcsNode * sinArgTrue * parent.cosInclination));
            float z = distance * (parent.sinInclination * sinArgTrue);

            return new Vector3(x, y, z);
        }
        public override Vector3 CalculateVelocity(Vector3 relativePosition, Vector3 orbitNormal, out float speed)
        {
            float posDst = relativePosition.magnitude;
            speed = MathLib.Sqrt(GM * ((2f).SafeDivision(posDst) - (1f).SafeDivision(parent.semimajorAxis)));

            return Vector3.Cross(relativePosition, orbitNormal).normalized * speed;
        }

        public override float MeanAnomalyEquation(float anomaly, float e, float M)
        {
            return 0f;
        }
        public override float d_MeanAnomalyEquation(float anomaly, float e)
        {
            return 1f;
        }
    }
}

