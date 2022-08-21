using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float acceleration = 50;       
    [SerializeField] private float accSprintMultiplier = 4; 
    [SerializeField] private float sensitivity = 1; 
    [SerializeField] private float damping = 5; 
    [SerializeField] private bool focusOnEnable = true; 

    Vector3 velocity;

    static bool Focused
    {
        get => Cursor.lockState == CursorLockMode.Locked;
        set
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }
    }

    void OnEnable()
    {
        if (focusOnEnable) Focused = true;
    }

    void OnDisable() => Focused = false;

    void Update()
    {
        if (Focused)
            UpdateInput();
        else if (Input.GetMouseButtonDown(0) && !GUIHoverListener.focusingOnGUI)
            Focused = true;

        velocity = Vector3.Lerp(velocity, Vector3.zero, damping * Time.deltaTime);
        transform.position += velocity * Time.deltaTime;
    }

    void UpdateInput()
    {
        // Position
        velocity += GetAccelerationVector() * Time.deltaTime;

        // Rotation
        Vector2 mouseDelta = sensitivity * new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
        Quaternion rotation = transform.rotation;
        Quaternion horizontal = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);
        Quaternion vertical = Quaternion.AngleAxis(mouseDelta.y, Vector3.right);
        transform.rotation = horizontal * rotation * vertical;

        // Leave cursor lock
        if (Input.GetKeyDown(KeyCode.Escape))
            Focused = false;
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

    public void FocusOnShip() {
        GameObject ship = GameObject.FindGameObjectWithTag("Player");
        transform.SetParent(ship.transform);
    }
}
