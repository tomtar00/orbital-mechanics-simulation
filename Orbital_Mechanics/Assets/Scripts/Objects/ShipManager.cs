using System.Collections.Generic;
using UnityEngine;

namespace Sim.Objects
{
    public class ShipManager : MonoBehaviour
    {
        public static ShipManager Instance;

        private List<Ship> ships;
        public List<Ship> Ships { get => ships; }

        private void Awake() {
            Instance = this;
            ships = new List<Ship>();

            // Sim.Math.KeplerianOrbit.Elements elements = 
            //     Sim.Math.KeplerianOrbit.CalculateOrbitElements(new Vector3(1,2,3), new Vector3(10,6,-5), 1);
            // Debug.Log(elements.semimajorAxis + " == " + 
            //     elements.eccentricity + " == " + 
            //     elements.inclination + " == " + 
            //     elements.lonAscNode + " == " + 
            //     elements.argPeriapsis + " == " + 
            //     elements.trueAnomaly + " == ");
        }
    }
}
