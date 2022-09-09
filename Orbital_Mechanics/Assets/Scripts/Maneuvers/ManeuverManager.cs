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

        public Maneuver CreateManeuver(OrbitDrawer baseDrawer, InOrbitObject inOrbitObject, Vector3 relativePressPosition) {

            if (baseDrawer.hasManeuver) {
                return null;
            }
            else baseDrawer.hasManeuver = true;

            Maneuver lastManeuver = maneuvers.Count > 0 ? maneuvers[maneuvers.Count - 1] : null;
            Orbit orbit = inOrbitObject == null ? maneuvers[maneuvers.Count - 1].orbit : inOrbitObject.Kepler.orbit;

            // create prefab
            GameObject maneuverObj = Instantiate(maneuverPrefab, maneuverHolder);
            maneuverObj.transform.position = relativePressPosition + orbit.centralBody.transform.position;
            OrbitDrawer drawer = maneuverObj.GetComponent<OrbitDrawer>();
            ManeuverEditor editor = maneuverObj.GetComponent<ManeuverEditor>();

            // calculate state vectors of pressed point
            float trueAnomaly = Vector3.SignedAngle(orbit.elements.eccVec, relativePressPosition, orbit.elements.angMomentum) * Mathf.Deg2Rad;
            var relativePressVelocity = orbit.CalculateVelocity(relativePressPosition, trueAnomaly);
            StateVectors pressStateVectors = new StateVectors(relativePressPosition, relativePressVelocity);

            // create new maneuver
            Maneuver maneuver = new Maneuver(orbit, drawer, pressStateVectors);
            maneuvers.Add(maneuver);
            editor.maneuver = maneuver;
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

    public class Maneuver {
        public Orbit orbit { get; private set; }
        public OrbitDrawer drawer { get; private set; }
        public float timeToManeuver { get; private set; }
        public StateVectors stateVectors { get; private set; }

        public float TimeToNextManeuver { get; set; }
        public Maneuver NextManeuver { get; set; }

        public Maneuver(Orbit orbit, OrbitDrawer drawer, StateVectors startStateVectors) {
            this.orbit = orbit;
            this.drawer = drawer;
            this.stateVectors = startStateVectors;

            // TODO: delete
            this.stateVectors.velocity += Random.insideUnitSphere;
            
            UpdateOnDrag(this.stateVectors.position);
        }

        public void UpdateOnDrag(Vector3 newRelativeWorldPosition) {
            float currentTruAnomaly = GetTrueAnomaly(newRelativeWorldPosition);
            timeToManeuver = GetTimeToManeuver(currentTruAnomaly);
            ChangePosition(newRelativeWorldPosition);
            UpdateNextManeuver();
        }
        public void UpdateNextManeuver() {
            if (NextManeuver == null) return;
            Vector3 newPosition = NextManeuver.GetPositionAfterTime(TimeToNextManeuver);
            NextManeuver.ChangePosition(newPosition);
        }

        public Vector3 GetPositionAfterTime(float time) {
            var mat = orbit.GetFutureAnomalies(time);
            return orbit.CalculateOrbitalPosition(mat.Item3);
        }
        public float GetTrueAnomaly(Vector3 relativePressPosition) {
            return Vector3.SignedAngle(orbit.elements.eccVec, relativePressPosition, orbit.elements.angMomentum) * Mathf.Deg2Rad;
        }
        public float GetTimeToManeuver(float trueAnomaly) {
            float anomaly = orbit.CalculateAnomalyFromTrueAnomaly(trueAnomaly);
            float meanAnomaly = orbit.CalculateMeanAnomalyFromAnomaly(anomaly);
            return (meanAnomaly - orbit.elements.meanAnomaly) / orbit.elements.meanMotion;
        }

        public void ChangeVelocity(Vector3 dV) {
            stateVectors.velocity += dV;
            drawer.DrawOrbits(stateVectors, orbit.centralBody, timeToManeuver);
        }
        public void ChangePosition(Vector3 newPosition) {
            StateVectors newVectors = new StateVectors(newPosition, stateVectors.velocity);
            drawer.DrawOrbits(newVectors, orbit.centralBody, timeToManeuver);
        }

        public void Remove() {
            ManeuverManager.Instance.maneuvers.Remove(this);
            MonoBehaviour.Destroy(drawer.gameObject);
        }
    }
}
