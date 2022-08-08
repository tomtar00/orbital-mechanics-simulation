using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sim.Visuals
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class LineButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public delegate void OnClick(Vector3 worldPosition);
        public event OnClick onLinePressed;

        public bool showPointIndication;
        [DrawIf("showPointIndication", true, ComparisonType.Equals)] 
        public GameObject indicationPrefab;

        private GameObject indicator;
        private PointerEventData pointerData;
        private bool hovering = false;
        private LineRenderer line;
        private MeshCollider _collider;
        private new bool enabled = true;

        private Func<Vector3, Vector3> converterFunction;

        public bool Enabled {
            get => enabled;
            set => enabled = value;
        }

        private void Start() {
            line = GetComponent<LineRenderer>();
            _collider = line.GetComponent<MeshCollider>();  

            if (showPointIndication) {
                indicator = Instantiate(indicationPrefab, transform);
                indicator.SetActive(false);
            }
        }

        private void Update() {
            if (!enabled) return;

            if (Input.GetMouseButtonUp(0)) {
                BakeMesh(); 
            }

            if (hovering && showPointIndication) {
                var worldPos = pointerData.pointerCurrentRaycast.worldPosition;
                if (converterFunction == null)
                    indicator.transform.position = worldPos;
                else
                    indicator.transform.position = converterFunction(worldPos);
            }
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
        }
        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            if (!enabled) return;
            hovering = true;
            pointerData = pointerEventData;
            if (showPointIndication) {
                indicator.SetActive(true);
            }
        }
        public void OnPointerExit(PointerEventData pointerEventData)
        {
            if (!enabled) return;
            hovering = false;
            if (showPointIndication) {
                indicator.SetActive(false);
            }
        }

        private void BakeMesh() {
            Mesh lineMesh = new Mesh();
            line.BakeMesh(lineMesh, true);
            _collider.sharedMesh = lineMesh;
        }
    }
}
