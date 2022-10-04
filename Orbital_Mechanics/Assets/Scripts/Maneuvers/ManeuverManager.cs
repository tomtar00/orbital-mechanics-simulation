using System.Collections.Generic;
using UnityEngine;
using Sim.Visuals;
using Sim.Math;
using Sim.Objects;
using System.Linq;

namespace Sim.Maneuvers {
    public class ManeuverManager : MonoBehaviour
    {
        [SerializeField] private GameObject maneuverPrefab;
        [SerializeField] private Transform maneuverHolder;
        public Transform maneuverOrbitsHolder;

        public List<Maneuver> maneuvers { get; private set; }
        public Maneuver NextManeuver {
            get {
                if (maneuvers.Count > 0 && maneuvers.Where(m => m.timeToManeuver > 0).Count() > 0)
                    return maneuvers.Where(m => m.timeToManeuver > 0).OrderBy(m => m.timeToManeuver).First();
                else 
                    return null;
            }
        }
        public Maneuver CurrentManeuver {
            get {
                if (maneuvers.Count > 0)
                {
                    var maneuver = maneuvers.OrderBy(m => m.timeToManeuver).First();
                    float time = maneuver.timeToManeuver;
                    float burn = maneuver.burnTime;
                    if (time < (burn / 2f)) {
                        if (time > -(burn / 2f))
                            return maneuver;
                        else {
                            if (!ManeuverNode.isDraggingAny) {
                                RemoveFirst();
                                HUDController.Instance.SetTimeScaleToPrevious();
                            }
                            return null;
                        }
                    }
                    else return null;
                }
                else 
                    return null;
            }
        }

        public static ManeuverManager Instance;
        private void Awake() {
            Instance = this;
            maneuvers = new List<Maneuver>();
        }

        private void Update() {
            foreach (var maneuver in maneuvers)
            {
                maneuver.Update();
            }
        }

        public Maneuver CreateManeuver(Orbit currentOrbit, InOrbitObject inOrbitObject, Vector3 relativePressPosition, float timeToOrbit, int futureOrbitIdx) {

            Maneuver lastManeuver = maneuvers.Count > 0 ? maneuvers[maneuvers.Count - 1] : null;
            Orbit orbit = inOrbitObject != null && futureOrbitIdx == 0 ? inOrbitObject.Kepler.orbit : currentOrbit;
            // if (inOrbitObject != null) {
            //     orbit.elements.meanAnomaly = inOrbitObject.Kepler.orbit.elements.meanAnomaly;
            // }

            // create prefab
            GameObject maneuverObj = Instantiate(maneuverPrefab, maneuverHolder);
            maneuverObj.name = "Maneuver Node " + maneuvers.Count;
            OrbitDrawer drawer = maneuverObj.GetComponent<OrbitDrawer>();
            ManeuverNode node = maneuverObj.GetComponent<ManeuverNode>();

            // calculate state vectors of pressed point
            float trueAnomaly = Vector3.SignedAngle(orbit.elements.eccVec, relativePressPosition, orbit.elements.angMomentum) * Mathf.Deg2Rad;
            var relativePressVelocity = orbit.CalculateVelocity(relativePressPosition, trueAnomaly);
            StateVectors pressStateVectors = new StateVectors(relativePressPosition, relativePressVelocity);

            // create new maneuver
            Maneuver maneuver = new Maneuver(orbit, drawer, pressStateVectors, node, lastManeuver, timeToOrbit, futureOrbitIdx);
            maneuvers.Add(maneuver);
            node.maneuver = maneuver;
            if (lastManeuver != null) {
                lastManeuver.NextManeuver = maneuver;
            }
            else maneuver.PreviousDrawer = inOrbitObject.GetComponent<OrbitDrawer>();
            return maneuver;
        }

        public void RemoveManeuvers(Maneuver maneuver) {
            int index = maneuvers.IndexOf(maneuver);
            if (index < 0) {
                Debug.LogWarning("Maneuver not found");
                return;
            }
            
            for (int i = maneuvers.Count - 1; i >= index; i--) {
                maneuvers[i].Remove();
            }
        }
        public void RemoveFirst() {
            if (maneuvers.Count > 0)
                maneuvers[0].Remove();
            else
                Debug.LogWarning("Tried to remove maneuver from empty list");
        }

        private void OnGUI() 
        {
            float startHeight = Screen.height - 300;
            float space = 20;
            int i = 0;

            foreach (var maneuver in maneuvers)
            {
                GUI.Label(new Rect(10, startHeight + space * i, 300, 20), $"Maneuver {i++}: {maneuver.timeToManeuver} Burn: {maneuver.burnTime}");
            }
            i++;
            Maneuver next = NextManeuver;
            if (NextManeuver != null)
                GUI.Label(new Rect(10, startHeight + space * i++, 300, 20), $"Next maneuver: Maneuver {maneuvers.IndexOf(next)}");
        }
    }
}
