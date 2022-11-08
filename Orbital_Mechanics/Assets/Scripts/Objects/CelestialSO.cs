using System.Collections.Generic;
using UnityEngine;
using Sim.Orbits;

namespace Sim.Objects
{
    [CreateAssetMenu(fileName = "Celestial", menuName = "Orbital_Mechanics/Celestial", order = 0)]
    public class CelestialSO : ScriptableObject
    {
        [SerializeField] private float mass;
        public float Mass { get => mass; set => mass = value; }
        
        [SerializeField] private float diameter;
        public float Diameter { get => diameter; set => diameter = value; }

        [SerializeField] private OrbitalElements orbit;
        public OrbitalElements Orbit { get => orbit; set => orbit = value; }

        [Space]
        [SerializeField] private double argPrecessionPeriod;
        public double ArgPrecessionPeriod { get => argPrecessionPeriod; }
        [SerializeField] private double ascPrecessionPeriod; 
        public double AscPrecessionPeriod { get => ascPrecessionPeriod; }

        [Space]

        [SerializeField] private Material material;
        public Material Material { get => material; }

        [Space]

        [SerializeField] private List<CelestialSO> bodiesOnOrbit;
        public List<CelestialSO> BodiesOnOrbit { get => bodiesOnOrbit; }

        [Space]

        [SerializeField] private CelestialBodyType type;
        public CelestialBodyType Type { get => type; }
    }

    public enum CelestialBodyType {
        STAR,
        PLANET,
        MOON
    }
}

