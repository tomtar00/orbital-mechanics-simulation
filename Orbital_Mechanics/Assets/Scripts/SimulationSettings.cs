using UnityEngine;

namespace Sim {
    public class SimulationSettings : MonoBehaviour
    {
        [Header("Materials")]
        public Material trajectoryMat;
        public Material dashedTrajectoryMat;
        [Header("Orbits")]
        public Color[] futureOrbitColors;
        public Color celestialOrbitColor;
        public GameObject indicationPrefab;
        public float linePressTolerance = 15f;
        [Space]
        public float lineScaleMultiplier = .001f;
        public float lineMinScale = 0.01f;
        public float lineMaxScale = 1e+20f;
        [Header("Maneuvers")]
        public float influenceChangeTimeSlowdownOffset = 3;
        public float maneuverTimeSlowdownOffset = 3;
        public float addVelocitySensitivity = 0.01f;
        public double[] maneuverPrecision;
        [Header("Constants")]
        public float G;
        public float scale = 1e-10f;
        public float gravityFalloff = 1f;
        [Header("Visual")]
        public float dontDrawPlanetOrbitMultiplier = 5;
        public float dontDrawMoonOrbitMultiplier = 1;

        public static SimulationSettings Instance;
        private void Awake() {
            Instance = this;
            G *= scale * scale;
            gravityFalloff *= scale;
        }

        private void Start() {
            Application.targetFrameRate = 144;
            QualitySettings.vSyncCount = 0;
        }
    }
}
