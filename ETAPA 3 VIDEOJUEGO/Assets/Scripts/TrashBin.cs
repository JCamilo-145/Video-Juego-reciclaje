using UnityEngine;
using UnityEngine.Events;

// Enum that must match the project's Tags exactly
public enum TrashTag
{
    Trash_Organic,
    Trash_NonRecyclable,
    Trash_Recyclable
}

public class TrashBin : MonoBehaviour
{
    [Header("Bin Settings")]
    [Tooltip("Select which type of trash this bin accepts")]
    public TrashTag acceptedTag;

    [Header("Counter Settings")]
    [Tooltip("Drag here the GameObject that has the Text Mesh (e.g. the one that shows 0/2)")]
    public TextMesh counterText;
    [Tooltip("Maximum amount of trash required for this bin")]
    public int maxCount = 2;
    private int currentCount = 0; // Starts at 0

    [Header("Events")]
    public UnityEvent OnCorrectTrash;
    public UnityEvent OnIncorrectTrash;

    private void Start()
    {
        // Initialize the displayed text when the game starts (e.g. "0/2")
        UpdateCounterDisplay();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Convert the selected enum value to a string so it can be compared
        string tagToMatch = acceptedTag.ToString();

        // 1. Check whether the colliding object has the correct tag
        if (other.CompareTag(tagToMatch))
        {
            // Fire the success event
            OnCorrectTrash.Invoke();

            // Destroy the object that entered (the trash)
            Destroy(other.gameObject);
        }
        // 2. If it is not correct, check that it is at least another type of trash
        else if (other.CompareTag(TrashTag.Trash_Organic.ToString()) ||
                 other.CompareTag(TrashTag.Trash_NonRecyclable.ToString()) ||
                 other.CompareTag(TrashTag.Trash_Recyclable.ToString()))
        {
            // Fire the error event
            OnIncorrectTrash.Invoke();
        }
    }

    /// <summary>
    /// Public method that adds 1 to the counter.
    /// It can be called from the "OnCorrectTrash" UnityEvent in the Inspector.
    /// </summary>
    public void IncrementCounter()
    {
        // Only add if the maximum has not been reached (to avoid showing 3/2)
        if (currentCount < maxCount)
        {
            currentCount++;
            UpdateCounterDisplay();
        }
    }

    // Updates the string of the Text Mesh component
    private void UpdateCounterDisplay()
    {
        if (counterText != null)
        {
            counterText.text = currentCount + "/" + maxCount;
        }
        else
        {
            Debug.LogWarning("The Text Mesh has not been assigned in the inspector of " + gameObject.name);
        }
    }
}