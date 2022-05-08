using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sim.Math;

namespace Sim.Visuals
{
    public class OrbitDrawer : MonoBehaviour
    {
        [SerializeField] private int orbitResolution = 30;

        private LineRenderer lineRenderer;
        private GameObject rendererObject;

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

        public void DrawOrbit(KeplerianOrbit.Elements elements, float influenceRadius)
        {
            List<Vector3> points = new List<Vector3>();
            lineRenderer.positionCount = 0;
            float PI2 = 2 * Mathf.PI;
            float currentEccAnomaly = KeplerianOrbit.CalculateEccentricAnomalyFromMean(elements, elements.meanAnomaly);
            float eccentricAnomalyBeyondInfluence = 0;
            float lastEccAnomaly = 0;

            // select points
            float orbitFraction = 1f / orbitResolution;
            for (int i = 0; i < orbitResolution; i++)
            {               
                float eccentricAnomaly = currentEccAnomaly + i * orbitFraction * PI2;
                if (eccentricAnomaly > PI2) eccentricAnomaly -= PI2;

                Vector3 position = KeplerianOrbit.CalculateOrbitalPosition(elements, eccentricAnomaly, out _);

                if (position.sqrMagnitude < influenceRadius * influenceRadius) {
                    points.Add(position);
                    lastEccAnomaly = eccentricAnomaly;
                }
                else {
                    eccentricAnomalyBeyondInfluence = eccentricAnomaly;
                    break;
                }
            }

            // check if exited influence
            if (points.Count != orbitResolution)
            {
                lineRenderer.loop = false;

                // choose best exit point
                int iter = 10;
                float delta = 1f / iter;
                Vector3 bestPoint = Vector3.zero;
                float dst = float.MaxValue;
                for (int i = 0; i < iter; i++) {
                    float eccAnomaly = Mathf.Lerp(lastEccAnomaly, eccentricAnomalyBeyondInfluence, i * delta);
                    Vector3 position = KeplerianOrbit.CalculateOrbitalPosition(elements, eccAnomaly, out _);
                    float dstDiff = Mathf.Abs(position.sqrMagnitude - influenceRadius * influenceRadius);
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

            // draw trajectory / orbit
            lineRenderer.positionCount = points.Count;
            int idx = 0;
            foreach (var point in points)
            {
                lineRenderer.SetPosition(idx++, point);
            }

        }
    }
}