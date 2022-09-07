using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sim.Visuals {
    public class ManeuverEditor : MonoBehaviour
    {

        [SerializeField] private Transform parent;

        public Maneuver maneuver { get; set; }
        private bool isDragging = false;

        private void Start() {
            LineButton.onLineHovering += (line, worldPos) => {
                if (isDragging) {
                    parent.position = worldPos;
                    maneuver.Draw(worldPos, Vector3.up);
                }
            };
        }

        private void OnMouseExit() {
            isDragging = false;
        }

        private void OnMouseDrag() {
            isDragging = true;
        }
    }
}
