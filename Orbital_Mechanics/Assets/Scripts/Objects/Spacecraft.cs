using UnityEngine;
using Sim.Visuals;
using Sim.Math;

// failing at
// v = 0, 0, 8
// r = 45, 0, -15

// encounter at
// v = 0, 0, 7.7
// r = 30, 0, 0

namespace Sim.Objects
{
    [RequireComponent(typeof(OrbitDrawer))]
    public class Spacecraft : InOrbitObject
    {
        [Header("Spacecraft")]

        [SerializeField] protected bool useCustomStartVelocity;

        [SerializeField]
        [DrawIf("useCustomStartVelocity", true, ComparisonType.Equals)] 
        protected Vector3 startVelocity;

        [SerializeField] protected Vector3 startRelativePosition;
        [Header("Controls")]
        [SerializeField] protected float rotationSpeed = 50;
        [SerializeField] protected float thrust;

        private bool canUpdateOrbit = true;

        private void Start()
        {
            if (isStationary)
            {
                relativePosition = startRelativePosition;
                Debug.LogWarning($"Ship object ({gameObject.name}) is stationary!");
            }
            else
            {
                // move out from start
                InitializeShip();
            }
        }

        private new void Update()
        {
            base.Update();

            UpdateOrbitRenderer();
            
            HandleControls();
            CheckCelestialInfluence();
        }

        private void OnValidate()
        {
            transform.position = centralBody.transform.position + startRelativePosition;
        }

        private void InitializeShip()
        {
            relativePosition = startRelativePosition;
            transform.position = centralBody.RelativePosition + startRelativePosition;

            if (useCustomStartVelocity)
                AddVelocity(startVelocity);
            else {
                Vector3 velDirection = Vector3.Cross(relativePosition, Vector3.up).normalized;
                AddVelocity(velDirection * CircularOrbitSpeed());
            }
        }

        private float CircularOrbitSpeed()
        {
            return MathLib.Sqrt(KeplerianOrbit.G * centralBody.Data.Mass / relativePosition.magnitude);
        }

        private void AddVelocity(Vector3 d_vel)
        {
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
                    transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                } else if (Input.GetKey(KeyCode.D)) {
                    transform.Rotate(-Vector3.up, rotationSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.W)){
                    transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
                } else if (Input.GetKey(KeyCode.S)) {
                    transform.Rotate(-Vector3.right, rotationSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.Space)) {
                    AddVelocity(transform.forward * thrust * Time.deltaTime);
                }
            }
        }
        private void CheckCelestialInfluence()
        {
            if (centralBody == null) return;

            if (relativePosition.sqrMagnitude - centralBody.InfluenceRadius * centralBody.InfluenceRadius > 0) // FIXME: change 0 to something else
            {
                ExitCelestialInfluence();
            }
            else
            {
                foreach (var orbitingCelestial in centralBody.celestialsOnOrbit)
                {
                    if ((transform.position - orbitingCelestial.transform.position).sqrMagnitude - orbitingCelestial.InfluenceRadius * orbitingCelestial.InfluenceRadius < -0.5f)
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

            //Debug.Log($"Exiting {centralBody.name} with vectors: R = {transform.position - centralBody.CentralBody.transform.position}, V = {velocity + previousCentralBodyVelocity}");

            centralBody = centralBody.CentralBody;
            kepler.orbit.ChangeCentralBody(centralBody);

            if (centralBody != null)
            {
                UpdateRelativePosition();
                AddVelocity(previousCentralBodyVelocity);
            }
        }
        private void EnterCelestialInfluence(Celestial celestial)
        {
            centralBody = celestial;
            kepler.orbit.ChangeCentralBody(centralBody);

            UpdateRelativePosition();
            AddVelocity(-centralBody.Velocity);

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

        private void OnGUI()
        {
            float startHeight = 20;
            float space = 20;
            int i = 0;

            OrbitElements elements = this.kepler.orbit.elements;

            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Semimajor Axis: {elements.semimajorAxis}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Eccentricity: {elements.eccentricity}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Inclination: {elements.inclination}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Longitude of the ascending node: {elements.lonAscNode}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Argument of periapsis: {elements.argPeriapsis}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"SemilatusRectum: {elements.semiLatusRectum}");
            i++;
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Mean anomaly: {elements.meanAnomaly}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"True anomaly:  {elements.trueAnomaly}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Anomaly:        {elements.anomaly}");
            i++;
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Time to periapsis: {elements.timeToPeriapsis}");
        }

    }
}
