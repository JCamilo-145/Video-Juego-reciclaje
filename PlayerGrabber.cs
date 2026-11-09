using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrabber : MonoBehaviour
{
    [Header("Configuración de Agarre")]
    [Tooltip("El objeto vacío (Empty) frente al jugador donde se colocará la basura")]
    public Transform holdPoint;

    private GameObject currentTarget;
    private GameObject heldObject;

    // El Player Input (Send Messages) llama a este método automáticamente
    // cuando presionas la acción "Action" (la tecla C).
    public void OnAction(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("1. Tecla de Acción (C) presionada.");
            
            if (heldObject == null && currentTarget != null)
            {
                GrabObject(currentTarget);
            }
            else if (currentTarget == null)
            {
                Debug.Log("2. No hay ninguna basura en el área del círculo verde.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsTrashTag(other.tag) && heldObject == null)
        {
            currentTarget = other.gameObject;
            Debug.Log("-> Basura detectada en el rango: " + other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentTarget == other.gameObject)
        {
            currentTarget = null;
            Debug.Log("<- Te alejaste de la basura.");
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

        // Emparenta la basura al HoldPoint
        heldObject.transform.SetParent(holdPoint);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        // Si tiene física, la apagamos para que no se caiga
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        Debug.Log("¡Basura agarrada con éxito!");
    }
}