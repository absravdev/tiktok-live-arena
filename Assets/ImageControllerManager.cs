using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Necessari per utilitzar TextMeshPro

public class ImageControllerManager : MonoBehaviour
{
    public Sprite[] sotaImages; // Assigna totes les imatges sota al inspector
    private static ImageControllerManager instance; // Singleton per accedir fàcilment des d'altres scripts

    // Estat dels temporitzadors
    public bool isFirstTimerActive { get; private set; }
    public bool isSecondTimerActive { get; private set; }
    public bool isThirdTimerActive { get; private set; }

    // TextMeshPro per mostrar els temporitzadors
    public TextMeshProUGUI firstTimerText;
    public TextMeshProUGUI secondTimerText;
    public TextMeshProUGUI thirdTimerText;

    void Awake()
    {
        // Configura aquest objecte com a singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Ja existeix una instància de ImageControllerManager!");
            Destroy(gameObject);
        }

        // Reinicia el pool de sotaImages quan el joc comença
        ResetSotaImages();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            AsignarImatgePerPosicio(2);
        }

        if (Input.GetKeyDown(KeyCode.T)) // Exemples per iniciar els temporitzadors
        {
            StartTimers();
        }

        // Actualitza els textos dels temporitzadors en temps real
        UpdateTimerTexts();
    }

    public static Sprite[] GetSotaImages()
    {
        if (instance == null)
        {
            Debug.LogError("No hi ha cap instància de ImageControllerManager a l'escena!");
            return null;
        }
        return instance.sotaImages;
    }

    public static void ResetSotaImages()
    {
        // Reinicialitzar totes les imatges sota quan calgui
        ImageController.ResetImagePool();
    }

    public static void AsignarImatgePerPosicio(int posicioBuscada)
    {
        // Busca tots els objectes amb el component ImageController
        ImageController[] controllers = FindObjectsOfType<ImageController>();

        foreach (ImageController controller in controllers)
        {
            if (controller.posicio == posicioBuscada)
            {
                controller.AsignarImatge(); // Crida el mètode de canvi d'imatge
                return; // Si ja hem trobat l'objecte amb la posició, no cal continuar
            }
        }

        Debug.LogWarning($"No s'ha trobat cap ImageController amb la posició {posicioBuscada}.");
    }

    // --- FUNCIONALITAT DELS TEMPORITZADORS ---
    public void StartTimers()
    {
        StartCoroutine(RunTimers());
    }

    private IEnumerator RunTimers()
    {
        Debug.Log("Iniciant el primer temporitzador de 5 segons...");
        isFirstTimerActive = true;
        yield return new WaitForSeconds(5f);
        isFirstTimerActive = false;
        Debug.Log("Primer temporitzador completat.");

        Debug.Log("Iniciant el segon temporitzador de 5 segons...");
        isSecondTimerActive = true;
        yield return new WaitForSeconds(5f);
        isSecondTimerActive = false;
        Debug.Log("Segon temporitzador completat.");

        Debug.Log("Iniciant el tercer temporitzador de 5 segons...");
        isThirdTimerActive = true;
        yield return new WaitForSeconds(5f);
        isThirdTimerActive = false;
        Debug.Log("Tercer temporitzador completat.");
    }

    public static bool IsFirstTimerActive()
    {
        return instance != null && instance.isFirstTimerActive;
    }

    public static bool IsSecondTimerActive()
    {
        return instance != null && instance.isSecondTimerActive;
    }

    public static bool IsThirdTimerActive()
    {
        return instance != null && instance.isThirdTimerActive;
    }

    private void UpdateTimerTexts()
    {
        // Actualitza els textos segons l'estat dels temporitzadors
        firstTimerText.text = isFirstTimerActive ? "Primer temporitzador: Actiu" : "Primer temporitzador: Inactiu";
        secondTimerText.text = isSecondTimerActive ? "Segon temporitzador: Actiu" : "Segon temporitzador: Inactiu";
        thirdTimerText.text = isThirdTimerActive ? "Tercer temporitzador: Actiu" : "Tercer temporitzador: Inactiu";
    }
}
