using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class PlaceObject : MonoBehaviour
{
    public static PlaceObject instance;

    [SerializeField]
    private GameObject prefab;
    private ARRaycastManager aRRaycastManager;
    private ARPlaneManager aRPlaneManager;
    [SerializeField]
    private Camera arCamera;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // public List<GameObject> spawned = new List<GameObject>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        aRRaycastManager = GetComponent<ARRaycastManager>();
        aRPlaneManager = GetComponent<ARPlaneManager>();
    }

    private void OnEnable()
    {
        try
        {
            EnhancedTouch.TouchSimulation.Enable();
            EnhancedTouch.EnhancedTouchSupport.Enable();
            EnhancedTouch.Touch.onFingerDown += FingerDown;
        }
        catch (System.Exception)
        {
        }
    }

    private void OnDisable()
    {
        try
        {
            EnhancedTouch.TouchSimulation.Disable();
            EnhancedTouch.EnhancedTouchSupport.Disable();
            EnhancedTouch.Touch.onFingerDown -= FingerDown;

        }
        catch (System.Exception)
        {
        }
    }

    private void FingerDown(EnhancedTouch.Finger finger)
    {
        if (finger.index != 0) return;
        if (GameManager.instance.WorldPlaced) return;


        if (aRRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            foreach (ARRaycastHit hit in hits)
            {
                if (aRPlaneManager.GetPlane(hit.trackableId).alignment == PlaneAlignment.HorizontalUp)
                {
                    Pose pose = hit.pose;
                    float scale = hit.distance * .05f;
                    MyScript.Log("scale = " + scale + "");
                    GameManager.instance.World = Instantiate(prefab, pose.position,
                        Quaternion.Euler(pose.rotation.eulerAngles.x, -pose.rotation.eulerAngles.y, pose.rotation.eulerAngles.z));
                    GameManager.instance.World.transform.localScale = new Vector3(scale, scale, scale);
                    GameManager.instance.WorldPlaced = true;
                    GameManager.instance.StartGame();
                }
            }
        }
    }
}