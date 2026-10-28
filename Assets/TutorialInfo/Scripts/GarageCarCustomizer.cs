using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TutorialInfo
{
    /// <summary>
    /// Handles interactive car customization in the mini garage workshop.
    /// Provides methods for cycling colors, textures, and resetting to defaults.
    /// </summary>
    public class GarageCarCustomizer : MonoBehaviour
    {
        [Header("Target Car Mesh")]
        [SerializeField] private Renderer carBodyRenderer;
        [SerializeField] private Transform carTransform;

        [Header("Color Palette (Bright & Colorful)")]
        [SerializeField] private Color[] colorPalette = new Color[]
        {
            new Color(0.00f, 0.72f, 1.00f), // Electric Cyan (Default)
            new Color(1.00f, 0.42f, 0.15f), // Sunset Orange
            new Color(1.00f, 0.82f, 0.20f), // Cyber Yellow
            new Color(0.12f, 0.85f, 0.50f), // Neon Mint
            new Color(0.95f, 0.20f, 0.40f), // Hot Magenta
            new Color(0.55f, 0.25f, 0.95f), // Purple Spark
            new Color(0.95f, 0.95f, 0.98f), // Gloss White
            new Color(0.18f, 0.20f, 0.24f)  // Sleek Charcoal
        };

        [Header("Texture Patterns")]
        [SerializeField] private Texture2D[] texturePalette;

        [Header("UI Buttons (Optional direct hookup)")]
        [SerializeField] private Button changeColorButton;
        [SerializeField] private Button changeTextureButton;
        [SerializeField] private Button resetButton;

        private int currentColorIndex = 0;
        private int currentTextureIndex = 0;
        private Color defaultColor;
        private Texture defaultTexture;
        private Material carMaterialInstance;
        private Coroutine punchCoroutine;

        private void Awake()
        {
            EnsureMaterial();
            if (carTransform == null)
            {
                carTransform = transform;
            }
        }

        private void EnsureMaterial()
        {
            if (carMaterialInstance == null && carBodyRenderer != null)
            {
                carMaterialInstance = Application.isPlaying ? carBodyRenderer.material : carBodyRenderer.sharedMaterial;
                if (defaultColor == default(Color))
                {
                    defaultColor = carMaterialInstance.color;
                    defaultTexture = carMaterialInstance.mainTexture;
                }
            }
        }

        private void Start()
        {
            // Wire UI buttons if assigned
            if (changeColorButton != null)
                changeColorButton.onClick.AddListener(ChangeColor);

            if (changeTextureButton != null)
                changeTextureButton.onClick.AddListener(ChangeTexture);

            if (resetButton != null)
                resetButton.onClick.AddListener(ResetCar);
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                if (kb.cKey.wasPressedThisFrame || kb.digit1Key.wasPressedThisFrame)
                {
                    ChangeColor();
                }
                if (kb.tKey.wasPressedThisFrame || kb.digit2Key.wasPressedThisFrame)
                {
                    ChangeTexture();
                }
                if (kb.rKey.wasPressedThisFrame || kb.digit3Key.wasPressedThisFrame)
                {
                    ResetCar();
                }
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Alpha1))
            {
                ChangeColor();
            }
            if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Alpha2))
            {
                ChangeTexture();
            }
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Alpha3))
            {
                ResetCar();
            }
#endif
        }

        /// <summary>
        /// Cycles to the next vibrant color in the palette.
        /// </summary>
        public void ChangeColor()
        {
            EnsureMaterial();
            if (colorPalette == null || colorPalette.Length == 0 || carMaterialInstance == null) return;

            currentColorIndex = (currentColorIndex + 1) % colorPalette.Length;
            Color nextColor = colorPalette[currentColorIndex];
            carMaterialInstance.color = nextColor;
            if (carMaterialInstance.HasProperty("_BaseColor"))
            {
                carMaterialInstance.SetColor("_BaseColor", nextColor);
            }

            TriggerJuice();
            Debug.Log($"[Garage] Car Color changed to: {nextColor} (Index {currentColorIndex})");
        }

        /// <summary>
        /// Cycles through available stylized textures / decals.
        /// </summary>
        public void ChangeTexture()
        {
            EnsureMaterial();
            if (texturePalette == null || texturePalette.Length == 0 || carMaterialInstance == null) return;

            currentTextureIndex = (currentTextureIndex + 1) % (texturePalette.Length + 1);

            Texture nextTex = null;
            if (currentTextureIndex > 0 && currentTextureIndex <= texturePalette.Length)
            {
                nextTex = texturePalette[currentTextureIndex - 1];
            }

            carMaterialInstance.mainTexture = nextTex;
            if (carMaterialInstance.HasProperty("_BaseMap"))
            {
                carMaterialInstance.SetTexture("_BaseMap", nextTex);
            }

            TriggerJuice();
            string texName = nextTex != null ? nextTex.name : "Plain Gloss (None)";
            Debug.Log($"[Garage] Car Texture changed to: {texName} (Index {currentTextureIndex})");
        }

        /// <summary>
        /// Resets the car body to its initial color and texture.
        /// </summary>
        public void ResetCar()
        {
            EnsureMaterial();
            if (carMaterialInstance == null) return;

            currentColorIndex = 0;
            currentTextureIndex = 0;

            carMaterialInstance.color = defaultColor;
            if (carMaterialInstance.HasProperty("_BaseColor"))
            {
                carMaterialInstance.SetColor("_BaseColor", defaultColor);
            }

            carMaterialInstance.mainTexture = defaultTexture;
            if (carMaterialInstance.HasProperty("_BaseMap"))
            {
                carMaterialInstance.SetTexture("_BaseMap", defaultTexture);
            }

            TriggerJuice();
            Debug.Log("[Garage] Car Reset to Default Style.");
        }

        private void TriggerJuice()
        {
            if (carTransform != null)
            {
                if (punchCoroutine != null) StopCoroutine(punchCoroutine);
                punchCoroutine = StartCoroutine(JuicePunch());
            }
        }

        private IEnumerator JuicePunch()
        {
            Vector3 startScale = Vector3.one;
            Vector3 punchScale = new Vector3(1.06f, 1.08f, 1.06f);
            float duration = 0.15f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                carTransform.localScale = Vector3.Lerp(punchScale, startScale, t);
                yield return null;
            }
            carTransform.localScale = startScale;
        }
    }
}
