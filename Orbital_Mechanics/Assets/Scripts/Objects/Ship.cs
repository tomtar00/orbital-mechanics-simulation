using UnityEngine;
using Sim.Visuals;
using Sim.Math;

namespace Sim.Objects
{
    [RequireComponent(typeof(OrbitDrawer))]
    public class Ship : InOrbitObject
    {
        [Header("Ship")]
        [SerializeField] protected Vector3 startVelocity;
        [SerializeField] protected float thrust;

        private void Awake()
        {
            ShipManager.Instance.Ships.Add(this);
        }

        private new void Start()
        {
            base.Start();

            relativePosition = transform.position - celestial.RelativePosition;
            if (isStationary)
            {
                Debug.LogWarning($"Ship object ({gameObject.name}) is stationary!");
            }
            else
            {
                // move out from start
                AddVelocity(startVelocity);
            }
        }

        private new void Update()
        {
            base.Update();

            HandleControls();
        }

        private void AddVelocity(Vector3 d_vel)
        {
            Vector3 newVelocity = this.velocity + d_vel;
            orbit = KeplerianOrbit.CalculateOrbitElements(relativePosition, newVelocity, celestial.Data.Mass);
            orbitDrawer.DrawOrbit(orbit);
            timeOnOrbit = 0;
        }

        private void HandleControls()
        {
            Vector3 thrustForward = this.velocity.normalized * thrust;
            if (Input.GetKey(KeyCode.W))
            {
                AddVelocity(thrustForward);
            }
            if (Input.GetKey(KeyCode.S))
            {
                AddVelocity(-thrustForward);
            }
        }

        // call this later
        private void ExitCelestialInfluence()
        {
            //celestial = new Celestial...
            orbitDrawer.DestroyOrbitRenderer();
        }

    }
}
