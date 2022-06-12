using UnityEngine;
using Sim.Visuals;
using Sim.Math;

namespace Sim.Objects
{
    [RequireComponent(typeof(OrbitDrawer))]
    public class Spacecraft : InOrbitObject
    {
        [Header("Spacecraft")]

        [SerializeField] protected bool useCustomStartVelocity;
        [SerializeField][DrawIf("useCustomStartVelocity", true, ComparisonType.Equals)] protected Vector3 startVelocity;

        [SerializeField] protected Vector3 startRelativePosition;
        [SerializeField] protected float thrust;

        private new void Start()
        {
            base.Start();
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
            Vector3 velDirection = Vector3.Cross(relativePosition, Vector3.up).normalized;
            Vector3 startVelocity;
            if (useCustomStartVelocity)
                startVelocity = this.startVelocity;
            else
                startVelocity = velDirection * CircularOrbitSpeed();
            AddVelocity(startVelocity);
        }

        private float CircularOrbitSpeed()
        {
            return MathLib.Sqrt(KeplerianOrbit.G * centralBody.Data.Mass / relativePosition.magnitude);
        }

        private void AddVelocity(Vector3 d_vel)
        {
            Vector3 newVelocity = this.velocity + d_vel;

            float e = trajectory.orbit.CalculateEccentricity(relativePosition, newVelocity);
            trajectory.CheckOrbitType(e);

            trajectory.orbit.CalculateMainOrbitElements(relativePosition, newVelocity);
            orbitDrawer.DrawOrbit(trajectory, centralBody.InfluenceRadius);

            orbitNormal = trajectory.orbit.angMomentum;
        }

        private void HandleControls()
        {
            Vector3 thrustForward = this.velocity.normalized * thrust * Time.deltaTime;
            if (Input.GetKey(KeyCode.M))
            {
                AddVelocity(thrustForward);
            }
            if (Input.GetKey(KeyCode.N))
            {
                AddVelocity(-thrustForward);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                InitializeShip();
            }
        }
        private void CheckCelestialInfluence()
        {
            if (centralBody == null) return;

            if (relativePosition.sqrMagnitude > centralBody.InfluenceRadius * centralBody.InfluenceRadius)
            {
                ExitCelestialInfluence();
            }
            else
            {
                foreach (var orbitingCelestial in centralBody.celestialsOnOrbit)
                {
                    if ((transform.position - orbitingCelestial.transform.position).sqrMagnitude < orbitingCelestial.InfluenceRadius * orbitingCelestial.InfluenceRadius)
                    {
                        EnterCelestialInfluence(orbitingCelestial);
                        break;
                    }
                }
            }
        }
        private void ExitCelestialInfluence()
        {
            // Debug.Log($"{gameObject.name} exited {centralBody.gameObject.name}");
            orbitDrawer.DestroyOrbitRenderer();
            Vector3 previousCentralBodyVelocity = centralBody.Velocity;

            centralBody = centralBody.CentralBody;
            trajectory.orbit.ChangeCentralBody(centralBody);

            if (centralBody != null)
            {
                UpdateRelativePosition();
                orbitDrawer.SetupOrbitRenderer(this, centralBody.transform);
                AddVelocity(previousCentralBodyVelocity);
            }
        }
        private void EnterCelestialInfluence(Celestial celestial)
        {
            // Debug.Log($"{gameObject.name} entered {celestial.gameObject.name}");
            orbitDrawer.DestroyOrbitRenderer();
            // Vector3 previousCentralBodyVelocity = centralBody.Velocity;

            centralBody = celestial;
            trajectory.orbit.ChangeCentralBody(centralBody);

            UpdateRelativePosition();
            orbitDrawer.SetupOrbitRenderer(this, centralBody.transform);
            AddVelocity(-centralBody.Velocity);
        }

        private void OnGUI()
        {
            float startHeight = 20;
            float space = 20;
            int i = 0;

            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Semimajor Axis: {trajectory.semimajorAxis}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Eccentricity: {trajectory.eccentricity}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Inclination: {trajectory.inclination}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Longitude of the ascending node: {trajectory.lonAscNode}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Argument of periapsis: {trajectory.argPeriapsis}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"SemilatusRectum: {trajectory.semiLatusRectum}");
            i++;
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Mean anomaly: {trajectory.meanAnomaly}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"True anomaly:  {trajectory.trueAnomaly}");
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Anomaly:        {trajectory.anomaly}");
            i++;
            GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Time: x{Time.timeScale}");
            if (GUI.Button(new Rect(10, startHeight + space * i, 30, 30), "<"))
                Time.timeScale -= 1;
            if (GUI.Button(new Rect(45, startHeight + space * i++, 30, 30), ">"))
                Time.timeScale += 1;
        }

    }
}
