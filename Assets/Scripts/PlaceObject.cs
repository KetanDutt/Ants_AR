using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager), typeof(ARAnchorManager))]
public class PlaceObject : MonoBehaviour
{
    private const float ScalePerMetre = 0.05f;
    private const float MinimumPlacementScale = 0.06f;
    private const float MaximumPlacementScale = 0.35f;

    public static PlaceObject instance;

    [SerializeField] private GameObject prefab;

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private ARAnchorManager anchorManager;
    private readonly List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

    private ARAnchor placedAnchor;
    private GameObject placedWorld;
    private bool isPlacing;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        anchorManager = GetComponent<ARAnchorManager>();
    }

    private void OnEnable()
    {
        EnhancedTouch.EnhancedTouchSupport.Enable();
#if UNITY_EDITOR
        EnhancedTouch.TouchSimulation.Enable();
#endif
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    private void OnDisable()
    {
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
#if UNITY_EDITOR
        EnhancedTouch.TouchSimulation.Disable();
#endif
        EnhancedTouch.EnhancedTouchSupport.Disable();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private void FingerDown(EnhancedTouch.Finger finger)
    {
        if (finger.index != 0 || isPlacing)
            return;

        GameManager gameManager = GameManager.instance;
        if (gameManager == null || !gameManager.CanPlaceWorld || prefab == null)
            return;

        Vector2 screenPosition = finger.currentTouch.screenPosition;
        if (IsPointerOverUI(screenPosition))
            return;

        if (!raycastManager.Raycast(screenPosition, raycastHits, TrackableType.PlaneWithinPolygon))
            return;

        // ARRaycastManager sorts hits from nearest to farthest. Place once on
        // the nearest upward-facing horizontal plane rather than spawning once
        // for every overlapping plane in the result list.
        for (int index = 0; index < raycastHits.Count; index++)
        {
            ARPlane plane = planeManager.GetPlane(raycastHits[index].trackableId);
            if (plane == null || plane.alignment != PlaneAlignment.HorizontalUp)
                continue;

            isPlacing = true;
            PlaceWorld(raycastHits[index], plane, gameManager);
            return;
        }
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            return false;

        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = screenPosition;
        uiRaycastResults.Clear();
        eventSystem.RaycastAll(pointerData, uiRaycastResults);
        return uiRaycastResults.Count > 0;
    }

    private void PlaceWorld(ARRaycastHit hit, ARPlane plane, GameManager gameManager)
    {
        Pose pose = hit.pose;
        float scale = Mathf.Clamp(hit.distance * ScalePerMetre, MinimumPlacementScale, MaximumPlacementScale);

        try
        {
            placedAnchor = anchorManager.AttachAnchor(plane, pose);
        }
        catch (Exception exception)
        {
            // Some AR providers/devices do not support plane-attached anchors.
            // The hit pose is still valid, so use an unanchored fallback.
            Debug.LogWarning("Could not attach an AR anchor; placing at the detected pose instead. " + exception.Message);
            placedAnchor = null;
        }

        if (placedAnchor != null)
        {
            placedWorld = Instantiate(prefab, placedAnchor.transform, false);
            placedWorld.transform.localPosition = Vector3.zero;
            placedWorld.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Quaternion yawRotation = Quaternion.Euler(0f, pose.rotation.eulerAngles.y, 0f);
            placedWorld = Instantiate(prefab, pose.position, yawRotation);
        }

        if (placedWorld == null)
        {
            CleanupFailedPlacement(gameManager);
            return;
        }

        placedWorld.name = "Placed Quiz World";
        placedWorld.transform.localScale = Vector3.one * scale;
        gameManager.World = placedWorld;

        if (!gameManager.StartGame())
        {
            CleanupFailedPlacement(gameManager);
            return;
        }

        isPlacing = false;
    }

    private void CleanupFailedPlacement(GameManager gameManager)
    {
        if (placedAnchor != null)
            Destroy(placedAnchor.gameObject);
        else if (placedWorld != null)
            Destroy(placedWorld);

        placedAnchor = null;
        placedWorld = null;
        gameManager.World = null;
        gameManager.WorldPlaced = false;
        isPlacing = false;
    }

    /// <summary>Destroys the current quiz world so the player can reposition it.</summary>
    public void ResetPlacement()
    {
        if (placedAnchor != null)
            Destroy(placedAnchor.gameObject);
        else if (placedWorld != null)
            Destroy(placedWorld);

        placedAnchor = null;
        placedWorld = null;
        isPlacing = false;

        if (GameManager.instance != null)
            GameManager.instance.OnPlacementReset();
    }
}
