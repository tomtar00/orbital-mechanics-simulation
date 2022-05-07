using UnityEngine;
using Sim.Visuals;
using Sim.Math;

namespace Sim.Objects
{
    [RequireComponent(typeof(OrbitDrawer))]
    public class Celestial : InOrbitObject
    {
        [Header("Celestial")]
        [SerializeField] protected CelestialSO data;
        public CelestialSO Data { get => data; }

        private Transform model;

        private new void Start()
        {
            base.Start();

            model = transform.GetChild(0);
            model.transform.localScale = Vector3.one * data.Radius;

            if (!isStationary)
            {
                orbit = data.Orbit;
                orbit.meta = KeplerianOrbit.CalculateMetaElements(orbit, celestial.Data.Mass);
                orbitDrawer.DrawOrbit(orbit);
            }
        }
    }
}