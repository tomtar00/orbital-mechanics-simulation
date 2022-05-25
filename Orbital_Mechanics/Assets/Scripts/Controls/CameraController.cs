using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float zoomSpeed;

    private void Update() {
        Move();
        Zoom();
    }

    private void Move() {
        Vector3 diff = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) {
            diff += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.A)) {
            diff += Vector3.left;
        }
        if (Input.GetKey(KeyCode.S)) {
            diff += Vector3.back;
        }
        if (Input.GetKey(KeyCode.D)) {
            diff += Vector3.right;
        }
        transform.position += diff * Time.unscaledDeltaTime * moveSpeed;
    }

    private void Zoom() {
        Vector3 diff = Vector3.zero;
        if (Input.GetAxis("Mouse ScrollWheel") < 0) {
            diff = Vector3.up;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0) {
            diff = Vector3.down;
        }
        transform.position += diff * Time.unscaledDeltaTime * zoomSpeed;
    }
}
