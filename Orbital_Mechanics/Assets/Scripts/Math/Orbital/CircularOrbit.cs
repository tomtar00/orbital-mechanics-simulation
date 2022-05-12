using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class CircularOrbit : KeplerianOrbit
    {
        public CircularOrbit(Celestial centralBody) : base(centralBody) { }

        public override void CalculateMainOrbitElements(Vector3 relativePosition, Vector3 velocity)
        {
            base.CalculateMainOrbitElements(relativePosition, velocity);

            // source: https://en.wikipedia.org/wiki/True_anomaly  
            if (inclination == 0)
            {
                // True longitude
                trueAnomaly = Mathf.Acos(relativePosition.x / posMagnitude);
                if (velocity.x > 0)
                    trueAnomaly = PI2 - trueAnomaly;
            }
            else
            {
                // Argument of latitude
                trueAnomaly = Mathf.Acos(Vector3.Dot(nodeVector, relativePosition).SafeDivision(nodeMag * posMagnitude));
                if (relativePosition.z < 0)
                    trueAnomaly = PI2 - trueAnomaly;
            }
        }

        public override void CalculateOtherElements()
        {
            base.CalculateOtherElements();

            semiminorAxis = semimajorAxis * Mathf.Sqrt(1 - eccentricity * eccentricity);
            trueAnomalyConstant = Mathf.Sqrt((1 + eccentricity).SafeDivision(1 - eccentricity));
            meanMotion = Mathf.Sqrt((GM).SafeDivision(Mathf.Pow(semimajorAxis, 3)));
            semiLatusRectum = semimajorAxis * (1 - eccentricity * eccentricity);
        }
    }
}

