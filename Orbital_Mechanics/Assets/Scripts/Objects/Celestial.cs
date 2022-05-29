using System.Collections.Generic;
using UnityEngine;
using Sim.Visuals;
using Sim.Math;

namespace Sim.Objects
{
    [RequireComponent(typeof(OrbitDrawer))]
    public class Celestial : InOrbitObject
    {
        public const float GRAVITY_FALLOFF = 0.05f;

        [Header("Celestial")]
        [SerializeField] protected CelestialSO data;
        public CelestialSO Data { get => data; }

        public List<Celestial> celestialsOnOrbit {get; private set;}

        [SerializeField] private bool infiniteInfluence;
        private float influenceRadius;
        public float InfluenceRadius { get => influenceRadius; }

        [SerializeField] private Transform model;
        [SerializeField] private Transform influenceSphere;

        private void Awake() {
            celestialsOnOrbit = new List<Celestial>();
            if (infiniteInfluence) influenceRadius = float.MaxValue;
            else influenceRadius = Mathf.Sqrt((KeplerianOrbit.G * data.Mass) / GRAVITY_FALLOFF);
        }

        private new void Start()
        {
            base.Start();

            influenceSphere.localScale = Vector3.one * influenceRadius * 2;
            model.transform.localScale = Vector3.one * data.Radius;

            if (!isStationary)
            {
                trajectory.ApplyElementsFromStruct(data.Orbit);

                relativePosition = transform.position - centralBody.RelativePosition;
                Vector3 perpendicularLastPosition = trajectory.orbit.CalculateOrbitalPositionTrue(trajectory.trueAnomaly - MathLib.PI / 2);
                orbitNormal = Vector3.Cross(perpendicularLastPosition, relativePosition).normalized;

                orbitDrawer.DrawOrbit(trajectory, centralBody.influenceRadius);
            }

            centralBody?.celestialsOnOrbit.Add(this); 

            //CheckOrbitType(); 
        }

        private new void OnDrawGizmos() {
            base.OnDrawGizmos();

            if (infiniteInfluence) return;

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