using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sim.Math;

namespace Sim.Objects
{
    [RequireComponent(typeof(Rigidbody))]
    public class Celestial : MonoBehaviour
    {
        private Rigidbody body;
        public Rigidbody Body { get => body; }

        // Object properties
        [SerializeField] private float mass;
        public float Mass { get => mass; set => mass = value; }
        [SerializeField] private float radius;
        public float Radius { get => radius; set => radius = value; }

        private void Start() {
            body = GetComponent<Rigidbody>();
            transform.localScale = Vector3.one * radius;
            body.mass = mass;
        }

        private void FixedUpdate() {
            foreach (var ship in ShipManager.Instance.Ships) {
                PullTowardsSelf(ship);
            }
        }

        private void PullTowardsSelf(Ship ship) {
            Vector3 direction = transform.position - ship.transform.position;
            float gravityForce = KeplerianOrbit.G * mass / Mathf.Pow(direction.magnitude, 2);
            ship.Body.velocity += direction.normalized * gravityForce;
        }
    }
}