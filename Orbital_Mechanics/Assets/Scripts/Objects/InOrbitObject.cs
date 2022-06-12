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
        protected float speed;

        // Orbit
        public Celestial CentralBody { get => centralBody; }
        protected OrbitDrawer orbitDrawer;
        protected Vector3 orbitNormal;

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
        }

        protected void Update()
        {           
            if (!isStationary)
            {
                if (centralBody == null)
                    transform.position += this.velocity * Time.deltaTime;
                else MoveAlongOrbit();   
            }    
            else {
                UpdateRelativePosition();
            }   
        }       

        protected void MoveAlongOrbit()
        {       
            trajectory.meanAnomaly = trajectory.orbit.CalculateMeanAnomaly(Time.deltaTime);
            trajectory.anomaly = trajectory.orbit.CalculateAnomaly(trajectory.meanAnomaly);
            trajectory.trueAnomaly = trajectory.orbit.CalculateTrueAnomaly(trajectory.anomaly);

            relativePosition = trajectory.orbit.CalculateOrbitalPosition(trajectory.trueAnomaly);
            transform.position = centralBody.transform.position + relativePosition;

            this.velocity = trajectory.orbit.CalculateVelocity(relativePosition, out this.speed);
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
                Debug.DrawLine(centralBody.transform.position, orbitNormal + centralBody.transform.position, Color.blue);
                // Debug.DrawLine(centralBody.transform.position, trajectory.orbit.eccVec + centralBody.transform.position, Color.yellow);
            }
        }
    }
}
