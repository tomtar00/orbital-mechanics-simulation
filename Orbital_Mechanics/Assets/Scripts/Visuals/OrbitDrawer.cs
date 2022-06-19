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
        [SerializeField] private int orbitResolution = 30;
        [SerializeField]
        [Range(1, 3)]
        private int depth = 2;

        private Queue<LineRenderer> lineRenderers;
        private InOrbitObject inOrbitObject;

        private void Awake()
        {
            lineRenderers = new Queue<LineRenderer>();
        }

        public LineRenderer SetupOrbitRenderer(InOrbitObject obj, Transform celestial)
        {
            this.inOrbitObject = obj;

            GameObject rendererObject = new GameObject("Orbit Renderer " + (lineRenderers.Count + 1));
            rendererObject.transform.SetParent(celestial);
            rendererObject.transform.localPosition = Vector3.zero;

            LineRenderer lineRenderer = rendererObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.startWidth = .1f;
            lineRenderer.loop = true;

            lineRenderers.Enqueue(lineRenderer);
            return lineRenderer;
        }

        public void DestroyOrbitRenderer()
        {
            if (lineRenderers.Count > 0)
                Destroy(lineRenderers.Dequeue().gameObject);
        }

        public void DrawOrbits(StateVectors stateVectors)
        {
            Celestial currentCelestial = inOrbitObject.CentralBody;
            Celestial nextCelestial;
            for (int i = 0; i < this.depth; i++)
            {
                LineRenderer line = lineRenderers.ElementAt(i);
                StateVectors gravityChangePoint = DrawOrbit(stateVectors, line, currentCelestial, out nextCelestial);
                if (gravityChangePoint == null || nextCelestial == null)
                    return;

                if (i < this.depth - 1)
                {
                    stateVectors = gravityChangePoint;
                    SetupOrbitRenderer(inOrbitObject, nextCelestial.transform);
                    currentCelestial = nextCelestial;
                }
            }

            Time.timeScale = 0;
        }
        private StateVectors DrawOrbit(StateVectors stateVectors, LineRenderer line, Celestial currentCelestial, out Celestial nextCelestial)
        {
            Orbit orbit = KeplerianOrbit.CreateOrbit(stateVectors, currentCelestial, out _);

            // get orbit points
            StateVectors gravityChangeVectors;
            Vector3[] points = orbit.GenerateOrbitPoints(
                orbitResolution,
                out gravityChangeVectors,
                out nextCelestial
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
                out gravityChangeVectors,
                out _
            );

            // loop if no gravity change reported
            LineRenderer line = lineRenderers.ElementAt(0);
            line.loop = gravityChangeVectors == null;
            line.positionCount = points.Length;
            line.SetPositions(points);
        }
    }
}