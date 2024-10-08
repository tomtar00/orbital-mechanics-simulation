﻿using System;
using Sim.Maneuvers;
using UnityEngine;
using Sim.Math;
using Sim.Objects;
using Sim.Orbits;

namespace Sim.Visuals
{
    public class OrbitDrawer : MonoBehaviour
    {
        [SerializeField] private float lineWidth = 0.01f;
        [SerializeField][Range(1, 500)] private int orbitResolution = 200;
        [SerializeField][Range(1, 5)] private int depth = 2;

        [SerializeField] private bool isManeuver;
        
        public LineRenderer[] lineRenderers { get; private set; }
        public Orbit[] orbits { get; private set; }
        public bool hasManeuver { get; set; }

        public LineButton[] lineButtons { get; private set; }
        private Color[] futureColors;
        private InOrbitObject inOrbitObject;
        private Celestial centralBody;

        private void Awake()
        {
            inOrbitObject = GetComponent<InOrbitObject>();
            futureColors = SimulationSettings.Instance.futureOrbitColors;
            lineRenderers = new LineRenderer[depth];
            lineButtons = new LineButton[depth];
            orbits = new Orbit[depth];
            if (isManeuver) SetupOrbitRenderers();
        }

        public void SetupOrbitRenderers()
        {
            for (int i = 0; i < depth; i++) {
                string lineName = isManeuver ? "Maneuver " + i : inOrbitObject.name + " - Orbit Renderer " + i;
                GameObject rendererObject = new GameObject(lineName);
                rendererObject.transform.SetParent(isManeuver ? ManeuverManager.Instance.maneuverOrbitsHolder : inOrbitObject.CentralBody.transform);
                rendererObject.transform.localPosition = Vector3.zero;
                
                LineButton lineButton = rendererObject.AddComponent<LineButton>();
                lineButton.showPointIndication = isManeuver || (inOrbitObject is Spacecraft);
                lineButton.indicationPrefab = SimulationSettings.Instance.indicationPrefab;
                
                lineButton.Enabled = isManeuver || (inOrbitObject is Spacecraft);
                lineButtons[i] = lineButton;

                LineRenderer lineRenderer = rendererObject.GetComponent<LineRenderer>();
                lineRenderer.useWorldSpace = false;
                lineRenderer.loop = true;
                lineRenderer.material = isManeuver ? SimulationSettings.Instance.dashedTrajectoryMat : SimulationSettings.Instance.trajectoryMat;
                lineRenderer.textureMode = isManeuver ? LineTextureMode.Tile : LineTextureMode.Stretch;
                lineRenderer.startWidth = lineWidth;

                lineRenderers[i] = lineRenderer;
                lineRenderer.colorGradient = CreateLineGradient(i);

                rendererObject.SetActive(false);
            }
        }
        private Gradient CreateLineGradient(int lineIdx, bool celestial = false) {
            Gradient gradient = new Gradient();
            LineRenderer line = lineRenderers[lineIdx];
            Color startColor, endColor;
            if (!celestial) {
                startColor = futureColors[lineIdx];
                endColor = (depth > 1 && !line.loop) ? futureColors[lineIdx + 1] : startColor;
            }
            else {
                startColor = SimulationSettings.Instance.celestialOrbitColor;
                endColor = SimulationSettings.Instance.celestialOrbitColor;
            }
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(startColor, 0.8f), 
                    new GradientColorKey(endColor, 1.0f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(startColor.a, 0.8f), 
                    new GradientAlphaKey(endColor.a, 1.0f) 
                }
            );
            return gradient;
        }

        public void TurnOffRenderersFrom(int idx)
        {
            for (int i = idx; i < depth; i++) {
                lineRenderers[i].gameObject.SetActive(false);
            }
        }
        public void TurnOnRenderersFrom(int idx)
        {
            for (int i = idx; i < depth; i++) {
                if (lineRenderers[i] != null)
                    lineRenderers[i].gameObject.SetActive(true);
            }
        }
        public void DestroyRenderers() {
            foreach(var line in lineRenderers) {
                Destroy(line.gameObject);
            }
            foreach(var line in lineButtons) {
                LineButton.allLineButtons.Remove(line);
            }
        }

        public void DrawOrbits(StateVectors stateVectors, Celestial centralBody, double timeToDraw = 0)
        {
            Celestial currentCelestial = centralBody;
            double timePassed = timeToDraw;

            for (int i = 0; i < this.depth; i++)
            {
                LineRenderer line = lineRenderers[i];
                line.gameObject.SetActive(true);
                line.gameObject.transform.SetParent(currentCelestial.transform);
                line.transform.localPosition = Vector3.zero;

                double timeToGravityChange;
                Celestial nextCelestial;
                StateVectors gravityChangePoint = DrawOrbit(stateVectors, i, currentCelestial, timePassed, out nextCelestial, out timeToGravityChange, out orbits[i]);
                line.colorGradient = CreateLineGradient(i);

                if (!isManeuver && i == 0) {
                    Spacecraft.current.timeToNextGravityChange = timeToGravityChange;
                }

                if (gravityChangePoint == null || nextCelestial == null) {
                    TurnOffRenderersFrom(i + 1);
                    return;
                }     
                
                stateVectors = gravityChangePoint;
                currentCelestial = nextCelestial;
                timePassed += timeToGravityChange;
            }
        }
        private StateVectors DrawOrbit(
            StateVectors stateVectors,
            int lineIdx,
            Celestial currentCelestial,
            double timePassed,
            out Celestial nextCelestial,
            out double timeToGravityChange,
            out Orbit orbit)
        {
            Orbit _orbit = KeplerianOrbit.CreateOrbit(stateVectors, currentCelestial, out _);
            orbit = _orbit;

            // get orbit points
            StateVectors gravityChangeVectors;
            Vector3Double[] points = orbit.GenerateOrbitPoints(
                orbitResolution,
                inOrbitObject,
                timePassed,
                out gravityChangeVectors,
                out nextCelestial,
                out timeToGravityChange
            );

            // make indicator show always on the orbit
            var lineButton = lineButtons[lineIdx];
            lineButton.orbit = orbit;
            lineButton.ClearAllClickHandlers();
            lineButton.onLinePressed += (worldPos) => {
                if (!hasManeuver) {
                    ManeuverManager.Instance.CreateManeuver(_orbit, inOrbitObject, worldPos, timePassed, lineIdx);
                    hasManeuver = true;
                }
                else {
                    Debug.LogWarning("This orbit drawer already has a maneuver");
                }
            };

            // loop if no gravity change reported
            var line = lineRenderers[lineIdx];
            line.loop = gravityChangeVectors == null;
            line.positionCount = points.Length;
            line.SetPositions(Array.ConvertAll(points, p => (Vector3)p));

            return gravityChangeVectors;
        }
        public void DrawOrbit(OrbitalElements elements)
        {
            Celestial body = inOrbitObject.CentralBody;
            Orbit orbit = KeplerianOrbit.CreateOrbit(elements, body, out _);

            // get orbit points
            StateVectors gravityChangeVectors;
            Vector3Double[] points = orbit.GenerateOrbitPoints(
                orbitResolution,
                inOrbitObject,
                0f,
                out gravityChangeVectors,
                out _,
                out _
            );

            lineButtons[0].orbit = orbit;

            // loop if no gravity change reported
            LineRenderer line = lineRenderers[0];
            line.colorGradient = CreateLineGradient(0, true);
            line.gameObject.SetActive(true);
            line.loop = gravityChangeVectors == null;
            line.positionCount = points.Length;
            line.SetPositions(Array.ConvertAll(points, p => (Vector3)p));
        }
    }
}