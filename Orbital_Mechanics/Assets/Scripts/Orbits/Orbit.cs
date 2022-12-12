using Sim.Objects;
using System.Collections.Generic;
using System.Linq;
using Sim.Math;
using UnityEngine;

namespace Sim.Orbits
{
    // Mostly based on: http://control.asu.edu/Classes/MAE462/462Lecture05.pdf
    public abstract class Orbit
    {
        public const double PI2 = 6.28318531;
        public const double FUTURE_PRECISION = .5;
        public const int MAX_BISECTION_STEPS = 50;

        public Celestial centralBody { get; private set; }
        public double GM { get; private set; }

        public OrbitalElements elements;

        protected StateVectors stateVectors;
        protected double distance, speed;
        protected double m, a, t;

        public Orbit(StateVectors stateVectors, Celestial centralBody)
        {
            ChangeCentralBody(centralBody);
            ConvertStateVectorsToOrbitElements(stateVectors);
            this.stateVectors = new StateVectors(Vector3Double.zero, Vector3Double.zero);
        }
        public Orbit(OrbitalElements elements, Celestial centralBody)
        {
            ChangeCentralBody(centralBody);
            this.elements = elements;
            this.elements = CalculateOtherElements(this.elements);
            this.stateVectors = new StateVectors(Vector3Double.zero, Vector3Double.zero);
        }

        public void ChangeCentralBody(Celestial centralBody)
        {
            this.centralBody = centralBody;
            if (centralBody != null)
                this.GM = KeplerianOrbit.G * centralBody.Data.Mass;
        }

        public void ConvertStateVectorsToOrbitElements(StateVectors stateVectors)
        {
            Vector3Double relativePosition = stateVectors.position;
            Vector3Double velocity = stateVectors.velocity;
            double posMagnitude = relativePosition.magnitude;
            double velMagnitude = velocity.magnitude;

            var elements = new OrbitalElements();

            // Semi-major axis
            elements.semimajorAxis = (GM * posMagnitude).SafeDivision((2 * GM - velMagnitude * velMagnitude * posMagnitude));

            // Eccentricity
            elements.angMomentum = Vector3Double.Cross(velocity, relativePosition);
            double angMomMag = elements.angMomentum.magnitude;
            elements.eccVec = (Vector3Double.Cross(elements.angMomentum, velocity) / GM) - (relativePosition.SafeDivision(posMagnitude));
            elements.eccentricity = elements.eccVec.magnitude;

            // Inclination
            elements.inclination = MathLib.Acos(elements.angMomentum.y.SafeDivision(angMomMag));

            // Longitude of the ascending node
            Vector3Double nodeVector = elements.inclination != 0 && elements.inclination != MathLib.PI ?
                -Vector3Double.Cross(Vector3Double.up, elements.angMomentum) : Vector3Double.right;
            double nodeMag = nodeVector.magnitude;
            elements.lonAscNode = MathLib.Acos(nodeVector.x.SafeDivision(nodeMag));
            if (nodeVector.z < 0)
                elements.lonAscNode = PI2 - elements.lonAscNode;

            // Argument of periapsis
            if (elements.lonAscNode != 0)
            {
                elements.argPeriapsis = MathLib.Acos(Vector3Double.Dot(nodeVector, elements.eccVec).SafeDivision(nodeMag * elements.eccentricity));
                if (elements.eccVec.y < 0)
                {
                    elements.argPeriapsis = PI2 - elements.argPeriapsis;
                }
            }
            else
            {
                elements.argPeriapsis = MathLib.Atan2(elements.eccVec.z, elements.eccVec.x);
                if (elements.angMomentum.y < 0)
                {
                    elements.argPeriapsis = PI2 - elements.argPeriapsis;
                }
            }

            // True anomaly               
            double eccPosDot = Vector3Double.Dot(elements.eccVec, relativePosition);
            elements.trueAnomaly = MathLib.Acos(eccPosDot.SafeDivision(elements.eccentricity * posMagnitude));
            if (Vector3Double.Dot(relativePosition, velocity) < 0)
                elements.trueAnomaly = PI2 - elements.trueAnomaly;

            elements = CalculateOtherElements(elements);
            this.elements = elements;
        }
        public abstract OrbitalElements CalculateOtherElements(OrbitalElements elements);
        public static double CalculateEccentricity(StateVectors stateVectors, double centralMass)
        {
            Vector3Double pos = stateVectors.position;
            Vector3Double vel = stateVectors.velocity;

            double GM = KeplerianOrbit.G * centralMass;
            Vector3Double angMomentum = Vector3Double.Cross(pos, vel);
            double angMomMag = angMomentum.magnitude;
            Vector3Double eccVec = (Vector3Double.Cross(vel, angMomentum) / GM) - (pos.SafeDivision(pos.magnitude));
            return eccVec.magnitude;
        }

