using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Sim.Maneuvers;
using Sim.Math;
using TMPro;

public class ManeuverTimeline : MonoBehaviour
{
    [SerializeField] private GameObject timelineHolder;
    [SerializeField] private Image timelineFill;
    [SerializeField] private RectTransform rectMarkers;

    public static ManeuverTimeline Instance { get; private set; }
    private void Awake() {
        Instance = this;
    }

    private void Update() {
        CheckTimelineVisibility();
        UpdateFill();
        UpdateMarkers();
    }

    private void CheckTimelineVisibility() {

        if (ManeuverManager.Instance.maneuvers.Count > 0) {
            if (!timelineHolder.activeSelf)
                timelineHolder.SetActive(true);
        }
        else if (timelineHolder.activeSelf) {
            timelineHolder.SetActive(false);
        }

    }

    private void UpdateFill() {

        if (!timelineHolder.activeSelf) return;

        var maneuver = ManeuverManager.Instance.maneuvers.Last();
        timelineFill.fillAmount = (float)(1.0 - maneuver.timeToManeuver / maneuver.fixedTimeToManeuver);

    }

    private void UpdateMarkers() 
    {
        if (!timelineHolder.activeSelf) return;
        var maneuver = ManeuverManager.Instance.maneuvers.Last();

        int i = 0;
        foreach (var m in ManeuverManager.Instance.maneuvers)
        {
            var marker = rectMarkers.GetChild(i) as RectTransform;

            marker.gameObject.SetActive(true);
            marker.anchoredPosition3D = (Vector3Double.up * (m.fixedTimeToManeuver / maneuver.fixedTimeToManeuver) * rectMarkers.rect.height);

            marker.transform.GetChild(1).GetComponent<TMP_Text>()
                .text = $"T {((m.timeToManeuver > 0) ? "-" : "+")} {MathLib.Abs(m.timeToManeuver).ToTimeSpan()}\nBurn: {m.burnTime.ToString("f2")}";

            i++;
        }
        for (int j = i; j < rectMarkers.childCount; j++) {
            rectMarkers.GetChild(i).gameObject.SetActive(false);
        }

    }
}
