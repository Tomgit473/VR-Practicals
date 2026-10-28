using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Video;

public class PlayVideoOnTrackedImage : MonoBehaviour
{
    [SerializeField] private GameObject videoPrefab;

    private ARTrackedImageManager trackedImageManager;
    private GameObject spawnedObject;
    private VideoPlayer videoPlayer;

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }

    void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
    }

    private static float GetVideoAspect(GameObject go)
    {
        var vp = go.GetComponentInChildren<VideoPlayer>();
        if (vp != null && vp.clip != null && vp.clip.width > 0)
            return (float)vp.clip.height / vp.clip.width;
        return 0.5625f; // fallback 16:9
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        // Newly added (detected) images
        foreach (var trackedImage in args.added)
        {
            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(videoPrefab, trackedImage.transform);
                spawnedObject.transform.localPosition = Vector3.zero;
                spawnedObject.transform.localRotation = Quaternion.identity;

                // Scale the video to match the physical size of the marker
                float width = trackedImage.size.x > 0f ? trackedImage.size.x : 0.1f; // fallback if physical size isn't set
                spawnedObject.transform.localScale = new Vector3(width, 1f, width * GetVideoAspect(spawnedObject)); // matches the clip's aspect ratio

                videoPlayer = spawnedObject.GetComponentInChildren<VideoPlayer>();
                if (videoPlayer != null)
                {
                    videoPlayer.Play();
                }
            }
        }

        // Updated tracking state
        foreach (var trackedImage in args.updated)
        {
            if (spawnedObject != null)
            {
                bool isTracking = trackedImage.trackingState == TrackingState.Tracking;
                spawnedObject.SetActive(isTracking);

                if (isTracking && videoPlayer != null && !videoPlayer.isPlaying)
                {
                    videoPlayer.Play();
                }
            }
        }

        // Removed (lost) images
        foreach (var trackedImage in args.removed)
        {
            if (spawnedObject != null)
            {
                Destroy(spawnedObject);
                spawnedObject = null;
            }
        }
    }
}