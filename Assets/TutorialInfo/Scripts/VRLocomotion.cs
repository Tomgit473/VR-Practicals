using UnityEngine;

namespace TutorialInfo
{
    /// <summary>
    /// VR Locomotion system providing both Smooth Locomotion and Snap Turning,
    /// plus Parabolic Teleportation for comfort.
    /// </summary>
    public class VRLocomotion : MonoBehaviour
    {
        [Header("Rig References")]
        [SerializeField] private Transform xrOriginTransform;
        [SerializeField] private Transform headCameraTransform;
        [SerializeField] private VRHandController leftHand;
        [SerializeField] private VRHandController rightHand;

        [Header("Smooth Locomotion")]
        [SerializeField] private float moveSpeed = 3.2f;
        [SerializeField] private Vector2 roomBoundsX = new Vector2(-6.2f, 6.2f);
        [SerializeField] private Vector2 roomBoundsZ = new Vector2(-6.2f, 6.2f);

        [Header("Snap Turning")]
        [SerializeField] private float snapAngle = 45f;
        [SerializeField] private float snapThreshold = 0.65f;
        private bool hasSnapTurned = false;

        [Header("Teleportation")]
        [SerializeField] private Material teleportArcMaterial;
        [SerializeField] private int arcSegments = 25;
        [SerializeField] private float arcVelocity = 8.5f;

        private LineRenderer teleportLine;
        private GameObject teleportMarker;
        private bool isTeleportAiming = false;
        private Vector3 currentTeleportTarget;

        private void Awake()
        {
            if (xrOriginTransform == null) xrOriginTransform = transform;
            if (headCameraTransform == null && Camera.main != null) headCameraTransform = Camera.main.transform;

            SetupTeleportVisuals();
        }

        private void SetupTeleportVisuals()
        {
            GameObject arcGo = new GameObject("VR_Teleport_Arc");
            arcGo.transform.SetParent(transform, false);
            teleportLine = arcGo.AddComponent<LineRenderer>();
            teleportLine.useWorldSpace = true;
            teleportLine.startWidth = 0.025f;
            teleportLine.endWidth = 0.01f;
            teleportLine.positionCount = arcSegments;

            if (teleportArcMaterial == null)
            {
                teleportArcMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/TutorialInfo/Materials/Mat_VRTeleportArc.mat");
            }
            if (teleportArcMaterial != null)
            {
                teleportLine.material = teleportArcMaterial;
            }
            teleportLine.enabled = false;

            // Teleport Ring Marker
            teleportMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            teleportMarker.name = "VR_Teleport_Marker";
            teleportMarker.transform.SetParent(transform, false);
            teleportMarker.transform.localScale = new Vector3(0.6f, 0.015f, 0.6f);
            var col = teleportMarker.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);

            if (teleportArcMaterial != null)
            {
                teleportMarker.GetComponent<Renderer>().sharedMaterial = teleportArcMaterial;
            }
            teleportMarker.SetActive(false);
        }

        private void Update()
        {
            if (xrOriginTransform == null || headCameraTransform == null) return;

            HandleSmoothMovement();
            HandleSnapTurn();
            HandleTeleportation();
        }

        private void HandleSmoothMovement()
        {
            Vector2 stick = Vector2.zero;
            if (leftHand != null)
            {
                stick = leftHand.ThumbstickAxis;
            }

            if (stick.sqrMagnitude > 0.04f)
            {
                // Move relative to HMD yaw
                Vector3 forward = headCameraTransform.forward;
                forward.y = 0f;
                forward.Normalize();

                Vector3 right = headCameraTransform.right;
                right.y = 0f;
                right.Normalize();

                Vector3 moveDir = forward * stick.y + right * stick.x;
                Vector3 newPos = xrOriginTransform.position + moveDir * (moveSpeed * Time.deltaTime);

                // Clamp to garage workshop bounds
                newPos.x = Mathf.Clamp(newPos.x, roomBoundsX.x, roomBoundsX.y);
                newPos.z = Mathf.Clamp(newPos.z, roomBoundsZ.x, roomBoundsZ.y);
                newPos.y = 0f;

                xrOriginTransform.position = newPos;
            }
        }

        private void HandleSnapTurn()
        {
            float turnX = 0f;
            if (rightHand != null)
            {
                turnX = rightHand.ThumbstickAxis.x;
            }

            if (Mathf.Abs(turnX) > snapThreshold)
            {
                if (!hasSnapTurned)
                {
                    float angle = Mathf.Sign(turnX) * snapAngle;
                    xrOriginTransform.RotateAround(headCameraTransform.position, Vector3.up, angle);
                    hasSnapTurned = true;
                }
            }
            else if (Mathf.Abs(turnX) < 0.2f)
            {
                hasSnapTurned = false;
            }
        }

        private void HandleTeleportation()
        {
            bool aimRequested = false;
            Transform aimOrigin = null;

            if (leftHand != null && leftHand.IsGripPressed)
            {
                aimRequested = true;
                aimOrigin = leftHand.transform;
            }
            else if (rightHand != null && rightHand.IsGripPressed)
            {
                aimRequested = true;
                aimOrigin = rightHand.transform;
            }

            if (aimRequested && aimOrigin != null)
            {
                isTeleportAiming = true;
                UpdateTeleportArc(aimOrigin);
            }
            else
            {
                if (isTeleportAiming)
                {
                    ExecuteTeleport();
                    isTeleportAiming = false;
                }
                teleportLine.enabled = false;
                teleportMarker.SetActive(false);
            }
        }

        private void UpdateTeleportArc(Transform origin)
        {
            teleportLine.enabled = true;
            Vector3 startPos = origin.position;
            Vector3 velocity = origin.forward * arcVelocity + Vector3.up * 1.5f;

            Vector3 currentPos = startPos;
            float timeStep = 0.05f;
            bool hitValid = false;

            teleportLine.positionCount = arcSegments;
            teleportLine.SetPosition(0, currentPos);

            for (int i = 1; i < arcSegments; i++)
            {
                Vector3 nextPos = currentPos + velocity * timeStep + Physics.gravity * (0.5f * timeStep * timeStep);
                velocity += Physics.gravity * timeStep;

                RaycastHit hit;
                if (Physics.Linecast(currentPos, nextPos, out hit))
                {
                    // Check if hit floor
                    if (hit.collider.name.Contains("Floor") || hit.point.y < 0.2f)
                    {
                        hitValid = true;
                        currentTeleportTarget = hit.point;
                        currentTeleportTarget.x = Mathf.Clamp(currentTeleportTarget.x, roomBoundsX.x, roomBoundsX.y);
                        currentTeleportTarget.z = Mathf.Clamp(currentTeleportTarget.z, roomBoundsZ.x, roomBoundsZ.y);
                        currentTeleportTarget.y = 0f;

                        teleportLine.SetPosition(i, hit.point);
                        teleportLine.positionCount = i + 1;
                        break;
                    }
                }

                currentPos = nextPos;
                teleportLine.SetPosition(i, currentPos);
            }

            if (hitValid)
            {
                teleportMarker.SetActive(true);
                teleportMarker.transform.position = currentTeleportTarget + Vector3.up * 0.01f;
            }
            else
            {
                teleportMarker.SetActive(false);
            }
        }

        private void ExecuteTeleport()
        {
            if (teleportMarker.activeSelf)
            {
                // Calculate camera offset relative to origin so user lands precisely
                Vector3 cameraOffset = headCameraTransform.position - xrOriginTransform.position;
                cameraOffset.y = 0f;

                xrOriginTransform.position = currentTeleportTarget - cameraOffset;
            }
        }
    }
}
