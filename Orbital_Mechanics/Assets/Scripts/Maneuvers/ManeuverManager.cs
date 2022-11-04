using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using Sim.Visuals;
using Sim.Math;
using Sim.Objects;
using Sim.Orbits;

namespace Sim.Maneuvers {
    public class ManeuverManager : MonoBehaviour
    {
        [SerializeField] private GameObject maneuverPrefab;
        [SerializeField] private Transform maneuverHolder;
        public Transform maneuverOrbitsHolder;

        public List<Maneuver> maneuvers { get; private set; }

        public Maneuver NextManeuver {
            get {
                if (maneuvers.Count > 0)
                    return maneuvers[0];
                else 
                    return null;
            }
        }

        public static ManeuverManager Instance;
        private void Awake() {
            Instance = this;
            maneuvers = new List<Maneuver>();
        }

        private void LateUpdate() {
            foreach (var maneuver in maneuvers)
            {
                maneuver.LateUpdate();
            }
        }

        public void DestroyManeuvers() {
            foreach(var maneuver in maneuvers) {
                maneuver.drawer.DestroyRenderers();
                Destroy(maneuver.drawer.gameObject);
            }
            maneuvers.Clear();
        }

        public Maneuver CreateManeuver(Orbit currentOrbit, InOrbitObject inOrbitObject, Vector3Double relativePressPosition, double timeToOrbit, int futureOrbitIdx) {

            Maneuver lastManeuver = maneuvers.Count > 0 ? maneuvers[maneuvers.Count - 1] : null;
            Orbit orbit = inOrbitObject != null && futureOrbitIdx == 0 ? inOrbitObject.Kepler.orbit : currentOrbit;

            // create prefab
            GameObject maneuverObj = Instantiate(maneuverPrefab, maneuverHolder);
            maneuverObj.name = "Maneuver Node " + maneuvers.Count;
            OrbitDrawer drawer = maneuverObj.GetComponent<OrbitDrawer>();
            ManeuverNode node = maneuverObj.GetComponent<ManeuverNode>();

            // calculate state vectors of pressed point
            double trueAnomaly = Vector3Double.SignedAngle(orbit.elements.eccVec, relativePressPosition, orbit.elements.angMomentum) * Mathf.Deg2Rad;
            var relativePressVelocity = orbit.CalculateVelocity(relativePressPosition, trueAnomaly);
            StateVectors pressStateVectors = new StateVectors(relativePressPosition, relativePressVelocity);    

            // create new maneuver
            Maneuver maneuver = new Maneuver(orbit, drawer, pressStateVectors, node, lastManeuver, timeToOrbit, futureOrbitIdx);
            maneuvers.Add(maneuver);
            maneuvers = maneuvers.OrderBy(m => m.timeToManeuver).ToList();
            lastManeuver = maneuver.PreviousManeuver = (maneuver == maneuvers[0]) ? null : lastManeuver;
            node.maneuver = maneuver;
            if (lastManeuver != null) {
                lastManeuver.NextManeuver = maneuver;
            }
            else maneuver.PreviousDrawer = inOrbitObject.GetComponent<OrbitDrawer>();

            return maneuver;
        }

        public void RemoveManeuvers(Maneuver maneuver) {
            int index = maneuvers.IndexOf(maneuver);
            if (index < 0) {
                Debug.LogWarning("Maneuver not found");
                return;
            }
            
            for (int i = maneuvers.Count - 1; i >= index; i--) {
                maneuvers[i].Remove();
            }
        }
        public void RemoveFirst() {
            if (maneuvers.Count > 0)
                maneuvers[0].Remove();
            else
                Debug.LogWarning("Tried to remove maneuver from empty list");
        }
    }
}
