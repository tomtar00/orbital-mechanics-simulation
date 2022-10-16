using UnityEngine;

namespace Sim.Objects {
public class SystemGenerator : MonoBehaviour
{
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private GameObject spacecraftPrefab;
    [SerializeField] private Transform systemParent;
    [Space]
    [SerializeField] private CelestialSO star;
    [Space]
    [SerializeField] private float modelScale = 1;

    private void Awake() {
        Generate();
    }

    private void Generate() {

        GenerateBody(star, centralBody: null);

    }

    private void GenerateBody(CelestialSO body, Celestial centralBody) {
        GameObject prefab = body.IsStar ? starPrefab : planetPrefab;
        GameObject go = Instantiate(prefab, systemParent);
        go.name = body.name;

        CelestialSO bodyData = Instantiate(body);

        bodyData.Diameter *= modelScale * SimulationSettings.Instance.scale;
        bodyData.Mass *= SimulationSettings.Instance.scale;

        var orbit = bodyData.Orbit;
        orbit.semimajorAxis *= SimulationSettings.Instance.scale;
        // orbit.inclination += Mathf.PI / 2;
        
        bodyData.Orbit = orbit;

        Celestial celestial = go.GetComponent<Celestial>();
        celestial.Init(centralBody, bodyData);

        if (body.HasSpacecraft) {
            SpawnSpacecraft(celestial);
        }

        foreach (var planet in bodyData.BodiesOnOrbit) {
            GenerateBody(planet, celestial);
        }
    }

    private void SpawnSpacecraft(Celestial celestial) {
        GameObject go = Instantiate(spacecraftPrefab, systemParent);
        Spacecraft craft = go.GetComponent<Spacecraft>();
        craft.CentralBody = celestial;
        craft.Init(celestial, null);
    }
}
}