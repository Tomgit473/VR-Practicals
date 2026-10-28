using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace TutorialInfo
{
    /// <summary>
    /// VR Laser Pointer that projects from the hand controller.
    /// Can point and click UI buttons on the garage wall, interact with the car, and control the TV screen.
    /// </summary>
    [RequireComponent(typeof(VRHandController))]
    public class VRLaserPointer : MonoBehaviour
    {
        [Header("Laser Configuration")]
        [SerializeField] private float maxDistance = 14f;
        [SerializeField] private float laserWidth = 0.007f;
        [SerializeField] private Material laserMaterial;
        [SerializeField] private Material reticleMaterial;

        [Header("Colors")]
        [SerializeField] private Color defaultColor = new Color(0.2f, 0.85f, 1.0f, 0.8f);
        [SerializeField] private Color uiHoverColor = new Color(0.3f, 1.0f, 0.45f, 0.95f);
        [SerializeField] private Color carHoverColor = new Color(1.0f, 0.65f, 0.15f, 0.95f);
        [SerializeField] private Color tvHoverColor = new Color(0.9f, 0.35f, 1.0f, 0.95f);

        private VRHandController handController;
        private LineRenderer lineRenderer;
        private GameObject reticle;
        private bool wasTriggerPressedLastFrame;

        // Currently hovered objects
        private Button hoveredButton;
        private Color originalButtonColor;
        private Image hoveredButtonImage;

        private void Awake()
        {
            handController = GetComponent<VRHandController>();
            SetupLaser();
        }

        private void SetupLaser()
        {
            // Setup LineRenderer
            lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = laserWidth;
            lineRenderer.endWidth = laserWidth * 0.4f;
            lineRenderer.positionCount = 2;

            if (laserMaterial == null)
            {
                laserMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/TutorialInfo/Materials/Mat_VRLaser.mat");
            }
            if (laserMaterial != null)
            {
                lineRenderer.material = laserMaterial;
            }

            // Setup Reticle Sphere
            reticle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            reticle.name = "VR_Laser_Reticle";
            reticle.transform.SetParent(transform, false);
            reticle.transform.localScale = Vector3.one * 0.038f;
            var col = reticle.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);

            if (reticleMaterial == null)
            {
                reticleMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/TutorialInfo/Materials/Mat_VRReticle.mat");
            }
            if (reticleMaterial != null)
            {
                reticle.GetComponent<Renderer>().sharedMaterial = reticleMaterial;
            }
        }

        private void Update()
        {
            Vector3 rayOrigin = transform.position;
            Vector3 rayDir = transform.forward;

            Ray ray = new Ray(rayOrigin, rayDir);
            RaycastHit hit;
            bool hasHit = Physics.Raycast(ray, out hit, maxDistance);

            Vector3 hitPoint = hasHit ? hit.point : rayOrigin + rayDir * maxDistance;

            // Update Laser line
            lineRenderer.SetPosition(0, rayOrigin);
            lineRenderer.SetPosition(1, hitPoint);

            // Update Reticle
            if (reticle != null)
            {
                reticle.SetActive(hasHit);
                if (hasHit)
                {
                    reticle.transform.position = hitPoint - rayDir * 0.01f;
                    reticle.transform.rotation = Quaternion.LookRotation(hit.normal);
                }
            }

            // Process Interactions
            Color activeLaserColor = defaultColor;
            Button currentButton = null;

            if (hasHit)
            {
                // 1. Check UI Button hit
                currentButton = hit.collider.GetComponent<Button>();
                if (currentButton == null)
                {
                    currentButton = hit.collider.GetComponentInParent<Button>();
                }

                if (currentButton != null)
                {
                    activeLaserColor = uiHoverColor;
                    HandleButtonHover(currentButton);

                    if (handController.IsTriggerPressed && !wasTriggerPressedLastFrame)
                    {
                        ClickButton(currentButton);
                    }
                }
                // 2. Check Car hit
                else if (hit.collider.name.Contains("Car") || hit.collider.transform.root.name.Contains("Car"))
                {
                    activeLaserColor = carHoverColor;
                    ClearButtonHover();

                    if (handController.IsTriggerPressed && !wasTriggerPressedLastFrame)
                    {
                        var customizer = Object.FindObjectOfType<GarageCarCustomizer>();
                        if (customizer != null)
                        {
                            customizer.ChangeColor();
                        }
                    }
                }
                // 3. Check TV Screen hit
                else if (hit.collider.name.Contains("TV") || hit.collider.name.Contains("Screen"))
                {
                    activeLaserColor = tvHoverColor;
                    ClearButtonHover();

                    if (handController.IsTriggerPressed && !wasTriggerPressedLastFrame)
                    {
                        var vp = Object.FindObjectOfType<VideoPlayer>();
                        if (vp != null)
                        {
                            if (vp.isPlaying) vp.Pause();
                            else vp.Play();
                        }
                    }
                }
                else
                {
                    ClearButtonHover();
                }
            }
            else
            {
                ClearButtonHover();
            }

            // Update Laser Color
            lineRenderer.startColor = activeLaserColor;
            lineRenderer.endColor = new Color(activeLaserColor.r, activeLaserColor.g, activeLaserColor.b, 0.2f);

            wasTriggerPressedLastFrame = handController.IsTriggerPressed;
        }

        private void HandleButtonHover(Button btn)
        {
            if (hoveredButton != btn)
            {
                ClearButtonHover();
                hoveredButton = btn;
                hoveredButtonImage = btn.GetComponent<Image>();
                if (hoveredButtonImage != null)
                {
                    originalButtonColor = hoveredButtonImage.color;
                    hoveredButtonImage.color = Color.Lerp(originalButtonColor, Color.white, 0.45f);
                }
            }
        }

        private void ClearButtonHover()
        {
            if (hoveredButton != null)
            {
                if (hoveredButtonImage != null)
                {
                    hoveredButtonImage.color = originalButtonColor;
                }
                hoveredButton = null;
                hoveredButtonImage = null;
            }
        }

        private void ClickButton(Button btn)
        {
            btn.onClick.Invoke();

            // Visual click flash
            if (hoveredButtonImage != null)
            {
                hoveredButtonImage.color = Color.white;
            }
        }

        private void OnDisable()
        {
            ClearButtonHover();
            if (reticle != null) reticle.SetActive(false);
            if (lineRenderer != null) lineRenderer.enabled = false;
        }

        private void OnEnable()
        {
            if (lineRenderer != null) lineRenderer.enabled = true;
        }
    }
}
