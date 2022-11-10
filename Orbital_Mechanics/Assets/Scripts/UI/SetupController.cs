using System;
using System.Text.RegularExpressions;
using UnityEngine;
using Sim.Objects;
using System.Linq;

public class SetupController : MonoBehaviour
{
    [SerializeField] private SystemGenerator systemGenerator;
    [Space]
    [SerializeField] private CanvasGroup hudCanvas;
    [SerializeField] private CanvasGroup setupCanvas;
    [Space]
    [SerializeField] private TMPro.TMP_Dropdown dropdown;
    [SerializeField] private TMPro.TMP_InputField dateInput;
    [SerializeField] private GameObject wrongDate;

    private int celestialIdx;

    private void Start() {
        celestialIdx = dropdown.value;
        dateInput.text = "1/1/2000 12:00:00";

        dropdown.AddOptions(systemGenerator.Star.BodiesOnOrbit.Select(b => b.name).ToList());
    }

    public void OnStartOrbitChanged(int celestialIdx) {
        this.celestialIdx = celestialIdx;
    }

    public void ResetSimulation() {
        foreach (var obj in InOrbitObject.allObjects) {
            Destroy(obj.gameObject);
        }
        hudCanvas.alpha = 0;
        hudCanvas.interactable = false;
        setupCanvas.alpha = 1;
        setupCanvas.interactable = true;

        wrongDate.SetActive(false);

        systemGenerator.ResetSystem();
        InOrbitObject.allObjects = null;
        Celestial.celestials = null;
    }

    public void FinishSetup() {

        if (!DateTime.TryParse(dateInput.text, out DateTime date)) {
            wrongDate.SetActive(true);
            return;
        }

        hudCanvas.alpha = 1;
        hudCanvas.interactable = true;
        setupCanvas.alpha = 0;
        setupCanvas.interactable = false;
        systemGenerator.Generate(date, dropdown.options[dropdown.value].text);
    }
}
