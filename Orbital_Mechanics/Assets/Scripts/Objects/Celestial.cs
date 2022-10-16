using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sim.Math;

namespace Sim.Objects
{
    public class Celestial : InOrbitObject
    {
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
        private bool camInsideInfluence = false;

        public override void Init(Celestial centralBody, CelestialSO data)
        {
            this.data = data;
            this.centralBody = centralBody;

            base.Init(centralBody, data);

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
                transform.localPosition = centralBody.transform.localPosition + kepler.orbit.CalculateOrbitalPosition(data.Orbit.trueAnomaly);

                UpdateRelativePosition();
                orbitDrawer.DrawOrbit(kepler.orbit.elements);
            }
        }

        private new void Update()
        {
            base.Update();

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
                        orbitDrawer.TurnOffRenderersFrom(0);
                        InOrbitObject.allObjects
                            .Where(o => o.OrbitDrawer != null && o.CentralBody.isStationary)
                            .ForEach(o => {
                                o.OrbitDrawer.TurnOffRenderersFrom(0);
                            });
                        camInsideInfluence = true;
                    }
                }
                else if (camInsideInfluence)
                {
                    orbitDrawer.TurnOnRenderersFrom(0);
                    InOrbitObject.allObjects
                            .Where(o => o.OrbitDrawer != null && o.CentralBody.isStationary && centralBody.IsStationary)
                            .ForEach(o => {
                                o.OrbitDrawer.TurnOnRenderersFrom(0);
                            });
                    camInsideInfluence = false;
                }
            }
        }
    }
}