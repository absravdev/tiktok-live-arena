using UnityEngine;
using System.Collections.Generic;

public class CameraFollow2D : MonoBehaviour
{
    public List<Transform> targets; // Llista d'objectes a seguir
    public Transform jugador1;
    public Transform jugador2;
    public Transform jugador3;
    public float smoothTime = 0.3f; // Temps de suavitzat per al moviment
    private Vector3 velocity = Vector3.zero;

    public float minSize = 5f;  // Zoom mínim (ortogonal)
    public float maxSize = 20f; // Zoom màxim (ortogonal)
    public float zoomLimiter = 10f; // Factor limitador del zoom

    public float yOffset = 2f; // Desplaçament vertical per mantenir la càmera més amunt
    public float singlePlayerZoom = 3f; // Zoom quan només queda un jugador

    void LateUpdate()
    {
        // Actualitza els jugadors actius en targets
        ReAddActivePlayers();

        // Si no hi ha cap jugador, no fer res
        if (targets.Count == 0)
            return;

        // Seguir els jugadors actius
        FollowTargets();

        // Ajustar el zoom de la càmera
        AdjustZoom();
    }

    void ReAddActivePlayers()
    {
        // Assegura que els jugadors actius estan a la llista de targets
        if (jugador1 != null && jugador1.gameObject.activeSelf && !targets.Contains(jugador1))
        {
            targets.Add(jugador1);
        }

        if (jugador2 != null && jugador2.gameObject.activeSelf && !targets.Contains(jugador2))
        {
            targets.Add(jugador2);
        }

        if (jugador3 != null && jugador3.gameObject.activeSelf && !targets.Contains(jugador3))
        {
            targets.Add(jugador3);
        }

        // Elimina jugadors que estiguin desactivats de la llista de targets
        targets.RemoveAll(target => target == null || !target.gameObject.activeSelf);
    }

    void FollowTargets()
    {
        if (targets.Count == 1)
        {
            // Si només hi ha un jugador actiu, segueix-lo directament
            Vector3 targetPosition = new Vector3(targets[0].position.x, targets[0].position.y + yOffset, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
        else
        {
            // Si hi ha més d'un jugador, segueix el centre dels jugadors actius
            Vector3 centerPoint = GetCenterPoint();
            Vector3 newPosition = new Vector3(centerPoint.x, centerPoint.y + yOffset, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
        }
    }

    Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
        {
            return targets[0].position;
        }

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 1; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.center;
    }

    void AdjustZoom()
    {
        if (targets.Count > 1)
        {
            float greatestDistance = GetGreatestDistance();
            float newSize = Mathf.Lerp(minSize, maxSize, greatestDistance / zoomLimiter);
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, newSize, Time.deltaTime);
        }
        else if (targets.Count == 1)
        {
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, singlePlayerZoom, Time.deltaTime);
        }
    }

    float GetGreatestDistance()
    {
        if (targets.Count <= 1) return 0;

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 1; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.size.y; // Utilitza només la distància en l'eix Y per al zoom
    }

    public void DeactivatePlayer(Transform player)
    {
        if (player != null && player.gameObject.activeSelf)
        {
            player.gameObject.SetActive(false); // Desactiva l'objecte
            targets.Remove(player); // Elimina el jugador de la llista de seguiment
            Debug.Log($"Jugador {player.name} desactivat.");
        }
    }
}
