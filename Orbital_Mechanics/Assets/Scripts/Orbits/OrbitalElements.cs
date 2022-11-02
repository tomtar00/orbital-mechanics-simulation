using Sim.Math;
using UnityEngine;
using System;

namespace Sim.Orbits {
    [System.Serializable]
    public struct OrbitalElements
    {
        public double semimajorAxis;
        public double eccentricity;
        public double inclination;
        public double lonAscNode;
        public double argPeriapsis;
        public double trueAnomaly;

        [Space]

        public double meanAnomaly;
        public double anomaly;

        public double semiminorAxis;
        public double meanMotion;
        public double semiLatusRectum;
        public double timeToPeriapsis;
        public double period;

        [NonSerialized] public double trueAnomalyConstant;
        [NonSerialized] public double periodConstant;
 
        [NonSerialized] public Vector3Double angMomentum;
        [NonSerialized] public Vector3Double eccVec;
    }
}