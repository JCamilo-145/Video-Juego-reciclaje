using UnityEngine;
using UnityEngine.Events;

public class PlayerTriggerEvent : MonoBehaviour
{
    [Header("Player Events")]
    [Tooltip("Runs when an object with the 'Player' tag enters the trigger.")]
    public UnityEvent OnPlayerEnter;

    private void OnTriggerEnter(Collider other)
    {
        // Check whether the object that entered has the exact tag "Player"
        if (other.CompareTag("Player"))
        {
            OnPlayerEnter.Invoke();
        }
    }
}