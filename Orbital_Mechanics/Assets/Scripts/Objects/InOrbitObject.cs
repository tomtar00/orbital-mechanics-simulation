using UnityEngine;
using Sim.Visuals;
using Sim.Math;

namespace Sim.Objects
{
    public abstract class InOrbitObject : MonoBehaviour
    {
        public const float EDELTA = 0.001f;

        [Header("Orbit")]
        [SerializeField] protected bool isStationary;
        [SerializeField] protected Celestial centralBody;
        [Space]
        [SerializeField] protected KeplerianOrbit trajectory;

        // Moving
        protected Vector3 velocity;
        public Vector3 Velocity { get => velocity; }
        protected float speed;

        // Orbit
        public Celestial CentralBody { get => centralBody; }
        protected OrbitDrawer orbitDrawer;
        protected Vector3 orbitNormal;

        // Position
        protected Vector3 perpendicularLastPosition;
        protected Vector3 relativePosition;
        public Vector3 RelativePosition { get => relativePosition; }

        protected void Start()
        {
            orbitDrawer = GetComponent<OrbitDrawer>();
            if (!isStationary) {
                trajectory = new KeplerianOrbit(OrbitType.HYPERBOLIC, centralBody);
                orbitDrawer.SetupOrbitRenderer(centralBody.transform);               
            }                 
        }

        protected void Update()
        {           
            if (!isStationary)
            {
                MoveAlongOrbit();   
            }    
            else {
                relativePosition = transform.position;
            }   
        }       

        protected void MoveAlongOrbit()
        {
            trajectory.anomaly = trajectory.orbit.CalculateAnomaly(Time.deltaTime);
            transform.position = centralBody.transform.position + trajectory.orbit.CalculateOrbitalPosition(trajectory.anomaly);
            relativePosition = transform.position - centralBody.RelativePosition;

            //perpendicularLastPosition = trajectory.orbit.CalculateOrbitalPositionTrue(trajectory.trueAnomaly - MathLib.PI / 2);
            orbitNormal = trajectory.orbit.angMomentum;//Vector3.Cross(perpendicularLastPosition, relativePosition).normalized;
            this.velocity = trajectory.orbit.CalculateVelocity(relativePosition, orbitNormal, out this.speed);
        }

        protected void CheckOrbitType(Vector3 relativePosition, Vector3 velocity) {
            float e = trajectory.orbit.CalculateEccentricity(relativePosition, velocity);
            // if (MathLib.Abs(e) < EDELTA) {
            //     if (!(trajectory.orbit is CircularOrbit)) {
            //         trajectory.ChangeOrbitType(OrbitType.CIRCULAR);
            //         Debug.Log($"{gameObject.name} switched to circular");
            //     }
            // }
            // else if (MathLib.Abs(e - 1) < EDELTA) {
            //     if (!(trajectory.orbit is ParabolicOrbit)) {
            //         trajectory.ChangeOrbitType(OrbitType.PARABOLIC);
            //         Debug.Log($"{gameObject.name} switched to parabolic");
            //     }
            // }
            
            if (e >= 0 && e < 1) {
                if (!(trajectory.orbit is EllipticOrbit)) {
                    trajectory.ChangeOrbitType(OrbitType.ELLIPTIC);
                    Debug.Log($"{gameObject.name} switched to elliptic");
                }
            }
            else if (e >= 1) {
                if (!(trajectory.orbit is HyperbolicOrbit)) {
                    trajectory.ChangeOrbitType(OrbitType.HYPERBOLIC);
                    Debug.Log($"{gameObject.name} switched to hyperbolic");
                }
            }
            else {
                Debug.LogWarning($"Wrong eccentricity value: {e}");
            }
        }

        // Vector debugging
        protected void OnDrawGizmos()
        {
            /// Draw velocity vector
            Debug.DrawLine(transform.position, this.velocity + transform.position);

            /// Draw orbit plane normal vector
            if (centralBody != null)
            {
                Debug.DrawLine(centralBody.transform.position, perpendicularLastPosition + centralBody.transform.position, Color.green);
                Debug.DrawLine(centralBody.transform.position, transform.position, Color.red);
                Debug.DrawLine(centralBody.transform.position, orbitNormal + centralBody.transform.position, Color.blue);
                // Debug.DrawLine(centralBody.transform.position, trajectory.orbit.eccVec + centralBody.transform.position, Color.yellow);
            }
        }
    }
}
