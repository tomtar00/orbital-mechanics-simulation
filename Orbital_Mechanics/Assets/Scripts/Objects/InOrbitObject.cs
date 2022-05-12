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
        [SerializeField] protected KeplerianOrbit.Elements orbit;

        // Moving
        protected Vector3 velocity;
        public Vector3 Velocity { get => velocity; }
        protected float speed;

        // Orbit
        public Celestial CentralBody { get => centralBody; }
        protected OrbitDrawer orbitDrawer;
        protected float timeOnOrbit;
        protected Vector3 orbitNormal;

        // Position
        protected Vector3 perpendicularLastPosition;
        protected Vector3 relativePosition;
        public Vector3 RelativePosition { get => relativePosition; }

        protected void Start()
        {
            orbitDrawer = GetComponent<OrbitDrawer>();
            if (!isStationary) {
                orbitDrawer.SetupOrbitRenderer(centralBody.transform);
            }
        }

        protected void Update()
        {           
            if (!isStationary)
            {
                MoveAlongOrbit();
       
                relativePosition = transform.position - centralBody.RelativePosition;
                orbitNormal = Vector3.Cross(perpendicularLastPosition, relativePosition);//.normalized;

                this.velocity = KeplerianOrbit.CalculateVelocity(orbit, relativePosition, orbitNormal, centralBody.Data.Mass, out this.speed);
            }    
            else {
                relativePosition = transform.position;
            }
        }

        protected void MoveAlongOrbit()
        {
            timeOnOrbit += Time.deltaTime;
            float eccentricAnomaly = KeplerianOrbit.CalculateEccentricAnomaly(orbit, timeOnOrbit, out orbit.meanAnomaly);
            transform.position = centralBody.transform.position + KeplerianOrbit.CalculateOrbitalPosition(orbit, eccentricAnomaly, out orbit.trueAnomaly);
            //orbit.meanAnomaly = KeplerianOrbit.ConvertTrueToMeanAnomaly(orbit.trueAnomaly, orbit.eccentricity);
            perpendicularLastPosition = KeplerianOrbit.CalculateOrbitalPosition(orbit, orbit.trueAnomaly - Mathf.PI / 2);
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
            }
        }
    }
}
