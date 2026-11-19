using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float amplitude = 0.1f;
    [SerializeField] private float rotationSpeed = 20f;

    private Vector3 startPos;
    private bool isFloating = true;

    private void Start()
    {
        // Store the initial position as the reference point
        startPos = transform.position;
    }

    private void Update()
    {
        if (isFloating)
        {
            // Vertical oscillating movement
            float newY = startPos.y + Mathf.Sin(Time.time * speed) * amplitude;
            transform.position = new Vector3(startPos.x, newY, startPos.z);

            // Continuous rotation around the Y axis
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Disables floating (useful when starting a drag, grab or click).
    /// </summary>
    public void DisableFloating()
    {
        isFloating = false;
    }

    /// <summary>
    /// Re-enables floating by updating the anchor point to the current position.
    /// </summary>
    public void EnableFloating()
    {
        startPos = transform.position;
        isFloating = true;
    }
}