using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageController : MonoBehaviour
{
    public int posicio; // Número de la posició assignada manualment
    public Sprite tapaImage; // Imatge inicial (tapa)
    private static List<Sprite> sotaImagePool; // Pool global de imatges que no s'han utilitzat

    public bool allowRepetitions = false; // Permet que les imatges es puguin repetir

    private Image imageComponent; // Component Image de l'objecte
    private Sprite sotaImageActual; // La imatge sota assignada actualment

    public float fadeDuration = 1f; // Duració del fade

    void Start()
    {
        // Inicialitzar el component Image
        imageComponent = GetComponent<Image>();
        if (imageComponent == null)
        {
            Debug.LogError("No s'ha trobat un component Image!");
            return;
        }

        // Assignar la imatge de tapa inicial
        imageComponent.sprite = tapaImage;

        // Inicialitzar el pool d'imatges si no està fet
        if (sotaImagePool == null || sotaImagePool.Count == 0)
        {
            ResetImagePool();
        }
    }

    public static void ResetImagePool()
    {
        Sprite[] sotaImages = ImageControllerManager.GetSotaImages();
        if (sotaImages == null || sotaImages.Length == 0)
        {
            Debug.LogError("No s'han trobat imatges de sota al ImageControllerManager!");
            return;
        }
        sotaImagePool = new List<Sprite>(sotaImages);
    }

    public void AsignarImatge()
    {
        // Reinicialitzar el pool si està buit i no permetem repeticions
        if (!allowRepetitions && sotaImagePool.Count == 0)
        {
            ResetImagePool();
        }

        // Obtenir una nova imatge
        sotaImageActual = allowRepetitions ? GetRandomSotaImage() : GetUniqueSotaImage();

        // Fer el fade out de l'actual i fade in de la nova
        StartCoroutine(FadeOutAndIn(imageComponent.sprite, sotaImageActual));
    }

    private IEnumerator FadeOutAndIn(Sprite oldSprite, Sprite newSprite)
    {
        float elapsedTime = 0f;
        Color imageColor = imageComponent.color;

        // Fade out
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            imageColor.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            imageComponent.color = imageColor;
            yield return null;
        }

        // Assignar la nova imatge
        imageComponent.sprite = newSprite;

        // Fade in
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            imageColor.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            imageComponent.color = imageColor;
            yield return null;
        }

        // Assegurar que l'alpha és 1 al final
        imageColor.a = 1f;
        imageComponent.color = imageColor;
    }

    private Sprite GetUniqueSotaImage()
    {
        // Obtenir una imatge única del pool i eliminar-la
        int randomIndex = Random.Range(0, sotaImagePool.Count);
        Sprite chosenImage = sotaImagePool[randomIndex];
        sotaImagePool.RemoveAt(randomIndex);
        return chosenImage;
    }

    private Sprite GetRandomSotaImage()
    {
        // Obtenir una imatge aleatòria sense eliminar-la del pool
        int randomIndex = Random.Range(0, sotaImagePool.Count);
        return sotaImagePool[randomIndex];
    }
}
