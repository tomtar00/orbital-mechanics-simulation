using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class HyperbolicOrbit : KeplerianOrbit
    {
        public HyperbolicOrbit(Celestial centralBody) : base(centralBody) { }

        public override void CalculateMainOrbitElements(Vector3 relativePosition, Vector3 velocity)
        {
            base.CalculateMainOrbitElements(relativePosition, velocity);

            // TODO
        }

        public override void CalculateOtherElements()
        {
            base.CalculateOtherElements();

            meanMotion = Mathf.Sqrt((GM).SafeDivision(Mathf.Pow(-semimajorAxis, 3)));
            semiminorAxis = -semimajorAxis * Mathf.Sqrt(eccentricity * eccentricity - 1);
            semiLatusRectum = semimajorAxis * (eccentricity * eccentricity - 1);
            //trueAnomalyConstant?
        }
    }
}
