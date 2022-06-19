using System.Collections.Generic;
using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class EllipticOrbit : Orbit
    {
        public EllipticOrbit(StateVectors stateVectors, Celestial centralBody) : base(stateVectors, centralBody) { }
        public EllipticOrbit(OrbitElements elements, Celestial centralBody) : base(elements, centralBody) { }

        public override OrbitElements CalculateOtherElements(OrbitElements elements)
        {
            float sqrt = MathLib.Sqrt((1 - elements.eccentricity).SafeDivision(1 + elements.eccentricity));
            elements.anomaly = 2 * MathLib.Atan(sqrt * MathLib.Tan(elements.trueAnomaly / 2));
            elements.meanAnomaly = elements.anomaly - elements.eccentricity * MathLib.Sin(elements.anomaly);

            elements.semiminorAxis = elements.semimajorAxis * MathLib.Sqrt(1 - elements.eccentricity * elements.eccentricity);
            elements.trueAnomalyConstant = MathLib.Sqrt((1 + elements.eccentricity).SafeDivision(1 - elements.eccentricity));
            elements.meanMotion = MathLib.Sqrt((GM).SafeDivision(MathLib.Pow(elements.semimajorAxis, 3)));
            elements.semiLatusRectum = elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity);

            return elements;
        }

        // source: https://phas.ubc.ca/~newhouse/p210/orbits/cometreport.pdf
        public override Vector3 CalculateOrbitalPosition(float trueAnomaly)
        {
            float distance = (elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity))
                            .SafeDivision(1 + elements.eccentricity * MathLib.Cos(trueAnomaly));

            float cosArgTrue = MathLib.Cos(elements.argPeriapsis + trueAnomaly);
            float sinArgTrue = MathLib.Sin(elements.argPeriapsis + trueAnomaly);

            float sinlon = Mathf.Sin(elements.lonAscNode);
            float coslon = Mathf.Cos(elements.lonAscNode);
            float sininc = Mathf.Sin(elements.inclination);
            float cosinc = Mathf.Cos(elements.inclination);

            float x = distance * ((coslon * cosArgTrue) - (sinlon * sinArgTrue * cosinc));
            float y = distance * ((sinlon * cosArgTrue) + (coslon * sinArgTrue * cosinc));
            float z = distance * (sininc * sinArgTrue);

            return new Vector3(x, y, z);
        }
        public override Vector3 CalculateVelocity(Vector3 relativePosition, float trueAnomaly)
        {
            float distance = (elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity))
                            .SafeDivision(1 + elements.eccentricity * MathLib.Cos(trueAnomaly));
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
            if (meanAnomaly > PI2) meanAnomaly -= PI2;
            return meanAnomaly;
        }
        public override float CalculateTrueAnomaly(float eccentricAnomaly)
        {
            float trueAnomaly = 2f * MathLib.Atan(elements.trueAnomalyConstant * MathLib.Tan(eccentricAnomaly / 2f));
            if (trueAnomaly < 0) trueAnomaly += 2f * MathLib.PI;
            return trueAnomaly;
        }

        public override float MeanAnomalyEquation(float E, float e, float M)
        {
            return M - E + e * MathLib.Sin(E); // M - E + e*sin(E) = 0
        }
        public override float d_MeanAnomalyEquation(float E, float e)
        {
            return -1f + e * MathLib.Cos(E); //  -1 + e*cos(E) = 0
        }

        public override Vector3[] GenerateOrbitPoints(float resolution, out StateVectors stateVectors, out Celestial nextCelestial)
        {
            List<Vector3> points = new List<Vector3>();
            float influenceRadius = this.centralBody.InfluenceRadius;
            bool encounter = false;

            nextCelestial = null;
            stateVectors = null;

            float orbitFraction = 1f / resolution;
            for (int i = 0; i < resolution; i++)
            {
                float eccentricAnomaly = elements.anomaly + i * orbitFraction * PI2;
                if (eccentricAnomaly > PI2) eccentricAnomaly -= PI2;

                float meanAnomalyAtPoint = eccentricAnomaly - elements.eccentricity * MathLib.Sin(eccentricAnomaly);
                float trueAnomaly = CalculateTrueAnomaly(eccentricAnomaly);
                Vector3 position = CalculateOrbitalPosition(trueAnomaly);

                // get time in which object is in this spot
                if (meanAnomalyAtPoint < elements.meanAnomaly) meanAnomalyAtPoint += PI2;
                float time = (meanAnomalyAtPoint - elements.meanAnomaly) / elements.meanMotion;

                // check if outside influence
                if (position.sqrMagnitude < influenceRadius * influenceRadius)
                {
                    points.Add(position);
                }
                else
                {
                    // get escape state vectors
                    Vector3 spacecraftVelocity = CalculateVelocity(position, trueAnomaly);

                    if (!this.centralBody.IsStationary)
                    {
                        (float, float, float) mat = this.centralBody.Kepler.orbit.GetFutureAnomalies(time);
                        Vector3 celestialPosition = this.centralBody.Kepler.orbit.CalculateOrbitalPosition(mat.Item3);
                        Vector3 celestialVelocity = this.centralBody.Kepler.orbit.CalculateVelocity(celestialPosition, trueAnomaly);

                        stateVectors = new StateVectors(position + celestialPosition, spacecraftVelocity + celestialVelocity);
                    }
                    else 
                        stateVectors = new StateVectors(position, spacecraftVelocity);
                    nextCelestial = this.centralBody.CentralBody;
                    break;
                }


                // check if any other object will be in range in that time
                foreach (var celestial in this.centralBody.celestialsOnOrbit)
                {
                    (float, float, float) mat = celestial.Kepler.orbit.GetFutureAnomalies(time);
                    Vector3 celestialPosition = celestial.Kepler.orbit.CalculateOrbitalPosition(mat.Item3);
                    Vector3 relativePosition = (position - celestialPosition);

                    if (relativePosition.sqrMagnitude < MathLib.Pow(celestial.InfluenceRadius, 2))
                    {
                        encounter = true;
Debug.Log("eeee");
                        // get encounter state vectors
                        Vector3 spacecraftVelocity = CalculateVelocity(position, trueAnomaly);
                        Vector3 celestialVelocity = celestial.Kepler.orbit.CalculateVelocity(celestialPosition, mat.Item3);
                        stateVectors = new StateVectors(relativePosition, spacecraftVelocity - celestialVelocity);
                        nextCelestial = celestial;

                        break;
                    }
                }

                if (encounter) break;
            }

            return points.ToArray();
        }
    }
}


