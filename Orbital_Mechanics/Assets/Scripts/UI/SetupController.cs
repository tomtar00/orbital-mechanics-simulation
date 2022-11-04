using System.Text.RegularExpressions;
using UnityEngine;
using Sim.Objects;

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
        dateInput.text = "1/1/2000";
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

        systemGenerator.ResetSystem();
        InOrbitObject.allObjects = null;
        Celestial.celestials = null;
    }

    public void FinishSetup() {

        Regex regex = new Regex("^(?:(?:31(\\/|-|\\.)(?:0?[13578]|1[02]))\\1|(?:(?:29|30)(\\/|-|\\.)(?:0?[13-9]|1[0-2])\\2))(?:(?:1[6-9]|[2-9]\\d)?\\d{2})$|^(?:29(\\/|-|\\.)0?2\\3(?:(?:(?:1[6-9]|[2-9]\\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\\d|2[0-8])(\\/|-|\\.)(?:(?:0?[1-9])|(?:1[0-2]))\\4(?:(?:1[6-9]|[2-9]\\d)?\\d{2})$");
        if (!regex.Match(dateInput.text).Success) {
            wrongDate.SetActive(true);
            return;
        }

        systemGenerator.Star.BodiesOnOrbit[celestialIdx].HasSpacecraft = true;
        hudCanvas.alpha = 1;
        hudCanvas.interactable = true;
        setupCanvas.alpha = 0;
        setupCanvas.interactable = false;
        systemGenerator.Generate();
        systemGenerator.Star.BodiesOnOrbit[celestialIdx].HasSpacecraft = false;
    }
}
