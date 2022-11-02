using UnityEngine;
using Sim.Objects;

public class SetupController : MonoBehaviour
{
    [SerializeField] private SystemGenerator systemGenerator;

    [SerializeField] private CanvasGroup hudCanvas;
    [SerializeField] private CanvasGroup setupCanvas;

    [SerializeField] private TMPro.TMP_Dropdown dropdown;
    private int celestialIdx;

    private void Start() {
        celestialIdx = dropdown.value;
    }

    public void OnStartOrbitChanged(int celestialIdx) {
        this.celestialIdx = celestialIdx;
    }

    public void FinishSetup() {
        systemGenerator.Star.BodiesOnOrbit[celestialIdx].HasSpacecraft = true;
        hudCanvas.alpha = 1;
        hudCanvas.interactable = true;
        setupCanvas.alpha = 0;
        setupCanvas.interactable = false;
        systemGenerator.Generate();
        systemGenerator.Star.BodiesOnOrbit[celestialIdx].HasSpacecraft = false;
    }
}
