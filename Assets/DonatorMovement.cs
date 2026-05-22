using UnityEngine;

public class DonatorMovement : MonoBehaviour
{
    [SerializeField] private float moveDistance = 1.0f;  // Distància de moviment per trucada

    // Funció per moure cap a la dreta
    public void MoveRight()
    {
        transform.position += Vector3.right * moveDistance;
    }

    // Funció per moure cap a l'esquerra
    public void MoveLeft()
    {
        transform.position += Vector3.left * moveDistance;
    }

    // Funció per moure cap amunt
    public void MoveUp()
    {
        transform.position += Vector3.up * moveDistance;
    }

    // Funció per moure cap avall
    public void MoveDown()
    {
        transform.position += Vector3.down * moveDistance;
    }
}
