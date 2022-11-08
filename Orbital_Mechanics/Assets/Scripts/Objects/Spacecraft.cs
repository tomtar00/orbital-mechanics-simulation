using UnityEngine;

using Sim.Orbits;
using Sim.Visuals;
using Sim.Math;
using Sim.Maneuvers;

namespace Sim.Objects
{
    [RequireComponent(typeof(OrbitDrawer))]
    public class Spacecraft : InOrbitObject
    {
        [Header("Spacecraft")]
        [SerializeField] protected Transform model;
        [SerializeField] protected Transform maneuverDirection;
        [Space]
        [SerializeField] protected bool useCustomStartVelocity;
        [SerializeField]
        [DrawIf("useCustomStartVelocity", true, ComparisonType.Equals)] 
        protected Vector3 startVelocity;
        [SerializeField] protected float startSurfaceAltitude;

        [Header("Controls")]
        [SerializeField] protected float rotationSpeed = 50;
        [SerializeField] protected float thrust;
        [Space]
        [SerializeField] private bool autopilot;
        [SerializeField] private Material maneuverSuccessMat;
        [SerializeField] private Material maneuverFailMat;
        [SerializeField] private float maneuverSuccessAngle = 5f;
        [SerializeField] private MeshRenderer[] arrowMeshRenderers;

        [Header("Scale")]
        [SerializeField] private float scaleMultiplier = .1f;
        [SerializeField] private float minScale = .2f;
        [SerializeField] private float maxScale = 10f;

        public static Spacecraft current { get; private set; }
        public double timeSinceVelocityChanged { get; private set; }
        public double timeToNextGravityChange { get; set; }
        public double timeSinceGravityChange { get; set; }
        public float Thrust { get => thrust; }
        public bool Autopilot { set => autopilot = value; }

        private bool canUpdateOrbit = true;
        private bool slowingDown = false;
        private double maneuverInaccuracy = float.MaxValue;

        public string TimeToGravityChange {
            get => (timeToNextGravityChange > 0) ? timeToNextGravityChange.ToTimeSpan() : "Inf";
        }

        private new void Awake() {
            base.Awake();
            current = this;
        }

        private void Start()
        {
            if (isStationary)
            {
                Debug.LogWarning($"Ship object ({gameObject.name}) is stationary!");
            }
        }

        private void LateUpdate()
        {
            timeSinceVelocityChanged += Time.deltaTime;
            timeToNextGravityChange -= Time.deltaTime;
            timeSinceGravityChange += Time.deltaTime;
            
            base.UpdateObject();

            UpdateOrbitRenderer();
            
            HandleControls();
            CheckWillSoonEnterExitInfluence();
            CheckCelestialInfluence();

            HandleManeuverDirection();  
            AutomateManeuvers();

            gameObject.transform.localScale = NumericExtensions.ScaleWithDistance(
                gameObject.transform.position, CameraController.Instance.cam.transform.position,
                scaleMultiplier, minScale, maxScale
            );
        }

        public void InitializeShip()
        {
            stateVectors = new StateVectors();
            orbitDrawer = GetComponent<OrbitDrawer>();
            orbitDrawer?.SetupOrbitRenderers();
            if (!isStationary)
            {
                kepler = new KeplerianOrbit();
            }

            stateVectors.position = Vector3Double.right * (startSurfaceAltitude + centralBody.Model.transform.localScale.x / 2);
            transform.localPosition = centralBody.StateVectors.position + stateVectors.position;

            if (useCustomStartVelocity)
                AddVelocity(startVelocity);
            else {
                Vector3 velDirection = Vector3.Cross(stateVectors.position, Vector3.up).normalized;
                AddVelocity(velDirection * CircularOrbitSpeed());
            }

            Debug.Log("spacecraft period: " + kepler.orbit.elements.period.ToTimeSpan());
        }
        private float CircularOrbitSpeed()
        {
            return (float)MathLib.Sqrt(KeplerianOrbit.G * centralBody.Data.Mass / stateVectors.position.magnitude) * 1.001f;
        }

        private void AddVelocity(Vector3 d_vel)
        {
            timeSinceVelocityChanged = 0;
            Vector3Double newVelocity = this.stateVectors.velocity + d_vel;

            StateVectors stateVectors = new StateVectors(this.stateVectors.position, newVelocity);
            kepler.CheckOrbitType(stateVectors, centralBody);

            kepler.orbit.ConvertStateVectorsToOrbitElements(stateVectors);

            orbitDrawer.DrawOrbits(stateVectors, centralBody);
        }

