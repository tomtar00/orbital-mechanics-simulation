using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Sim.Orbits;
using Sim.Maneuvers;
using Sim.Math;

namespace Sim.Visuals
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class LineButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public static List<LineButton> allLineButtons = null;

        public delegate void OnClick(Vector3Double worldPosition);
        public event OnClick onLinePressed;
        public delegate void OnHover(LineButton lineButton, Vector3 worldPosition);
        public static event OnHover onLineHovering;

        public bool showPointIndication;
        [DrawIf("showPointIndication", true, ComparisonType.Equals)] 
        public GameObject indicationPrefab;
        
        private float scaleMultiplier = .02f;
        private float minScale = .1f;
        private float maxScale = 60f;

        public GameObject indicator { get; private set; }
        public LineRenderer line { get; private set; }

        private PointerEventData pointerData;
        private MeshCollider _collider;

        private bool hovering = false;
        private new bool enabled = true;
        
        public Orbit orbit { get; set; }

        Vector3Double closestWorldPoint;

        public bool Enabled {
            get => enabled;
            set {
                enabled = value;
                if (_collider == null)
                    _collider = gameObject.GetComponent<MeshCollider>();
                
                _collider.enabled = value;
            }
        }

        private void Start() {
            if (allLineButtons == null) allLineButtons = new List<LineButton>();
            allLineButtons.Add(this);
            line = GetComponent<LineRenderer>();
            if (_collider == null) _collider = line.GetComponent<MeshCollider>();  

            if (showPointIndication) {
                indicator = Instantiate(indicationPrefab, transform);
                indicator.SetActive(false);
            }
        }

        public static void EnableAllLineButtons(bool enabled, LineButton exception) {
            foreach(var line in allLineButtons) {
                if (line != exception) {
                    line.Enabled = enabled;
                }
            }
        }
        public void ClearAllClickHandlers() {
            onLinePressed = null;
        }

        private void LateUpdate() {

            if (orbit != null)
            {
                closestWorldPoint = ConvertWorldPosToOrbitPos(CameraController.Instance.cam.transform.position, true);

                double distance = Vector3Double.Distance(closestWorldPoint, (Vector3Double)CameraController.Instance.cam.transform.position);
                distance = MathLib.Clamp(distance, 0, orbit.centralBody.InfluenceRadius);
                distance = MathLib.Clamp(distance, SimulationSettings.Instance.lineMinScale, SimulationSettings.Instance.lineMaxScale);
                line.startWidth = (float)(distance * SimulationSettings.Instance.lineScaleMultiplier);
            }

            if (!enabled) return;

            if (Input.GetKey(KeyCode.Q)) {
                BakeMesh(); 
            }

            if (hovering && showPointIndication && !ManeuverNode.isSelectedAny) {
                var worldPos = pointerData.pointerCurrentRaycast.worldPosition;
                var convertedPos = ConvertWorldPosToOrbitPos(worldPos);
                indicator.transform.localPosition = (Vector3)convertedPos;
                if (onLineHovering != null)
                    onLineHovering(this, (Vector3)convertedPos);

                if (showPointIndication && indicator.activeSelf) {
                    indicator.transform.localScale = NumericExtensions.ScaleWithDistance(
                        indicator.transform.position, CameraController.Instance.cam.transform.position,
                        scaleMultiplier, minScale, maxScale
                    );
                }
            }
            else {
                if (showPointIndication && indicator.activeSelf)
                {
                    indicator.SetActive(false);
                }
            }
        }

        public Vector3Double ConvertWorldPosToOrbitPos(Vector3 worldPos, bool returnWorldOrbitPos = false) {
            if (orbit != null) {
                var pressLocalPosition = worldPos - orbit.centralBody.transform.position;
                double angleToPoint = -Vector3Double.SignedAngle(orbit.elements.eccVec, (Vector3Double)pressLocalPosition, orbit.elements.angMomentum);
                if (!returnWorldOrbitPos)
                    return orbit.CalculateOrbitalPosition(angleToPoint * MathLib.Deg2Rad);
                else {
                    return orbit.CalculateOrbitalPosition(angleToPoint * MathLib.Deg2Rad) + (Vector3Double)orbit.centralBody.transform.position;
                }
            }
            else return (Vector3Double)worldPos;
        }

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            if (!enabled) return;
            if (onLinePressed == null) return;

            var worldPos = pointerEventData.pointerCurrentRaycast.worldPosition;
            onLinePressed(ConvertWorldPosToOrbitPos(worldPos));
            if (showPointIndication) {
                indicator.SetActive(false);
            }
        }
        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            if (!enabled) return;
            hovering = true;
            pointerData = pointerEventData;

            if (ManeuverNode.current == null) {
                if (showPointIndication) indicator.SetActive(true);
            }
            else if (!ManeuverNode.current.isDragging) {
                if (showPointIndication) indicator.SetActive(true);
            }
            else indicator.SetActive(false);
        }
        public void OnPointerExit(PointerEventData pointerEventData)
        {
            if (!enabled) return;
            hovering = false;
            if (showPointIndication) {
                indicator.SetActive(false);
            }
        }

        public void BakeMesh() {
            Mesh lineMesh = new Mesh();
            line.startWidth *= SimulationSettings.Instance.linePressTolerance;
            line.BakeMesh(lineMesh, CameraController.Instance.dummyCamera, true);
            line.startWidth /= SimulationSettings.Instance.linePressTolerance;
            _collider.sharedMesh = lineMesh;
        }
    }
}
