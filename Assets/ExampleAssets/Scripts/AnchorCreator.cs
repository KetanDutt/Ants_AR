using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// Example utility for placing a marker prefab on a detected AR plane.
[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class AnchorCreator : MonoBehaviour
{
    [SerializeField] private GameObject m_AnchorPrefab;

    public GameObject AnchorPrefab
    {
        get { return m_AnchorPrefab; }
        set { m_AnchorPrefab = value; }
    }

    private static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    private List<ARAnchor> m_AnchorPoints;
    private ARRaycastManager m_RaycastManager;
    private ARAnchorManager m_AnchorManager;
    private ARPlaneManager m_PlaneManager;

    public void RemoveAllAnchors()
    {
        if (m_AnchorPoints == null)
            return;

        foreach (ARAnchor anchor in m_AnchorPoints)
        {
            if (anchor != null)
                Destroy(anchor.gameObject);
        }

        m_AnchorPoints.Clear();
    }

    private void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_AnchorManager = GetComponent<ARAnchorManager>();
        m_PlaneManager = GetComponent<ARPlaneManager>();
        m_AnchorPoints = new List<ARAnchor>();
    }

    private void Update()
    {
        if (Input.touchCount == 0 || m_AnchorPrefab == null)
            return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began ||
            !m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            return;
        }

        // Raycast results are ordered by distance. Stop after the nearest valid
        // trackable rather than trying to attach to a missing plane.
        foreach (ARRaycastHit hit in s_Hits)
        {
            ARPlane plane = m_PlaneManager.GetPlane(hit.trackableId);
            if (plane == null)
                continue;

            ARAnchor anchor = m_AnchorManager.AttachAnchor(plane, hit.pose);
            if (anchor == null)
            {
                Debug.LogWarning("AnchorCreator could not create an AR anchor.");
                return;
            }

            Instantiate(m_AnchorPrefab, anchor.transform, false);
            m_AnchorPoints.Add(anchor);
            return;
        }
    }
}
