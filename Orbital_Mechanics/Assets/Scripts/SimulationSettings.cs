using UnityEngine;

namespace Sim {
    public class SimulationSettings : MonoBehaviour
    {
        public Material trajectoryMat;
        public Material dashedTrajectoryMat;
        [Space]
        public Color[] futureOrbitColors;
        public Color celestialOrbitColor;
        public GameObject indicationPrefab;
        [Space]
        public float influenceChangeTimeSlowdownOffset = 3;
        public float maneuverTimeSlowdownOffset = 3;
        [Space]
        public float G;

        public static SimulationSettings Instance;
        private void Awake() {
            Instance = this;
        }
    }
}
