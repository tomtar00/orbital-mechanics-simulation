using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class ParabolicOrbit : Orbit
    {
        public ParabolicOrbit(KeplerianOrbit trajectory, Celestial centralBody) : base(trajectory, centralBody) { }

        public override void CalculateMainOrbitElements(Vector3 relativePosition, Vector3 velocity)
        {
            base.CalculateMainOrbitElements(relativePosition, velocity);

            // TODO

            CalculateOtherElements();
        }

        public override void CalculateOtherElements()
        {
            base.CalculateOtherElements();

            parent.meanMotion = MathLib.Sqrt(GM);

            // TODO
        }

        public override void CalculateTrueAnomaly(float anomaly)
        {
            throw new System.NotImplementedException();
        }
        public override Vector3 CalculateOrbitalPositionTrue(float trueAnomaly)
        {
            throw new System.NotImplementedException();
        }
        public override Vector3 CalculateVelocity(Vector3 relativePosition, Vector3 orbitNormal, out float speed)
        {
            throw new System.NotImplementedException();
        }

        public override float MeanAnomalyEquation(float anomaly, float e, float M)
        {
            throw new System.NotImplementedException();
        }
        public override float d_MeanAnomalyEquation(float anomaly, float e)
        {
            throw new System.NotImplementedException();
        }
    }
}
