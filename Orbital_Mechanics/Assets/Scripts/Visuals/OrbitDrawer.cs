using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sim.Math;

namespace Sim.Visuals
{
    [RequireComponent(typeof(LineRenderer))]
    public class OrbitDrawer : MonoBehaviour
    {
        [SerializeField] private int orbitResolution = 30;
        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        public void DrawOrbit(KeplerianOrbit.Elements elements)
        {
            lineRenderer.SetPositions(new Vector3[] { });
            lineRenderer.positionCount = orbitResolution + 1;

            float orbitFraction = 1f / orbitResolution;
            for (int i = 0; i < orbitResolution; i++)
            {
                float eccentricAnomaly = i * orbitFraction * 2 * Mathf.PI;        
                float trueAnomaly = 2 * Mathf.Atan(elements.meta.trueAnomalyConstant * Mathf.Tan(eccentricAnomaly / 2));
                float distance = elements.semimajorAxis * (1 - elements.eccentricity * Mathf.Cos(eccentricAnomaly));

                float cosArgTrue = Mathf.Cos(elements.argPeriapsis + trueAnomaly);
                float sinArgTrue = Mathf.Sin(elements.argPeriapsis + trueAnomaly);

                float x = distance * ((elements.meta.cosLonAcsNode * cosArgTrue) - (elements.meta.sinLonAcsNode * sinArgTrue * elements.meta.cosInclination));
                float y = distance * ((elements.meta.sinLonAcsNode * cosArgTrue) + (elements.meta.cosLonAcsNode * sinArgTrue * elements.meta.cosInclination));
                float z = distance * (elements.meta.sinInclination * sinArgTrue);

                lineRenderer.SetPosition(i, new Vector3(x, y, z));
            }

            lineRenderer.SetPosition(orbitResolution, lineRenderer.GetPosition(0));
        }
    }
}