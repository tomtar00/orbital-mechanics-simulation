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
        public Vector3 addedVelocity { get; private set; }
        public float currentTrueAnomaly { get; private set; }

        public Maneuver NextManeuver { get; set; }
        public ManeuverNode Node { get; set; }

        public Maneuver(Orbit orbit, OrbitDrawer drawer, StateVectors startStateVectors, ManeuverNode node) {
            this.orbit = orbit;
            this.drawer = drawer;
            this.stateVectors = startStateVectors;
            this.Node = node;
            this.addedVelocity = Vector3.zero;
            
            UpdateOnDrag(this.stateVectors.position);
        }

        public void UpdateOnDrag(Vector3 newWorldPosition) {
            if (NextManeuver != null) return;

            currentTrueAnomaly = GetTrueAnomaly(newWorldPosition);
            timeToManeuver = GetTimeToManeuver(currentTrueAnomaly);
            this.stateVectors.velocity = orbit.CalculateVelocity(newWorldPosition, currentTrueAnomaly);
            
            RotateNode(newWorldPosition);
            ChangePosition(newWorldPosition);
        }

        public float GetTrueAnomaly(Vector3 relativePressPosition) {
            return Vector3.SignedAngle(orbit.elements.eccVec, relativePressPosition, orbit.elements.angMomentum) * Mathf.Deg2Rad;
        }
        public float GetTimeToManeuver(float trueAnomaly) {
            float anomaly = orbit.CalculateAnomalyFromTrueAnomaly(trueAnomaly);
            float meanAnomaly = orbit.CalculateMeanAnomalyFromAnomaly(anomaly);
            return (meanAnomaly - orbit.elements.meanAnomaly) / orbit.elements.meanMotion;
        }
        public void RotateNode(Vector3 orbitPosition) {
            Node.gameObject.transform.rotation = Quaternion.LookRotation(stateVectors.velocity);
        }

        public void ChangeVelocity(Vector3 dV) {
            addedVelocity += dV;
            StateVectors newVectors = new StateVectors(stateVectors.position, stateVectors.velocity + addedVelocity);

            drawer.DrawOrbits(newVectors, orbit.centralBody, timeToManeuver);
        }
        public void ChangePosition(Vector3 newPosition) {
            Node.gameObject.transform.position = newPosition + orbit.centralBody.transform.position;

            stateVectors.position = newPosition;
            StateVectors newVectors = new StateVectors(newPosition, stateVectors.velocity + addedVelocity);

            drawer.DrawOrbits(newVectors, orbit.centralBody, timeToManeuver);
        }

        public void Remove() {
            ManeuverManager.Instance.maneuvers.Remove(this);
            MonoBehaviour.Destroy(drawer.gameObject);
        }
    }
}
