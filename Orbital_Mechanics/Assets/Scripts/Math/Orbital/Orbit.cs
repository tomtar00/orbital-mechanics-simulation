using Sim.Objects;
using Sim.Visuals;
using UnityEngine;
using System.Collections.Generic;

namespace Sim.Math
{
    // Mostly based on: http://control.asu.edu/Classes/MAE462/462Lecture05.pdf
    public abstract class Orbit
    {
        public const float PI2 = 6.28318531f;
        public const float FUTURE_PRECISION = .5f;

        public Celestial centralBody { get; private set; }
        public float GM { get; private set; }

        public OrbitElements elements;

        public Orbit(StateVectors stateVectors, Celestial centralBody)
        {
            ChangeCentralBody(centralBody);
            ConvertStateVectorsToOrbitElements(stateVectors);
        }
        public Orbit(OrbitElements elements, Celestial centralBody)
        {
            ChangeCentralBody(centralBody);
            this.elements = elements;
            this.elements = CalculateOtherElements(this.elements);
        }

        public void ChangeCentralBody(Celestial centralBody)
        {
            this.centralBody = centralBody;
            if (centralBody != null)
                this.GM = KeplerianOrbit.G * centralBody.Data.Mass;
        }

        public virtual void ConvertStateVectorsToOrbitElements(StateVectors stateVectors)
        {
            Vector3 relativePosition = stateVectors.position;
            Vector3 velocity = stateVectors.velocity;
            float posMagnitude = relativePosition.magnitude;
            float velMagnitude = velocity.magnitude;

            var elements = new OrbitElements();

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

            elements = CalculateOtherElements(elements);
            this.elements = elements;
        }
        public abstract OrbitElements CalculateOtherElements(OrbitElements elements);
        public static float CalculateEccentricity(StateVectors stateVectors, float centralMass)
        {
            Vector3 pos = stateVectors.position;
            Vector3 vel = stateVectors.velocity;

            float GM = KeplerianOrbit.G * centralMass;
            Vector3 angMomentum = Vector3.Cross(pos, vel);
            float angMomMag = angMomentum.magnitude;
            Vector3 eccVec = (Vector3.Cross(vel, angMomentum) / GM) - (pos.SafeDivision(pos.magnitude));
            return eccVec.magnitude;
        }

        public abstract Vector3 CalculateOrbitalPosition(float trueAnomaly);
        public abstract Vector3 CalculateVelocity(Vector3 relativePosition, float trueAnomaly);
        public StateVectors ConvertOrbitElementsToStateVectors(float trueAnomaly)
        {
            var pos = CalculateOrbitalPosition(trueAnomaly);
            var vel = CalculateVelocity(pos, trueAnomaly);
            return new StateVectors(pos, vel);
        }

        public abstract float CalculateMeanAnomaly(float time);
        public virtual float CalculateAnomaly(float meanAnomaly)
        {
            // source: https://en.wikipedia.org/wiki/Eccentric_anomaly
            // numerical method: https://en.wikipedia.org/wiki/Newton%27s_method

            float a1 = meanAnomaly;
            float a0 = float.MaxValue;

            while (MathLib.Abs(a1 - a0) > 0.0001f)
            {
                a0 = a1;
                double eq = MeanAnomalyEquation(a0, elements.eccentricity, meanAnomaly);
                double deq = d_MeanAnomalyEquation(a0, elements.eccentricity);
                a1 = a0 - (float)eq.SafeDivision(deq);
            }

            return a1;
        }
        public abstract float CalculateAnomalyFromTrueAnomaly(float trueAnomaly);
        public abstract float CalculateTrueAnomaly(float anomaly);
        public abstract float CalculateMeanAnomalyFromAnomaly(float anomaly);
        public (float, float, float) GetFutureAnomalies(float time)
        {
            float m = CalculateMeanAnomaly(time);
            float a = CalculateAnomaly(m);
            float t = CalculateTrueAnomaly(a);
            return (m, a, t);
        }

        public float CalculateTimeToPeriapsis(float meanAnomaly)
        {
            return (PI2 - meanAnomaly) * elements.periodConstant;
        }

        public abstract double MeanAnomalyEquation(float anomaly, float e, float M);
        public abstract double d_MeanAnomalyEquation(float anomaly, float e);

