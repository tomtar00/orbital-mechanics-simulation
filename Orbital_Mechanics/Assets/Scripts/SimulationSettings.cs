using UnityEngine;

namespace Sim {
    public class SimulationSettings : MonoBehaviour
    {
        public Material trajectoryMat;
        public Material dashedTrajectoryMat;

        public Color[] futureOrbitColors;
        public Color celestialOrbitColor;
        public GameObject indicationPrefab;

        public static SimulationSettings Instance;
        private void Awake() {
            Instance = this;
        }
    }
}
