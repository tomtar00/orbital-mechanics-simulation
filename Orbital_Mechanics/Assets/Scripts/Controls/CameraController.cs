using UnityEngine;
using Sim.Objects;

public class CameraController : MonoBehaviour
{
    [Header("Dummy")]
    public Camera dummyCamera;
    [Header("Fly")]
    [SerializeField] private float acceleration = 50;
    [SerializeField] private float accSprintMultiplier = 4;
    [SerializeField] private float sensitivity = 1;
    [SerializeField] private float damping = 5;
    [SerializeField] private bool focusOnEnable = true;
    [SerializeField] private Transform solarSystemHolder;
    [Header("Orbit")]
    [SerializeField] private float distance = 5;
    [SerializeField] private float rotationSpeed = 90;
    [SerializeField, Range(-89f, 89f)] float minVerticalAngle = -80f, maxVerticalAngle = 80f;
    [SerializeField] private float scrollSensitivity = 7;
    [SerializeField] private float scrollDamping = 10;
    [SerializeField] private float minDistance, maxDistance;

    private Vector3 velocity;
    private float targetDistance;
    private Vector3 targetPosition;
    public bool initialized { get; set; } = false;

    private Transform focusObject;
    private Vector2 orbitAngles = new Vector2(45f, 0f);
    public bool focusingOnObject { get; private set; } = false;
    public Camera cam { get; private set; }

    static bool Focused
    {
        get => Cursor.lockState == CursorLockMode.Locked;
        set
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }
    }

    void OnDisable() => Focused = false;

    public static CameraController Instance;

    public void Init()
    {
        if (initialized) return;
        Instance = this;
        if (focusOnEnable) Focused = true;
        targetDistance = distance;
        cam = GetComponent<Camera>();
        Focus(Spacecraft.current?.transform);
        initialized = true;
    }

    private void LateUpdate()
    {
        if (!initialized) return;

        if (focusingOnObject)
        {
            Orbit();
        }
        else
        {
            if (Focused)
                Fly();
        }

        if (!Focused && Input.GetMouseButtonDown(0) && !GUIHoverListener.focusingOnGUI)
        {
            Focused = true;
        }

        // Leave cursor lock
        if (Input.GetKeyDown(KeyCode.Q))
            Focused = false;
    }

    void Fly()
    {
        // Position
        velocity += GetAccelerationVector() * Time.unscaledDeltaTime;

        // Rotation
        Vector2 mouseDelta = sensitivity * new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
        Quaternion rotation = transform.rotation;
        Quaternion horizontal = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);
        Quaternion vertical = Quaternion.AngleAxis(mouseDelta.y, Vector3.right);
        transform.rotation = horizontal * rotation * vertical;

        velocity = Vector3.Lerp(velocity, Vector3.zero, damping * Time.unscaledDeltaTime);
        solarSystemHolder.position -= velocity * Time.unscaledDeltaTime;
    }
    Vector3 GetAccelerationVector()
    {
        Vector3 moveInput = default;

        void AddMovement(KeyCode key, Vector3 dir)
        {
            if (Input.GetKey(key))
                moveInput += dir;
        }

        AddMovement(KeyCode.W, Vector3.forward);
        AddMovement(KeyCode.S, Vector3.back);
        AddMovement(KeyCode.D, Vector3.right);
        AddMovement(KeyCode.A, Vector3.left);
        AddMovement(KeyCode.Space, Vector3.up);
        AddMovement(KeyCode.LeftControl, Vector3.down);
        Vector3 direction = transform.TransformVector(moveInput.normalized);

        if (Input.GetKey(KeyCode.LeftShift))
            return direction * (acceleration * accSprintMultiplier);
        return direction * acceleration;
    }

    private void Orbit()
    {
        UpdateRotation();
        ConstrainAngles();
        UpdateDistance();
    }
    void UpdateRotation()
    {
        Vector2 input = Focused ? new Vector2(
            -Input.GetAxis("Mouse Y"),
            Input.GetAxis("Mouse X")
        ) : Vector2.zero;
        solarSystemHolder.position = -focusObject.localPosition;
        orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
        Quaternion lookRotation = Quaternion.Euler(orbitAngles);
        Vector3 lookDirection = lookRotation * -Vector3.forward;
        Vector3 lookPosition = lookDirection * distance;
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }
    void UpdateDistance()
    {
        targetDistance -= Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        distance = Mathf.Lerp(distance, targetDistance, Time.unscaledDeltaTime * scrollDamping);
    }
    void ConstrainAngles()
    {
        orbitAngles.x =
            Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

        if (orbitAngles.y < 0f)
        {
            orbitAngles.y += 360f;
        }
        else if (orbitAngles.y >= 360f)
        {
            orbitAngles.y -= 360f;
        }
    }

    public void Focus(Transform objectTransform)
    {
        if (objectTransform != null)
        {
            focusObject = objectTransform;
            focusingOnObject = true;
        }
        else
        {
            focusingOnObject = false;
        }
        Focused = true;
    }
}
