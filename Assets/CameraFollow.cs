using UnityEngine;
using System.Collections.Generic;

public class CameraFollow : MonoBehaviour
{
    public List<Transform> targets; // Llista d'objectes a seguir
    public Vector3 offset;          // Offset entre la càmera i els objectes
    public float smoothTime = 0.3f; // Temps de suavitzat per al moviment
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        // Si no hi ha cap objecte, no fer res
        if (targets.Count == 0)
            return;

        // Calcular la posició mitjana
        Vector3 centerPoint = GetCenterPoint();

        // Ajustar la posició de la càmera
        Vector3 newPosition = centerPoint + offset;
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    Vector3 GetCenterPoint()
    {
        // Si només hi ha un objecte, retorna la seva posició
        if (targets.Count == 1)
            return targets[0].position;

        // Calcular el centre entre tots els objectes
        Bounds bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 1; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.center;
    }

    public void RemoveTarget(Transform target)
    {
        // Eliminar un objecte de la llista
        if (targets.Contains(target))
        {
            targets.Remove(target);
        }
    }
}
