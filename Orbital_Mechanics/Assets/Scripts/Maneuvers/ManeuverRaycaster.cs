using UnityEngine;

namespace Sim.Maneuvers
{
    using Time = UnityEngine.Time;
    public class ManeuverRaycaster : MonoBehaviour
    {
        [SerializeField] private float maxClickDuration;

        private float lastSelectTime;
        private bool addingVelocity = false;
        private bool hitNode = false;
        private Vector3 direction;

        private void Update() 
        {
            if (CameraController.Instance == null) return;

            if (Input.GetMouseButtonDown(0))
            {
                Physics.autoSimulation = false;
                Physics.Simulate(Time.fixedDeltaTime);  
                Physics.autoSimulation = true;

                Ray ray = CameraController.Instance.cam.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    // check node hit
                    if (hit.collider.gameObject.CompareTag("ManeuverNode")) 
                    {
                        var node = hit.collider.gameObject.GetComponent<ManeuverNode>();
                        ManeuverNode.current = node;
                        hitNode = true;
                        lastSelectTime = Time.unscaledTime;
                    }   // check direction vector hit
                    else if (ManeuverNode.directionsTags.Contains(hit.collider.gameObject.tag)) 
                    {
                        direction = ManeuverNode.current.directions[hit.collider.gameObject.tag];
                        addingVelocity = true;
                    }
                    else {
                        addingVelocity = false;
                        hitNode = false;
                    }
                }
                else {
                    hitNode = false;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (ManeuverNode.current != null) {

                    ManeuverNode.current.OnEndDrag();

                    if (ManeuverNode.current.selected && !addingVelocity) {
                        ManeuverNode.current.OnDeselect();
                    }
                    else if (Time.unscaledTime - lastSelectTime <= maxClickDuration) {
                        ManeuverNode.current.OnSelect();
                    }            
                }
                hitNode = false;
                addingVelocity = false;
            }
            else if (Input.GetMouseButton(0)) {
                if (addingVelocity) {
                    ManeuverNode.current.AddVelocity(direction, SimulationSettings.Instance.G * SimulationSettings.Instance.addVelocitySensitivity);
                }
            }

            if (hitNode && ManeuverNode.current != null && Time.unscaledTime - lastSelectTime > maxClickDuration) {
                ManeuverNode.current.OnStartDrag();
            }
        }
    }
}
