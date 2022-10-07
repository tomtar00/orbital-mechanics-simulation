using System.Collections.Generic;
using UnityEngine;
using Sim.Objects;

namespace Sim.Math
{
    public class EllipticOrbit : Orbit
    {
        float cosArgTrue, sinArgTrue;
        float sinlon, coslon, sininc, cosinc;
        float x, y, z;

        public EllipticOrbit(StateVectors stateVectors, Celestial centralBody) : base(stateVectors, centralBody) { }
        public EllipticOrbit(OrbitElements elements, Celestial centralBody) : base(elements, centralBody) { }

        public override OrbitElements CalculateOtherElements(OrbitElements elements)
        {
            float sqrt = MathLib.Sqrt((1 - elements.eccentricity).SafeDivision(1 + elements.eccentricity));
            elements.anomaly = 2 * MathLib.Atan(sqrt * MathLib.Tan(elements.trueAnomaly / 2));
            elements.meanAnomaly = elements.anomaly - elements.eccentricity * MathLib.Sin(elements.anomaly);
            
            elements.semiminorAxis = elements.semimajorAxis * MathLib.Sqrt(1 - elements.eccentricity * elements.eccentricity);
            elements.meanMotion = MathLib.Sqrt((GM).SafeDivision(MathLib.Pow(elements.semimajorAxis, 3)));
            elements.semiLatusRectum = elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity);

            elements.trueAnomalyConstant = MathLib.Sqrt((1 + elements.eccentricity).SafeDivision(1 - elements.eccentricity));
            elements.periodConstant = MathLib.Sqrt((MathLib.Pow(elements.semimajorAxis, 3) / GM));
            elements.period = 2 * Mathf.PI * elements.periodConstant;

            return elements;
        }

        // source: https://phas.ubc.ca/~newhouse/p210/orbits/cometreport.pdf
        public override Vector3 CalculateOrbitalPosition(float trueAnomaly)
        {
            this.distance = (elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity))
                            .SafeDivision(1 + elements.eccentricity * MathLib.Cos(trueAnomaly));

            cosArgTrue = MathLib.Cos(elements.argPeriapsis + trueAnomaly);
            sinArgTrue = MathLib.Sin(elements.argPeriapsis + trueAnomaly);

            sinlon = Mathf.Sin(elements.lonAscNode);
            coslon = Mathf.Cos(elements.lonAscNode);
            sininc = Mathf.Sin(elements.inclination);
            cosinc = Mathf.Cos(elements.inclination);

            x = this.distance * ((coslon * cosArgTrue) - (sinlon * sinArgTrue * cosinc));
            y = this.distance * ((sinlon * cosArgTrue) + (coslon * sinArgTrue * cosinc));
            z = this.distance * (sininc * sinArgTrue);

            return new Vector3(x, y, z);
        }
        public override Vector3 CalculateVelocity(Vector3 relativePosition, float trueAnomaly)
        {
            this.distance = (elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity))
                            .SafeDivision(1 + elements.eccentricity * MathLib.Cos(trueAnomaly));
            this.speed = MathLib.Sqrt(GM * ((2f).SafeDivision(this.distance) - (1f).SafeDivision(elements.semimajorAxis)));

            // source: https://en.wikipedia.org/wiki/Elliptic_orbit#Flight_path_angle
            float pathAngle = MathLib.Atan((elements.eccentricity * MathLib.Sin(trueAnomaly)) / (1 + elements.eccentricity * MathLib.Cos(trueAnomaly)));

            return Quaternion.AngleAxis(-pathAngle * MathLib.Rad2Deg, elements.angMomentum) *
                            Quaternion.AngleAxis(90, elements.angMomentum) * relativePosition.normalized *
                            this.speed;
        }

        public override float CalculateMeanAnomaly(float time)
        {
            float meanAnomaly = elements.meanAnomaly;
            meanAnomaly += elements.meanMotion * time;
            meanAnomaly = NumericExtensions.FitBetween0And2PI(meanAnomaly);
            return meanAnomaly;
        }
        public override float CalculateMeanAnomalyFromAnomaly(float anomaly) {
            float meanAnomaly = anomaly - elements.eccentricity * MathLib.Sin(anomaly);
            meanAnomaly = NumericExtensions.FitBetween0And2PI(meanAnomaly);
            return meanAnomaly;
        }
        public override float CalculateTrueAnomaly(float eccentricAnomaly)
        {
            float trueAnomaly = 2f * MathLib.Atan(elements.trueAnomalyConstant * MathLib.Tan(eccentricAnomaly / 2f));
            trueAnomaly = NumericExtensions.FitBetween0And2PI(trueAnomaly);
            return trueAnomaly;
        }
        public override float CalculateAnomalyFromTrueAnomaly(float trueAnomaly) {
            float anomaly = 2f * MathLib.Atan(MathLib.Tan(trueAnomaly / 2f) / elements.trueAnomalyConstant);
            anomaly = NumericExtensions.FitBetween0And2PI(anomaly);
            return anomaly;
        }

        public override double MeanAnomalyEquation(float E, float e, float M)
        {
            return M - E + e * MathLib.Sin(E); // M - E + e*sin(E) = 0
        }
        public override double d_MeanAnomalyEquation(float E, float e)
        {
            return -1f + e * MathLib.Cos(E); //  -1 + e*cos(E) = 0
        }

        public override Vector3 GetPointOnOrbit(int i, float orbitFraction, out float meanAnomaly, out float trueAnomaly)
        {
            float eccentricAnomaly = elements.anomaly + i * orbitFraction * PI2;
            meanAnomaly = eccentricAnomaly - elements.eccentricity * MathLib.Sin(eccentricAnomaly);
            trueAnomaly = CalculateTrueAnomaly(eccentricAnomaly);
            return CalculateOrbitalPosition(trueAnomaly);
        } 
    }
}


