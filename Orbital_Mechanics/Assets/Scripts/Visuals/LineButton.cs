using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Sim.Maneuvers;
using Sim.Math;

namespace Sim.Visuals
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class LineButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public static List<LineButton> allLineButtons = null;

        public delegate void OnClick(Vector3 worldPosition);
        public event OnClick onLinePressed;
        public delegate void OnHover(LineButton lineButton, Vector3 worldPosition);
        public static event OnHover onLineHovering;

        public bool showPointIndication;
        [DrawIf("showPointIndication", true, ComparisonType.Equals)] 
        public GameObject indicationPrefab;
        
        private float scaleMultiplier = .02f;
        private float minScale = .1f;
        private float maxScale = 6f;

        // private float lineScaleMultiplier = .02f;
        // private float lineMinScale = .1f;
        // private float lineMaxScale = 3f;

        public GameObject indicator { get; private set; }
        public LineRenderer line { get; private set; }

        private PointerEventData pointerData;
        private MeshCollider _collider;

        private bool hovering = false;
        private new bool enabled = true;

        private Func<Vector3, Vector3> converterFunction;

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

        private void Update() {
            if (!enabled) return;

            if (Input.GetKey(KeyCode.Q)) {
                BakeMesh(); 
            }

            if (hovering && showPointIndication) {
                var worldPos = pointerData.pointerCurrentRaycast.worldPosition;
                var convertedPos = converterFunction == null ? worldPos : converterFunction(worldPos);
                indicator.transform.position = convertedPos;
                if (onLineHovering != null)
                    onLineHovering(this, convertedPos);

                if (showPointIndication && indicator.activeSelf) {
                    indicator.transform.localScale = NumericExtensions.ScaleWithDistance(
                        indicator.transform.position, CameraController.Instance.cam.transform.position,
                        scaleMultiplier, minScale, maxScale
                    );
                }
            }

            // line.startWidth = NumericExtensions.ScaleWithDistance(
            //     line.gameObject.transform.position, CameraController.Instance.cam.transform.position,
            //     lineScaleMultiplier, lineMinScale, lineMaxScale
            // ).x;
        }

        public void SetCustomIndicatorPositionConverter(Func<Vector3, Vector3> func) {
            converterFunction = func;
        }

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            if (!enabled) return;
            if (onLinePressed == null) return;

            var worldPos = pointerEventData.pointerCurrentRaycast.worldPosition;
            onLinePressed(converterFunction == null ? worldPos : converterFunction(worldPos));
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
            line.startWidth *= 10f;
            // lineMesh.SetNormals(Enumerable.Repeat(Vector3.up, lineMesh.vertices.Count()).ToArray());
            line.BakeMesh(lineMesh);
            line.startWidth /= 10f;
            _collider.sharedMesh = lineMesh;
        }
    }
}
