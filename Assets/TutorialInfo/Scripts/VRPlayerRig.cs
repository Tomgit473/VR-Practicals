using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

namespace TutorialInfo
{
    /// <summary>
    /// Master VR Rig coordinator. Integrates Unity.XR.CoreUtils.XROrigin,
    /// manages HMD detection, VR controller hands, laser pointers, and desktop simulation fallback.
    /// </summary>
    public class VRPlayerRig : MonoBehaviour
    {
        [Header("XR Origin Components")]
        [SerializeField] private XROrigin xrOrigin;
        [SerializeField] private GameObject cameraFloorOffset;
        [SerializeField] private Camera hmdCamera;
        [SerializeField] private VRHandController leftHandController;
        [SerializeField] private VRHandController rightHandController;
        [SerializeField] private VRLaserPointer rightLaserPointer;
        [SerializeField] private VRLocomotion locomotion;

        [Header("Desktop Simulation")]
        [SerializeField] private bool simulatedVRActive = false;
        [SerializeField] private FreeFlyCamera desktopFlyCam;

        public bool IsHMDActive { get; private set; }

        private void Awake()
        {
            SetupRigReferences();
            EnsureUIButtonColliders();
        }

        private void Start()
        {
            CheckHMDStatus();
            ApplyModeState();
        }

        private void Update()
        {
            CheckHMDStatus();

#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.vKey.wasPressedThisFrame)
            {
                simulatedVRActive = !simulatedVRActive;
                ApplyModeState();
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.V))
            {
                simulatedVRActive = !simulatedVRActive;
                ApplyModeState();
            }
#endif
        }

        private void CheckHMDStatus()
        {
            var headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            bool headValid = headDevice.isValid;
            bool xrActive = XRSettings.isDeviceActive;

            IsHMDActive = headValid || xrActive;
        }

        private void ApplyModeState()
        {
            bool inVR = IsHMDActive || simulatedVRActive;

            if (desktopFlyCam != null)
            {
                // In true VR, HMD handles rotation, disable flycam look
                desktopFlyCam.enabled = !IsHMDActive;
            }

            if (leftHandController != null)
            {
                leftHandController.gameObject.SetActive(inVR);
            }

            if (rightHandController != null)
            {
                rightHandController.gameObject.SetActive(inVR);
            }

            if (locomotion != null)
            {
                locomotion.enabled = inVR;
            }
        }

        public void SetupRigReferences()
        {
            if (xrOrigin == null)
            {
                xrOrigin = GetComponent<XROrigin>();
                if (xrOrigin == null) xrOrigin = gameObject.AddComponent<XROrigin>();
            }

            if (cameraFloorOffset == null)
            {
                var offset = transform.Find("Camera Offset");
                if (offset != null) cameraFloorOffset = offset.gameObject;
            }

            if (hmdCamera == null)
            {
                hmdCamera = GetComponentInChildren<Camera>();
                if (hmdCamera == null) hmdCamera = Camera.main;
            }

            if (hmdCamera != null)
            {
                hmdCamera.nearClipPlane = 0.03f;
                desktopFlyCam = hmdCamera.GetComponent<FreeFlyCamera>();
            }

            if (xrOrigin != null && hmdCamera != null)
            {
                xrOrigin.Camera = hmdCamera;
                if (cameraFloorOffset != null)
                {
                    xrOrigin.CameraFloorOffsetObject = cameraFloorOffset;
                }
                xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            }

            if (leftHandController == null)
            {
                leftHandController = GetComponentInChildren<VRHandController>();
            }
            if (rightHandController == null)
            {
                var hands = GetComponentsInChildren<VRHandController>();
                foreach (var h in hands)
                {
                    if (h != leftHandController) rightHandController = h;
                }
            }

            if (rightLaserPointer == null && rightHandController != null)
            {
                rightLaserPointer = rightHandController.GetComponent<VRLaserPointer>();
            }

            if (locomotion == null)
            {
                locomotion = GetComponent<VRLocomotion>();
            }
        }

        public static void EnsureUIButtonColliders()
        {
            var canvasGo = GameObject.Find("UI_WorldCanvas");
            if (canvasGo == null) return;

            var buttons = canvasGo.GetComponentsInChildren<UnityEngine.UI.Button>();
            foreach (var btn in buttons)
            {
                var box = btn.GetComponent<BoxCollider>();
                if (box == null)
                {
                    box = btn.gameObject.AddComponent<BoxCollider>();
                }

                var rt = btn.GetComponent<RectTransform>();
                if (rt != null)
                {
                    box.size = new Vector3(rt.rect.width, rt.rect.height, 12f);
                    box.center = new Vector3(0f, 0f, 0f);
                }
            }
        }
    }
}
