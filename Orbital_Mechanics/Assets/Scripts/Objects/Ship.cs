using UnityEngine;
using Sim.Visuals;
using Sim.Math;

namespace Sim.Objects
{
    [RequireComponent(typeof(OrbitDrawer))]
    public class Ship : InOrbitObject
    {
        [Header("Ship")]
        [SerializeField] protected Vector3 startRelativePosition;
        [SerializeField] protected float thrust;

        // private void Awake()
        // {
        //     ShipManager.Instance.Ships.Add(this);
        // }

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

            //Debug.Log(orbit.trueAnomaly + " -- " + orbit.meanAnomaly + " -- " + orbit.eccentricity);
        }

        private void InitializeShip() {
            relativePosition = startRelativePosition;
            transform.position = centralBody.RelativePosition + startRelativePosition;
            Vector3 velDirection = Vector3.Cross(relativePosition, Vector3.up).normalized;
            Vector3 startVelocity = velDirection * Mathf.Sqrt(KeplerianOrbit.G * centralBody.Data.Mass / relativePosition.magnitude);
            AddVelocity(startVelocity);
        }

        private void AddVelocity(Vector3 d_vel)
        {
            Vector3 newVelocity = this.velocity + d_vel;
            orbit = KeplerianOrbit.CalculateOrbitElements(relativePosition, newVelocity, centralBody.Data.Mass);
            //Debug.Log(orbit.eccentricity);
            orbitDrawer.DrawOrbit(orbit, centralBody.InfluenceRadius);
            timeOnOrbit = 0;
        }

        private void HandleControls()
        {
            Vector3 thrustForward = this.velocity.normalized * thrust;
            if (Input.GetKey(KeyCode.M))
            {
                AddVelocity(thrustForward);
            }
            if (Input.GetKey(KeyCode.N))
            {
                AddVelocity(-thrustForward);
            }

            if (Input.GetKeyDown(KeyCode.R)) {
                InitializeShip();
            }
        }

        private void CheckCelestialInfluence() {
            if (relativePosition.sqrMagnitude > centralBody.InfluenceRadius * centralBody.InfluenceRadius) {
                ExitCelestialInfluence();
            }
        }

        // call this later
        private void ExitCelestialInfluence()
        {
            orbitDrawer.DestroyOrbitRenderer();
            Vector3 previousCentralBodyVelocity = centralBody.Velocity;
            centralBody = centralBody.CentralBody;
            relativePosition = transform.position - centralBody.RelativePosition;
            orbitDrawer.SetupOrbitRenderer(centralBody.transform);
            AddVelocity(previousCentralBodyVelocity);
            orbitDrawer.DrawOrbit(orbit, centralBody.InfluenceRadius);
        }

    }
}
