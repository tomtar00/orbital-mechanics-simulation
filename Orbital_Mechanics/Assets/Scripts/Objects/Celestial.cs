using UnityEngine;
using Sim.Visuals;
using Sim.Math;

namespace Sim.Objects
{
    [RequireComponent(typeof(OrbitDrawer))]
    public class Celestial : InOrbitObject
    {
        public const float MIN_INFLUENCE_FORCE = 0.05f;

        [Header("Celestial")]
        [SerializeField] protected CelestialSO data;
        public CelestialSO Data { get => data; }

        [SerializeField] private float influenceRadius;
        public float InfluenceRadius { get => influenceRadius; }

        private Transform model;

        private void Awake() {
            influenceRadius = Mathf.Sqrt((KeplerianOrbit.G * data.Mass) / MIN_INFLUENCE_FORCE);
        }

        private new void Start()
        {
            base.Start();

            model = transform.GetChild(0);
            model.transform.localScale = Vector3.one * data.Radius;

            if (!isStationary)
            {
                orbit = data.Orbit;
                orbit.meta = KeplerianOrbit.CalculateMetaElements(orbit, centralBody.Data.Mass);
                orbitDrawer.DrawOrbit(orbit, centralBody.influenceRadius);
            }
        }

        private new void OnDrawGizmos() {
            base.OnDrawGizmos();

            int res = 30;
            float circleFraction = 1f / res;
            Vector3[] points = new Vector3[res];
            for (int i = 0; i < res; i++)
            {
                float angle = i * circleFraction * 2 * Mathf.PI;        
                float x = influenceRadius * Mathf.Sin(angle);
                float z = influenceRadius * Mathf.Cos(angle);

                points[i] = transform.position + new Vector3(x, 0, z);
            }
            for (int i = 0; i < res; i++) {
                Vector3 endPoint = i+1 < res ? points[i+1] : points[0];
                Debug.DrawLine(points[i], endPoint, Color.cyan);
            }
        }
    }
}