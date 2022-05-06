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
            lineRenderer = rendererObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.startWidth = .1f;
        }

        public void DestroyOrbitRenderer() {
            Destroy(rendererObject);
        }

        public void DrawOrbit(KeplerianOrbit.Elements elements)
        {
            lineRenderer.SetPositions(new Vector3[] { });
            lineRenderer.positionCount = orbitResolution + 1;

            float orbitFraction = 1f / orbitResolution;
            for (int i = 0; i < orbitResolution; i++)
            {
                float eccentricAnomaly = i * orbitFraction * 2 * Mathf.PI;        
                Vector3 position = KeplerianOrbit.CalculateOrbitalPosition(elements, eccentricAnomaly, out _);

                lineRenderer.SetPosition(i, position);
            }

            lineRenderer.SetPosition(orbitResolution, lineRenderer.GetPosition(0));
        }
    }
}