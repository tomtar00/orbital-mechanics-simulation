using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sim.Maneuvers;

public class HUDController : MonoBehaviour
{
    [Header("Time")]
    [SerializeField] private TMP_Text timeScaleText;
    [SerializeField] private float[] timeScales;
    [SerializeField] private int currentTimeScaleIdx = 1;
    [Header("Maneuver")]
    [SerializeField] private Button removeManeuver;
    public Button RemoveManeuver { get => removeManeuver; }

    public static HUDController Instance;
    private void Awake() {
        Instance = this;
        Time.timeScale = timeScales[currentTimeScaleIdx];
        UpdateTimeScaleText();
    }

    private void UpdateTimeScaleText() {
        timeScaleText.text = "x" + Time.timeScale;
    }

    public void IncreaseTimeScale() {
        if (++currentTimeScaleIdx >= timeScales.Length) currentTimeScaleIdx = timeScales.Length - 1;
        Time.timeScale = timeScales[currentTimeScaleIdx];
        UpdateTimeScaleText();
    }
    public void DecreaseTimeScale() {
        if (--currentTimeScaleIdx < 0) currentTimeScaleIdx = 0;
        Time.timeScale = timeScales[currentTimeScaleIdx];
        UpdateTimeScaleText();
    }

    public void ChangeFocus() {
        CameraController.Instance.FocusOnShip();
    }

    public void RemoveCurrentManeuver() {
        ManeuverManager.Instance.RemoveManeuvers(ManeuverNode.current.maneuver);
    }
}
