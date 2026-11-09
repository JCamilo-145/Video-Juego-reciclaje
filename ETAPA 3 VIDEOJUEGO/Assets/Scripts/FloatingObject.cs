using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Ajustes de Animación")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float amplitude = 0.1f;
    [SerializeField] private float rotationSpeed = 20f;

    private Vector3 startPos;
    private bool isFloating = true;

    private void Start()
    {
        // Guardamos la posición inicial como referencia
        startPos = transform.position;
    }

    private void Update()
    {
        if (isFloating)
        {
            // Movimiento oscilatorio vertical
            float newY = startPos.y + Mathf.Sin(Time.time * speed) * amplitude;
            transform.position = new Vector3(startPos.x, newY, startPos.z);

            // Rotación continua en el eje Y
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Desactiva la flotación (útil al iniciar arrastre, agarrar o hacer clic).
    /// </summary>
    public void DisableFloating()
    {
        isFloating = false;
    }

    /// <summary>
    /// Reactiva la flotación actualizando el punto de ancla a la posición actual.
    /// </summary>
    public void EnableFloating()
    {
        startPos = transform.position;
        isFloating = true;
    }
}