using System;
using System.Collections.Generic;
using UnityEngine;
using Sim.Orbits;
using Sim.Visuals;
using Sim.Math;

namespace Sim.Objects
{
    public class Celestial : InOrbitObject
    {
        public static List<Celestial> celestials { get; set; }

        [Header("Celestial")]
        [SerializeField] protected CelestialSO data;
        public CelestialSO Data { get => data;  set => data = value; }

        public List<Celestial> celestialsOnOrbit { get; private set; } = new List<Celestial>();

        [SerializeField] private bool infiniteInfluence;
        private float influenceRadius;
        public float InfluenceRadius { get => influenceRadius; }

        [SerializeField] private Transform model;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Transform influenceSphere;

        public Transform Model { get => model; }

        private float currentCamDistance;

        public void InitializeCelestial(Celestial centralBody, CelestialSO data, double secondsDiff)
        {
            this.data = data;
            this.centralBody = centralBody;

            stateVectors = new StateVectors();
            orbitDrawer = GetComponent<OrbitDrawer>();
            orbitDrawer?.SetupOrbitRenderers();
            if (!isStationary)
            {
                kepler = new KeplerianOrbit();
            }

            if (infiniteInfluence) {
                influenceRadius = float.MaxValue;
                influenceSphere.gameObject.SetActive(false);
            }
            else {
                influenceRadius = Mathf.Sqrt((SimulationSettings.Instance.G * data.Mass) / SimulationSettings.Instance.gravityFalloff);
                influenceSphere.localScale = Vector3.one * influenceRadius / 5f;
            }

            this.centralBody?.celestialsOnOrbit.Add(this);

            model.localScale = Vector3.one * data.Diameter;
            meshRenderer.material = data.Material;

            if (!isStationary)
            {
                kepler.ApplyElementsFromStruct(data.Orbit, centralBody);
                // kepler.ApplyPrecessionChanges(data, secondsDiff);
                mat = kepler.UpdateAnomalies(secondsDiff);
                stateVectors = kepler.UpdateStateVectors(mat.Item3);
                transform.localPosition = centralBody.transform.localPosition + stateVectors.position;

                UpdateRelativePosition();
                orbitDrawer.DrawOrbit(kepler.orbit.elements);
            }

            if (celestials == null) celestials = new List<Celestial>();
            celestials.Add(this);
        }

        private void Update()
        {
            base.UpdateObject();

            if (!isStationary)
            {
                float dontDrawMultiplier = data.Type == CelestialBodyType.PLANET ?
                        SimulationSettings.Instance.dontDrawPlanetOrbitMultiplier : 
                        SimulationSettings.Instance.dontDrawMoonOrbitMultiplier;

                // disable and enable orbit line renderers when close to body
                currentCamDistance = (CameraController.Instance.cam.transform.position - transform.position).sqrMagnitude;
                if (currentCamDistance < Mathf.Pow(influenceRadius * dontDrawMultiplier, 2))
                {
                    if (!camInsideInfluence)
                    {
                        EnableOtherOrbitRenderer(false);
                    }
                }
                else if (camInsideInfluence)
                {
                    EnableOtherOrbitRenderer(true);
                }
            }
        }
    }
}