        private void HandleControls()
        {
            if (CameraController.Instance.focusingOnObject) {
                if (Input.GetKey(KeyCode.A)){
                    model.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                } else if (Input.GetKey(KeyCode.D)) {
                    model.transform.Rotate(-Vector3.up, rotationSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.W)){
                    model.transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
                } else if (Input.GetKey(KeyCode.S)) {
                    model.transform.Rotate(-Vector3.right, rotationSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.Space)) {
                    AddVelocity(model.transform.forward * thrust * Time.deltaTime);
                }
            }
        }
        private void HandleManeuverDirection() {
            Maneuver next = ManeuverManager.Instance.NextManeuver;
            if (next != null) {
                if (next.addedVelocity == Vector3.zero) return;
                if (!maneuverDirection.gameObject.activeSelf) {
                    maneuverDirection.gameObject.SetActive(true);
                }
                maneuverDirection.rotation = Quaternion.LookRotation(next.addedVelocity);
                if (Vector3Double.Angle(model.transform.forward, next.addedVelocity) < maneuverSuccessAngle) {
                    foreach(var mesh in arrowMeshRenderers)
                        mesh.material = maneuverSuccessMat;
                }
                else {
                    foreach(var mesh in arrowMeshRenderers)
                        mesh.material = maneuverFailMat;
                }
            } 
            else if (maneuverDirection.gameObject.activeSelf) {
                maneuverDirection.gameObject.SetActive(false);
            }
        }
        private void AutomateManeuvers() {
            if (!autopilot) return;
            Maneuver next = ManeuverManager.Instance.NextManeuver;
            if (next == null) return;
                
            double currentManeuverInaccuracy;
            bool isTargetOrbit = kepler.orbit.Equals(next.drawer.orbits[0], SimulationSettings.Instance.maneuverPrecision, out currentManeuverInaccuracy);
            
            if (next.timeToManeuver < next.burnTime / 2 && !isTargetOrbit && currentManeuverInaccuracy < maneuverInaccuracy) {
                AddVelocity(model.transform.forward * thrust * Time.deltaTime);
                maneuverInaccuracy = currentManeuverInaccuracy;
            }
            else
            {
                maneuverInaccuracy = float.MaxValue;
                if (ManeuverNode.isDraggingAny) return;
                if (next.timeToManeuver < -next.burnTime / 2) 
                {
                    HUDController.Instance.SetTimeScaleToPrevious();
                    ManeuverManager.Instance.RemoveFirst();
                }
                else if (next.timeToManeuver < next.burnTime / 2 + SimulationSettings.Instance.maneuverTimeSlowdownOffset) {
                    HUDController.Instance.SetTimeScaleToDefault();
                }
            }

            if (next.addedVelocity != Vector3.zero)
            {
                model.rotation = Quaternion.RotateTowards(model.rotation, Quaternion.LookRotation(next.addedVelocity), rotationSpeed * Time.deltaTime);
            }
            
        }
        private void CheckCelestialInfluence()
        {
            if (centralBody == null) return;

            if (stateVectors.position.sqrMagnitude - centralBody.InfluenceRadius * centralBody.InfluenceRadius > 0) // .5f
            {
                ExitCelestialInfluence();
            }
            else
            {
                foreach (var orbitingCelestial in centralBody.celestialsOnOrbit)
                {
                    if ((transform.localPosition - orbitingCelestial.transform.localPosition).sqrMagnitude - orbitingCelestial.InfluenceRadius * orbitingCelestial.InfluenceRadius < 0) // -.5f
                    {
                        EnterCelestialInfluence(orbitingCelestial);
                        break;
                    }
                }
            }
        }
        private void ExitCelestialInfluence()
        {
            Vector3 previousCentralBodyVelocity = centralBody.StateVectors.velocity;

            //Debug.Log($"Exiting {centralBody.name} with vectors: R = {transform.localPosition - centralBody.CentralBody.transform.localPosition}, V = {velocity + previousCentralBodyVelocity}");

            centralBody = centralBody.CentralBody;
            kepler.orbit.ChangeCentralBody(centralBody);

            if (centralBody != null)
            {
                UpdateRelativePosition();
                AddVelocity(previousCentralBodyVelocity);
            }

            timeSinceGravityChange = 0;
        }
        private void EnterCelestialInfluence(Celestial celestial)
        {
            centralBody = celestial;
            kepler.orbit.ChangeCentralBody(centralBody);

            UpdateRelativePosition();
            AddVelocity(-centralBody.StateVectors.velocity);

            timeSinceGravityChange = 0;

            //Debug.Log($"Entering {celestial.name} with vectors: R = {relativePosition}, V = {velocity - centralBody.Velocity}");
        }
        private void UpdateOrbitRenderer() {
            if (kepler.orbit.elements.timeToPeriapsis < 0.1f && 
                kepler.orbitType == OrbitType.ELLIPTIC &&
                orbitDrawer.lineRenderers[0].loop &&
                canUpdateOrbit) 
            {
                orbitDrawer.DrawOrbits(stateVectors, centralBody);
                canUpdateOrbit = false;
            }
            else canUpdateOrbit = true;
        }
        private void CheckWillSoonEnterExitInfluence() {
            if (timeToNextGravityChange < SimulationSettings.Instance.influenceChangeTimeSlowdownOffset && timeToNextGravityChange > 0) {
                HUDController.Instance.SetTimeScaleToDefault();
                slowingDown = true;
            }
            else if (timeSinceGravityChange > SimulationSettings.Instance.influenceChangeTimeSlowdownOffset && slowingDown) {
                HUDController.Instance.SetTimeScaleToPrevious();
                slowingDown = false;
            }
        }
    }
}
