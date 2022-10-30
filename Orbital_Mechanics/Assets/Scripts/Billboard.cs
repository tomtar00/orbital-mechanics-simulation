using UnityEngine;
using Sim.Math;

public class Billboard : MonoBehaviour
{
    public bool scaleWithCameraDistance = true;
    public Vector3 eulerNorm;

    public float multiplier = 0.001f;
    public float minScale = 1f;
    public float maxScale = 1000f;

    void Update()
    {
        transform.LookAt(Camera.main.transform.position);
        transform.Rotate(eulerNorm * 90);

        if (scaleWithCameraDistance) {
            transform.localScale = NumericExtensions.ScaleWithDistance(
                transform.position, CameraController.Instance.cam.transform.position,
                multiplier, minScale, maxScale
            );
        }
    }
}
