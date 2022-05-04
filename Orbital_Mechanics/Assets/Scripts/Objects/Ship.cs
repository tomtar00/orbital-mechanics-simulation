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
        private float timeOnOrbit = 0;

        private void Awake() {
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

        bool control = false;
        private void Update()
        {
            this.velocity = CalculateVelocity(out this.speed);  
            if (control) HandleControls();  
            MoveAlongOrbit(); 

            control = !control;
        }

        private void AddVelocity(Vector3 d_vel)
        {
            this.velocity += d_vel; 
            orbit = KeplerianOrbit.CalculateOrbitElements(transform.position, this.velocity, celestial);
            orbitDrawer.DrawOrbit(orbit);
            timeOnOrbit = 0;
        }

        private void MoveAlongOrbit()
        {
            float eccentricAnomaly = KeplerianOrbit.CalculateEccentricAnomaly(orbit, timeOnOrbit);
            transform.position = celestial.transform.position + KeplerianOrbit.CalculateOrbitalPosition(orbit, eccentricAnomaly);
            timeOnOrbit += Time.deltaTime;
        }

        private Vector3 CalculateVelocity(out float speed, bool global = false)
        {
            speed = Mathf.Sqrt(KeplerianOrbit.G * celestial.Mass * (2/transform.position.magnitude - 1/orbit.semimajorAxis));
            Vector3 velocity;
            if (global)
            {
                velocity = (transform.position - lastPosition) / Time.deltaTime;
            }
            else
            {
                velocity = ((transform.position - lastPosition) - celestial.transform.position) / Time.deltaTime;
            }
            lastPosition = transform.position;
            return velocity.normalized * speed;
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
        private void ExitCelestialInfluence() {
            //celestial = new Celestial...
            orbitDrawer.DestroyOrbitRenderer();
        }
        
    }
}
