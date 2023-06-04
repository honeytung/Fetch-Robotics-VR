using UnityEngine;
using UnityEngine.EventSystems;

public class CursorPositionGetter : MonoBehaviour
{
    public Canvas screenSpaceCanvas; // Assign this in the inspector
    private Camera mainCamera;

    void Start()
    {
        // Assuming the Canvas uses the main camera to render its UI elements
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            // Get the cursor position in screen coordinates
            Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            // Convert screen coordinates to local Canvas coordinates
            RectTransform canvasRectTransform = screenSpaceCanvas.GetComponent<RectTransform>();
            Vector2 localCursor;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition, mainCamera, out localCursor))
            {
                // Print the cursor position
                UnityEngine.Debug.Log("Cursor Position on Canvas: " + localCursor);
            }
        }
    }
}
