using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sim.Visuals {
    public class ManeuverEditor : MonoBehaviour
    {
        [SerializeField] private Transform parent;

        public Maneuver maneuver { get; set; }
        private bool isDragging = false;
        private LineButton lineButton = null;

        private void Start() {
            LineButton.onLineHovering += (line, worldPos) => {
                if (lineButton == null) lineButton = line;
                if (isDragging && lineButton == line) {
                    parent.position = worldPos;
                    maneuver.Draw(worldPos, Vector3.up);
                }
            };
        }

        private void Update() {
            RaycastHit hit;
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    isDragging = true;
                    //maneuver.Drawer.EnableLineButtons(false);
                }
            }
            else if (Input.GetMouseButton(0))
            {
                isDragging = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                //maneuver.Drawer.EnableLineButtons(true);
            }
        }

        public void ChangeVelocity(Vector3 velDifference) {
            maneuver.Draw(parent.position, velDifference);
        }
    }
}
