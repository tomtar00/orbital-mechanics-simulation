using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sim.Math;

namespace Sim.Objects
{
    public class Celestial : MonoBehaviour
    {
        // Object properties
        [SerializeField] private float mass;
        public float Mass { get => mass; set => mass = value; }
        [SerializeField] private float radius;
        public float Radius { get => radius; set => radius = value; }

        private void Start() {
            transform.localScale = Vector3.one * radius;
        }
    }
}