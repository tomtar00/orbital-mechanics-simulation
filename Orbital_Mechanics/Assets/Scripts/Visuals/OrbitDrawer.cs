using Sim.Maneuvers;
using UnityEngine;
using Sim.Math;
using Sim.Objects;

namespace Sim.Visuals
{
    public class OrbitDrawer : MonoBehaviour
    {
        [SerializeField] private float lineWidth = 3f;
        [SerializeField][Range(1, 200)] private int orbitResolution = 200;
        [SerializeField][Range(1, 5)] private int depth = 2;

        [SerializeField] private bool isManeuver;
        
        public LineRenderer[] lineRenderers { get; private set; }
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
            SetupOrbitRenderers(isManeuver);
        }

        private void SetupOrbitRenderers(bool isManeuver)
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
        public void DestroyRenderers() {
            foreach(var line in lineRenderers) {
                Destroy(line.gameObject);
            }
            foreach(var line in lineButtons) {
                LineButton.allLineButtons.Remove(line);
            }
        }

        public void DrawOrbits(StateVectors stateVectors, Celestial centralBody, float timeToDraw = 0)
        {
            Celestial currentCelestial = centralBody;
            float timePassed = timeToDraw;

            for (int i = 0; i < this.depth; i++)
            {
                LineRenderer line = lineRenderers[i];
                line.gameObject.SetActive(true);
                line.gameObject.transform.SetParent(currentCelestial.transform);
                line.transform.localPosition = Vector3.zero;

                float timeToGravityChange;
                Celestial nextCelestial;
                StateVectors gravityChangePoint = DrawOrbit(stateVectors, i, currentCelestial, timePassed, out nextCelestial, out timeToGravityChange);
                line.colorGradient = CreateLineGradient(i);
                if (gravityChangePoint == null || nextCelestial == null) {
                    TurnOffRenderersFrom(i + 1);
                    return;
                }
                
                stateVectors = gravityChangePoint;
                currentCelestial = nextCelestial;
                timePassed += timeToGravityChange;
            }
        }
        private StateVectors DrawOrbit(StateVectors stateVectors, int lineIdx, Celestial currentCelestial, float timePassed, out Celestial nextCelestial, out float timeToGravityChange)
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

            // make indicator show always on the orbit
            var lineButton = lineButtons[lineIdx];
            lineButton.SetCustomIndicatorPositionConverter(pressWorldPosition => {
                var pressLocalPosition = pressWorldPosition - currentCelestial.transform.position;
                float angleToPoint = Vector3.SignedAngle(orbit.elements.eccVec, pressLocalPosition, orbit.elements.angMomentum);
                return orbit.CalculateOrbitalPosition(angleToPoint * Mathf.Deg2Rad) + currentCelestial.transform.position;
            });
            lineButton.ClearAllClickHandlers();
            lineButton.onLinePressed += (worldPos) => {
                if (!hasManeuver) {
                    var pressRelativePosition = worldPos - currentCelestial.transform.position;
                    ManeuverManager.Instance.CreateManeuver(orbit, inOrbitObject, pressRelativePosition);
                    hasManeuver = true;
                }
            };

            // loop if no gravity change reported
            var line = lineRenderers[lineIdx];
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
            line.colorGradient = CreateLineGradient(0, true);
            line.gameObject.SetActive(true);
            line.loop = gravityChangeVectors == null;
            line.positionCount = points.Length;
            line.SetPositions(points);
        }
    }
}