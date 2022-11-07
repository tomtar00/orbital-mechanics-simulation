using UnityEngine;
using Sim.Objects;
using Sim.Math;

namespace Sim.Orbits
{
    public class EllipticOrbit : Orbit
    {
        double cosArgTrue, sinArgTrue;
        double sinlon, coslon, sininc, cosinc;
        double x, y, z;

        public EllipticOrbit(StateVectors stateVectors, Celestial centralBody) : base(stateVectors, centralBody) { }
        public EllipticOrbit(OrbitalElements elements, Celestial centralBody) : base(elements, centralBody) { }

        public override OrbitalElements CalculateOtherElements(OrbitalElements elements)
        {
            elements.trueAnomalyConstant = this.elements.trueAnomalyConstant = MathLib.Sqrt((1 + elements.eccentricity).SafeDivision(1 - elements.eccentricity));
            elements.anomaly = CalculateAnomalyFromTrueAnomaly(elements.trueAnomaly);
            elements.meanAnomaly = CalculateMeanAnomalyFromAnomaly(elements.anomaly);

            elements.semiminorAxis = elements.semimajorAxis * MathLib.Sqrt(1 - elements.eccentricity * elements.eccentricity);
            elements.meanMotion = MathLib.Sqrt((GM).SafeDivision(MathLib.Pow(elements.semimajorAxis, 3)));
            elements.semiLatusRectum = elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity);

            elements.periodConstant = MathLib.Sqrt((MathLib.Pow(elements.semimajorAxis, 3) / GM));
            elements.period = 2 * MathLib.PI * elements.periodConstant;

            return elements;
        }

        // source: https://phas.ubc.ca/~newhouse/p210/orbits/cometreport.pdf
        public override Vector3Double CalculateOrbitalPosition(double trueAnomaly)
        {
            this.distance = (elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity))
                            .SafeDivision(1 + elements.eccentricity * MathLib.Cos(trueAnomaly));

            cosArgTrue = MathLib.Cos(elements.argPeriapsis + trueAnomaly);
            sinArgTrue = MathLib.Sin(elements.argPeriapsis + trueAnomaly);

            sinlon = MathLib.Sin(elements.lonAscNode);
            coslon = MathLib.Cos(elements.lonAscNode);
            sininc = MathLib.Sin(elements.inclination);
            cosinc = MathLib.Cos(elements.inclination);

            x = this.distance * ((coslon * cosArgTrue) - (sinlon * sinArgTrue * cosinc));
            y = this.distance * ((sinlon * cosArgTrue) + (coslon * sinArgTrue * cosinc));
            z = this.distance * (sininc * sinArgTrue);

            // reverse y and z axis to sync with unity
            return new Vector3Double(x, z, y);
        }
        public override Vector3Double CalculateVelocity(Vector3Double relativePosition, double trueAnomaly)
        {
            this.distance = (elements.semimajorAxis * (1 - elements.eccentricity * elements.eccentricity))
                            .SafeDivision(1 + elements.eccentricity * MathLib.Cos(trueAnomaly));
            this.speed = MathLib.Sqrt(GM * ((2.0).SafeDivision(this.distance) - (1.0).SafeDivision(elements.semimajorAxis)));

            // source: https://en.wikipedia.org/wiki/Elliptic_orbit#Flight_path_angle
            double pathAngle = MathLib.Atan((elements.eccentricity * MathLib.Sin(trueAnomaly)) / (1 + elements.eccentricity * MathLib.Cos(trueAnomaly))) * MathLib.Rad2Deg;

            return (Quaternion.AngleAxis((float)pathAngle, elements.angMomentum) *
                            Quaternion.AngleAxis(-90, elements.angMomentum) * relativePosition.normalized *
                            (float)this.speed);
        }

        public override double CalculateMeanAnomaly(double time)
        {
            double meanAnomaly = elements.meanAnomaly;
            meanAnomaly += elements.meanMotion * time;
            meanAnomaly = MathLib.Repeat(meanAnomaly, PI2);
            return meanAnomaly;
        }
        public override double CalculateMeanAnomalyFromAnomaly(double anomaly)
        {
            double meanAnomaly = anomaly - elements.eccentricity * MathLib.Sin(anomaly);
            meanAnomaly = MathLib.Repeat(meanAnomaly, PI2);
            return meanAnomaly;
        }
        public override double CalculateTrueAnomaly(double eccentricAnomaly)
        {
            double trueAnomaly = 2f * MathLib.Atan(elements.trueAnomalyConstant * MathLib.Tan(eccentricAnomaly / 2f));
            trueAnomaly = MathLib.Repeat(trueAnomaly, PI2);
            return trueAnomaly;
        }
        public override double CalculateAnomalyFromTrueAnomaly(double trueAnomaly)
        {
            double anomaly = 2f * MathLib.Atan(MathLib.Tan(trueAnomaly / 2f) / elements.trueAnomalyConstant);
            anomaly = MathLib.Repeat(anomaly, 2 * MathLib.PI);
            return anomaly;
        }

        public override double MeanAnomalyEquation(double E, double e, double M)
        {
            return M - E + e * MathLib.Sin(E); // M - E + e*sin(E) = 0
        }
        public override double d_MeanAnomalyEquation(double E, double e)
        {
            return -1f + e * MathLib.Cos(E); //  -1 + e*cos(E) = 0
        }

        public override Vector3Double GetPointOnOrbit(int i, double orbitFraction, out double meanAnomaly, out double trueAnomaly)
        {
            double eccentricAnomaly = elements.anomaly + i * orbitFraction * PI2;
            eccentricAnomaly = MathLib.Repeat(eccentricAnomaly, PI2);
            meanAnomaly = eccentricAnomaly - elements.eccentricity * MathLib.Sin(eccentricAnomaly);
            trueAnomaly = CalculateTrueAnomaly(eccentricAnomaly);
            return CalculateOrbitalPosition(trueAnomaly);
        }
    }
}


