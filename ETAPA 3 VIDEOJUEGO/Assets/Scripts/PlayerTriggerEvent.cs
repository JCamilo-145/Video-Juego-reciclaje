using UnityEngine;
using UnityEngine.Events;

public class PlayerTriggerEvent : MonoBehaviour
{
    [Header("Eventos del Jugador")]
    [Tooltip("Se ejecuta cuando un objeto con el tag 'Player' entra en el trigger.")]
    public UnityEvent OnPlayerEnter;

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si el objeto que entró tiene el tag exacto "Player"
        if (other.CompareTag("Player"))
        {
            OnPlayerEnter.Invoke();
        }
    }
}