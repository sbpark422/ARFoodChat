using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Manages AR anchors for detected objects using ARFoundation. It handles anchor placement and removal in response to object detection and user interactions.
/// Modified from https://github.com/derenlei/Unity_Detection2AR
/// <summary>
public class AnchorCreator : MonoBehaviour
{
    private const TrackableType trackableTypes = TrackableType.Planes | TrackableType.FeaturePoint;
    private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    public PhoneARCamera phoneARCamera;
    public ARRaycastManager m_RaycastManager;
    public ARAnchorManager m_AnchorManager;
    public TextMesh anchorObj_mesh;

    private IDictionary<ARAnchor, BoundingBox> anchorDic = new Dictionary<ARAnchor, BoundingBox>();
    private List<BoundingBox> boxSavedOutlines;
    private float shiftX;
    private float shiftY;
    private float scaleFactor;

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_AnchorManager = GetComponent<ARAnchorManager>();
        GameObject cameraImage = GameObject.Find("Camera Image");
        phoneARCamera = cameraImage.GetComponent<PhoneARCamera>();
    }

    void Update()
    {
        // If bounding boxes are not stable, return directly without raycast
        if (!phoneARCamera.localization)
        {
            return;
        }

        boxSavedOutlines = phoneARCamera.boxSavedOutlines;
        shiftX = phoneARCamera.shiftX;
        shiftY = phoneARCamera.shiftY;
        scaleFactor = phoneARCamera.scaleFactor;
        // Remove outdated anchor that is not in boxSavedOutlines
        // Currently not using. Can be removed.
        if (anchorDic.Count != 0)
        {
            List<ARAnchor> itemsToRemove = new List<ARAnchor>();
            foreach (KeyValuePair<ARAnchor, BoundingBox> pair in anchorDic)
            {
                if (!boxSavedOutlines.Contains(pair.Value))
                {
                    Debug.Log($"DEBUG: anchor removed. {pair.Value.Label}: {(int)(pair.Value.Confidence * 100)}%");

                    itemsToRemove.Add(pair.Key);
                    m_AnchorManager.RemoveAnchor(pair.Key);
                    s_Hits.Clear();
                }
            }
            foreach (var item in itemsToRemove) {
                anchorDic.Remove(item);
            }
        }

        // return if no bounding boxes
        if (boxSavedOutlines.Count == 0)
        {
            return;
        }
        // create anchor for new bounding boxes
        foreach (var outline in boxSavedOutlines)
        {
            if (outline.Used)
            {
                continue;
            }

            float xMin, yMin;
            float center_x, center_y;
            float width,height;

            Debug.Log($"Screen.width {Screen.width}");
            Debug.Log($"Screen.height {Screen.height}");

            width = outline.Dimensions.Width * this.scaleFactor;
            height = outline.Dimensions.Height * this.scaleFactor;

            // Check if the device is in landscape mode
            if (Screen.width > Screen.height) // Landscape
            {
                // Need to find most optimal adjustment for Landscape
                // Landscape-specific coordinate adjustments
                xMin = Screen.width - (outline.Dimensions.X * this.scaleFactor + shiftX);
                yMin = outline.Dimensions.Y * this.scaleFactor + shiftY;
            }
            else // Portrait
            {
                // Portrait-specific coordinate adjustments
                xMin = outline.Dimensions.X * this.scaleFactor + shiftX;
                yMin = Screen.height - (outline.Dimensions.Y * this.scaleFactor + shiftY);
            }

            center_x = xMin + width / 2f;
            center_y = yMin - height / 2f;


            if (Pos2Anchor(center_x, center_y, outline))
            {
                Debug.Log("Outline used is true");
                outline.Used = true;
            }
            else
            {
                //Debug.Log("Outline used is false");
            }
        }

    }

    public void RemoveAllAnchors()
    {
        Debug.Log($"DEBUG: Removing all anchors ({anchorDic.Count})");
        foreach (var anchor in anchorDic)
        {
            Destroy(anchor.Key.gameObject);
        }
        s_Hits.Clear();
        anchorDic.Clear();
    }

    ARAnchor CreateAnchor(in ARRaycastHit hit)
    {
        // create a regular anchor at the hit pose
        Debug.Log($"DEBUG: Creating regular anchor. distance: {hit.distance}. session distance: {hit.sessionRelativeDistance} type: {hit.hitType}.");
        return m_AnchorManager.AddAnchor(hit.pose);
    }

    private bool Pos2Anchor(float x, float y, BoundingBox outline)
    {
        anchorObj_mesh.text = $"{outline.Label}: {(int)(outline.Confidence * 100)}%";

        // Load a 3d prefab based on the outline label (now only for "apple")
        // ** Future plan **: using ScriptableObjects to map labels to their corresponding 3D models
        var prefab = Resources.Load<GameObject>(outline.Label);
        if (prefab != null)
        {
            var instantiatedPrefab = Instantiate(prefab);
            instantiatedPrefab.transform.SetParent(anchorObj_mesh.transform, false);
        }
        else
        {
            foreach (Transform child in anchorObj_mesh.transform)
            {
                child.gameObject.SetActive(false);
            }
            Debug.LogWarning($"Prefab with name '{outline.Label}' not found in Resources.");
        }

        // Perform the raycast
        if (m_RaycastManager.Raycast(new Vector2(x, y), s_Hits, trackableTypes))
        {
            // Raycast hits are sorted by distance, so the first one will be the closest hit.
            var hit = s_Hits[0];

            // Create a new anchor
            Debug.Log("Creating Anchor");
            var anchor = CreateAnchor(hit);

            if (anchor)
            {
                Debug.Log($"DEBUG: creating anchor. {outline}");
                // Remember the anchor so we can remove it later.
                anchorDic.Add(anchor, outline);
                Debug.Log($"DEBUG: Current number of anchors {anchorDic.Count}.");
                return true;
            }
            else
            {
                Debug.Log("DEBUG: Error creating anchor");
                return false;
            }

        }
        else
        {
            //Debug.Log("Couldn't raycast");
        }
        return false;
    }


}
