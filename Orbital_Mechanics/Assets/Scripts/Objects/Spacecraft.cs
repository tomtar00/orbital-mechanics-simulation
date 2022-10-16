using System;
using System.Threading;
using UnityEngine;
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
        [SerializeField] private bool autoManeuvers;
        [SerializeField] private Material maneuverSuccessMat;
        [SerializeField] private Material maneuverFailMat;
        [SerializeField] private float maneuverSuccessAngle = 5f;
        [SerializeField] private MeshRenderer[] arrowMeshRenderers;

        [Header("Scale")]
        [SerializeField] private float scaleMultiplier = .1f;
        [SerializeField] private float minScale = .2f;
        [SerializeField] private float maxScale = 10f;

        public static Spacecraft current { get; private set; }
        public float timeSinceVelocityChanged { get; private set; }
        public float timeToNextGravityChange { get; set; }
        public float timeSinceGravityChange { get; set; }
        public float Thrust { get => thrust; }
        public bool AutoManeuvers { set => autoManeuvers = value; }

        private bool canUpdateOrbit = true;
        private bool slowingDown = false;

        private void Start()
        {
            current = this;
            if (isStationary)
            {
                relativePosition = Vector3.zero;
                Debug.LogWarning($"Ship object ({gameObject.name}) is stationary!");
            }
            else
            {
                InitializeShip();
            }
        }

        private new void Update()
        {
            timeSinceVelocityChanged += Time.deltaTime;
            timeToNextGravityChange -= Time.deltaTime;
            timeSinceGravityChange += Time.deltaTime;
            
            base.Update();

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

        private void InitializeShip()
        {
            relativePosition = Vector3.right * (startSurfaceAltitude + centralBody.Model.transform.localScale.x / 2);
            transform.localPosition = centralBody.RelativePosition + relativePosition;

            if (useCustomStartVelocity)
                AddVelocity(startVelocity);
            else {
                Vector3 velDirection = Vector3.Cross(relativePosition, Vector3.up).normalized;
                AddVelocity(velDirection * CircularOrbitSpeed());
                Debug.Log(JsonUtility.ToJson(kepler.orbit.elements, true));
                Debug.Log(JsonUtility.ToJson(centralBody.Kepler.orbit.elements, true));
            }

            Debug.Log("spacecraft period: " + kepler.orbit.elements.period.ToTimeSpan());
            Debug.Log(centralBody.name + " period: " + centralBody.Kepler.orbit.elements.period.ToTimeSpan());
        }
        private float CircularOrbitSpeed()
        {
            return MathLib.Sqrt(KeplerianOrbit.G * centralBody.Data.Mass / relativePosition.magnitude) * 1.001f;
        }

        private void AddVelocity(Vector3 d_vel)
        {
            timeSinceVelocityChanged = 0;
            Vector3 newVelocity = this.velocity + d_vel;

            StateVectors stateVectors = new StateVectors(relativePosition, newVelocity);
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
                if (Vector3.Angle(model.transform.forward, next.addedVelocity) < maneuverSuccessAngle) {
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
            if (!autoManeuvers) return;
            Maneuver next = ManeuverManager.Instance.NextManeuver;
            if (next == null) return;
                
            if (Mathf.Abs(next.timeToManeuver) <= next.burnTime / 2) {
                AddVelocity(model.transform.forward * thrust * Time.deltaTime);
            }
            else
            {
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

            if (relativePosition.sqrMagnitude - centralBody.InfluenceRadius * centralBody.InfluenceRadius > .1f) // .5f
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
            Vector3 previousCentralBodyVelocity = centralBody.Velocity;

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
            AddVelocity(-centralBody.Velocity);

            timeSinceGravityChange = 0;

            //Debug.Log($"Entering {celestial.name} with vectors: R = {relativePosition}, V = {velocity - centralBody.Velocity}");
        }
        private void UpdateOrbitRenderer() {
            if (kepler.orbit.elements.timeToPeriapsis < 0.1f && 
                kepler.orbitType == OrbitType.ELLIPTIC &&
                orbitDrawer.lineRenderers[0].loop &&
                canUpdateOrbit) 
            {
                StateVectors stateVectors = new StateVectors(relativePosition, velocity);
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

        private void OnGUI()
        {
            float startHeight = /* 20 */ Screen.height - 300;
            float space = 20;
            int i = 0;

            // OrbitElements elements = this.kepler.orbit.elements;
            string timeToGravityChange = (timeToNextGravityChange > 0) ? timeToNextGravityChange+"" : "Inf";

            // GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Semimajor Axis: {elements.semimajorAxis}");
            // GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Eccentricity: {elements.eccentricity}");
            // GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Inclination: {elements.inclination}");
            // GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Longitude of the ascending node: {elements.lonAscNode}");
            // GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Argument of periapsis: {elements.argPeriapsis}");
            // GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"SemilatusRectum: {elements.semiLatusRectum}");
            // i++;
            // GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Mean anomaly: {elements.meanAnomaly}");
            // GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"True anomaly:  {elements.trueAnomaly}");
            // GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Anomaly:        {elements.anomaly}");
            // i++;
            // GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Time to periapsis: {elements.timeToPeriapsis}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Time since velocity changed: {timeSinceVelocityChanged}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Time to next gravity change: {timeToGravityChange}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Time since gravity changed: {timeSinceGravityChange}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Velocity: {velocity.magnitude / SimulationSettings.Instance.scale} m/s");
        }

    }
}
