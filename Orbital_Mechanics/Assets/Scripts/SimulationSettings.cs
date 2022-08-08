using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sim {
    public class SimulationSettings : MonoBehaviour
    {
        public Material trajectoryMat;
        public Color[] futureOrbitColors;
        public Color celestialOrbitColor;
        public GameObject indicationPrefab;

        public static SimulationSettings Instance;
        private void Awake() {
            Instance = this;
        }
    }
}
