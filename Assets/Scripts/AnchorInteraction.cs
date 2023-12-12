using UnityEngine;

/// <summary>
/// Manages user touch interactions with AR objects tagged as 'Food' (e.g., Apple AR object).
/// Applies a visual outline to selected objects, starts speech recording upon selection, 
/// and removes the outline upon speech-to-text transcription completion.
/// </summary>
public class AnchorInteraction : MonoBehaviour
{
    [SerializeField] private Camera arCamera;
    [SerializeField] private Material outlineMaterial; // Material for outlining hit objects.
    [SerializeField] private SpeechToText speechToText;

    private Vector2 touchPosition = default;
    private GameObject lastOutlinedObject; // Reference to the last object that was outlined

    private void Awake()
    {
        speechToText.OnTranscriptionComplete += RemoveLastOutline;
    }

    private void OnDestroy()
    {
        speechToText.OnTranscriptionComplete -= RemoveLastOutline;
    }

    void Update()
    {
        // Check if there is any touch input on the screen.
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchPosition = touch.position;

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = arCamera.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hitObject))
                {
                    // If the raycast hits an object tagged as 'Food', start the interaction.
                    if (hitObject.transform.gameObject.CompareTag("Food"))
                    {
                        speechToText.StartRecording();
                        ApplyOutline(hitObject.transform.gameObject);
                    }
                }
            }
        }
    }

    private void ApplyOutline(GameObject obj)
    {
        Renderer objRenderer = obj.GetComponent<Renderer>();
        if (objRenderer != null && outlineMaterial != null)
        {
            Material[] currentMaterials = objRenderer.materials;
            Material[] newMaterials = new Material[currentMaterials.Length + 1];
            currentMaterials.CopyTo(newMaterials, 0);
            newMaterials[currentMaterials.Length] = outlineMaterial;

            objRenderer.materials = newMaterials;
            lastOutlinedObject = obj;
        }
    }

    // <summary>
    // Removes the outline material from the last outlined object upon transcription completion,
    // reverting it back to its original material.
    /// </summary>
    private void RemoveLastOutline(string transcriptionText = null)
    {
        if (lastOutlinedObject != null)
        {
            Renderer objRenderer = lastOutlinedObject.GetComponent<Renderer>();
            if (objRenderer != null)
            {
                Material[] newMaterials = new Material[objRenderer.materials.Length - 1];
                System.Array.Copy(objRenderer.materials, newMaterials, newMaterials.Length);
                objRenderer.materials = newMaterials;
            }

            lastOutlinedObject = null;
        }
    }

}
