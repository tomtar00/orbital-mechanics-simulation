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

        private OrbitDrawer orbitDrawer;
        [SerializeField] private Vector3 velocity;
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

        private void Update()
        {
            HandleControls(thrust);
            MoveAlongOrbit();
            CalculateVelocity();  
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
            transform.position = celestial.transform.position + KeplerianOrbit.CalculatePositionOnOrbit(orbit, eccentricAnomaly);
            timeOnOrbit += Time.deltaTime;
        }

        private void CalculateVelocity(bool global = false)
        {
            if (global)
            {
                this.velocity = (transform.position - lastPosition) / Time.deltaTime;
            }
            else
            {
                this.velocity = ((transform.position - lastPosition) - celestial.transform.position) / Time.deltaTime;
            }
            lastPosition = transform.position;
        }

        private void HandleControls(float thrust)
        {
            Vector3 thrustForward = this.velocity.normalized * thrust;
            if (Input.GetKeyDown(KeyCode.W))
            {
                AddVelocity(thrustForward);
            }
            if (Input.GetKeyDown(KeyCode.S))
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
