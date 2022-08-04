using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sim {
    public class SimulationSettings : MonoBehaviour
    {
        public Material trajectoryMat;

        public static SimulationSettings Instance;
        private void Awake() {
            Instance = this;
        }
    }
}
