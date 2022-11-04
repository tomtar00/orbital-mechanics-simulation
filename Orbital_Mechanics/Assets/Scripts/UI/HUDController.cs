// using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sim.Maneuvers;
using Sim.Objects;
using Time = Sim.Time;

public class HUDController : MonoBehaviour
{
    [Header("Orbit")]
    [SerializeField] private TMP_Text orbitElementsText;
    [Header("Time")]
    [SerializeField] private TMP_Text timeScaleText;
    [SerializeField] private float[] timeScales;
    [SerializeField] private int defaultTimeScaleIdx = 3;
    [Header("Maneuver")]
    [SerializeField] private Button removeManeuver;

    public Button RemoveManeuverBtn { get => removeManeuver; }
    public bool blockTimeChange { get; private set; } = false;

    private int previousTimeScaleIdx = 1;
    private int currentTimeScaleIdx = 3;

    public static HUDController Instance;
    private void Awake() {
        Instance = this;
        Time.timeScale = timeScales[currentTimeScaleIdx];
        UpdateTimeScaleText();
        currentTimeScaleIdx = defaultTimeScaleIdx;
    }
    private void Update() {
        ApplyOrbitElements();
    }

    private void UpdateTimeScaleText() {
        Time.timeScale = timeScales[currentTimeScaleIdx];
        timeScaleText.text = "x" + Time.timeScale;
    }

    public void IncreaseTimeScale() {
        if (blockTimeChange) return;
        if (++currentTimeScaleIdx >= timeScales.Length) currentTimeScaleIdx = timeScales.Length - 1;
        previousTimeScaleIdx = currentTimeScaleIdx;
        UpdateTimeScaleText();
    }
    public void DecreaseTimeScale() {
        if (blockTimeChange) return;
        if (--currentTimeScaleIdx < 0) currentTimeScaleIdx = 0;
        previousTimeScaleIdx = currentTimeScaleIdx;
        UpdateTimeScaleText();
    }

    public void SetTimeScaleToDefault() {
        if (blockTimeChange) return;
        currentTimeScaleIdx = defaultTimeScaleIdx;
        UpdateTimeScaleText();
        blockTimeChange = true;
    }
    public void SetTimeScaleToPrevious() {
        if (!blockTimeChange) return;
        currentTimeScaleIdx = previousTimeScaleIdx;
        UpdateTimeScaleText();
        blockTimeChange = false;
    }

    public void SpacecraftFocus() {
        CameraController.Instance.Focus(Spacecraft.current.transform);
    }
    public void FreeCamera() {
        CameraController.Instance.Focus(null);
    }

    public void RemoveCurrentManeuver() {
        ManeuverManager.Instance.RemoveManeuvers(ManeuverNode.current.maneuver);
    }

    public void HandleAutoManeuversValueChange(bool isOn) {
        Spacecraft.current.Autopilot = isOn;
    }

    private void ApplyOrbitElements() {
        if (Spacecraft.current == null || Spacecraft.current.Kepler.orbit == null) return;
        orbitElementsText.text = $@"
Spacecraft orbit:
{ JsonUtility.ToJson(Spacecraft.current.Kepler.orbit.elements, true) }

Velocity: { Spacecraft.current.Speed } m/s
Time to gravity change: { Spacecraft.current.TimeToGravityChange }";
    }
}
