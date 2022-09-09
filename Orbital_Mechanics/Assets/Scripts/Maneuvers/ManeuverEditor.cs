using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Sim.Visuals {
    public class ManeuverEditor : MonoBehaviour
    {
        [SerializeField] private Transform parent;

        public Maneuver maneuver { get; set; }
        public static bool isDragging { get; private set; } = false;
        private LineButton lineButton = null;

        private void Start() {
            LineButton.onLineHovering += (line, worldPos) => {
                if (lineButton == null) lineButton = line;
                if (isDragging && lineButton == line) {
                    parent.position = worldPos;
                    maneuver.UpdateOnDrag(worldPos);
                }
            };
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = CameraController.Instance.cam.ScreenPointToRay(Input.mousePosition);
                int layer_mask = LayerMask.GetMask("Maneuvers");

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer_mask, QueryTriggerInteraction.Collide))
                {
                    Debug.Log("hit on layer     " + hit.collider.gameObject);

                    // Debug.Log("CLICK " + hit.collider.gameObject);
                    if (hit.collider.gameObject.CompareTag("ManeuverNode")) {
                        Debug.Log("pressing node");
                        isDragging = true;
                        maneuver.drawer.EnableLineButtons(false);
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                maneuver.drawer.EnableLineButtons(true);
            }
        }
    }
}
