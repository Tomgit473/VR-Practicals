using UnityEngine;

namespace TutorialInfo
{
    /// <summary>
    /// Smooth freefly camera controller for exploring the 3D workshop.
    /// Controls:
    /// - Hold Right Mouse Button (RMB) to look around (Unity Scene View flythrough style)
    /// - WASD / Arrow keys: Move Forward, Backward, Strafe Left, Strafe Right
    /// - Space / E: Ascend (fly up)
    /// - Left Ctrl / Q: Descend (fly down)
    /// - Left Shift: Boost speed
    /// - Left Alt: Precision slow speed
    /// - Mouse Scroll Wheel: Increase / decrease base flight speed
    /// - Backspace / Home: Reset camera to initial position
    /// - H: Toggle on-screen control hint
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class FreeFlyCamera : MonoBehaviour
    {
        [Header("Movement Speeds")]
        [Tooltip("Normal flight speed in meters per second.")]
        [SerializeField] private float baseSpeed = 4.5f;

        [Tooltip("Multiplier applied when holding Left Shift.")]
        [SerializeField] private float boostMultiplier = 2.5f;

        [Tooltip("Multiplier applied when holding Left Alt.")]
        [SerializeField] private float slowMultiplier = 0.35f;

        [Tooltip("Speed range min and max adjustable via mouse scroll.")]
        [SerializeField] private Vector2 speedRange = new Vector2(1.0f, 18.0f);

        [Header("Look Sensitivity & Smoothing")]
        [Tooltip("Sensitivity for mouse look.")]
        [SerializeField] private float lookSensitivity = 0.18f;

        [Tooltip("Smoothing factor for movement.")]
        [SerializeField] private float moveSmoothing = 14f;

        [Tooltip("Smoothing factor for rotation.")]
        [SerializeField] private float rotationSmoothing = 18f;

        [Header("Floor Boundary")]
        [Tooltip("Prevent camera from sinking below the garage floor.")]
        [SerializeField] private float minHeight = 0.35f;

        [Header("UI Controls Hint")]
        [SerializeField] private bool showControlsHud = true;

        // Internal State
        private float currentYaw;
        private float currentPitch;
        private float targetYaw;
        private float targetPitch;

        private Vector3 currentVelocity;
        private Vector3 targetVelocity;

        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private bool isLooking;
        private Camera targetCamera;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            initialPosition = transform.position;
            initialRotation = transform.rotation;

            Vector3 euler = transform.eulerAngles;
            targetYaw = euler.y;
            targetPitch = euler.x;
            if (targetPitch > 180f) targetPitch -= 360f;

            currentYaw = targetYaw;
            currentPitch = targetPitch;
        }

        private void Update()
        {
            HandleInput();
            ApplyMotion();
        }

        private void HandleInput()
        {
            Vector3 inputDir = Vector3.zero;
            Vector2 mouseDelta = Vector2.zero;
            float scrollDelta = 0f;
            bool boost = false;
            bool slow = false;
            bool resetRequested = false;
            bool toggleHudRequested = false;

#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            var mouse = UnityEngine.InputSystem.Mouse.current;

            if (mouse != null)
            {
                isLooking = mouse.rightButton.isPressed;
                if (isLooking)
                {
                    mouseDelta = mouse.delta.ReadValue();
                }

                scrollDelta = mouse.scroll.ReadValue().y;
            }

            if (kb != null)
            {
                // Movement
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) inputDir += Vector3.forward;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) inputDir += Vector3.back;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) inputDir += Vector3.right;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) inputDir += Vector3.left;

                // Vertical
                if (kb.spaceKey.isPressed || kb.eKey.isPressed) inputDir += Vector3.up;
                if (kb.leftCtrlKey.isPressed || kb.qKey.isPressed) inputDir += Vector3.down;

                // Modifiers
                boost = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
                slow = kb.leftAltKey.isPressed || kb.rightAltKey.isPressed;

                // Reset & Help
                if (kb.backspaceKey.wasPressedThisFrame || kb.homeKey.wasPressedThisFrame) resetRequested = true;
                if (kb.hKey.wasPressedThisFrame) toggleHudRequested = true;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            isLooking = Input.GetMouseButton(1);
            if (isLooking)
            {
                mouseDelta = new Vector2(Input.GetAxis("Mouse X") * 10f, Input.GetAxis("Mouse Y") * 10f);
            }

            scrollDelta = Input.GetAxis("Mouse ScrollWheel");

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputDir += Vector3.forward;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputDir += Vector3.back;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputDir += Vector3.right;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputDir += Vector3.left;

            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E)) inputDir += Vector3.up;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.Q)) inputDir += Vector3.down;

            boost = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            slow = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Home)) resetRequested = true;
            if (Input.GetKeyDown(KeyCode.H)) toggleHudRequested = true;
#endif

            // Cursor lock during look
            if (isLooking)
            {
                if (Cursor.lockState != CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }

                targetYaw += mouseDelta.x * lookSensitivity;
                targetPitch -= mouseDelta.y * lookSensitivity;
                targetPitch = Mathf.Clamp(targetPitch, -86f, 86f);
            }
            else
            {
                if (Cursor.lockState != CursorLockMode.None)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }

            // Scroll wheel adjust base speed
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                baseSpeed += (scrollDelta > 0 ? 0.75f : -0.75f);
                baseSpeed = Mathf.Clamp(baseSpeed, speedRange.x, speedRange.y);
            }

            // Speed multiplier
            float speed = baseSpeed;
            if (boost) speed *= boostMultiplier;
            else if (slow) speed *= slowMultiplier;

            // Velocity in camera local space
            Vector3 worldDir = transform.TransformDirection(inputDir.normalized);
            // If flying straight up/down with Space/Ctrl, keep purely vertical
            if (Mathf.Abs(inputDir.y) > 0.01f && Mathf.Abs(inputDir.x) < 0.01f && Mathf.Abs(inputDir.z) < 0.01f)
            {
                worldDir = new Vector3(0f, Mathf.Sign(inputDir.y), 0f);
            }

            targetVelocity = worldDir * speed;

            // Reset camera position
            if (resetRequested)
            {
                ResetCamera();
            }

            // Toggle HUD
            if (toggleHudRequested)
            {
                showControlsHud = !showControlsHud;
            }
        }

        private void ApplyMotion()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            // Smooth rotation
            currentYaw = Mathf.Lerp(currentYaw, targetYaw, dt * rotationSmoothing);
            currentPitch = Mathf.Lerp(currentPitch, targetPitch, dt * rotationSmoothing);
            transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

            // Smooth translation
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, dt * moveSmoothing);
            transform.position += currentVelocity * dt;

            // Floor boundary
            if (transform.position.y < minHeight)
            {
                Vector3 p = transform.position;
                p.y = minHeight;
                transform.position = p;
                if (currentVelocity.y < 0f) currentVelocity.y = 0f;
            }
        }

        public void ResetCamera()
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            Vector3 euler = initialRotation.eulerAngles;
            targetYaw = euler.y;
            targetPitch = euler.x;
            if (targetPitch > 180f) targetPitch -= 360f;
            currentYaw = targetYaw;
            currentPitch = targetPitch;
            currentVelocity = Vector3.zero;
            targetVelocity = Vector3.zero;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
