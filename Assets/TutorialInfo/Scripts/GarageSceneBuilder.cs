using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor;

namespace TutorialInfo
{
    public static class GarageSceneBuilder
    {
        [MenuItem("Tools/Build Mini Garage Scene")]
        public static void BuildScene()
        {
            // 1. Cleanup old garage objects if they exist
            string[] oldRoots = new string[] {
                "--- GARAGE WORKSHOP ---",
                "Garage_Environment",
                "LeftWall_Screen",
                "RightWall_UI",
                "Car_Root",
                "Workbench_Root",
                "Studio_Lighting",
                "EventSystem"
            };

            foreach (var name in oldRoots)
            {
                var existing = GameObject.Find(name);
                if (existing != null)
                {
                    Undo.DestroyObjectImmediate(existing);
                }
            }

            // Root container
            GameObject root = new GameObject("--- GARAGE WORKSHOP ---");
            Undo.RegisterCreatedObjectUndo(root, "Build Garage Scene");

            // Load Materials
            Material matFloor = AssetDatabase.LoadAssetAtPath<Material>("Assets/TutorialInfo/Materials/Mat_ConcreteFloor.mat");
            Material matCar = AssetDatabase.LoadAssetAtPath<Material>("Assets/TutorialInfo/Materials/Mat_CarBody.mat");
            Material matWheel = AssetDatabase.LoadAssetAtPath<Material>("Assets/TutorialInfo/Materials/Mat_CarWheel.mat");
            Material matWood = AssetDatabase.LoadAssetAtPath<Material>("Assets/TutorialInfo/Materials/Mat_WoodBench.mat");
            Material matToolRed = AssetDatabase.LoadAssetAtPath<Material>("Assets/TutorialInfo/Materials/Mat_ToolRed.mat");
            Material matToolBlue = AssetDatabase.LoadAssetAtPath<Material>("Assets/TutorialInfo/Materials/Mat_ToolBlue.mat");
            Material matWall = AssetDatabase.LoadAssetAtPath<Material>("Assets/TutorialInfo/Materials/Mat_Wall.mat");
            Material matTrim = AssetDatabase.LoadAssetAtPath<Material>("Assets/TutorialInfo/Materials/Mat_Trim.mat");
            Material matTVFrame = AssetDatabase.LoadAssetAtPath<Material>("Assets/TutorialInfo/Materials/Mat_TVFrame.mat");
            Material matTVScreen = AssetDatabase.LoadAssetAtPath<Material>("Assets/TutorialInfo/Materials/Mat_TVScreen.mat");

            // Textures for car customization
            Texture2D texStripes = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TutorialInfo/Texture_CarStripes.png");
            Texture2D texChecker = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TutorialInfo/Texture_CarChecker.png");
            Texture2D texTech = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TutorialInfo/Texture_CarTech.png");
            VideoClip videoClip = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/TutorialInfo/CarVideo.mp4");
            RenderTexture rtScreen = AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/TutorialInfo/RT_CarVideoScreen.renderTexture");

            // ==========================================
            // 1. ENVIRONMENT (Floor & Walls)
            // ==========================================
            GameObject envRoot = new GameObject("Garage_Environment");
            envRoot.transform.SetParent(root.transform);

            // Large flat plane floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Garage_Floor_Concrete";
            floor.transform.SetParent(envRoot.transform);
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(1.7f, 1f, 1.7f);
            if (matFloor != null) floor.GetComponent<Renderer>().sharedMaterial = matFloor;

            // Back Wall
            GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backWall.name = "Wall_Back";
            backWall.transform.SetParent(envRoot.transform);
            backWall.transform.position = new Vector3(0f, 3.2f, 7.8f);
            backWall.transform.localScale = new Vector3(15.5f, 6.5f, 0.4f);
            if (matWall != null) backWall.GetComponent<Renderer>().sharedMaterial = matWall;

            // Back Wall Baseboard
            GameObject backTrim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backTrim.name = "Trim_Back_Baseboard";
            backTrim.transform.SetParent(envRoot.transform);
            backTrim.transform.position = new Vector3(0f, 0.25f, 7.55f);
            backTrim.transform.localScale = new Vector3(15.5f, 0.5f, 0.15f);
            if (matTrim != null) backTrim.GetComponent<Renderer>().sharedMaterial = matTrim;

            // Back Wall Accent Stripe
            GameObject backStripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backStripe.name = "Trim_Back_Stripe";
            backStripe.transform.SetParent(envRoot.transform);
            backStripe.transform.position = new Vector3(0f, 5.8f, 7.55f);
            backStripe.transform.localScale = new Vector3(15.5f, 0.35f, 0.15f);
            if (matTrim != null) backStripe.GetComponent<Renderer>().sharedMaterial = matTrim;

            // Left Wall
            GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.name = "Wall_Left";
            leftWall.transform.SetParent(envRoot.transform);
            leftWall.transform.position = new Vector3(-7.5f, 3.2f, 0f);
            leftWall.transform.localScale = new Vector3(0.4f, 6.5f, 16f);
            if (matWall != null) leftWall.GetComponent<Renderer>().sharedMaterial = matWall;

            // Left Wall Baseboard
            GameObject leftTrim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftTrim.name = "Trim_Left_Baseboard";
            leftTrim.transform.SetParent(envRoot.transform);
            leftTrim.transform.position = new Vector3(-7.25f, 0.25f, 0f);
            leftTrim.transform.localScale = new Vector3(0.15f, 0.5f, 16f);
            if (matTrim != null) leftTrim.GetComponent<Renderer>().sharedMaterial = matTrim;

            // Right Wall
            GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.name = "Wall_Right";
            rightWall.transform.SetParent(envRoot.transform);
            rightWall.transform.position = new Vector3(7.5f, 3.2f, 0f);
            rightWall.transform.localScale = new Vector3(0.4f, 6.5f, 16f);
            if (matWall != null) rightWall.GetComponent<Renderer>().sharedMaterial = matWall;

            // Right Wall Baseboard
            GameObject rightTrim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightTrim.name = "Trim_Right_Baseboard";
            rightTrim.transform.SetParent(envRoot.transform);
            rightTrim.transform.position = new Vector3(7.25f, 0.25f, 0f);
            rightTrim.transform.localScale = new Vector3(0.15f, 0.5f, 16f);
            if (matTrim != null) rightTrim.GetComponent<Renderer>().sharedMaterial = matTrim;

            // Overhead stylized ceiling beams
            for (int b = -1; b <= 1; b++)
            {
                GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
                beam.name = $"Ceiling_Beam_{b + 2}";
                beam.transform.SetParent(envRoot.transform);
                beam.transform.position = new Vector3(0f, 6.2f, b * 4.5f + 1f);
                beam.transform.localScale = new Vector3(15.5f, 0.3f, 0.3f);
                if (matTrim != null) beam.GetComponent<Renderer>().sharedMaterial = matTrim;
            }

            // ==========================================
            // 2. LEFT WALL: FLAT SCREEN PLAYING CAR VIDEO
            // ==========================================
            // "On the left wall there is a flat screen (quad) playing a car video."
            GameObject screenRoot = new GameObject("LeftWall_Screen");
            screenRoot.transform.SetParent(root.transform);
            screenRoot.transform.position = new Vector3(-7.2f, 3.2f, 0.2f);
            // Rotated so +Z faces into room (+X)
            screenRoot.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            // Sleek TV Mounting Frame
            GameObject tvFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tvFrame.name = "TV_Bezel_Frame";
            tvFrame.transform.SetParent(screenRoot.transform, false);
            tvFrame.transform.localPosition = new Vector3(0f, 0f, 0f);
            tvFrame.transform.localScale = new Vector3(5.2f, 2.5f, 0.12f);
            if (matTVFrame != null) tvFrame.GetComponent<Renderer>().sharedMaterial = matTVFrame;

            // Flat Screen Quad (Rotated 180 so normal points into the room)
            GameObject tvQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            tvQuad.name = "TV_Screen_Quad";
            tvQuad.transform.SetParent(screenRoot.transform, false);
            tvQuad.transform.localPosition = new Vector3(0f, 0f, 0.07f);
            tvQuad.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            tvQuad.transform.localScale = new Vector3(4.9f, 2.2f, 1f);
            if (matTVScreen != null) tvQuad.GetComponent<Renderer>().sharedMaterial = matTVScreen;

            // Add VideoPlayer component to Quad
            var videoPlayer = tvQuad.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = true;
            videoPlayer.isLooping = true;
            videoPlayer.clip = videoClip;
            videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            videoPlayer.targetMaterialRenderer = tvQuad.GetComponent<Renderer>();
            videoPlayer.targetMaterialProperty = "_BaseMap";
            videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

            // ==========================================
            // 3. CENTER: SMALL CAR MADE FROM BASIC SHAPES
            // ==========================================
            // "In the center there is a small car made from basic shapes: a rectangular cube as the car body and four spheres as wheels. 
            // The car is slightly raised above a large flat plane that acts as the garage floor with a light grey concrete texture."
            GameObject carRoot = new GameObject("Car_Root");
            carRoot.transform.SetParent(root.transform);
            carRoot.transform.position = new Vector3(-0.3f, 0f, 0.3f);
            // Angled front-quarter towards camera
            carRoot.transform.rotation = Quaternion.Euler(0f, 150f, 0f);

            // Car Body: Rectangular Cube raised above floor
            GameObject carBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            carBody.name = "Car_Body_RectangularCube";
            carBody.transform.SetParent(carRoot.transform, false);
            carBody.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            carBody.transform.localScale = new Vector3(1.85f, 0.75f, 3.4f);
            if (matCar != null) carBody.GetComponent<Renderer>().sharedMaterial = matCar;

            // Stylized Cabin/Windshield Cube on top of body
            GameObject carCabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            carCabin.name = "Car_Cabin_Cube";
            carCabin.transform.SetParent(carRoot.transform, false);
            carCabin.transform.localPosition = new Vector3(0f, 1.45f, -0.2f);
            carCabin.transform.localScale = new Vector3(1.48f, 0.65f, 1.8f);
            if (matTrim != null) carCabin.GetComponent<Renderer>().sharedMaterial = matTrim;

            // Four Spheres as Wheels
            GameObject wheelsRoot = new GameObject("Car_Wheels_Spheres");
            wheelsRoot.transform.SetParent(carRoot.transform, false);

            Vector3[] wheelOffsets = new Vector3[]
            {
                new Vector3(-1.02f, 0.45f, 1.05f),  // Front Left
                new Vector3( 1.02f, 0.45f, 1.05f),  // Front Right
                new Vector3(-1.02f, 0.45f, -1.05f), // Rear Left
                new Vector3( 1.02f, 0.45f, -1.05f)  // Rear Right
            };
            string[] wheelNames = new string[] { "Wheel_Sphere_FL", "Wheel_Sphere_FR", "Wheel_Sphere_RL", "Wheel_Sphere_RR" };

            for (int w = 0; w < 4; w++)
            {
                GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                wheel.name = wheelNames[w];
                wheel.transform.SetParent(wheelsRoot.transform, false);
                wheel.transform.localPosition = wheelOffsets[w];
                wheel.transform.localScale = new Vector3(0.42f, 0.9f, 0.9f);
                if (matWheel != null) wheel.GetComponent<Renderer>().sharedMaterial = matWheel;
            }

            // Headlights & Taillights (Basic cube accents)
            for (int s = -1; s <= 1; s += 2)
            {
                // Front Headlight (Warm yellow cube)
                GameObject headlight = GameObject.CreatePrimitive(PrimitiveType.Cube);
                headlight.name = $"Headlight_{(s < 0 ? "L" : "R")}";
                headlight.transform.SetParent(carRoot.transform, false);
                headlight.transform.localPosition = new Vector3(s * 0.65f, 0.85f, 1.72f);
                headlight.transform.localScale = new Vector3(0.35f, 0.22f, 0.1f);
                var matHead = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                matHead.color = new Color(1f, 0.95f, 0.6f);
                headlight.GetComponent<Renderer>().sharedMaterial = matHead;

                // Rear Taillight (Red cube)
                GameObject taillight = GameObject.CreatePrimitive(PrimitiveType.Cube);
                taillight.name = $"Taillight_{(s < 0 ? "L" : "R")}";
                taillight.transform.SetParent(carRoot.transform, false);
                taillight.transform.localPosition = new Vector3(s * 0.65f, 0.85f, -1.72f);
                taillight.transform.localScale = new Vector3(0.35f, 0.22f, 0.1f);
                var matTail = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                matTail.color = new Color(1f, 0.15f, 0.15f);
                taillight.GetComponent<Renderer>().sharedMaterial = matTail;
            }

            // Add Customizer Component to Car
            var customizer = carRoot.AddComponent<GarageCarCustomizer>();
            var customizerSo = new SerializedObject(customizer);
            customizerSo.FindProperty("carBodyRenderer").objectReferenceValue = carBody.GetComponent<Renderer>();
            customizerSo.FindProperty("carTransform").objectReferenceValue = carRoot.transform;

            var texPaletteProp = customizerSo.FindProperty("texturePalette");
            texPaletteProp.arraySize = 3;
            texPaletteProp.GetArrayElementAtIndex(0).objectReferenceValue = texStripes;
            texPaletteProp.GetArrayElementAtIndex(1).objectReferenceValue = texChecker;
            texPaletteProp.GetArrayElementAtIndex(2).objectReferenceValue = texTech;
            customizerSo.ApplyModifiedProperties();

            // ==========================================
            // 4. WOODEN WORKBENCH NEAR CAR WITH TWO SMALL TOOL CUBES
            // ==========================================
            // "A wooden workbench (cube) is placed near the car with two small tool cubes on it."
            GameObject benchRoot = new GameObject("Workbench_Root");
            benchRoot.transform.SetParent(root.transform);
            benchRoot.transform.position = new Vector3(3.2f, 0f, 0.3f);
            benchRoot.transform.rotation = Quaternion.Euler(0f, -22f, 0f);

            // Wooden Workbench Cube
            GameObject benchMain = GameObject.CreatePrimitive(PrimitiveType.Cube);
            benchMain.name = "Workbench_Wood_Cube";
            benchMain.transform.SetParent(benchRoot.transform, false);
            benchMain.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            benchMain.transform.localScale = new Vector3(2.4f, 1.3f, 1.3f);
            if (matWood != null) benchMain.GetComponent<Renderer>().sharedMaterial = matWood;

            // Workbench tabletop overhang
            GameObject benchTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            benchTop.name = "Workbench_Top_Slab";
            benchTop.transform.SetParent(benchRoot.transform, false);
            benchTop.transform.localPosition = new Vector3(0f, 1.33f, 0f);
            benchTop.transform.localScale = new Vector3(2.6f, 0.12f, 1.45f);
            if (matWood != null) benchTop.GetComponent<Renderer>().sharedMaterial = matWood;

            // Tool Cube 1: Red Tool Box Cube on bench
            GameObject toolCube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            toolCube1.name = "Tool_Cube_Red";
            toolCube1.transform.SetParent(benchRoot.transform, false);
            toolCube1.transform.localPosition = new Vector3(-0.55f, 1.62f, 0.05f);
            toolCube1.transform.localRotation = Quaternion.Euler(0f, 12f, 0f);
            toolCube1.transform.localScale = new Vector3(0.55f, 0.44f, 0.42f);
            if (matToolRed != null) toolCube1.GetComponent<Renderer>().sharedMaterial = matToolRed;

            // Handle on Red Tool Cube
            GameObject tool1Handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tool1Handle.name = "Tool_Red_Handle";
            tool1Handle.transform.SetParent(toolCube1.transform, false);
            tool1Handle.transform.localPosition = new Vector3(0f, 0.58f, 0f);
            tool1Handle.transform.localScale = new Vector3(0.55f, 0.22f, 0.18f);
            if (matTrim != null) tool1Handle.GetComponent<Renderer>().sharedMaterial = matTrim;

            // Tool Cube 2: Small Blue/Steel Tool Cube on bench
            GameObject toolCube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            toolCube2.name = "Tool_Cube_Blue";
            toolCube2.transform.SetParent(benchRoot.transform, false);
            toolCube2.transform.localPosition = new Vector3(0.42f, 1.55f, -0.05f);
            toolCube2.transform.localRotation = Quaternion.Euler(0f, -24f, 0f);
            toolCube2.transform.localScale = new Vector3(0.42f, 0.32f, 0.32f);
            if (matToolBlue != null) toolCube2.GetComponent<Renderer>().sharedMaterial = matToolBlue;

            // Small accent tool bar on bench
            GameObject miniTool = GameObject.CreatePrimitive(PrimitiveType.Cube);
            miniTool.name = "Tool_Wrench_Bar";
            miniTool.transform.SetParent(benchRoot.transform, false);
            miniTool.transform.localPosition = new Vector3(0.85f, 1.42f, 0.15f);
            miniTool.transform.localRotation = Quaternion.Euler(0f, 35f, 0f);
            miniTool.transform.localScale = new Vector3(0.48f, 0.05f, 0.12f);
            if (matTrim != null) miniTool.GetComponent<Renderer>().sharedMaterial = matTrim;

            // ==========================================
            // 5. RIGHT WALL: THREE SIMPLE UI BUTTONS
            // ==========================================
            // "On the right wall there are three simple UI buttons labeled “Change Color”, “Change Texture” and “Reset”."
            GameObject uiRoot = new GameObject("RightWall_UI");
            uiRoot.transform.SetParent(root.transform);
            uiRoot.transform.position = new Vector3(7.2f, 3.2f, 0.2f);
            // Face into the garage workshop towards -X
            uiRoot.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

            // Backboard mount plate
            GameObject uiBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            uiBoard.name = "UI_Mount_Board";
            uiBoard.transform.SetParent(uiRoot.transform, false);
            uiBoard.transform.localPosition = new Vector3(0f, 0f, 0f);
            uiBoard.transform.localScale = new Vector3(3.4f, 4.2f, 0.12f);
            if (matTrim != null) uiBoard.GetComponent<Renderer>().sharedMaterial = matTrim;

            // World Space Canvas
            GameObject canvasGo = new GameObject("UI_WorldCanvas");
            canvasGo.transform.SetParent(uiRoot.transform, false);
            canvasGo.transform.localPosition = new Vector3(0f, 0f, -0.07f);
            // Face forward into the room
            canvasGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            canvasGo.transform.localScale = new Vector3(0.0065f, 0.0065f, 0.0065f);

            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            RectTransform canvasRt = canvasGo.GetComponent<RectTransform>();
            canvasRt.sizeDelta = new Vector2(480f, 600f);

            // Font
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Header Title
            GameObject titleGo = new GameObject("Text_Title");
            titleGo.transform.SetParent(canvasGo.transform, false);
            RectTransform titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0f, 220f);
            titleRt.sizeDelta = new Vector2(440f, 60f);
            Text titleText = titleGo.AddComponent<Text>();
            titleText.text = "GARAGE WORKSHOP";
            titleText.font = font;
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(0.95f, 0.95f, 0.98f);

