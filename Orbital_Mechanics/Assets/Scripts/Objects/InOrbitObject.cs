using UnityEngine;
using Sim.Visuals;
using Sim.Math;

namespace Sim.Objects
{
    public abstract class InOrbitObject : MonoBehaviour
    {
        [Header("Orbit")]
        [SerializeField] protected bool isStationary;
        [SerializeField] protected Celestial celestial;
        [Space]
        [SerializeField] protected KeplerianOrbit.Elements orbit;

        // Moving
        protected Vector3 velocity;
        protected float speed;

        // Orbit
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
                orbitDrawer.SetupOrbitRenderer(celestial.transform);
            }
        }

        protected void Update()
        {           
            if (!isStationary)
            {
                MoveAlongOrbit();
       
                relativePosition = transform.position - celestial.RelativePosition;
                orbitNormal = Vector3.Cross(perpendicularLastPosition, relativePosition).normalized;

                this.velocity = KeplerianOrbit.CalculateVelocity(orbit, relativePosition, orbitNormal, celestial.Data.Mass, out this.speed);
            }    
            else {
                relativePosition = transform.position;
            }
        }

        protected void MoveAlongOrbit()
        {
            timeOnOrbit += Time.deltaTime;
            float eccentricAnomaly = KeplerianOrbit.CalculateEccentricAnomaly(orbit, timeOnOrbit);
            transform.position = celestial.transform.position + KeplerianOrbit.CalculateOrbitalPosition(orbit, eccentricAnomaly, out orbit.trueAnomaly);

            perpendicularLastPosition = KeplerianOrbit.CalculateOrbitalPosition(orbit, orbit.trueAnomaly - Mathf.PI / 2);
        }

        // Vector debugging
        protected void OnDrawGizmos()
        {
            /// Draw velocity vector
            Debug.DrawLine(transform.position, this.velocity + transform.position);

            /// Draw orbit plane normal vector
            if (celestial != null)
            {
                Debug.DrawLine(celestial.transform.position, perpendicularLastPosition + celestial.transform.position, Color.green);
                Debug.DrawLine(celestial.transform.position, transform.position, Color.red);
                Debug.DrawLine(celestial.transform.position, orbitNormal + celestial.transform.position, Color.blue);
            }
        }
    }
}
