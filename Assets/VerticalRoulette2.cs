using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class VerticalRoulette2 : MonoBehaviour
{
    public VerticalRoulette verticalRoulette;
    public GameManager gameManager;
    public DataManager dataManager;
    public VerticalRoulette3 verticalRoulette3;
    public GameObject[] imatges; // Opcions de la ruleta
    public float[] probabilitats; // Probabilitats (pesos) per a cada imatge
    public float velocitatInicial = 0.05f; // Velocitat inicial de la ruleta
    public float tempsMin = 5f; // Temps mínim de rotació
    public float tempsMax = 6f; // Temps màxim de rotació
    public float numTriades = 3f; // Nombre d'imatges seleccionades per rotació
    public Camera cameraPrincipal; // Referència a la càmera principal
    public float offsetY = 2f; // Offset en Y per posicionar la ruleta més amunt

    private string resultatRuleta1; // Guarda el resultat de la ruleta
    private List<GameObject> imatgesTriades; // Imatges seleccionades aleatòriament
    private Coroutine rotacioCoroutine; // Per controlar la corutina de rotació

    void Start()
    {
        if (cameraPrincipal == null)
        {
            cameraPrincipal = Camera.main; // Assigna la càmera principal si no està configurada
        }
    }

    void Update()
    {
        // Assegura que la ruleta estigui sempre centrada a la càmera amb un offset en Y
        if (cameraPrincipal != null)
        {
            //Vector3 posicioCamera = cameraPrincipal.transform.position;
            //transform.position = new Vector3(posicioCamera.x, posicioCamera.y + offsetY, 0); // Aplica el desplaçament vertical
        }
    }

    public void IniciarRuleta()
    {
        // Només permet iniciar una rotació si no hi ha cap rotació en marxa
        if (rotacioCoroutine == null)
        {
            rotacioCoroutine = StartCoroutine(RotarImatges());
        }
        else
        {
            Debug.LogWarning("La ruleta ja està girant!");
        }
    }

    IEnumerator RotarImatges()
    {
        // Selecciona les imatges per aquesta rotació
        imatgesTriades = SeleccionarImatgesTriades();

        // Desactiva les imatges no seleccionades
        foreach (GameObject imatge in imatges)
        {
            imatge.SetActive(imatgesTriades.Contains(imatge));
        }

        float tempsTotal = Random.Range(tempsMin, tempsMax);
        float velocitatActual = velocitatInicial;
        int indexActual = 0;
        float tempsTranscorregut = 0f;

        GameObject imatgeFinal = null;

        while (tempsTranscorregut < tempsTotal)
        {
            // Amaga totes les imatges triades
            foreach (GameObject imatge in imatgesTriades)
            {
                imatge.SetActive(false);
            }

            // Mostra la imatge actual
            imatgesTriades[indexActual].SetActive(true);

            // Assigna l'última imatge mostrada abans de l'aturada
            imatgeFinal = imatgesTriades[indexActual];

            // Espera segons la velocitat actual
            yield return new WaitForSeconds(velocitatActual);

            // Incrementa el temps transcorregut i mou l'índex
            tempsTranscorregut += velocitatActual;
            indexActual = (indexActual + 1) % imatgesTriades.Count;

            // Redueix la velocitat progressivament
            float percentatgeCompletat = tempsTranscorregut / tempsTotal;
            velocitatActual = Mathf.Lerp(velocitatInicial, 0.5f, percentatgeCompletat);
        }

        // Quan s'acaba, assigna el resultat real
        resultatRuleta1 = imatgeFinal.name;
        Debug.Log("Resultat de la ruleta: " + resultatRuleta1);

        // Executa accions segons el resultat
        AccioSegonsResultat(resultatRuleta1);

        // Marca que la rotació ha finalitzat
        rotacioCoroutine = null;
    }

    private List<GameObject> SeleccionarImatgesTriades()
    {
        if (probabilitats.Length != imatges.Length)
        {
            Debug.LogError("El nombre de probabilitats no coincideix amb el nombre d'imatges!");
            return new List<GameObject>();
        }

        float sumaTotal = probabilitats.Sum();
        List<GameObject> triades = new List<GameObject>();

        while (triades.Count < numTriades)
        {
            float valorAleatori = Random.Range(0, sumaTotal);
            float acumulador = 0f;

            for (int i = 0; i < probabilitats.Length; i++)
            {
                acumulador += probabilitats[i];
                if (valorAleatori <= acumulador && !triades.Contains(imatges[i]))
                {
                    triades.Add(imatges[i]);
                    break;
                }
            }
        }

        Debug.Log("Imatges triades: " + string.Join(", ", triades.Select(t => t.name)));
        return triades;
    }

    private void AccioSegonsResultat(string resultat)
    {
        switch (resultat)
        {
            case "Imatge1": //ALL
                Debug.Log("Acció: Imatge1 seleccionada!");
                gameManager.ApplyEfect("ALL");
                dataManager.resultatRuleta2 = 1;
                //dataManager.resultatRuleta2 = 1;
                break;
            case "Imatge2": //NONE
                Debug.Log("Acció: Imatge2 seleccionada!");
                gameManager.ApplyEfect("NONE");
                dataManager.resultatRuleta2 = 2;
                StartCoroutine(WaitAndStartRoulette());
                break;
            case "Imatge3":
                Debug.Log("Acció: Imatge1 seleccionada!");
                if (dataManager.jugadorsRestants == 2)
                {
                    gameManager.ApplyEfect("ALL");
                    dataManager.resultatRuleta2 = 1;
                }
                else
                {
                    verticalRoulette3.IniciarRuleta();
                    dataManager.resultatRuleta2 = 3;
                }
                break;
            case "Imatge4":
                Debug.Log("Acció: Imatge1 seleccionada!");
                verticalRoulette3.IniciarRuleta();
                dataManager.resultatRuleta2 = 4;
                break;
            default:
                Debug.Log("Cap acció específica per aquest resultat.");
                break;
        }
    }
    private IEnumerator WaitAndStartRoulette()
    {
        yield return new WaitForSeconds(1f); // Espera 1 segon
        verticalRoulette.IniciarRuleta(); // Inicia la ruleta després del retard
    }
}
