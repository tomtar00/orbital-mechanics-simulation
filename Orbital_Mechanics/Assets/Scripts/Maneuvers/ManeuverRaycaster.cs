using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Sim.Maneuvers
{
    public class ManeuverRaycaster : MonoBehaviour
    {
        [SerializeField] private float maxClickDuration;

        private float lastSelectTime;
        private bool addingVelocity = false;
        private bool hitNode = false;

        private void Update() 
        {
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
                        var direction = ManeuverNode.current.directions[hit.collider.gameObject.tag];
                        ManeuverNode.current.AddVelocity(direction, SimulationSettings.Instance.G);
                        addingVelocity = true;
                    }
                    else {
                        addingVelocity = false;
                        hitNode = false;
                    }
                }
                else {
                    addingVelocity = false;
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
            }

            if (hitNode && ManeuverNode.current != null && Time.unscaledTime - lastSelectTime > maxClickDuration) {
                ManeuverNode.current.OnStartDrag();
            }
        }
    }
}
