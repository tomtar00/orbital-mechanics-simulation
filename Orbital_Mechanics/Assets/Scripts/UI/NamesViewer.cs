using System.Collections.Generic;
using UnityEngine;
using Sim.Objects;
using UnityEngine.UI;

public class NamesViewer : MonoBehaviour
{
    [SerializeField] private GameObject namePrefab;
    [SerializeField] private Transform namesHolder;

    private Dictionary<Transform, Celestial> names;

    public void Init() {
        names = new Dictionary<Transform, Celestial>();
        foreach(Celestial celestial in Celestial.celestials) {
            SetupName(celestial);
        }
    }

    private void LateUpdate() {
        foreach(KeyValuePair<Transform, Celestial> keyValue in names) {
            UpdateName(keyValue);
        }
    }

    private void SetupName(Celestial celestial) {
        GameObject go = Instantiate(namePrefab, namesHolder);
        go.name = celestial.name;
        go.GetComponent<TMPro.TMP_Text>().text = celestial.name;
        names[go.transform] = celestial;

        go.GetComponent<Button>().onClick.AddListener(() => {
            CameraController.Instance.Focus(celestial.transform);
        });
    }

    private void UpdateName(KeyValuePair<Transform, Celestial> pair) {

        if (pair.Value.CentralBody != null && !pair.Value.CentralBody.IsStationary && !pair.Value.CentralBody.camInsideInfluence) {
            if (pair.Key.gameObject.activeSelf)
                pair.Key.gameObject.SetActive(false);
        }
        else {
            pair.Key.position = CameraController.Instance.cam.WorldToScreenPoint(pair.Value.transform.position);
            if (pair.Key.position.z > 0)
                pair.Key.position = new Vector3(pair.Key.position.x, pair.Key.position.y, 0);
            else
                pair.Key.position = Vector3.up * 1000f;

            if (!pair.Key.gameObject.activeSelf)
                pair.Key.gameObject.SetActive(true);
        }
    }
}
