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
        [SerializeField] private int depth = 2;

        private Queue<LineRenderer> lineRenderers;
        private InOrbitObject inOrbitObject;

        private void Awake() {
            lineRenderers = new Queue<LineRenderer>();
        }

        public LineRenderer SetupOrbitRenderer(InOrbitObject obj, Transform celestial)
        {
            this.inOrbitObject = obj;

            GameObject rendererObject = new GameObject("Orbit Renderer");
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

        public void DrawOrbits(StateVectors stateVectors, float influenceRadius)
        {
            for (int i = 0; i < this.depth; i++)
            {
                LineRenderer line = lineRenderers.ElementAt(i);
                StateVectors gravityChangePoint = DrawOrbit(stateVectors, line);
                if (gravityChangePoint == null)
                    break;

                stateVectors = gravityChangePoint;
            }
        }
        private StateVectors DrawOrbit(StateVectors stateVectors, LineRenderer line)
        {
            Celestial body = inOrbitObject.CentralBody;
            float mass = body.Data.Mass;

            Orbit orbit = KeplerianOrbit.CreateOrbit(stateVectors, body, out _);

            // get orbit points
            StateVectors gravityChangeVectors;
            Vector3[] points = orbit.GenerateOrbitPoints(
                orbitResolution, 
                inOrbitObject, 
                out gravityChangeVectors
            );

            // loop if no gravity change reported
            line.loop = gravityChangeVectors == null;
            line.positionCount = points.Length;
            line.SetPositions(points);

            return gravityChangeVectors;
        }
        public void DrawOrbit(KeplerianOrbit.Elements elements)
        {
            Celestial body = inOrbitObject.CentralBody;
            float mass = body.Data.Mass;

            Orbit orbit = KeplerianOrbit.CreateOrbit(elements, body, out _);

            // get orbit points
            StateVectors gravityChangeVectors;
            Vector3[] points = orbit.GenerateOrbitPoints(
                orbitResolution, 
                inOrbitObject,
                out gravityChangeVectors
            );

            // loop if no gravity change reported
            LineRenderer line = lineRenderers.ElementAt(0);
            line.loop = gravityChangeVectors == null;
            line.positionCount = points.Length;
            line.SetPositions(points);
        }
    }
}