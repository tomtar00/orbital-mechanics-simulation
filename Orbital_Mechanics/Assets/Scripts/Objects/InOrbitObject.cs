using System.Collections.Generic;
using UnityEngine;
using Sim.Visuals;
using Sim.Math;

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

        // Moving
        protected Vector3 velocity;
        public Vector3 Velocity { get => velocity; }
        public bool IsStationary { get => isStationary; }

        // Orbit
        public Celestial CentralBody { get => centralBody; set => centralBody = value; }
        protected OrbitDrawer orbitDrawer;
        public OrbitDrawer OrbitDrawer { get => orbitDrawer; }

        // Position
        protected Vector3 relativePosition;
        public Vector3 RelativePosition { get => relativePosition; }

        (float, float, float) mat;
        StateVectors stateVectors;

        private void Awake() {
            if (allObjects == null) {
                allObjects = new List<InOrbitObject>();
            }
            allObjects.Add(this);
        }

        public virtual void Init(Celestial centralBody, CelestialSO data)
        {
            orbitDrawer = GetComponent<OrbitDrawer>();
            orbitDrawer?.SetupOrbitRenderers();
            if (!isStationary)
            {
                kepler = new KeplerianOrbit();
            }
        }

        protected void Update()
        {
            if (!isStationary)
            {
                if (centralBody == null)
                    transform.localPosition += this.velocity * Time.deltaTime;
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

            relativePosition = stateVectors.position;
            velocity = stateVectors.velocity;

            transform.localPosition = centralBody.transform.localPosition + relativePosition;
        }

        protected void UpdateRelativePosition()
        {
            if (centralBody != null)
                relativePosition = transform.localPosition - centralBody.transform.localPosition;
            else
                relativePosition = transform.localPosition;
        }

        // Vector debugging
        protected void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                /// Draw velocity vector
                Debug.DrawLine(transform.position, this.velocity*1000f + transform.position);

                /// Draw orbit plane normal vector
                if (centralBody != null)
                {
                    Debug.DrawLine(centralBody.transform.position, transform.position, Color.red);
                    Debug.DrawLine(centralBody.transform.position, kepler.orbit.elements.angMomentum*100000f + centralBody.transform.position, Color.blue);
                    Debug.DrawLine(centralBody.transform.position, kepler.orbit.elements.eccVec * 100000f + centralBody.transform.position, Color.yellow);
                }
            }
        }

        private void OnDestroy() {
            allObjects.Remove(this);
        }
    }
}