        public abstract Vector3Double CalculateOrbitalPosition(double trueAnomaly);
        public abstract Vector3Double CalculateVelocity(Vector3Double relativePosition, double trueAnomaly);
        public StateVectors ConvertOrbitElementsToStateVectors(double trueAnomaly)
        {
            stateVectors.position = CalculateOrbitalPosition(trueAnomaly);
            stateVectors.velocity = CalculateVelocity(stateVectors.position, trueAnomaly);
            return stateVectors;
        }

        public abstract double CalculateMeanAnomaly(double time);
        public virtual double CalculateAnomaly(double meanAnomaly)
        {
            // source: https://en.wikipedia.org/wiki/Eccentric_anomaly
            // numerical method: https://en.wikipedia.org/wiki/Newton%27s_method

            double a1 = meanAnomaly;
            double a0 = double.MaxValue;
            double eq, deq;

            while (MathLib.Abs(a1 - a0) > 0.0001f)
            {
                a0 = a1;
                eq = KeplerEquation(a0, elements.eccentricity, meanAnomaly);
                deq = d_KeplerEquation(a0, elements.eccentricity);
                a1 = a0 - eq.SafeDivision(deq);
            }

            return a1;
        }
        public abstract double CalculateAnomalyFromTrueAnomaly(double trueAnomaly);
        public abstract double CalculateTrueAnomaly(double anomaly);
        public abstract double CalculateMeanAnomalyFromAnomaly(double anomaly);
        public (double, double, double) GetFutureAnomalies(double time)
        {
            m = CalculateMeanAnomaly(time);
            a = CalculateAnomaly(m);
            t = CalculateTrueAnomaly(a);
            return (m, a, t);
        }

        public double CalculateTimeToPeriapsis(double meanAnomaly)
        {
            return (PI2 - meanAnomaly) * elements.periodConstant;
        }

        public abstract double KeplerEquation(double anomaly, double e, double M);
        public abstract double d_KeplerEquation(double anomaly, double e);

