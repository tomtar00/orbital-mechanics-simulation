using System.Collections.Generic;
using UnityEngine;
using Sim.Visuals;
using Sim.Math;
using System.Linq;

namespace Sim.Objects
{
    public abstract class InOrbitObject : MonoBehaviour
    {
        public static List<InOrbitObject> allObjects { get; private set; }

        [Header("Orbit")]
        [SerializeField] protected bool isStationary;
        [SerializeField] protected Celestial centralBody;
        [Space]
        [SerializeField] protected KeplerianOrbit kepler;

        public KeplerianOrbit Kepler { get => kepler; }

        // State
        protected StateVectors stateVectors;
        public StateVectors StateVectors { get => stateVectors; }
        public bool IsStationary { get => isStationary; }

        // Orbit
        public Celestial CentralBody { get => centralBody; set => centralBody = value; }
        protected OrbitDrawer orbitDrawer;
        public OrbitDrawer OrbitDrawer { get => orbitDrawer; }

        protected (double, double, double) mat;
        
        public bool camInsideInfluence { get; private set; } = false;
        public static bool camInsideAnyInfluence = false;

        private void Awake() {
            if (allObjects == null) {
                allObjects = new List<InOrbitObject>();
            }
            allObjects.Add(this);
        }

        public virtual void Init(Celestial centralBody, CelestialSO data)
        {
            stateVectors = new StateVectors();
            orbitDrawer = GetComponent<OrbitDrawer>();
            orbitDrawer?.SetupOrbitRenderers();
            if (!isStationary)
            {
                kepler = new KeplerianOrbit();
            }
        }

        protected void UpdateObject()
        {
            if (!isStationary)
            {
                if (centralBody == null)
                    transform.localPosition += stateVectors.velocity * Time.deltaTime;
                else MoveAlongOrbit();
            }
            else
            {
                UpdateRelativePosition();
            }
        }

        protected void MoveAlongOrbit()
        {
            mat = kepler.UpdateAnomalies(Time.deltaTime);
            stateVectors = kepler.UpdateStateVectors(mat.Item3);
            kepler.UpdateTimeToPeriapsis();

            transform.localPosition = centralBody.transform.localPosition + stateVectors.position;
        }

        protected void UpdateRelativePosition()
        {
            if (centralBody != null)
                stateVectors.position = (Vector3Double)(transform.localPosition - centralBody.transform.localPosition);
            else
                stateVectors.position = (Vector3Double)(transform.localPosition);
        }

        public void EnableOtherOrbitRenderer(bool enable)
        {
            camInsideInfluence = !enable;

            if (enable)
            {
                camInsideAnyInfluence = InOrbitObject.allObjects.Where(o => o is Celestial && !(o as Celestial).IsStationary).Any(o => o.camInsideInfluence);
                if (camInsideAnyInfluence && centralBody.IsStationary) return;

                orbitDrawer.TurnOnRenderersFrom(0);
                allObjects
                        .Where(o => o.OrbitDrawer != null && o.CentralBody.isStationary && centralBody.IsStationary)
                        .ForEach(o =>
                        {
                            o.OrbitDrawer.TurnOnRenderersFrom(0);
                        });
            }
            else
            {
                orbitDrawer.TurnOffRenderersFrom(0);
                allObjects
                    .Where(o => o.OrbitDrawer != null && o.CentralBody.isStationary)
                    .ForEach(o =>
                    {
                        o.OrbitDrawer.TurnOffRenderersFrom(0);
                    });

                camInsideAnyInfluence = true;
            }   
        }

        // Vector debugging
        protected void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                /// Draw velocity vector
                Debug.DrawLine(transform.position, stateVectors.velocity * 1000f + transform.position);

                /// Draw orbit plane normal vector
                if (centralBody != null && this is Spacecraft)
                {
                    Debug.DrawLine(centralBody.transform.position, transform.position, Color.red);
                    Debug.DrawLine(centralBody.transform.position, (Vector3)(kepler.orbit.elements.angMomentum) * 10000f + centralBody.transform.position, Color.blue);
                    Debug.DrawLine(centralBody.transform.position, (Vector3)(kepler.orbit.elements.eccVec) * 1000f + centralBody.transform.position, Color.yellow);
                }
            }
        }

        private void OnDestroy() {
            allObjects.Remove(this);
        }
    }
}
