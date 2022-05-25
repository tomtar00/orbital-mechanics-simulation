using System;
using System.Collections.Generic;
using UnityEngine;
using Sim.Math;

namespace Sim.Visuals
{
    public class OrbitDrawer : MonoBehaviour
    {
        [SerializeField] private int orbitResolution = 30;

        private LineRenderer lineRenderer;
        private GameObject rendererObject;

        float currentAnomaly = 0;
        private float anomalyBeyondInfluence = 0;
        private float lastAnomaly = 0;
        private float PI2 = 2 * MathLib.PI;
        private List<Vector3> points = new List<Vector3>();

        public void SetupOrbitRenderer(Transform celestial)
        {
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
            if (e > 0 && e < 1) {
                DrawElliptic(trajectory, influenceRadius);
            }
            else {
                DrawHyperbolic(trajectory, influenceRadius);
            }
        }

        private void DrawElliptic(KeplerianOrbit trajectory, float influenceRadius) {    
            lineRenderer.positionCount = 0;

            currentAnomaly = trajectory.anomaly;

            // select points
            float orbitFraction = 1f / orbitResolution;
            for (int i = 0; i < orbitResolution; i++)
            {               
                float eccentricAnomaly = currentAnomaly + i * orbitFraction * PI2;
                if (eccentricAnomaly > PI2) eccentricAnomaly -= PI2;

                Vector3 position = trajectory.orbit.CalculateOrbitalPosition(eccentricAnomaly);

                if (position.sqrMagnitude < influenceRadius * influenceRadius) {
                    points.Add(position);
                    lastAnomaly = eccentricAnomaly;
                }
                else {
                    anomalyBeyondInfluence = eccentricAnomaly;
                    break;
                }
            }

            // check if exited influence
            AddLastPointBeyondInfluence(trajectory, influenceRadius, false);

            // draw trajectory / orbit
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());        
        }

        private void DrawHyperbolic(KeplerianOrbit trajectory, float influenceRadius) {
            lineRenderer.positionCount = 0;

            currentAnomaly = trajectory.trueAnomaly;
            float theta = MathLib.Acos(-1.0f / trajectory.eccentricity) - 0.01f;

            // select points
            float orbitFraction = 1f / (orbitResolution-1);
            for (int i = 0; i < orbitResolution; i++)
            {               
                float trueAnomaly = -theta + i * orbitFraction * 2 * theta;
                //float trueAnomaly = 0 + i * orbitFraction * theta;

                Vector3 position = trajectory.orbit.CalculateOrbitalPositionTrue(trueAnomaly);

                if (position.sqrMagnitude < influenceRadius * influenceRadius) {
                    points.Add(position);
                    lastAnomaly = trueAnomaly;
                }
                else {
                    anomalyBeyondInfluence = trueAnomaly;
                    break;
                }
            }

            // check if exited influence
            AddLastPointBeyondInfluence(trajectory, influenceRadius, true);

            // draw trajectory / orbit
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
            lineRenderer.loop = false; 
        }

        private void AddLastPointBeyondInfluence(KeplerianOrbit trajectory, float influenceRadius, bool posFromTrue) {
            Func<float, Vector3> CalculatePosition = posFromTrue ? 
                (Func<float, Vector3>)trajectory.orbit.CalculateOrbitalPositionTrue : 
                (Func<float, Vector3>)trajectory.orbit.CalculateOrbitalPosition;
            if (points.Count != orbitResolution)
            {
                lineRenderer.loop = false;

                // choose best exit point
                int iter = 10;
                float delta = 1f / iter;
                Vector3 bestPoint = Vector3.zero;
                float dst = float.MaxValue;
                for (int i = 0; i < iter; i++) {
                    float anomaly = Mathf.Lerp(lastAnomaly, anomalyBeyondInfluence, i * delta);
                    Vector3 position = CalculatePosition(anomaly);
                    float dstDiff = MathLib.Abs(position.sqrMagnitude - influenceRadius * influenceRadius);
                    if (dstDiff < dst)
                    {
                        dst = dstDiff;
                        bestPoint = position;
                    }
                }
                points.Add(bestPoint);
            }
            else {
                lineRenderer.loop = true;
            }
        }
    }
}