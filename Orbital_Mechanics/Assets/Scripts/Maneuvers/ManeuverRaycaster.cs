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
                        var node = hit.collider.gameObject.GetComponent<ManeuverNode>();
                        ManeuverNode.current = node;
                        ManeuverNode.current.OnStartDrag();

                        lastSelectTime = Time.unscaledTime;
                    }

                    if (ManeuverNode.directionsTags.Contains(hit.collider.gameObject.tag)) {
                        var direction = ManeuverNode.current.directions[hit.collider.gameObject.tag];
                        ManeuverNode.current.AddVelocity(direction, .1f);
                    }
                }
                else {
                    ManeuverNode.current?.OnDeselect();
                    ManeuverNode.current = null;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                ManeuverNode.current?.OnEndDrag();
                if (Time.unscaledTime - lastSelectTime <= maxClickDuration) {
                    ManeuverNode.current?.OnSelect();
                }
            }
        }
    }
}
