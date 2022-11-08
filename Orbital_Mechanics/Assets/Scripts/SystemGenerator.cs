using System;
using UnityEngine;

namespace Sim.Objects
{
    public class SystemGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject starPrefab;
        [SerializeField] private GameObject planetPrefab;
        [SerializeField] private GameObject spacecraftPrefab;
        [SerializeField] private Transform systemParent;
        [Space]
        [SerializeField] private CelestialSO star;
        [Space]
        [SerializeField] private float modelScale = 1;
        [Space]
        [SerializeField] private NamesViewer namesViewer;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private Maneuvers.ManeuverManager maneuverManager;

        public CelestialSO Star { get => star; }

        private static double secondsDiff;
        private bool generated = false;
        private string spacecraftSpawnOrbit;

        public void Generate(DateTime dateTime, string spacecraftSpawnOrbit)
        {
            this.spacecraftSpawnOrbit = spacecraftSpawnOrbit;
            this.generated = true;
            SystemGenerator.secondsDiff = (dateTime - new DateTime(2000, 1, 1)).TotalSeconds;
            GenerateBody(star, centralBody: null);
            namesViewer.Init();
            cameraController.Init();
        }
        public void ResetSystem()
        {
            this.generated = false;
            namesViewer.DestroyNames();
            cameraController.initialized = false;
            maneuverManager.DestroyManeuvers();
            Sim.Visuals.LineButton.allLineButtons = null;
        }

        private void Update()
        {
            if (generated)
            {
                secondsDiff += Sim.Time.deltaTime;
            }
        }
        public static DateTime GetCurrentSimulationDate() {
            return new DateTime(2000, 1, 1).AddSeconds(secondsDiff);
        }

        private void GenerateBody(CelestialSO body, Celestial centralBody)
        {
            GameObject prefab = body.Type == CelestialBodyType.STAR ? starPrefab : planetPrefab;
            GameObject go = Instantiate(prefab, systemParent);
            go.name = body.name;

            CelestialSO bodyData = Instantiate(body);

            bodyData.Diameter *= modelScale * SimulationSettings.Instance.scale;
            bodyData.Mass *= SimulationSettings.Instance.scale;

            var orbit = bodyData.Orbit;
            orbit.semimajorAxis *= SimulationSettings.Instance.scale;

            bodyData.Orbit = orbit;

            Celestial celestial = go.GetComponent<Celestial>();
            celestial.InitializeCelestial(centralBody, bodyData, secondsDiff);

            if (spacecraftSpawnOrbit == body.name)
            {
                SpawnSpacecraft(celestial);
            }

            foreach (var planet in bodyData.BodiesOnOrbit)
            {
                GenerateBody(planet, celestial);
            }
        }

        private void SpawnSpacecraft(Celestial celestial)
        {
            GameObject go = Instantiate(spacecraftPrefab, systemParent);
            go.name = "Spacecraft";
            Spacecraft craft = go.GetComponent<Spacecraft>();
            craft.CentralBody = celestial;
            craft.InitializeShip();
        }
    }
}