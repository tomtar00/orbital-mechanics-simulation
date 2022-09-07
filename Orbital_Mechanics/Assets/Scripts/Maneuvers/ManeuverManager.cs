using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sim.Math;
using Sim.Objects;

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

        public Maneuver CreateManeuver(InOrbitObject inOrbitObject, Vector3 relativePressPosition) {

            Orbit orbit = inOrbitObject.Kepler.orbit;

            // create prefab
            GameObject maneuverObj = Instantiate(maneuverPrefab, maneuverHolder);
            maneuverObj.transform.position = relativePressPosition + orbit.centralBody.transform.position;
            OrbitDrawer drawer = maneuverObj.GetComponent<OrbitDrawer>();
            ManeuverEditor editor = maneuverObj.transform.GetChild(0).GetComponent<ManeuverEditor>();

            // calculate state vectors of pressed point
            float trueAnomaly = Vector3.SignedAngle(orbit.elements.eccVec, relativePressPosition, orbit.elements.angMomentum) * Mathf.Deg2Rad;
            var relativePressVelocity = orbit.CalculateVelocity(relativePressPosition, trueAnomaly);
            StateVectors pressStateVectors = new StateVectors(relativePressPosition, relativePressVelocity);

            // calculate time to maneuver
            float anomaly = orbit.CalculateAnomalyFromTrueAnomaly(trueAnomaly);
            float meanAnomaly = orbit.CalculateMeanAnomalyFromAnomaly(anomaly);
            float timeToManeuver = (meanAnomaly - orbit.elements.meanAnomaly) / orbit.elements.meanMotion;

            // create new maneuver
            Maneuver maneuver = new Maneuver(orbit, drawer, pressStateVectors, timeToManeuver);
            maneuvers.Add(maneuver);
            editor.maneuver = maneuver;
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
        private StateVectors startStateVectors;
        private float timeToManeuver;

        public Maneuver(Orbit orbit, OrbitDrawer drawer, StateVectors startStateVectors, float timeToManeuver) {
            this.orbit = orbit;
            this.drawer = drawer;
            this.startStateVectors = startStateVectors;
            this.timeToManeuver = timeToManeuver;
        }

        public void Draw(Vector3 newPosition, Vector3 addedVelocity) {
            StateVectors newVectors = new StateVectors(newPosition, startStateVectors.velocity + addedVelocity);
            drawer.DrawOrbits(newVectors, orbit.centralBody, timeToManeuver);
        }

        public void Remove() {
            ManeuverManager.Instance.maneuvers.Remove(this);
            MonoBehaviour.Destroy(drawer.gameObject);
        }
    }
}
