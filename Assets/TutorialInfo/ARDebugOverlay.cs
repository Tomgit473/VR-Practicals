using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// Temporary on-screen diagnostics for AR startup problems. Safe to delete once AR works.
public class ARDebugOverlay : MonoBehaviour
{
    private ARCameraManager cameraManager;
    private ARTrackedImageManager imageManager;
    private int frames;
    private float lastFrameTime = -1f;
    private GUIStyle style;

    void Awake()
    {
        cameraManager = FindFirstObjectByType<ARCameraManager>();
        imageManager = FindFirstObjectByType<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        if (cameraManager != null) cameraManager.frameReceived += OnFrame;
    }

    void OnDisable()
    {
        if (cameraManager != null) cameraManager.frameReceived -= OnFrame;
    }

    private void OnFrame(ARCameraFrameEventArgs args)
    {
        frames++;
        lastFrameTime = Time.realtimeSinceStartup;
    }

    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = Mathf.Max(22, Screen.height / 50);
            style.wordWrap = true;
            style.normal.textColor = Color.yellow;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("gfx: " + SystemInfo.graphicsDeviceType + " (" + SystemInfo.graphicsDeviceName + ")");
        sb.AppendLine("ARSession.state: " + ARSession.state + " | notTracking: " + ARSession.notTrackingReason);

        if (cameraManager == null)
        {
            sb.AppendLine("ARCameraManager: NOT FOUND");
        }
        else
        {
            sb.AppendLine("camera subsystem running: " + (cameraManager.subsystem != null && cameraManager.subsystem.running));
            sb.AppendLine("camera permissionGranted: " + cameraManager.permissionGranted);
            float age = lastFrameTime < 0f ? -1f : Time.realtimeSinceStartup - lastFrameTime;
            sb.AppendLine("camera frames received: " + frames + " (last " + (age < 0f ? "never" : age.ToString("F1") + "s ago") + ")");
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        sb.AppendLine("Android CAMERA permission: " + UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera));
#endif
        if (imageManager == null)
        {
            sb.AppendLine("ARTrackedImageManager: NOT FOUND");
        }
        else
        {
            sb.AppendLine("image subsystem running: " + (imageManager.subsystem != null && imageManager.subsystem.running));
            sb.AppendLine("tracked images: " + imageManager.trackables.count);
            foreach (var t in imageManager.trackables)
                sb.AppendLine("  - " + t.referenceImage.name + ": " + t.trackingState + " size=" + t.size);
        }

        float top = Screen.height - Screen.safeArea.yMax + 10f;
        var rect = new Rect(Screen.safeArea.x + 10f, top, Screen.safeArea.width - 20f, Screen.safeArea.height * 0.45f);
        var prev = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = prev;
        GUI.Label(rect, sb.ToString(), style);
    }
}
