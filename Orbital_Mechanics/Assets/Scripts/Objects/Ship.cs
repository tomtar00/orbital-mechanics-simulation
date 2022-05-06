using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sim.Visuals;
using Sim.Math;

namespace Sim.Objects
{
    [RequireComponent(typeof(OrbitDrawer))]
    public class Ship : MonoBehaviour
    {
        [SerializeField] private Vector3 startVelocity;
        [SerializeField] private float thrust;
        [SerializeField] private Celestial celestial;
        [SerializeField] private KeplerianOrbit.Elements orbit;
        [SerializeField] private Vector3 velocity;
        [SerializeField] private float speed;

        private OrbitDrawer orbitDrawer;
        private Vector3 lastPosition;
        private float timeOnOrbit;

        private void Awake()
        {
            ShipManager.Instance.Ships.Add(this);
        }

        private void Start()
        {
            orbitDrawer = GetComponent<OrbitDrawer>();
            orbitDrawer.SetupOrbitRenderer(celestial.transform);

            this.velocity = Vector3.zero;
            AddVelocity(startVelocity);

            lastPosition = transform.position;
        }

        private void Update()
        {   
            this.velocity = CalculateVelocity(out this.speed);
            HandleControls();
            MoveAlongOrbit();
        }

        private void OnDrawGizmos()
        {
            Debug.DrawLine(transform.position, this.velocity + transform.position);
        }

        private void AddVelocity(Vector3 d_vel)
        {
            Vector3 newVelocity = this.velocity + d_vel;
            orbit = KeplerianOrbit.CalculateOrbitElements(transform.position, newVelocity, celestial);
            orbitDrawer.DrawOrbit(orbit);
            timeOnOrbit = 0;
        }

        private void MoveAlongOrbit()
        {
            float eccentricAnomaly = KeplerianOrbit.CalculateEccentricAnomaly(orbit, timeOnOrbit);
            transform.position = celestial.transform.position + KeplerianOrbit.CalculateOrbitalPosition(orbit, eccentricAnomaly, out orbit.trueAnomaly);
            timeOnOrbit += Time.deltaTime;
        }

        // source: https://www.orbiter-forum.com/threads/calculate-not-derive-orbital-velocity-vector.22367/
        private Vector3 CalculateVelocity(out float speed, bool global = false)
        {
            Vector3 localPos = transform.position - celestial.transform.position;
            speed = Mathf.Sqrt(KeplerianOrbit.G * celestial.Mass * (2 / localPos.magnitude - 1 / orbit.semimajorAxis));

            Vector3 orbitNormal = Vector3.up; // EDIT
            float e = orbit.eccentricity;
            float k = localPos.magnitude / orbit.semimajorAxis;
            float alpha = Mathf.Acos((2 - 2 * e * e) / (k * (2 - k)) - 1);
            float angle = alpha + ((Mathf.PI - alpha) / 2);         
            if (orbit.trueAnomaly < Mathf.PI)
                angle = Mathf.PI - angle;
            Vector3 velocity = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, orbitNormal) * localPos.normalized * speed;
            return velocity;
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
