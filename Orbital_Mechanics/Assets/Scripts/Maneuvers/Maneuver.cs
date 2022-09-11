using Sim.Visuals;
using Sim.Math;
using UnityEngine;

namespace Sim.Maneuvers
{
    public class Maneuver 
    {
        public Orbit orbit { get; private set; }
        public OrbitDrawer drawer { get; private set; }
        public float timeToManeuver { get; private set; }
        public StateVectors stateVectors { get; private set; }

        public float TimeToNextManeuver { get; set; }
        public Maneuver NextManeuver { get; set; }
        public ManeuverNode node { get; set; }

        public Maneuver(Orbit orbit, OrbitDrawer drawer, StateVectors startStateVectors, ManeuverNode node) {
            this.orbit = orbit;
            this.drawer = drawer;
            this.stateVectors = startStateVectors;
            this.node = node;

            // TODO: delete
            this.stateVectors.velocity += Random.insideUnitSphere;
            
            UpdateOnDrag(this.stateVectors.position);
        }

        public void UpdateOnDrag(Vector3 newWorldPosition) {
            float currentTruAnomaly = GetTrueAnomaly(newWorldPosition);
            timeToManeuver = GetTimeToManeuver(currentTruAnomaly);
            ChangePosition(newWorldPosition);
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
            node.gameObject.transform.position = newPosition + orbit.centralBody.transform.position;
            StateVectors newVectors = new StateVectors(newPosition, stateVectors.velocity);
            drawer.DrawOrbits(newVectors, orbit.centralBody, timeToManeuver);
        }

        public void Remove() {
            ManeuverManager.Instance.maneuvers.Remove(this);
            MonoBehaviour.Destroy(drawer.gameObject);
        }
    }
}
