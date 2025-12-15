using UnityEngine;
using UnityEngine.InputSystem;

public class moving_in_map : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;

    [Header("Clamp Camera Y Position")]
    public float minY = 0f;   // smallest height
    public float maxY = 120f; // biggest height

    private Vector2 lastPointerPos;
    private bool isDragging = false;

    void Update()
    {
        // Detect mouse or touch press (New Input System)
        if (Pointer.current != null)
        {
            if (Pointer.current.press.isPressed)
            {
                Vector2 currentPos = Pointer.current.position.ReadValue();

                if (!isDragging)
                {
                    // Start drag
                    lastPointerPos = currentPos;
                    isDragging = true;
                }
                else
                {
                    Vector2 delta = currentPos - lastPointerPos;

                    // If dragging DOWN → camera moves UP
                    float moveY = -delta.y * moveSpeed * Time.deltaTime;

                    Vector3 newPos = transform.position + new Vector3(0, moveY, 0);

                    // Clamp
                    newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

                    transform.position = newPos;

                    lastPointerPos = currentPos;
                }
            }
            else
            {
                // No press → no dragging
                isDragging = false;
            }
        }
    }
}
