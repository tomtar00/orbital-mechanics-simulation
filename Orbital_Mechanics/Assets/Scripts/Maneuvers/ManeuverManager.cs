using System.Collections.Generic;
using UnityEngine;
using Sim.Visuals;
using Sim.Math;
using Sim.Objects;

namespace Sim.Maneuvers {
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

        public Maneuver CreateManeuver(OrbitDrawer baseDrawer, Orbit currentOrbit, Vector3 relativePressPosition) {

            if (baseDrawer.hasManeuver) {
                return null;
            }
            else baseDrawer.hasManeuver = true;

            Maneuver lastManeuver = maneuvers.Count > 0 ? maneuvers[maneuvers.Count - 1] : null;
            Orbit orbit = currentOrbit;

            // create prefab
            GameObject maneuverObj = Instantiate(maneuverPrefab, maneuverHolder);
            maneuverObj.name = "Maneuver Node " + maneuvers.Count;
            OrbitDrawer drawer = maneuverObj.GetComponent<OrbitDrawer>();
            ManeuverNode node = maneuverObj.GetComponent<ManeuverNode>();

            // calculate state vectors of pressed point
            float trueAnomaly = Vector3.SignedAngle(orbit.elements.eccVec, relativePressPosition, orbit.elements.angMomentum) * Mathf.Deg2Rad;
            var relativePressVelocity = orbit.CalculateVelocity(relativePressPosition, trueAnomaly);
            StateVectors pressStateVectors = new StateVectors(relativePressPosition, relativePressVelocity);

            // create new maneuver
            Debug.Log(orbit.centralBody.name);
            Maneuver maneuver = new Maneuver(orbit, drawer, pressStateVectors, node);
            maneuvers.Add(maneuver);
            node.maneuver = maneuver;
            if (lastManeuver != null) {
                lastManeuver.NextManeuver = maneuver;
                lastManeuver.TimeToNextManeuver = maneuver.timeToManeuver;
            }
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
}
