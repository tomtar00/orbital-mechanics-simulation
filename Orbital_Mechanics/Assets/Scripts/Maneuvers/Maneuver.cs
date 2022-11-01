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
        public Vector3Double addedVelocity { get; private set; }
        public double currentTrueAnomaly { get; private set; }
        public double timeToManeuver { get; private set; }
        public double fixedTimeToManeuver { get; private set; } = -1f;
        public double burnTime { get; private set; }

        public Maneuver PreviousManeuver { get; set; }
        public OrbitDrawer PreviousDrawer { get; set; }
        public Maneuver NextManeuver { get; set; }
        public ManeuverNode Node { get; set; }
        public Spacecraft spacecraft { get; set; }

        private Vector3Double lastVelocity;
        private double timeToOrbit;
        private int orbitIdx;

        public Maneuver(Orbit orbit, OrbitDrawer drawer, StateVectors startStateVectors, ManeuverNode node, Maneuver lastManeuver, double timeToOrbit, int orbitIdx) {
            this.orbit = orbit;
            this.drawer = drawer;
            this.stateVectors = startStateVectors;
            this.Node = node;
            this.addedVelocity = Vector3Double.zero;
            this.spacecraft = Spacecraft.current;
            this.PreviousManeuver = lastManeuver;
            this.timeToOrbit = timeToOrbit;
            this.orbitIdx = orbitIdx;

            this.PreviousManeuver?.drawer.TurnOffRenderersFrom(orbitIdx + 1);
            
            UpdateOnDrag(this.stateVectors.position);
        }

        public void UpdateOnDrag(Vector3Double newWorldPosition) {
            if (NextManeuver != null) return;

            currentTrueAnomaly = GetTrueAnomaly(newWorldPosition);
            timeToManeuver = GetTimeToManeuver(currentTrueAnomaly);

            fixedTimeToManeuver = timeToManeuver;
            lastVelocity = this.stateVectors.velocity;
            this.stateVectors.velocity = orbit.CalculateVelocity(newWorldPosition, currentTrueAnomaly);
            
            RotateNode(newWorldPosition);
            ChangePosition(newWorldPosition);
        }

        public void LateUpdate() {
            timeToManeuver -= Time.deltaTime;
            if (!Node.isDragging) {
                Node.gameObject.transform.localPosition = (Vector3)stateVectors.position + orbit.centralBody.transform.localPosition;
            }
        }

        public double GetTrueAnomaly(Vector3Double relativePressPosition) {
            return -Vector3Double.SignedAngle(orbit.elements.eccVec, relativePressPosition, orbit.elements.angMomentum) * MathLib.Deg2Rad;
        }
        public double GetTimeToManeuver(double trueAnomaly) {
            double anomaly = orbit.CalculateAnomalyFromTrueAnomaly(trueAnomaly);
            double meanAnomaly = orbit.CalculateMeanAnomalyFromAnomaly(anomaly);

            double enterMeanAnomaly = orbit.elements.meanAnomaly;
            double timeOnCurrentOrbit = PreviousManeuver == null ? Spacecraft.current.timeSinceVelocityChanged : 0f;
            double currentTimeToOrbit = MathLib.Max(timeToOrbit - timeOnCurrentOrbit, 0f); 
            
            if (enterMeanAnomaly > meanAnomaly) enterMeanAnomaly -= MathLib.PI * 2f;
            double time = ((meanAnomaly - enterMeanAnomaly) / orbit.elements.meanMotion) + currentTimeToOrbit;

            // Debug.Log("===== MANEUVER ======");
            // Debug.Log("maneuver anomaly: " + anomaly + " mean: " + meanAnomaly);
            // Debug.Log("enter mean: " + enterMeanAnomaly + " mean motion: " + orbit.elements.meanMotion);
            // Debug.Log("time on orbit: " +  (time - currentTimeToOrbit).ToTimeSpan() + " to orbit: " + currentTimeToOrbit.ToTimeSpan());
            // Debug.Log("=====================");

            if (time < 0) {
                time += orbit.elements.period;
            }
            return time;
        }
        private double GetBurnTime() {
            return addedVelocity.magnitude / spacecraft.Thrust;
        }
        public void RotateNode(Vector3Double orbitPosition) {
            Node.gameObject.transform.rotation = Quaternion.LookRotation((Vector3)stateVectors.velocity);
        }

        public void ChangeVelocity(Vector3Double dV) {
            addedVelocity += dV;
            StateVectors newVectors = new StateVectors(stateVectors.position, stateVectors.velocity + addedVelocity);

            drawer.DrawOrbits(newVectors, orbit.centralBody, timeToManeuver);

            burnTime = GetBurnTime();
        }
        public void ChangePosition(Vector3Double newPosition) {
            Node.gameObject.transform.localPosition = (Vector3)newPosition + orbit.centralBody.transform.localPosition;

            // rotate vector to match new position
            double angle = Vector3Double.SignedAngle(lastVelocity, this.stateVectors.velocity, orbit.elements.angMomentum);
            addedVelocity = (Vector3Double)(Quaternion.AngleAxis((float)angle, (Vector3)orbit.elements.angMomentum) * (Vector3)addedVelocity);

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
                PreviousManeuver.drawer.TurnOnRenderersFrom(orbitIdx + 1);
            }
            else if (PreviousDrawer != null) {
                PreviousDrawer.hasManeuver = false;
            }
        }
    }
}
