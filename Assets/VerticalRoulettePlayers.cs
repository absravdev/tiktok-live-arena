using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class VerticalRoulettePlayers : MonoBehaviour
{
    public DataManager dataManager;
    public GameManager gameManager;
    public MoveForward MoveForwardPlayer1;
    public MoveForward MoveForwardPlayer2;
    public MoveForward MoveForwardPlayer3;
    public Transform player1;
    public Transform player2;
    public Transform player3;
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
        dataManager.jugadorsRestants=0;
        if (player1 != null)
        {
            dataManager.jugadorsRestants++;
        }
        if (player2 != null)
        {
            dataManager.jugadorsRestants++;
        }
        if (player3 != null)
        {
            dataManager.jugadorsRestants++;
        }
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

        // Desactiva totes les imatges abans de començar
        foreach (GameObject imatge in imatges)
        {
            imatge.SetActive(false);
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

        // Manté l'última imatge visible durant 1 segon
        if (imatgeFinal != null)
        {
            imatgeFinal.SetActive(true);
            yield return new WaitForSeconds(2f);
        }

        // Executa accions segons el resultat
        AccioSegonsResultat(resultatRuleta1);

        // Amaga totes les imatges al final de la rotació
        foreach (GameObject imatge in imatges)
        {
            imatge.SetActive(false);
        }

        // Marca que la rotació ha finalitzat
        rotacioCoroutine = null;
    }



    private List<GameObject> SeleccionarImatgesTriades()
    {
        // Llista per contenir les imatges disponibles en funció dels jugadors actius
        List<GameObject> imatgesDisponibles = new List<GameObject>();
        List<float> probabilitatsDisponibles = new List<float>();

        // Comprova si cada jugador és actiu i afegeix la seva imatge i probabilitat si ho és
        if (player1 != null)
        {
            imatgesDisponibles.Add(imatges[0]); // Suposem que la imatge del player1 és la primera
            probabilitatsDisponibles.Add(probabilitats[0]);
            dataManager.jugadorsRestants++;
        }
        if (player2 != null)
        {
            imatgesDisponibles.Add(imatges[1]); // Suposem que la imatge del player2 és la segona
            probabilitatsDisponibles.Add(probabilitats[1]);
            dataManager.jugadorsRestants++;
        }
        if (player3 != null)
        {
            imatgesDisponibles.Add(imatges[2]); // Suposem que la imatge del player3 és la tercera
            probabilitatsDisponibles.Add(probabilitats[2]);
            dataManager.jugadorsRestants++;
        }

        // Si no hi ha imatges disponibles, retorna una llista buida
        if (imatgesDisponibles.Count == 0)
        {
            Debug.LogWarning("No hi ha jugadors actius, cap imatge seleccionada.");
            return new List<GameObject>();
        }

        // Calcula la suma total de les probabilitats disponibles
        float sumaTotal = probabilitatsDisponibles.Sum();
        List<GameObject> triades = new List<GameObject>();

        // Determina el nombre d'imatges a triar segons el nombre de jugadors actius
        int numTriadesActual = Mathf.Min(imatgesDisponibles.Count, (int)numTriades);

        while (triades.Count < numTriadesActual)
        {
            float valorAleatori = Random.Range(0, sumaTotal);
            float acumulador = 0f;

            for (int i = 0; i < probabilitatsDisponibles.Count; i++)
            {
                acumulador += probabilitatsDisponibles[i];
                if (valorAleatori <= acumulador && !triades.Contains(imatgesDisponibles[i]))
                {
                    triades.Add(imatgesDisponibles[i]);
                    break;
                }
            }
        }

        Debug.Log("Imatges triades: " + string.Join(", ", triades.Select(t => t.name)));
        return triades;
    }


    private void AccioSegonsResultat(string resultat)
    {
        if (dataManager.resultatRuleta2 == 4)
        {
            switch (resultat)
            {
                case "Imatge1":
                    Debug.Log("Acció: Imatge1 seleccionada!");
                    gameManager.ApplyEfect("player1");
                    break;
                case "Imatge2":
                    Debug.Log("Acció: Imatge2 seleccionada!");
                    gameManager.ApplyEfect("player2");
                    break;
                case "Imatge3":
                    Debug.Log("Acció: Imatge3 seleccionada!");
                    gameManager.ApplyEfect("player3");
                    break;
                default:
                    Debug.Log("Cap acció específica per aquest resultat.");
                    break;
            }
        }
        else if ((dataManager.resultatRuleta2 == 3))
        {
            switch (resultat)
            {
                case "Imatge1":
                    Debug.Log("Acció: Imatge1 seleccionada!");
                    gameManager.ApplyEfect("player2");
                    gameManager.ApplyEfect("player3");
                    break;
                case "Imatge2":
                    Debug.Log("Acció: Imatge2 seleccionada!");
                    gameManager.ApplyEfect("player3");
                    gameManager.ApplyEfect("player1");
                    break;
                case "Imatge3":
                    Debug.Log("Acció: Imatge3 seleccionada!");
                    gameManager.ApplyEfect("player2");
                    gameManager.ApplyEfect("player1");
                    break;
                default:
                    Debug.Log("Cap acció específica per aquest resultat.");
                    break;
            }
        }

    }
}