        public Vector3[] GenerateOrbitPoints(int resolution, InOrbitObject self, float timePassed, out StateVectors stateVectors, out Celestial nextCelestial, out float timeToGravityChange)
        {
            List<Vector3> points = new List<Vector3>();
            float influenceRadius = this.centralBody.InfluenceRadius;
            bool encounter = false;

            float meanAnomaly, trueAnomaly;
            float lastTime = 0;

            nextCelestial = null;
            stateVectors = null;
            timeToGravityChange = -1f;

            float orbitFraction = 1f / resolution;
            for (int i = 0; i < resolution; i++)
            {
                Vector3 position = GetPointOnOrbit(i, orbitFraction, out meanAnomaly, out trueAnomaly);

                // get time in which object is in this spot
                if (meanAnomaly < elements.meanAnomaly) meanAnomaly += PI2;
                float time = (meanAnomaly - elements.meanAnomaly) / elements.meanMotion;

                // check if outside influence
                if (position.sqrMagnitude > influenceRadius * influenceRadius)
                {
                    // move last point closer to influence border         
                    timeToGravityChange = GetTimeToOutsideInfluence(lastTime, time, influenceRadius, 50);

                    (float, float, float) _mat = GetFutureAnomalies(timeToGravityChange);
                    position = CalculateOrbitalPosition(_mat.Item3);
                    points.Add(position);

                    // get escape state vectors
                    Vector3 spacecraftVelocity = CalculateVelocity(position, _mat.Item3);

                    if (!this.centralBody.IsStationary)
                    {
                        Orbit centralBodyOrbit = this.centralBody.Kepler.orbit;
                        (float, float, float) mat = centralBodyOrbit.GetFutureAnomalies(timeToGravityChange + timePassed);
                        Vector3 celestialPosition = centralBodyOrbit.CalculateOrbitalPosition(mat.Item3);
                        Vector3 celestialVelocity = centralBodyOrbit.CalculateVelocity(celestialPosition, mat.Item3);

                        stateVectors = new StateVectors(position + celestialPosition, spacecraftVelocity + celestialVelocity);
                    }
                    else
                        stateVectors = new StateVectors(position, spacecraftVelocity);

                    //Debug.Log($"Will exit {this.centralBody.name} with vectors: R = {stateVectors.position}, V = {stateVectors.velocity} after {timeToGravityChange}");
                    nextCelestial = this.centralBody.CentralBody;

                    break;
                }

                // check if any other object will be in range in that time
                foreach (var celestial in this.centralBody.celestialsOnOrbit)
                {
                    if (celestial == self)
                        continue;

                    (float, float, float) mat = celestial.Kepler.orbit.GetFutureAnomalies(time + timePassed);
                    Vector3 celestialPosition = celestial.Kepler.orbit.CalculateOrbitalPosition(mat.Item3);
                    Vector3 relativePosition = (position - celestialPosition);

                    if (relativePosition.sqrMagnitude < MathLib.Pow(celestial.InfluenceRadius, 2))
                    {
                        float t1 = time;
                        float t2 = lastTime;

                        int iter = 0;
                        float diff = 1;
                        Vector3 spacecraftPosition = Vector3.zero;
                        (float, float, float) _mat = (0, 0, 0);

                        while (MathLib.Abs(diff) > FUTURE_PRECISION || diff > 0)
                        {
                            if (++iter > 50)
                            {
                                break;
                            }

                            timeToGravityChange = (t1 + t2) / 2f;

                            _mat = GetFutureAnomalies(timeToGravityChange);
                            spacecraftPosition = CalculateOrbitalPosition(_mat.Item3);

                            mat = celestial.Kepler.orbit.GetFutureAnomalies(timeToGravityChange + timePassed);
                            celestialPosition = celestial.Kepler.orbit.CalculateOrbitalPosition(mat.Item3);

                            relativePosition = (spacecraftPosition - celestialPosition);
                            diff = relativePosition.sqrMagnitude - MathLib.Pow(celestial.InfluenceRadius, 2);           

                            if (diff < 0)
                            {
                                t1 = timeToGravityChange;
                            }
                            else t2 = timeToGravityChange;
                        }

                        points.Add(relativePosition + celestialPosition);
                        encounter = true;
                        // get encounter state vectors
                        Vector3 spacecraftVelocity = CalculateVelocity(spacecraftPosition, _mat.Item3);
                        Vector3 celestialVelocity = celestial.Kepler.orbit.CalculateVelocity(celestialPosition, mat.Item3);
                        stateVectors = new StateVectors(relativePosition, spacecraftVelocity - celestialVelocity);
                        nextCelestial = celestial;

                        //Debug.Log($"Will enter {nextCelestial.name} with vectors: R = {stateVectors.position}, V = {stateVectors.velocity} after {timeToGravityChange}");

                        break;
                    }
                }

                lastTime = time;
                if (encounter) break;
                points.Add(position);
            }

            return points.ToArray();
        }
        public abstract Vector3 GetPointOnOrbit(int i, float orbitFraction, out float meanAnomaly, out float trueAnomaly);

        protected float GetTimeToOutsideInfluence(float lastTime, float time, float influenceRadius, int steps)
        {
            // Bisection method
            float diff = 1;
            float timeToChange = 0;
            int i = 0;
            Vector3 pos = Vector3.zero;

            while (Mathf.Abs(diff) > FUTURE_PRECISION || diff < 0)
            {
                if (++i > steps)
                {
                    break;
                }

                timeToChange = (lastTime + time) / 2f;
                (float, float, float) mat = GetFutureAnomalies(timeToChange);
                pos = CalculateOrbitalPosition(mat.Item3);
                diff = pos.sqrMagnitude - influenceRadius * influenceRadius;

                if (diff < 0)
                {
                    lastTime = timeToChange;
                }
                else time = timeToChange;
            }

            return timeToChange;
        }
    }
}