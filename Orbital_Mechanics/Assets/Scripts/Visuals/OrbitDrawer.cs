using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sim.Math;
using Sim.Objects;

namespace Sim.Visuals
{
    public class OrbitDrawer : MonoBehaviour
    {
        [SerializeField] private int orbitResolution = 30;

        private LineRenderer lineRenderer;
        private GameObject rendererObject;

        private float anomalyBeyondInfluence = 0;
        private float lastAnomaly = 0;
        private float PI2 = 2 * MathLib.PI;
        private List<Vector3> points = new List<Vector3>();

        private InOrbitObject inOrbitObject;

        public void SetupOrbitRenderer(InOrbitObject obj, Transform celestial)
        {
            this.inOrbitObject = obj;
            rendererObject = new GameObject("Orbit Renderer");
            rendererObject.transform.SetParent(celestial);
            rendererObject.transform.localPosition = Vector3.zero;
            lineRenderer = rendererObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.startWidth = .1f;
            lineRenderer.loop = true;
        }

        public void DestroyOrbitRenderer()
        {
            Destroy(rendererObject);
        }

        public void DrawOrbit(KeplerianOrbit trajectory, float influenceRadius)
        {
            anomalyBeyondInfluence = 0;
            lastAnomaly = 0;
            points = new List<Vector3>();

            float e = trajectory.eccentricity;
            if (e >= 0 && e < 1)
            {
                DrawElliptic(trajectory, influenceRadius);
            }
            else if (e >= 1)
            {
                DrawHyperbolic(trajectory, influenceRadius);
            }
        }

        private void DrawElliptic(KeplerianOrbit trajectory, float influenceRadius)
        {
            lineRenderer.positionCount = 0;

            bool encounter = false;
            float currentAnomaly = trajectory.anomaly;
            float currentMean = trajectory.meanAnomaly;

            // select points
            float orbitFraction = 1f / orbitResolution;
            for (int i = 0; i < orbitResolution; i++)
            {
                float eccentricAnomaly = currentAnomaly + i * orbitFraction * PI2;
                if (eccentricAnomaly > PI2) eccentricAnomaly -= PI2;

                float meanAnomalyAtPoint = eccentricAnomaly - trajectory.eccentricity * MathLib.Sin(eccentricAnomaly);
                float trueAnomaly = trajectory.orbit.CalculateTrueAnomaly(eccentricAnomaly);
                Vector3 position = trajectory.orbit.CalculateOrbitalPosition(trueAnomaly);

                if (position.sqrMagnitude < influenceRadius * influenceRadius)
                {
                    points.Add(position);
                    lastAnomaly = trueAnomaly;
                }
                else
                {
                    anomalyBeyondInfluence = trueAnomaly;
                    break;
                }

                // get time in which object is in this spot
                if (meanAnomalyAtPoint < currentMean) meanAnomalyAtPoint += PI2;
                float time = (meanAnomalyAtPoint - currentMean) / trajectory.meanMotion;
                // check if any other object will be in range in that time
                foreach (var celestial in inOrbitObject.CentralBody.celestialsOnOrbit)
                {
                    float _meanAnomaly = celestial.Trajectory.orbit.CalculateMeanAnomaly(time);
                    float _anomaly = celestial.Trajectory.orbit.CalculateAnomaly(_meanAnomaly);
                    float _trueAnomaly = celestial.Trajectory.orbit.CalculateTrueAnomaly(_anomaly);
                    Vector3 pos = celestial.Trajectory.orbit.CalculateOrbitalPosition(_trueAnomaly);

                    if ((position - pos).sqrMagnitude < MathLib.Pow(celestial.InfluenceRadius, 2))
                    {
                        // Debug.Log($"Arriving to {celestial.name} in {time}");
                        encounter = true;
                        break;
                    }
                }

                if (encounter) break;
            }

            // check if exited influence
            if (!encounter)
                AddLastPointBeyondInfluence(trajectory, influenceRadius);
            else lineRenderer.loop = false;

            // draw trajectory / orbit
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }
        private void DrawHyperbolic(KeplerianOrbit trajectory, float influenceRadius)
        {
            lineRenderer.positionCount = 0;

            bool encounter = false;
            float currentTrue = trajectory.trueAnomaly;
            float currentMean = trajectory.meanAnomaly;
            float theta = MathLib.Acos(-1.0f / trajectory.eccentricity) - 0.01f;

            // select points
            float orbitFraction = 1f / (orbitResolution - 1);
            for (int i = 0; i < orbitResolution; i++)
            {
                float trueAnomaly = currentTrue + i * orbitFraction * 2 * theta;
                Vector3 position = trajectory.orbit.CalculateOrbitalPosition(trueAnomaly);

                if (position.sqrMagnitude < influenceRadius * influenceRadius)
                {
                    points.Add(position);
                    lastAnomaly = trueAnomaly;
                }
                else
                {
                    anomalyBeyondInfluence = trueAnomaly;
                    break;
                }
                
                // get time in which object is in this spot
                float e = trajectory.eccentricity;
                float hyperbolicAnomaly = 2 * MathLib.Atanh(MathLib.Sqrt((e - 1) / (e + 1)) * MathLib.Tan(trueAnomaly / 2));
                float nextMean = trajectory.eccentricity * MathLib.Sinh(hyperbolicAnomaly) - hyperbolicAnomaly;
                if (nextMean < currentMean) nextMean += 2*Mathf.PI;
                float time = (nextMean - currentMean) / trajectory.meanMotion;
                // check if any other object will be in range in that time
                foreach (var celestial in inOrbitObject.CentralBody.celestialsOnOrbit)
                {
                    var mat = celestial.Trajectory.orbit.UpdateAnomalies(celestial.Trajectory.elements, time);
                    Vector3 pos = celestial.Trajectory.orbit.CalculateOrbitalPosition(mat.Item3);

                    if ((position - pos).sqrMagnitude < MathLib.Pow(celestial.InfluenceRadius, 2))
                    {
                        // Debug.Log($"Arriving to {celestial.name} in {time}");
                        encounter = true;
                        break;
                    }
                }

                if (encounter) break;
            }

            // check if exited influence
            if (!encounter)
                AddLastPointBeyondInfluence(trajectory, influenceRadius);

            // draw trajectory / orbit
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
            lineRenderer.loop = false;
        }

        private void AddLastPointBeyondInfluence(KeplerianOrbit trajectory, float influenceRadius)
        {
            if (points.Count != orbitResolution)
            {
                lineRenderer.loop = false;

                // choose best exit point
                int iter = 10;
                float delta = 1f / iter;
                Vector3 bestPoint = Vector3.zero;
                float dst = float.MaxValue;
                for (int i = 0; i < iter; i++)
                {
                    float anomaly = Mathf.Lerp(lastAnomaly, anomalyBeyondInfluence, i * delta);
                    Vector3 position = trajectory.orbit.CalculateOrbitalPosition(anomaly);
                    float dstDiff = MathLib.Abs(position.sqrMagnitude - influenceRadius * influenceRadius);
                    if (dstDiff < dst)
                    {
                        dst = dstDiff;
                        bestPoint = position;
                    }
                }
                points.Add(bestPoint);
            }
            else
            {
                lineRenderer.loop = true;
            }
        }
    }
}