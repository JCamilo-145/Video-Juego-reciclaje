using UnityEngine;
using UnityEngine.Events;

// Definimos el Enum para que coincida exactamente con los Tags de tu imagen
public enum TrashTag
{
    Trash_Organic,
    Trash_NonRecyclable,
    Trash_Recyclable
}

public class TrashBin : MonoBehaviour
{
    [Header("Configuración del Contenedor")]
    [Tooltip("Selecciona qué tipo de basura acepta este contenedor")]
    public TrashTag acceptedTag;

    [Header("Configuración del Contador")]
    [Tooltip("Arrastra aquí el GameObject que tiene el Text Mesh (ej. el que dice 0/2)")]
    public TextMesh counterText;
    [Tooltip("Cantidad máxima de basura requerida para este contenedor")]
    public int maxCount = 2;
    private int currentCount = 0; // Inicia en 0

    [Header("Eventos")]
    public UnityEvent OnCorrectTrash;
    public UnityEvent OnIncorrectTrash;

    private void Start()
    {
        // Inicializa el texto visualmente cuando arranca el juego (ej. "0/2")
        UpdateCounterDisplay();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Convertimos el valor seleccionado del enum a un string para compararlo
        string tagToMatch = acceptedTag.ToString();

        // 1. Verificamos si el objeto que colisiona tiene el tag correcto
        if (other.CompareTag(tagToMatch))
        {
            // Dispara el evento de acierto
            OnCorrectTrash.Invoke();
            
            // Destruye el objeto que entró (la basura)
            Destroy(other.gameObject);
        }
        // 2. Si no es correcto, verificamos que al menos sea otro tipo de basura 
        else if (other.CompareTag(TrashTag.Trash_Organic.ToString()) || 
                 other.CompareTag(TrashTag.Trash_NonRecyclable.ToString()) || 
                 other.CompareTag(TrashTag.Trash_Recyclable.ToString()))
        {
            // Dispara el evento de error
            OnIncorrectTrash.Invoke();
        }
    }

    /// <summary>
    /// Método público para sumar 1 al contador. 
    /// Puedes llamarlo desde el UnityEvent "OnCorrectTrash" en el Inspector.
    /// </summary>
    public void IncrementCounter()
    {
        // Solo suma si no ha llegado al máximo (para evitar que diga 3/2)
        if (currentCount < maxCount)
        {
            currentCount++;
            UpdateCounterDisplay();
        }
    }

    // Actualiza el string del componente Text Mesh
    private void UpdateCounterDisplay()
    {
        if (counterText != null)
        {
            counterText.text = currentCount + "/" + maxCount;
        }
        else
        {
            Debug.LogWarning("No has asignado el Text Mesh en el inspector de " + gameObject.name);
        }
    }
}