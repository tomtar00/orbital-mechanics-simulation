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

        [HideInInspector] public double meanAnomaly;
        [HideInInspector] public double anomaly;

        [HideInInspector] public double semiminorAxis;
        [HideInInspector] public double meanMotion;
        [HideInInspector] public double semiLatusRectum;
        [HideInInspector] public double timeToPeriapsis;
        [HideInInspector] public double period;

        [NonSerialized] public double trueAnomalyConstant;
        [NonSerialized] public double periodConstant;
 
        [NonSerialized] public Vector3Double angMomentum;
        [NonSerialized] public Vector3Double eccVec;
    }
}