        public Vector3Double[] GenerateOrbitPoints(int resolution, InOrbitObject self, double timePassed, out StateVectors stateVectors, out Celestial nextCelestial, out double timeToGravityChange)
        {
            List<Vector3Double> points = new List<Vector3Double>(resolution);
            double influenceRadius = this.centralBody.InfluenceRadius;
            bool encounter = false;

            double meanAnomaly, trueAnomaly;
            double time = 0, lastTime = 0;

            Vector3Double position, relativePosition;
            Vector3Double spacecraftPosition, celestialPosition;
            Vector3Double spacecraftVelocity, celestialVelocity;

            nextCelestial = null;
            stateVectors = null;
            timeToGravityChange = -1f;

            double orbitFraction = 1f / resolution;
            for (int i = 0; i < resolution; i++)
            {
                position = GetPointOnOrbit(i, orbitFraction, out meanAnomaly, out trueAnomaly);

                // get time in which object is in this spot
                if (meanAnomaly < elements.meanAnomaly) meanAnomaly += PI2;
                time = (meanAnomaly - elements.meanAnomaly) / elements.meanMotion;

                // check if outside influence
                if (position.sqrMagnitude > influenceRadius * influenceRadius)
                {
                    // move last point closer to influence border         
                    timeToGravityChange = GetTimeToOutsideInfluence(lastTime, time, influenceRadius);

                    (double, double, double) _mat = GetFutureAnomalies(timeToGravityChange);
                    position = CalculateOrbitalPosition(_mat.Item3);
                    points.Add(position);

                    // get escape state vectors
                    spacecraftVelocity = CalculateVelocity(position, _mat.Item3);

                    if (!this.centralBody.IsStationary)
                    {
                        Orbit centralBodyOrbit = this.centralBody.Kepler.orbit;
                        (double, double, double) mat = centralBodyOrbit.GetFutureAnomalies(timeToGravityChange + timePassed);
                        celestialPosition = centralBodyOrbit.CalculateOrbitalPosition(mat.Item3);
                        celestialVelocity = centralBodyOrbit.CalculateVelocity(celestialPosition, mat.Item3);

                        if (stateVectors == null)
                        {
                            stateVectors = new StateVectors(position + celestialPosition, spacecraftVelocity + celestialVelocity);
                        }
                        else
                        {
                            stateVectors.position = position + celestialPosition;
                            stateVectors.velocity = spacecraftVelocity + celestialVelocity;
                        }
                    }
                    else
                    {
                        if (stateVectors == null)
                        {
                            stateVectors = new StateVectors(position, spacecraftVelocity);
                        }
                        else
                        {
                            stateVectors.position = position;
                            stateVectors.velocity = spacecraftVelocity;
                        }
                    }

                    //Debug.Log($"Will exit {this.centralBody.name} with vectors: R = {stateVectors.position}, V = {stateVectors.velocity} in {timeToGravityChange}");
                    nextCelestial = this.centralBody.CentralBody;

                    break;
                }

                // check if any other object will be in range in that time
                foreach (var celestial in this.centralBody.celestialsOnOrbit)
                {
                    if (celestial == self)
                        continue;

                    (double, double, double) mat = celestial.Kepler.orbit.GetFutureAnomalies(time + timePassed);
                    celestialPosition = celestial.Kepler.orbit.CalculateOrbitalPosition(mat.Item3);
                    relativePosition = (position - celestialPosition);

                    if (relativePosition.sqrMagnitude < MathLib.Pow(celestial.InfluenceRadius, 2))
                    {
                        double t1 = time;
                        double t2 = lastTime;

                        int iter = 0;
                        double diff = 1;
                        spacecraftPosition = Vector3Double.zero;
                        (double, double, double) _mat = (0, 0, 0);

                        while (MathLib.Abs(diff) > FUTURE_PRECISION || diff > 0)
                        {
                            if (++iter > MAX_BISECTION_STEPS)
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

                        points.Add(spacecraftPosition);
                        encounter = true;
                        // get encounter state vectors
                        spacecraftVelocity = CalculateVelocity(spacecraftPosition, _mat.Item3);
                        celestialVelocity = celestial.Kepler.orbit.CalculateVelocity(celestialPosition, mat.Item3);
                        if (stateVectors == null)
                        {
                            stateVectors = new StateVectors(relativePosition, spacecraftVelocity - celestialVelocity);
                        }
                        else
                        {
                            stateVectors.position = relativePosition;
                            stateVectors.velocity = spacecraftVelocity - celestialVelocity;
                        }
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
        public abstract Vector3Double GetPointOnOrbit(int i, double orbitFraction, out double meanAnomaly, out double trueAnomaly);

        protected double GetTimeToOutsideInfluence(double lastTime, double time, double influenceRadius)
        {
            // Bisection method
            double diff = 1;
            double timeToChange = 0;
            int i = 0;
            Vector3Double pos = Vector3Double.zero;
            double resultTime = 0;

            while (MathLib.Abs(diff) > FUTURE_PRECISION || diff < 0)
            {
                if (++i > MAX_BISECTION_STEPS)
                {
                    break;
                }

                timeToChange = (lastTime + time) / 2f;
                (double, double, double) mat = GetFutureAnomalies(timeToChange);
                pos = CalculateOrbitalPosition(mat.Item3);
                diff = pos.sqrMagnitude - influenceRadius * influenceRadius;

                if (diff < 0)
                {
                    lastTime = timeToChange;
                }
                else time = timeToChange;

                if (diff > 0)
                    resultTime = timeToChange;
            }

            return resultTime;
        }

        public bool Equals(Orbit orbit, double[] precision)
        {
            double[] diffs = new[] {
                MathLib.Abs(elements.semimajorAxis - orbit.elements.semimajorAxis),
                MathLib.Abs(elements.eccentricity - orbit.elements.eccentricity),
                MathLib.Abs(elements.inclination - orbit.elements.inclination)/* ,
                MathLib.Abs(elements.argPeriapsis - orbit.elements.argPeriapsis),
                MathLib.Abs(elements.lonAscNode - orbit.elements.lonAscNode) */
            };

            bool isSameType = (elements.eccentricity > 1 && orbit.elements.eccentricity > 1)
                              || (elements.eccentricity < 1 && orbit.elements.eccentricity < 1);

            // int[] noMatch = diffs.Select((value, i) => (value, i)).Where(item => item.value > precision[item.i]).Select(item => item.i).ToArray();
            // Debug.Log(string.Join(",", noMatch));
            // Debug.Log(string.Join(" -- ", diffs));

            return isSameType && diffs.Select((value, i) => (value, i)).All(item => item.value < precision[item.i]);
        }
    }
}