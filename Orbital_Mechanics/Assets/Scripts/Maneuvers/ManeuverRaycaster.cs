using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sim.Maneuvers
{
    public class ManeuverRaycaster : MonoBehaviour
    {
        private ManeuverNode lastManeuverNode;

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
                    if (hit.collider.gameObject.CompareTag("ManeuverNode")) 
                    {
                        lastManeuverNode = hit.collider.gameObject.GetComponent<ManeuverNode>();
                        lastManeuverNode.OnStartDrag();
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                lastManeuverNode?.OnEndDrag();
            }
        }
    }
}
