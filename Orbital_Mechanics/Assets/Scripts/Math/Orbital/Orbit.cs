using Sim.Objects;
using UnityEngine;

namespace Sim.Math
{
    public abstract class Orbit
    {
        public const float PI2 = 6.28318531f;

        public Celestial centralBody { get; private set; }
        public float GM { get; private set; }

        public Orbit(Celestial centralBody)
        {
            ChangeCentralBody(centralBody);
        }

        public void ChangeCentralBody(Celestial centralBody)
        {
            this.centralBody = centralBody;
            if (centralBody != null)
                this.GM = KeplerianOrbit.G * centralBody.Data.Mass;
        }  

        public virtual KeplerianOrbit.Elements ConvertStateVectorsToOrbitElements(Vector3 relativePosition, Vector3 velocity)
        {
            float posMagnitude = relativePosition.magnitude;
            float velMagnitude = velocity.magnitude;

            var elements = new KeplerianOrbit.Elements();

            // Semi-major axis
            // source: wyprowadzenie_semimajor.png
            // source2: https://en.wikipedia.org/wiki/Vis-viva_equation
            elements.semimajorAxis = (GM * posMagnitude).SafeDivision((2 * GM - velMagnitude * velMagnitude * posMagnitude));

            // Eccentricity
            // source: https://en.wikipedia.org/wiki/Eccentricity_vector
            elements.angMomentum = Vector3.Cross(relativePosition, velocity);
            float angMomMag = elements.angMomentum.magnitude;
            elements.eccVec = (Vector3.Cross(velocity, elements.angMomentum) / GM) - (relativePosition.SafeDivision(posMagnitude));
            elements.eccentricity = elements.eccVec.magnitude;

            // Inclination
            // source: https://en.wikipedia.org/wiki/Orbital_inclination
            elements.inclination = MathLib.Acos(elements.angMomentum.z.SafeDivision(angMomMag));

            // Longitude of the ascending node
            // source: https://en.wikipedia.org/wiki/Longitude_of_the_ascending_node
            Vector3 nodeVector = Vector3.Cross(Vector3.forward, elements.angMomentum);
            float nodeMag = nodeVector.magnitude;
            elements.lonAscNode = MathLib.Acos(nodeVector.x.SafeDivision(nodeMag));
            if (nodeVector.y < 0)
                elements.lonAscNode = PI2 - elements.lonAscNode;

            // Argument of periapsis
            // source: https://en.wikipedia.org/wiki/Argument_of_periapsis
            elements.argPeriapsis = MathLib.Acos(Vector3.Dot(nodeVector, elements.eccVec).SafeDivision(nodeMag * elements.eccentricity));
            if (elements.eccVec.z < 0)
                elements.argPeriapsis = PI2 - elements.argPeriapsis;

            // True anomaly
            // source: https://en.wikipedia.org/wiki/True_anomaly                
            float eccPosDot = Vector3.Dot(elements.eccVec, relativePosition);
            elements.trueAnomaly = MathLib.Acos(eccPosDot.SafeDivision(elements.eccentricity * posMagnitude));
            if (Vector3.Dot(relativePosition, velocity) < 0)
                elements.trueAnomaly = PI2 - elements.trueAnomaly; 

            CalculateOtherElements(elements);

            return elements;
        }
        public abstract KeplerianOrbit.Elements CalculateOtherElements(KeplerianOrbit.Elements elements);
        public float CalculateEccentricity(Vector3 relativePosition, Vector3 velocity)
        {
            Vector3 angMomentum = Vector3.Cross(relativePosition, velocity);
            float angMomMag = angMomentum.magnitude;
            Vector3 eccVec = (Vector3.Cross(velocity, angMomentum) / GM) - (relativePosition.SafeDivision(relativePosition.magnitude));
            return eccVec.magnitude;
        }
            
        public (Vector3, Vector3) ConvertOrbitElementsToStateVectors(KeplerianOrbit.Elements elements) {
            var pos = CalculateOrbitalPosition(elements);
            var vel = CalculateVelocity(elements, pos);
            return (pos, vel);
        }
        public virtual Vector3 CalculateVelocity(KeplerianOrbit.Elements elements, Vector3 relativePosition)
        {
            // source: https://en.wikipedia.org/wiki/Elliptic_orbit#Flight_path_angle
            float posDst = relativePosition.magnitude;
            float speed = MathLib.Sqrt(GM * ((2f).SafeDivision(posDst) - (1f).SafeDivision(elements.semimajorAxis)));
        
            float e = elements.eccentricity;
            float pathAngle = MathLib.Atan((e * MathLib.Sin(elements.trueAnomaly)) / (1 + e * MathLib.Cos(elements.trueAnomaly)));
            Vector3 radDir = Quaternion.AngleAxis(90, elements.angMomentum) * relativePosition.normalized;
            Vector3 dir = Quaternion.AngleAxis(-pathAngle * MathLib.Rad2Deg, elements.angMomentum) * radDir;

            return dir * speed;
        } 
        public abstract Vector3 CalculateOrbitalPosition(KeplerianOrbit.Elements elements);
        
        public abstract float CalculateMeanAnomaly(float currentMean, float meanMotion, float time);
        public virtual float CalculateAnomaly(float eccentricity, float meanAnomaly, float anomaly)
        {
            // source: https://en.wikipedia.org/wiki/Eccentric_anomaly
            // numerical method: https://en.wikipedia.org/wiki/Newton%27s_method

            float a1 = meanAnomaly;
            float a0 = float.MaxValue;

            while (MathLib.Abs(a1 - a0) > 0.0001f)
            {
                a0 = a1;
                float eq = MeanAnomalyEquation(a0, eccentricity, meanAnomaly); 
                float deq = d_MeanAnomalyEquation(a0, eccentricity);
                a1 = a0 - eq.SafeDivision(deq);
            }

            return a1;
        }
        public abstract float CalculateTrueAnomaly(float constant, float anomaly);
        public (float, float, float) UpdateAnomalies(KeplerianOrbit.Elements elements, float time) {
            float m = CalculateMeanAnomaly(elements.meanAnomaly, elements.meanMotion, time);
            float a = CalculateAnomaly(elements.eccentricity, elements.meanAnomaly, elements.anomaly);
            float t = CalculateTrueAnomaly(elements.trueAnomalyConstant, elements.anomaly);
            return (m, a, t);
        }

        public abstract float MeanAnomalyEquation(float anomaly, float e, float M);
        public abstract float d_MeanAnomalyEquation(float anomaly, float e);
    }
}