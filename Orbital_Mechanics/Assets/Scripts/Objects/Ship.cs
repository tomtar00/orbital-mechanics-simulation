using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sim.Visuals;
using Sim.Math;

namespace Sim.Objects
{
    [RequireComponent(typeof(Rigidbody))]
    public class Ship : MonoBehaviour
    {
        private Rigidbody body;
        public Rigidbody Body { get => body; }

        [SerializeField] private Vector3 startVelocity;
        [SerializeField] private Celestial celestial;
        [SerializeField] private KeplerianOrbit.Elements orbit;

        private OrbitDrawer orbitDrawer;

        private void Start()
        {
            body = GetComponent<Rigidbody>();
            orbitDrawer = GetComponent<OrbitDrawer>();

            ShipManager.Instance.Ships.Add(this);

            AddVelocity(startVelocity);
        }

        private void AddVelocity(Vector3 velocity) {
            var targetVelocity = body.velocity + velocity;
            body.velocity += velocity * 7.07f; // ????
            orbit = KeplerianOrbit.CalculateOrbitElements(transform.position, targetVelocity / 7.07f, celestial);
            orbitDrawer.DrawOrbit(orbit);
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.W)) {
                AddVelocity(body.velocity.normalized * 0.01f);
            }
            if (Input.GetKeyDown(KeyCode.S)) {
                AddVelocity(-body.velocity.normalized * 0.01f);
            }
        }
    }
}
