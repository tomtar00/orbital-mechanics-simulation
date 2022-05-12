using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class ParabolicOrbit : KeplerianOrbit
    {
        public ParabolicOrbit(Celestial centralBody) : base(centralBody) { }

        public override void CalculateMainOrbitElements(Vector3 relativePosition, Vector3 velocity)
        {
            base.CalculateMainOrbitElements(relativePosition, velocity);

            // TODO
        }

        public override void CalculateOtherElements()
        {
            base.CalculateOtherElements();

            meanMotion = Mathf.Sqrt(GM);

            // TODO
        }
    }
}
