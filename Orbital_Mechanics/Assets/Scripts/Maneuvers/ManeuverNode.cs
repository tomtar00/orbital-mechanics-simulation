using System.Collections.Generic;
using UnityEngine;
using Sim.Visuals;
using Sim.Math;

namespace Sim.Maneuvers {
    public class ManeuverNode : MonoBehaviour
    {
        public static bool isDraggingAny { get; private set; }
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

        // [SerializeField] private float orbitWidthOnDrag = 5;
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
        public Collider _collider { get; private set; }

        private void Start() {
            _collider = GetComponent<Collider>();

            LineButton.onLineHovering += (line, worldPos) =>
            {
                if (lineButton == null) lineButton = line;
                if (isDragging && lineButton == line)
                {
                    directions = new Dictionary<string, Vector3> {
                        { "Prograde",    gameObject.transform.forward   },
                        { "Retrograde", -gameObject.transform.forward   },
                        { "Normal",      gameObject.transform.up        },
                        { "Antinormal", -gameObject.transform.up        },
                        { "In",         -gameObject.transform.right     },
                        { "Out",         gameObject.transform.right     },
                    };
                    maneuver.UpdateOnDrag(worldPos);
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

            HUDController.Instance.RemoveManeuverBtn.gameObject.SetActive(true);
            current = this;
            selected = true;

            if (maneuver.NextManeuver != null) return;

            vectorsHolder.SetActive(true);
        }
        public void OnDeselect() {
            if (!selected) return;
            vectorsHolder.SetActive(false);
            selected = false;
            current = null;

            HUDController.Instance.RemoveManeuverBtn.gameObject.SetActive(false);
        }
        public void OnStartDrag() {
            if(isDragging) return;
            isDragging = true;
            isDraggingAny = true;
            LineButton.EnableAllLineButtons(false, lineButton);
            // lineButton.line.startWidth *= orbitWidthOnDrag;
            lineButton.BakeMesh();
            lineButton.indicator.SetActive(false);
            _collider.enabled = false;
            vectorsHolder.SetActive(false);
        }
        public void OnEndDrag() {
            if (!isDragging) return;
            isDragging = false;
            isDraggingAny = false;
            LineButton.EnableAllLineButtons(true, lineButton);
            // lineButton.line.startWidth /= orbitWidthOnDrag;
            lineButton.BakeMesh();
            _collider.enabled = true;
            if (selected) vectorsHolder.SetActive(true);
        }
        
    }
}