            // Subtitle
            GameObject subGo = new GameObject("Text_Subtitle");
            subGo.transform.SetParent(canvasGo.transform, false);
            RectTransform subRt = subGo.AddComponent<RectTransform>();
            subRt.anchoredPosition = new Vector2(0f, 175f);
            subRt.sizeDelta = new Vector2(440f, 40f);
            Text subText = subGo.AddComponent<Text>();
            subText.text = "CAR CUSTOMIZER";
            subText.font = font;
            subText.fontSize = 20;
            subText.fontStyle = FontStyle.Bold;
            subText.alignment = TextAnchor.MiddleCenter;
            subText.color = new Color(0.2f, 0.8f, 1f);

            // Create 3 Buttons
            Button btnColor = CreateUIButton("Button_ChangeColor", canvasGo.transform, new Vector2(0f, 70f), "Change Color", new Color(0.05f, 0.65f, 0.95f), font);
            Button btnTexture = CreateUIButton("Button_ChangeTexture", canvasGo.transform, new Vector2(0f, -35f), "Change Texture", new Color(0.98f, 0.52f, 0.12f), font);
            Button btnReset = CreateUIButton("Button_Reset", canvasGo.transform, new Vector2(0f, -140f), "Reset", new Color(0.90f, 0.22f, 0.28f), font);

