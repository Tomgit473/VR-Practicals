using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace TutorialInfo
{
    /// <summary>
    /// Handles tracking, input reading, and visual representation for a VR controller.
    /// Supports OpenXR hardware devices with automatic desktop simulation fallback.
    /// </summary>
    public class VRHandController : MonoBehaviour
    {
        public enum HandType { Left, Right }

        [Header("Hand Setup")]
        [SerializeField] private HandType handType = HandType.Right;
        [SerializeField] private Material controllerDarkMat;
        [SerializeField] private Material controllerAccentMat;

        [Header("Simulation Offsets (When no VR hardware is connected)")]
        [SerializeField] private Vector3 simLocalPos = new Vector3(0.22f, -0.22f, 0.45f);
        [SerializeField] private Vector3 simLocalRot = new Vector3(10f, 0f, 0f);

        // Input States
        public bool IsTriggerPressed { get; private set; }
        public float TriggerValue { get; private set; }
        public bool IsGripPressed { get; private set; }
        public float GripValue { get; private set; }
        public Vector2 ThumbstickAxis { get; private set; }
        public bool IsPrimaryPressed { get; private set; }
        public bool IsSecondaryPressed { get; private set; }
        public bool IsHardwareTracked { get; private set; }

        private InputDevice targetDevice;
        private XRNode xrNode;

        // Visual Transforms for animations
        private Transform triggerVisual;
        private Transform thumbstickVisual;

        private void Awake()
        {
            xrNode = (handType == HandType.Left) ? XRNode.LeftHand : XRNode.RightHand;
            if (handType == HandType.Left && simLocalPos.x > 0)
            {
                simLocalPos.x = -simLocalPos.x;
            }
            BuildControllerVisual();
        }

        private void OnEnable()
        {
            InputDevices.deviceConnected += OnDeviceConnected;
        }

        private void OnDisable()
        {
            InputDevices.deviceConnected -= OnDeviceConnected;
        }

        private void OnDeviceConnected(InputDevice device)
        {
            CheckDevice();
        }

        private void Start()
        {
            CheckDevice();
        }

        private void CheckDevice()
        {
            targetDevice = InputDevices.GetDeviceAtXRNode(xrNode);
            IsHardwareTracked = targetDevice.isValid;
        }

        private void Update()
        {
            if (!targetDevice.isValid)
            {
                targetDevice = InputDevices.GetDeviceAtXRNode(xrNode);
            }

            IsHardwareTracked = targetDevice.isValid;

            if (IsHardwareTracked)
            {
                // Read 6-DoF Pose from XR Hardware
                if (targetDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
                {
                    transform.localPosition = pos;
                }
                if (targetDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
                {
                    transform.localRotation = rot;
                }

                // Read Inputs
                targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float trigVal);
                TriggerValue = trigVal;
                targetDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool trigBtn);
                IsTriggerPressed = trigBtn || (trigVal > 0.5f);

                targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripVal);
                GripValue = gripVal;
                targetDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripBtn);
                IsGripPressed = gripBtn || (gripVal > 0.5f);

                targetDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 stick);
                ThumbstickAxis = stick;

                targetDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool prim);
                IsPrimaryPressed = prim;

                targetDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool sec);
                IsSecondaryPressed = sec;
            }
            else
            {
                // Desktop Simulation Mode
                UpdateSimulationInput();
            }

            AnimateVisuals();
        }

        private void UpdateSimulationInput()
        {
            // Position relative to parent (camera or camera offset)
            transform.localPosition = simLocalPos;
            transform.localRotation = Quaternion.Euler(simLocalRot);

#if ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            var kb = UnityEngine.InputSystem.Keyboard.current;

            if (handType == HandType.Right)
            {
                if (mouse != null)
                {
                    IsTriggerPressed = mouse.leftButton.isPressed;
                    TriggerValue = IsTriggerPressed ? 1.0f : 0.0f;
                }
                if (kb != null)
                {
                    Vector2 stick = Vector2.zero;
                    if (kb.leftArrowKey.isPressed) stick.x -= 1f;
                    if (kb.rightArrowKey.isPressed) stick.x += 1f;
                    ThumbstickAxis = stick;
                }
            }
            else // Left Hand
            {
                if (kb != null)
                {
                    Vector2 stick = Vector2.zero;
                    if (kb.wKey.isPressed) stick.y += 1f;
                    if (kb.sKey.isPressed) stick.y -= 1f;
                    if (kb.aKey.isPressed) stick.x -= 1f;
                    if (kb.dKey.isPressed) stick.x += 1f;
                    ThumbstickAxis = stick;

                    IsGripPressed = kb.leftShiftKey.isPressed;
                    GripValue = IsGripPressed ? 1.0f : 0.0f;
                }
            }
#endif
        }

        private void AnimateVisuals()
        {
            if (triggerVisual != null)
            {
                triggerVisual.localRotation = Quaternion.Euler(Mathf.Lerp(0f, -22f, TriggerValue), 0f, 0f);
            }
            if (thumbstickVisual != null)
            {
                thumbstickVisual.localRotation = Quaternion.Euler(ThumbstickAxis.y * 18f, 0f, -ThumbstickAxis.x * 18f);
            }
        }

        public void BuildControllerVisual()
        {
            // Check if visuals already exist
            if (transform.Find("Visuals") != null) return;

            GameObject visRoot = new GameObject("Visuals");
            visRoot.transform.SetParent(transform, false);

            if (controllerDarkMat == null)
            {
                controllerDarkMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/TutorialInfo/Materials/Mat_VRController_Dark.mat");
            }
            if (controllerAccentMat == null)
            {
                string accentPath = (handType == HandType.Right) 
                    ? "Assets/TutorialInfo/Materials/Mat_VRController_Cyan.mat" 
                    : "Assets/TutorialInfo/Materials/Mat_VRController_Orange.mat";
                controllerAccentMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(accentPath);
            }

            // 1. Grip Handle
            GameObject grip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            grip.name = "Grip_Handle";
            grip.transform.SetParent(visRoot.transform, false);
            grip.transform.localPosition = new Vector3(0f, -0.05f, -0.04f);
            grip.transform.localRotation = Quaternion.Euler(22f, 0f, 0f);
            grip.transform.localScale = new Vector3(0.034f, 0.065f, 0.038f);
            StripCollider(grip);
            if (controllerDarkMat != null) grip.GetComponent<Renderer>().sharedMaterial = controllerDarkMat;

            // 2. Main Body / Sensor Plate
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "Sensor_Plate";
            plate.transform.SetParent(visRoot.transform, false);
            plate.transform.localPosition = new Vector3(0f, 0.012f, 0.02f);
            plate.transform.localRotation = Quaternion.Euler(-15f, 0f, 0f);
            plate.transform.localScale = new Vector3(0.056f, 0.028f, 0.075f);
            StripCollider(plate);
            if (controllerDarkMat != null) plate.GetComponent<Renderer>().sharedMaterial = controllerDarkMat;

            // 3. Tracking Halo Ring (Stylized Torus / Hex Ring)
            GameObject halo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            halo.name = "Tracking_Halo";
            halo.transform.SetParent(visRoot.transform, false);
            halo.transform.localPosition = new Vector3(0f, 0.035f, 0.045f);
            halo.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
            halo.transform.localScale = new Vector3(0.095f, 0.008f, 0.095f);
            StripCollider(halo);
            if (controllerAccentMat != null) halo.GetComponent<Renderer>().sharedMaterial = controllerAccentMat;

            // 4. Thumbstick
            GameObject stickBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stickBase.name = "Thumbstick_Stem";
            stickBase.transform.SetParent(plate.transform, false);
            stickBase.transform.localPosition = new Vector3(0f, 0.65f, 0.15f);
            stickBase.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
            StripCollider(stickBase);
            if (controllerAccentMat != null) stickBase.GetComponent<Renderer>().sharedMaterial = controllerAccentMat;
            thumbstickVisual = stickBase.transform;

            // 5. Trigger
            GameObject trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trigger.name = "Index_Trigger";
            trigger.transform.SetParent(visRoot.transform, false);
            trigger.transform.localPosition = new Vector3(0f, -0.015f, 0.025f);
            trigger.transform.localScale = new Vector3(0.018f, 0.028f, 0.022f);
            StripCollider(trigger);
            if (controllerAccentMat != null) trigger.GetComponent<Renderer>().sharedMaterial = controllerAccentMat;
            triggerVisual = trigger.transform;
        }

        private static void StripCollider(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying) Destroy(col);
                else DestroyImmediate(col);
            }
        }
    }
}
