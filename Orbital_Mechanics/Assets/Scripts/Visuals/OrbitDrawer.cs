﻿using System.Net.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sim.Math;
using Sim.Objects;

namespace Sim.Visuals
{
    public class OrbitDrawer : MonoBehaviour
    {
        [SerializeField][Range(1, 200)] private int orbitResolution = 200;
        [SerializeField][Range(1, 5)] private int depth = 2;
        [SerializeField] private Color[] futureColors;

        public LineRenderer[] lineRenderers { get; private set; }
        private InOrbitObject inOrbitObject;
        private Celestial centralBody;

        private IEnumerator<Color> startColors;
        private IEnumerator<Color> endColors;

        private void Awake()
        {
            inOrbitObject = GetComponent<InOrbitObject>();

            lineRenderers = new LineRenderer[depth];
            SetupOrbitRenderers();

            startColors = futureColors.RepeatIndefinitely();
            endColors = futureColors.RepeatIndefinitely();
            endColors.MoveNext();

            onEnterExitInfluence();
        }

        public void onEnterExitInfluence() {
            startColors.MoveNext();
            endColors.MoveNext();
        }

        private void SetupOrbitRenderers()
        {
            for (int i = 0; i < depth; i++) {
                GameObject rendererObject = new GameObject(inOrbitObject.name + " - Orbit Renderer " + i);
                rendererObject.transform.SetParent(inOrbitObject.CentralBody.transform);
                rendererObject.transform.localPosition = Vector3.zero;

                LineRenderer lineRenderer = rendererObject.AddComponent<LineRenderer>();
                lineRenderer.useWorldSpace = false;
                lineRenderer.startWidth = .1f;
                lineRenderer.loop = true;
                lineRenderer.material = SimulationSettings.Instance.trajectoryMat;

                lineRenderers[i] = lineRenderer;

                rendererObject.SetActive(false);
            }
        }

        public void TurnOffRenderersFrom(int idx)
        {
            for (int i = idx; i < depth; i++) {
                lineRenderers[i].gameObject.SetActive(false);
            }
        }

        public void DrawOrbits(StateVectors stateVectors)
        {
            Celestial currentCelestial = inOrbitObject.CentralBody;
            Celestial nextCelestial;
            float timePassed = 0;

            for (int i = 0; i < this.depth; i++)
            {
                LineRenderer line = lineRenderers[i];
                line.gameObject.SetActive(true);
                line.gameObject.transform.SetParent(currentCelestial.transform);
                line.transform.localPosition = Vector3.zero;

                float timeToGravityChange;
                StateVectors gravityChangePoint = DrawOrbit(stateVectors, line, currentCelestial, timePassed, out nextCelestial, out timeToGravityChange);
                if (gravityChangePoint == null || nextCelestial == null) {
                    TurnOffRenderersFrom(i + 1);
                    line.material.color = futureColors[0];
                    return;
                }

                Color startColor = startColors.Current;
                Color endColor = endColors.Current;
                Gradient gradient = new Gradient();
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
                line.colorGradient = gradient;

                stateVectors = gravityChangePoint;
                currentCelestial = nextCelestial;
                timePassed += timeToGravityChange;
            }
        }
        private StateVectors DrawOrbit(StateVectors stateVectors, LineRenderer line, Celestial currentCelestial, float timePassed, out Celestial nextCelestial, out float timeToGravityChange)
        {
            Orbit orbit = KeplerianOrbit.CreateOrbit(stateVectors, currentCelestial, out _);

            // get orbit points
            StateVectors gravityChangeVectors;
            Vector3[] points = orbit.GenerateOrbitPoints(
                orbitResolution,
                inOrbitObject,
                timePassed,
                out gravityChangeVectors,
                out nextCelestial,
                out timeToGravityChange
            );

            // loop if no gravity change reported
            line.loop = gravityChangeVectors == null;
            line.positionCount = points.Length;
            line.SetPositions(points);

            return gravityChangeVectors;
        }
        public void DrawOrbit(OrbitElements elements)
        {
            Celestial body = inOrbitObject.CentralBody;
            Orbit orbit = KeplerianOrbit.CreateOrbit(elements, body, out _);

            // get orbit points
            StateVectors gravityChangeVectors;
            Vector3[] points = orbit.GenerateOrbitPoints(
                orbitResolution,
                inOrbitObject,
                0f,
                out gravityChangeVectors,
                out _,
                out _
            );

            // loop if no gravity change reported
            LineRenderer line = lineRenderers[0];
            line.gameObject.SetActive(true);
            line.loop = gravityChangeVectors == null;
            line.positionCount = points.Length;
            line.SetPositions(points);
        }
    }
}