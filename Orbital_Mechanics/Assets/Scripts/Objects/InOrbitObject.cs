using UnityEngine;
using Sim.Visuals;
using Sim.Math;

namespace Sim.Objects
{
    public abstract class InOrbitObject : MonoBehaviour
    {
        [Header("Orbit")]
        [SerializeField] protected bool isStationary;
        [SerializeField] protected Celestial centralBody;
        [Space]
        [SerializeField] protected KeplerianOrbit trajectory;
        
        public KeplerianOrbit Trajectory { get => trajectory; }

        // Moving
        protected Vector3 velocity;
        public Vector3 Velocity { get => velocity; }

        // Orbit
        public Celestial CentralBody { get => centralBody; }
        protected OrbitDrawer orbitDrawer;

        // Position
        protected Vector3 relativePosition;
        public Vector3 RelativePosition { get => relativePosition; }

        protected void Start()
        {
            orbitDrawer = GetComponent<OrbitDrawer>();
            if (!isStationary) {
                trajectory = new KeplerianOrbit(OrbitType.ELLIPTIC, centralBody);
                orbitDrawer.SetupOrbitRenderer(this, centralBody.transform);               
            }   
            else {
                UpdateRelativePosition();
            }                 
        }

        protected void Update()
        {           
            if (!isStationary)
            {
                if (centralBody == null)
                    transform.position += this.velocity * Time.deltaTime;
                else MoveAlongOrbit();   
            }                
        }       

        protected void MoveAlongOrbit()
        {       
            trajectory.UpdateAnomalies(Time.deltaTime);

            var stateVectors = trajectory.UpdateStateVectors();
            relativePosition = stateVectors.Item1;
            velocity = stateVectors.Item2;
            
            transform.position = centralBody.transform.position + relativePosition;
        }

        protected void UpdateRelativePosition() {
            if (centralBody != null)
                relativePosition = transform.position - centralBody.transform.position;
            else 
                relativePosition = transform.position;
        }

        // Vector debugging
        protected void OnDrawGizmos()
        {
            /// Draw velocity vector
            Debug.DrawLine(transform.position, this.velocity + transform.position);

            /// Draw orbit plane normal vector
            if (centralBody != null)
            {
                Debug.DrawLine(centralBody.transform.position, transform.position, Color.red);
                Debug.DrawLine(centralBody.transform.position, trajectory.elements.angMomentum + centralBody.transform.position, Color.blue);
                // Debug.DrawLine(centralBody.transform.position, trajectory.orbit.eccVec + centralBody.transform.position, Color.yellow);
            }
        }
    }
}
