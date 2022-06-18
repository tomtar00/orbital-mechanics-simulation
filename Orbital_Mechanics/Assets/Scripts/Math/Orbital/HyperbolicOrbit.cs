using System.Collections.Generic;
using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class HyperbolicOrbit : Orbit
    {
        public HyperbolicOrbit(StateVectors stateVectors, Celestial centralBody) : base(stateVectors, centralBody) { }
        public HyperbolicOrbit(KeplerianOrbit.Elements elements, Celestial centralBody) : base(elements, centralBody) { }

        public override KeplerianOrbit.Elements CalculateOtherElements(KeplerianOrbit.Elements elements)
        {
            float sqrt = MathLib.Sqrt((elements.eccentricity - 1).SafeDivision(elements.eccentricity + 1));
            elements.anomaly = 2f * MathLib.Atanh(sqrt * MathLib.Tan(elements.trueAnomaly / 2f));
            elements.meanAnomaly = elements.eccentricity * MathLib.Sinh(elements.anomaly) - elements.anomaly; 

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

        public override float CalculateMeanAnomaly(float time)
        {
            float meanAnomaly = elements.meanAnomaly;
            meanAnomaly += elements.meanMotion * time;
            return meanAnomaly;
        }
        public override float CalculateAnomaly(float meanAnomaly)
        {
            if (MathLib.Abs(meanAnomaly) > Mathf.PI) {
                return MathLib.Asinh((meanAnomaly + elements.anomaly) / elements.eccentricity);
            }
            else {
                return base.CalculateAnomaly(meanAnomaly);
            }
        }
        public override float CalculateTrueAnomaly(float anomaly)
        {
            return 2f * MathLib.Atan(elements.trueAnomalyConstant * MathLib.Tanh(anomaly / 2f));
        }

        public override float MeanAnomalyEquation(float H, float e, float M)
        {
            return M - e * MathLib.Sinh(H) + H; // M - e*sinh(H) + H = 0
        }
        public override float d_MeanAnomalyEquation(float H, float e)
        {
            return -e * MathLib.Cosh(H) + 1f; //  -e*cosh(H) + 1 = 0
        }

        public override Vector3[] GenerateOrbitPoints(float resolution, InOrbitObject inOrbitObject, out StateVectors stateVectors)
        {
            List<Vector3> points = new List<Vector3>();
            float influenceRadius = inOrbitObject.CentralBody.InfluenceRadius;
            bool encounter = false;
            bool outsideInfluence = false;
            Vector3 lastPosition = Vector3.zero;
            float theta = MathLib.Acos(-1.0f / elements.eccentricity) - 0.01f;

           float orbitFraction = 1f / (resolution - 1);
            for (int i = 0; i < resolution; i++)
            {
                float trueAnomaly = elements.trueAnomaly + i * orbitFraction * 2 * theta;
                Vector3 position = CalculateOrbitalPosition(trueAnomaly);

                if (position.sqrMagnitude < influenceRadius * influenceRadius)
                {
                    points.Add(position);
                    lastPosition = position;
                }
                else {
                    outsideInfluence = true;
                    break;
                }

                // get time in which object is in this spot
                float e = elements.eccentricity;
                float hyperbolicAnomaly = 2 * MathLib.Atanh(MathLib.Sqrt((e - 1) / (e + 1)) * MathLib.Tan(trueAnomaly / 2));
                float nextMean = e * MathLib.Sinh(hyperbolicAnomaly) - hyperbolicAnomaly;
                if (nextMean < elements.meanAnomaly) nextMean += 2 * Mathf.PI;
                float time = (nextMean - elements.meanAnomaly) / elements.meanMotion;
                // check if any other object will be in range in that time
                foreach (var celestial in inOrbitObject.CentralBody.celestialsOnOrbit)
                {
                    (float, float, float) mat = celestial.Kepler.orbit.GetFutureAnomalies(time);
                    Vector3 pos = celestial.Kepler.orbit.CalculateOrbitalPosition(mat.Item3);
                    if ((position - pos).sqrMagnitude < MathLib.Pow(celestial.InfluenceRadius, 2))
                    {
                        encounter = true;
                        break;
                    }
                }

                if (encounter) break;
            }

            if (outsideInfluence || encounter) {
                Vector3 velocity = CalculateVelocity(lastPosition);
                stateVectors = new StateVectors(lastPosition, velocity);
            }
            else
                stateVectors = null;
            return points.ToArray();
        }
    }
}
