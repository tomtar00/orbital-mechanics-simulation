using Sim.Visuals;
using Sim.Math;
using Sim.Objects;
using UnityEngine;

namespace Sim.Maneuvers
{
    public class Maneuver 
    {
        public Orbit orbit { get; private set; }
        public OrbitDrawer drawer { get; private set; }
        public StateVectors stateVectors { get; private set; }
        public Vector3 addedVelocity { get; private set; }
        public float currentTrueAnomaly { get; private set; }
        public float timeToManeuver { get; private set; }
        public float burnTime { get; private set; }

        public Maneuver PreviousManeuver { get; set; }
        public OrbitDrawer PreviousDrawer { get; set; }
        public Maneuver NextManeuver { get; set; }
        public ManeuverNode Node { get; set; }
        public Spacecraft spacecraft { get; set; }

        private Vector3 lastVelocity;
        private float timeToOrbit;

        public Maneuver(Orbit orbit, OrbitDrawer drawer, StateVectors startStateVectors, ManeuverNode node, Maneuver lastManeuver, float timeToOrbit) {
            this.orbit = orbit;
            this.drawer = drawer;
            this.stateVectors = startStateVectors;
            this.Node = node;
            this.addedVelocity = Vector3.zero;
            this.spacecraft = Spacecraft.current;
            this.PreviousManeuver = lastManeuver;
            this.timeToOrbit = timeToOrbit;
            
            UpdateOnDrag(this.stateVectors.position);
        }

        public void UpdateOnDrag(Vector3 newWorldPosition) {
            if (NextManeuver != null) return;

            currentTrueAnomaly = GetTrueAnomaly(newWorldPosition);
            timeToManeuver = GetTimeToManeuver(currentTrueAnomaly);
            lastVelocity = this.stateVectors.velocity;
            this.stateVectors.velocity = orbit.CalculateVelocity(newWorldPosition, currentTrueAnomaly);
            
            RotateNode(newWorldPosition);
            ChangePosition(newWorldPosition);
        }

        public void Update() {
            timeToManeuver -= Time.deltaTime;
            if (!Node.isDragging)
                Node.gameObject.transform.position = stateVectors.position + orbit.centralBody.transform.position;

            if (timeToManeuver < burnTime * 3 && Time.timeScale != 1) {
                HUDController.Instance.SetTimeScaleToOne();
            }
        }

        public float GetTrueAnomaly(Vector3 relativePressPosition) {
            return Vector3.SignedAngle(orbit.elements.eccVec, relativePressPosition, orbit.elements.angMomentum) * Mathf.Deg2Rad;
        }
        public float GetTimeToManeuver(float trueAnomaly) {
            float anomaly = orbit.CalculateAnomalyFromTrueAnomaly(trueAnomaly);
            float meanAnomaly = orbit.CalculateMeanAnomalyFromAnomaly(anomaly);

            float enterMeanAnomaly = orbit.elements.meanAnomaly;
            // if (enterMeanAnomaly > meanAnomaly) enterMeanAnomaly -= 2f * Mathf.PI;
            float time = Mathf.Abs(meanAnomaly - enterMeanAnomaly) / orbit.elements.meanMotion + timeToOrbit;

            Debug.Log("mean: " + meanAnomaly + " enter: " + enterMeanAnomaly);

            if (time < 0) time += orbit.elements.period;
            return time;
        }
        private float GetBurnTime() {
            return addedVelocity.magnitude / spacecraft.Thrust;
        }
        public void RotateNode(Vector3 orbitPosition) {
            Node.gameObject.transform.rotation = Quaternion.LookRotation(stateVectors.velocity);
        }

        public void ChangeVelocity(Vector3 dV) {
            addedVelocity += dV;
            StateVectors newVectors = new StateVectors(stateVectors.position, stateVectors.velocity + addedVelocity);

            drawer.DrawOrbits(newVectors, orbit.centralBody, timeToManeuver);

            burnTime = GetBurnTime();
        }
        public void ChangePosition(Vector3 newPosition) {
            Node.gameObject.transform.position = newPosition + orbit.centralBody.transform.position;

            // rotate vector to match new position
            float angle = Vector3.SignedAngle(lastVelocity, this.stateVectors.velocity, orbit.elements.angMomentum);
            addedVelocity = Quaternion.AngleAxis(angle, orbit.elements.angMomentum) * addedVelocity;

            stateVectors.position = newPosition;
            StateVectors newVectors = new StateVectors(newPosition, stateVectors.velocity + addedVelocity);

            drawer.DrawOrbits(newVectors, orbit.centralBody, timeToManeuver);
        }

        public void Remove() {
            ManeuverManager.Instance.maneuvers.Remove(this);
            drawer.DestroyRenderers();
            MonoBehaviour.Destroy(drawer.gameObject);
            if (PreviousManeuver != null) {
                PreviousManeuver.drawer.hasManeuver = false;
                PreviousManeuver.NextManeuver = null;
            }
            else PreviousDrawer.hasManeuver = false;
        }
    }
}
