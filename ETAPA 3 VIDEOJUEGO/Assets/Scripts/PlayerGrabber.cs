using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrabber : MonoBehaviour
{
    [Header("Grab Settings")]
    [Tooltip("Empty object in front of the player where the trash will be placed")]
    public Transform holdPoint;

    private GameObject currentTarget;
    private GameObject heldObject;

    // Player Input (Send Messages) calls this method automatically
    // when the "Action" input is pressed (the C key).
    public void OnAction(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("1. Action key (C) pressed.");

            if (heldObject == null && currentTarget != null)
            {
                GrabObject(currentTarget);
            }
            else if (currentTarget == null)
            {
                Debug.Log("2. There is no trash inside the green circle area.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsTrashTag(other.tag) && heldObject == null)
        {
            currentTarget = other.gameObject;
            Debug.Log("-> Trash detected in range: " + other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentTarget == other.gameObject)
        {
            currentTarget = null;
            Debug.Log("<- You moved away from the trash.");
        }
    }

    private bool IsTrashTag(string tagToCheck)
    {
        return tagToCheck == "Trash_Organic" ||
               tagToCheck == "Trash_NonRecyclable" ||
               tagToCheck == "Trash_Recyclable";
    }

    private void GrabObject(GameObject obj)
    {
        heldObject = obj;
        currentTarget = null;

        // Parent the trash to the HoldPoint
        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        // If it has physics, turn them off so the object does not fall
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Debug.Log("Trash grabbed successfully!");
    }
}