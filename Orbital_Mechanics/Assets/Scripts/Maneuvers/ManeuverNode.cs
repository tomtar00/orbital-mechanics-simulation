using System.Collections.Generic;
using UnityEngine;
using Sim.Visuals;
using Sim.Math;

namespace Sim.Maneuvers {
    public class ManeuverNode : MonoBehaviour
    {
        public static ManeuverNode current { get; set; }
        public static List<string> directionsTags = new List<string> {
            "Prograde",  
            "Retrograde",
            "Normal",    
            "Antinormal",
            "In",        
            "Out",       
        };

        public Dictionary<string, Vector3> directions { get; private set; }

        [SerializeField] private float orbitWidthOnDrag = 5;
        [SerializeField] private float scaleMultiplier = .1f;
        [SerializeField] private float minScale = .2f;
        [SerializeField] private float maxScale = 10f;
        [Space]
        [SerializeField] private GameObject vectorsHolder;
        [Space]
        [SerializeField] private GameObject prograde;
        [SerializeField] private GameObject retrograde;
        [SerializeField] private GameObject normal;
        [SerializeField] private GameObject antinormal;
        [SerializeField] private GameObject radialIn;
        [SerializeField] private GameObject radialOut;

        public Maneuver maneuver { get; set; }
        public bool isDragging { get; private set; } = false;
        public LineButton lineButton { get; private set; } = null;
        public bool selected { get; private set; } = false;

        private void Start() {
            LineButton.onLineHovering += (line, worldPos) => {
                if (lineButton == null) lineButton = line;
                if (isDragging && lineButton == line) {
                    var relativePosition = worldPos - maneuver.orbit.centralBody.transform.position;
                    maneuver.UpdateOnDrag(relativePosition);
                }
            };

            directions = new Dictionary<string, Vector3> {
                { "Prograde",    gameObject.transform.forward   },
                { "Retrograde", -gameObject.transform.forward   },
                { "Normal",      gameObject.transform.up        },
                { "Antinormal", -gameObject.transform.up        },
                { "In",         -gameObject.transform.right     },
                { "Out",         gameObject.transform.right     },
            };
        }

        private void Update() {
            gameObject.transform.localScale = NumericExtensions.ScaleWithDistance(
                gameObject.transform.position, CameraController.Instance.cam.transform.position,
                scaleMultiplier, minScale, maxScale
            );
        }

        public void AddVelocity(Vector3 direction, float strength) {
            maneuver.ChangeVelocity(direction * strength);
        }

        public void OnSelect() {
            if (selected) return;
            if (maneuver.NextManeuver != null) return;
            vectorsHolder.SetActive(true);
            current = this;
            selected = true;
        }
        public void OnDeselect() {
            if (!selected) return;
            vectorsHolder.SetActive(false);
            selected = false;
        }
        public void OnStartDrag() {
            isDragging = true;
            maneuver.drawer.EnableLineButtons(false);
            lineButton.line.startWidth *= orbitWidthOnDrag;
            lineButton.BakeMesh();
            lineButton.indicator.SetActive(false);
        }
        public void OnEndDrag() {
            isDragging = false;
            maneuver.drawer.EnableLineButtons(true);
            lineButton.line.startWidth /= orbitWidthOnDrag;
            lineButton.BakeMesh();
        }
    }
}
