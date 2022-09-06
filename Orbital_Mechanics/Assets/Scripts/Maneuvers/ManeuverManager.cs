using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sim.Math;

namespace Sim.Visuals {
    public class ManeuverManager : MonoBehaviour
    {
        [SerializeField] private GameObject maneuverPrefab;
        [SerializeField] private Transform maneuverHolder;

        public List<Maneuver> maneuvers { get; private set; }

        public static ManeuverManager Instance;
        private void Awake() {
            Instance = this;
            maneuvers = new List<Maneuver>();
        }

        public Maneuver CreateManeuver(Orbit orbit) {
            GameObject maneuverObj = Instantiate(maneuverPrefab, maneuverHolder);
            OrbitDrawer drawer = maneuverObj.GetComponent<OrbitDrawer>();
            Maneuver maneuver = new Maneuver(orbit, drawer);
            return maneuver;
        }

        public void RemoveManeuvers(Maneuver maneuver) {
            int index = maneuvers.IndexOf(maneuver);
            if (index < 0) {
                Debug.LogWarning("Maneuver not found");
                return;
            }
            
            for (int i = index; i < maneuvers.Count; i++) {
                maneuvers[i].Remove();
            }
        }
    }

    public class Maneuver {
        private Orbit orbit;
        private OrbitDrawer drawer;

        public Maneuver(Orbit orbit, OrbitDrawer drawer) {
            this.orbit = orbit;
            this.drawer = drawer;
        }

        public void Draw(StateVectors stateVectors, float timeToManeuver) {
            drawer.DrawOrbits(stateVectors, orbit.centralBody, timeToManeuver);
        }
        public void Draw(float trueAnomaly, float timeToManeuver) {
            StateVectors stateVectors = orbit.ConvertOrbitElementsToStateVectors(trueAnomaly);
            drawer.DrawOrbits(stateVectors, orbit.centralBody, timeToManeuver);
        }

        public void Remove() {
            ManeuverManager.Instance.maneuvers.Remove(this);
            MonoBehaviour.Destroy(drawer.gameObject);
        }
    }
}