            // Connect Buttons to Customizer
            btnColor.onClick.AddListener(customizer.ChangeColor);
            btnTexture.onClick.AddListener(customizer.ChangeTexture);
            btnReset.onClick.AddListener(customizer.ResetCar);

            // Also serialize button links into GarageCarCustomizer
            var cSo = new SerializedObject(customizer);
            cSo.FindProperty("changeColorButton").objectReferenceValue = btnColor;
            cSo.FindProperty("changeTextureButton").objectReferenceValue = btnTexture;
            cSo.FindProperty("resetButton").objectReferenceValue = btnReset;
            cSo.ApplyModifiedProperties();

            // EventSystem with new Input System support
            var es = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                es = esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            }

            var legacy = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (legacy != null)
            {
                Object.DestroyImmediate(legacy);
            }

            if (es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
            {
                es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // ==========================================
            // 6. SOFT STUDIO LIGHTING
            // ==========================================
            // "Soft studio lighting, clean materials, simple textures, modern game art style, no realism, easy to recreate in Unity."
            GameObject lightRoot = new GameObject("Studio_Lighting");
            lightRoot.transform.SetParent(root.transform);

            // Re-purpose or create Key Light
            Light keyLight = null;
            var existingLights = Object.FindObjectsOfType<Light>();
            foreach (var l in existingLights)
            {
                if (l.type == LightType.Directional)
                {
                    keyLight = l;
                    break;
                }
            }

            if (keyLight == null)
            {
                var klGo = new GameObject("Studio_Key_Light");
                keyLight = klGo.AddComponent<Light>();
                keyLight.type = LightType.Directional;
            }
            keyLight.name = "Studio_Directional_Key";
            keyLight.transform.SetParent(lightRoot.transform);
            keyLight.transform.rotation = Quaternion.Euler(42f, -32f, 0f);
            keyLight.color = new Color(1.0f, 0.97f, 0.92f);
            keyLight.intensity = 1.0f;
            keyLight.shadows = LightShadows.Soft;
            keyLight.shadowStrength = 0.40f; // Soft, gentle studio shadows
            keyLight.shadowBias = 0.15f;
            keyLight.shadowNormalBias = 0.6f;

            // Fill Light (Soft bluish tone)
            GameObject fillGo = new GameObject("Studio_Directional_Fill");
            fillGo.transform.SetParent(lightRoot.transform);
            fillGo.transform.rotation = Quaternion.Euler(35f, 145f, 0f);
            Light fillLight = fillGo.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(0.78f, 0.88f, 1.0f);
            fillLight.intensity = 0.55f;
            fillLight.shadows = LightShadows.None;

            // Overhead Car Studio Softlight (Point light without shadow acne)
            GameObject carLightGo = new GameObject("Studio_Car_OverheadLight");
            carLightGo.transform.SetParent(lightRoot.transform);
            carLightGo.transform.position = new Vector3(-0.3f, 4.8f, 0.3f);
            Light carLight = carLightGo.AddComponent<Light>();
            carLight.type = LightType.Point;
            carLight.color = new Color(1.0f, 0.98f, 0.95f);
            carLight.intensity = 1.0f;
            carLight.range = 12f;
            carLight.shadows = LightShadows.None;

            // Overhead Workbench Highlight Light
            GameObject benchLightGo = new GameObject("Studio_Workbench_OverheadLight");
            benchLightGo.transform.SetParent(lightRoot.transform);
            benchLightGo.transform.position = new Vector3(3.2f, 4.2f, 0.3f);
            Light benchLight = benchLightGo.AddComponent<Light>();
            benchLight.type = LightType.Point;
            benchLight.color = new Color(1.0f, 0.94f, 0.82f);
            benchLight.intensity = 0.8f;
            benchLight.range = 8f;
            benchLight.shadows = LightShadows.None;

            // Environment ambient settings
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.85f, 0.88f, 0.93f);
            RenderSettings.ambientEquatorColor = new Color(0.76f, 0.80f, 0.84f);
            RenderSettings.ambientGroundColor = new Color(0.66f, 0.70f, 0.74f);

            // ==========================================
            // 7. CAMERA POSITIONING & FRAMING
            // ==========================================
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0.0f, 3.85f, -7.2f);
                cam.transform.rotation = Quaternion.Euler(17.5f, 0f, 0f);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.84f, 0.87f, 0.91f); // Studio soft background
                cam.fieldOfView = 53f;

                var urpCam = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                if (urpCam == null)
                {
                    urpCam = cam.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                }
                urpCam.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.FastApproximateAntialiasing;
                urpCam.renderPostProcessing = true;
                urpCam.requiresDepthTexture = true;
                urpCam.requiresColorTexture = true;

                var freeFly = cam.GetComponent<FreeFlyCamera>();
                if (freeFly == null)
                {
                    freeFly = cam.gameObject.AddComponent<FreeFlyCamera>();
                }

                // ==========================================
                // 8. VR RIG & OPENXR SETUP
                // ==========================================
                GameObject xrOriginGo = GameObject.Find("XR_Origin");
                if (xrOriginGo == null)
                {
                    xrOriginGo = new GameObject("XR_Origin");
                }
                xrOriginGo.transform.SetParent(root.transform);
                xrOriginGo.transform.position = new Vector3(0f, 0f, -5.2f);
                xrOriginGo.transform.rotation = Quaternion.identity;

                Transform cameraOffset = xrOriginGo.transform.Find("Camera Offset");
                if (cameraOffset == null)
                {
                    var offsetGo = new GameObject("Camera Offset");
                    cameraOffset = offsetGo.transform;
                    cameraOffset.SetParent(xrOriginGo.transform, false);
                }

                cam.transform.SetParent(cameraOffset, false);
                cam.transform.localPosition = new Vector3(0f, 1.6f, 0f);
                cam.transform.localRotation = Quaternion.identity;
                cam.nearClipPlane = 0.03f;

                var spPoseDriver = cam.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
                if (spPoseDriver == null)
                {
                    spPoseDriver = cam.gameObject.AddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
                }
                spPoseDriver.SetPoseSource(UnityEngine.SpatialTracking.TrackedPoseDriver.DeviceType.GenericXRDevice, UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.Center);
                spPoseDriver.trackingType = UnityEngine.SpatialTracking.TrackedPoseDriver.TrackingType.RotationAndPosition;
                spPoseDriver.updateType = UnityEngine.SpatialTracking.TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;

                // Left Controller
                Transform leftHand = cameraOffset.Find("LeftHand_Controller");
                if (leftHand == null)
                {
                    var lhGo = new GameObject("LeftHand_Controller");
                    leftHand = lhGo.transform;
                    leftHand.SetParent(cameraOffset, false);
                }
                var leftHandCtrl = leftHand.GetComponent<VRHandController>();
                if (leftHandCtrl == null)
                {
                    leftHandCtrl = leftHand.gameObject.AddComponent<VRHandController>();
                    var so = new SerializedObject(leftHandCtrl);
                    so.FindProperty("handType").enumValueIndex = (int)VRHandController.HandType.Left;
                    so.ApplyModifiedProperties();
                }

                // Right Controller
                Transform rightHand = cameraOffset.Find("RightHand_Controller");
                if (rightHand == null)
                {
                    var rhGo = new GameObject("RightHand_Controller");
                    rightHand = rhGo.transform;
                    rightHand.SetParent(cameraOffset, false);
                }
                var rightHandCtrl = rightHand.GetComponent<VRHandController>();
                if (rightHandCtrl == null)
                {
                    rightHandCtrl = rightHand.gameObject.AddComponent<VRHandController>();
                    var so = new SerializedObject(rightHandCtrl);
                    so.FindProperty("handType").enumValueIndex = (int)VRHandController.HandType.Right;
                    so.ApplyModifiedProperties();
                }

                // Right Laser Pointer
                var laser = rightHand.GetComponent<VRLaserPointer>();
                if (laser == null)
                {
                    laser = rightHand.gameObject.AddComponent<VRLaserPointer>();
                }

                // Locomotion
                var locomotion = xrOriginGo.GetComponent<VRLocomotion>();
                if (locomotion == null)
                {
                    locomotion = xrOriginGo.AddComponent<VRLocomotion>();
                }
                var locoSo = new SerializedObject(locomotion);
                locoSo.FindProperty("xrOriginTransform").objectReferenceValue = xrOriginGo.transform;
                locoSo.FindProperty("headCameraTransform").objectReferenceValue = cam.transform;
                locoSo.FindProperty("leftHand").objectReferenceValue = leftHandCtrl;
                locoSo.FindProperty("rightHand").objectReferenceValue = rightHandCtrl;
                locoSo.ApplyModifiedProperties();

                // XROrigin
                var xrOrigin = xrOriginGo.GetComponent<Unity.XR.CoreUtils.XROrigin>();
                if (xrOrigin == null)
                {
                    xrOrigin = xrOriginGo.AddComponent<Unity.XR.CoreUtils.XROrigin>();
                }
                xrOrigin.Camera = cam;
                xrOrigin.CameraFloorOffsetObject = cameraOffset.gameObject;
                xrOrigin.RequestedTrackingOriginMode = Unity.XR.CoreUtils.XROrigin.TrackingOriginMode.Floor;

                // Player Rig
                var playerRig = xrOriginGo.GetComponent<VRPlayerRig>();
                if (playerRig == null)
                {
                    playerRig = xrOriginGo.AddComponent<VRPlayerRig>();
                }
                playerRig.SetupRigReferences();

                VRPlayerRig.EnsureUIButtonColliders();
            }

            // Save Scene
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log("[GarageSceneBuilder] Mini garage workshop scene built and saved successfully!");
        }

        private static Button CreateUIButton(string name, Transform parent, Vector2 pos, string label, Color bgColor, Font font)
        {
            GameObject btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);

            RectTransform rt = btnGo.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(420f, 82f);

            Image img = btnGo.AddComponent<Image>();
            img.color = bgColor;

            Button btn = btnGo.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = Color.Lerp(bgColor, Color.white, 0.25f);
            cb.pressedColor = Color.Lerp(bgColor, Color.black, 0.25f);
            cb.selectedColor = bgColor;
            btn.colors = cb;

            // Button Label
            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            RectTransform textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            Text txt = textGo.AddComponent<Text>();
            txt.text = label;
            txt.font = font;
            txt.fontSize = 28;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;

            return btn;
        }
    }
}
