using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sim.Maneuvers;
using Sim.Objects;

public class HUDController : MonoBehaviour
{
    [Header("Orbit")]
    [SerializeField] private TMP_Text orbitElementsText;
    [Header("Time")]
    [SerializeField] private TMP_Text timeScaleText;
    [SerializeField] private float[] timeScales;
    [SerializeField] private int currentTimeScaleIdx = 2;
    [Header("Maneuver")]
    [SerializeField] private Button removeManeuver;

    public Button RemoveManeuverBtn { get => removeManeuver; }
    public bool blockTimeChange { get; private set; } = false;

    private int previousTimeScaleIdx = 1;

    public static HUDController Instance;
    private void Awake() {
        Instance = this;
        Time.timeScale = timeScales[currentTimeScaleIdx];
        UpdateTimeScaleText();
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
        currentTimeScaleIdx = 2;
        UpdateTimeScaleText();
        blockTimeChange = true;
    }
    public void SetTimeScaleToPrevious() {
        if (!blockTimeChange) return;
        currentTimeScaleIdx = previousTimeScaleIdx;
        UpdateTimeScaleText();
        blockTimeChange = false;
    }

    public void ChangeFocus() {
        CameraController.Instance.FocusOnShip();
    }

    public void RemoveCurrentManeuver() {
        ManeuverManager.Instance.RemoveManeuvers(ManeuverNode.current.maneuver);
    }

    public void HandleAutoManeuversValueChange(bool isOn) {
        Spacecraft.current.AutoManeuvers = isOn;
    }

    private void ApplyOrbitElements() {
        // StringBuilder builder = new StringBuilder();

        // foreach (var item in Spacecraft.current.Kepler.orbit.elements)
        // {
            
        // }

        // orbitElementsText.text = builder.ToString();
        orbitElementsText.text = JsonUtility.ToJson(Spacecraft.current.Kepler.orbit.elements, true);
    }
}
