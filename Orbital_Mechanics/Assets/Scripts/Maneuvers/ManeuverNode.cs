using UnityEngine;
using Sim.Visuals;

namespace Sim.Maneuvers {
    public class ManeuverNode : MonoBehaviour
    {
        [SerializeField] private float orbitWidthOnDrag = 5;

        public Maneuver maneuver { get; set; }
        public bool isDragging { get; private set; } = false;
        public LineButton lineButton { get; private set; } = null;

        private void Start() {
            LineButton.onLineHovering += (line, worldPos) => {
                if (lineButton == null) lineButton = line;
                if (isDragging && lineButton == line) {
                    var relativePosition = worldPos - maneuver.orbit.centralBody.transform.position;
                    maneuver.UpdateOnDrag(relativePosition);
                }
            };
        }

        public void OnStartDrag() {
            isDragging = true;
            maneuver.drawer.EnableLineButtons(false);
            lineButton.line.startWidth = orbitWidthOnDrag;
            lineButton.BakeMesh();
        }
        public void OnEndDrag() {
            isDragging = false;
            maneuver.drawer.EnableLineButtons(true);
            lineButton.line.startWidth = 1;
            lineButton.BakeMesh();
        }
    }
}
