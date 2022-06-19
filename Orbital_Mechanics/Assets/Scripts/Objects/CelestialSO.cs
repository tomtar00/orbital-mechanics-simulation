using UnityEngine;
using Sim.Math;

namespace Sim.Objects
{
    [CreateAssetMenu(fileName = "Celestial", menuName = "Orbital_Mechanics/Celestial", order = 0)]
    public class CelestialSO : ScriptableObject
    {
        [SerializeField] private float mass;
        public float Mass { get => mass; }
        
        [SerializeField] private float radius;
        public float Radius { get => radius; }

        [SerializeField] private OrbitElements orbit;
        public OrbitElements Orbit { get => orbit; }
    }
}

