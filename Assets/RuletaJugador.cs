using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI; // Per a la imatge negra si cal

public class RuletaJugador : MonoBehaviour
{
    public DataManager dataManager;
    public MoveForward moveForward;
    public GameObject[] imatges; // Opcions de la ruleta del jugador
    public float[] probabilitats; // Probabilitats (pesos) per a cada imatge
    public float velocitatInicial = 0.05f; // Velocitat inicial de la ruleta
    public float tempsMin = 3f; // Temps mínim de rotació
    public float tempsMax = 4f; // Temps màxim de rotació
    public int numTriades = 3; // Nombre d'imatges seleccionades
    public Transform jugador; // Referència al Transform del jugador
    public Vector3 offset = new Vector3(0, 1f, 0); // Desplaçament respecte al jugador

    public GameObject imatgeNegre; // Imatge negra mostrada si el jugador és null
    public GameObject imatgeCreu; // Imatge negra mostrada si el jugador és null
    public GameObject imatgeInterrogant; // Imatge negra mostrada si el jugador és null
    private string resultatJugador; // Resultat de la ruleta del jugador
    private List<GameObject> imatgesTriades; // Les imatges seleccionades aleatòriament
    private Coroutine rotacioCoroutine; // Per controlar la corutina de rotació
    void Start()
    {
        if (imatgeInterrogant != null)
        {
            imatgeInterrogant.SetActive(true);
        }
    }
    void Update()
    {
        // Si el jugador és null, atura la ruleta i mostra la imatge negra
        if (jugador == null)
        {
            if (rotacioCoroutine != null)
            {
                StopCoroutine(rotacioCoroutine); // Atura la corutina si està activa
                rotacioCoroutine = null;
            }

            imatgeNegre.SetActive(true); // Mostra la imatge negra
        }
        else
        {
            imatgeNegre.SetActive(false); // Oculta la imatge negra
            //transform.position = jugador.position + offset; // Actualitza la posició
        }
    }

    public void IniciarRuleta()
    {
        if (imatgeInterrogant != null)
        {
            imatgeInterrogant.SetActive(false);
        }
        // Selecciona les imatges aleatòriament basades en les probabilitats
        imatgesTriades = SeleccionarImatgesTriades();

        // Desactiva les imatges que no han estat seleccionades
        foreach (GameObject imatge in imatges)
        {
            imatge.SetActive(imatgesTriades.Contains(imatge));
        }

        // Comença la rotació amb les imatges triades si no està ja activa
        if (rotacioCoroutine == null)
        {
            rotacioCoroutine = StartCoroutine(RotarImatges());
        }
    }

    IEnumerator RotarImatges()
    {
        float tempsTotal = Random.Range(tempsMin, tempsMax);
        float velocitatActual = velocitatInicial;
        int indexActual = 0;
        float tempsTranscorregut = 0f;

        GameObject imatgeFinal = null; // Variable per guardar l'última imatge mostrada

        while (tempsTranscorregut < tempsTotal)
        {
            // Comprova si el jugador és null i atura la rotació
            if (jugador == null)
            {
                Debug.Log("Jugador eliminat, aturant ruleta.");
                yield break; // Finalitza la corutina
            }

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
        resultatJugador = imatgeFinal.name;

        Debug.Log("Resultat jugador: " + resultatJugador);

        // Reseteja la referència a la corutina
        AccioSegonsResultat(resultatJugador);
        rotacioCoroutine = null;
    }

    private List<GameObject> SeleccionarImatgesTriades()
    {
        // Comprova que les probabilitats tenen la mateixa longitud que les imatges
        if (probabilitats.Length != imatges.Length)
        {
            Debug.LogError("El nombre de probabilitats no coincideix amb el nombre d'imatges!");
            return new List<GameObject>();
        }

        // Calcula la suma total de les probabilitats
        float sumaTotal = probabilitats.Sum();

        // Llista per contenir les imatges triades
        List<GameObject> triades = new List<GameObject>();

        while (triades.Count < numTriades)
        {
            // Genera un número aleatori entre 0 i la suma total
            float valorAleatori = Random.Range(0, sumaTotal);

            // Itera sobre les probabilitats per trobar el resultat seleccionat
            float acumulador = 0f;
            for (int i = 0; i < probabilitats.Length; i++)
            {
                acumulador += probabilitats[i];
                if (valorAleatori <= acumulador)
                {
                    if (!triades.Contains(imatges[i]))
                    {
                        triades.Add(imatges[i]);
                    }
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
            case "Imatge1":
                dataManager.playerGirat++; // Incrementa el comptador de jugadors que han acabat

                if (dataManager.playerGirat == dataManager.jugadorsRestants) // Si tots els jugadors han acabat
                {
                    // Busca totes les instàncies de RuletaJugador i aplica el boost a cada jugador
                    RuletaJugador[] totesLesRuletes = FindObjectsOfType<RuletaJugador>();
                    foreach (RuletaJugador ruleta in totesLesRuletes)
                    {
                        if (ruleta.moveForward != null)
                        {
                            ruleta.moveForward.ApplyBoost(dataManager.multi, 1f);
                            dataManager.playerAplicat++;
                        }
                    }
                    // Reinicia el comptador per a la següent ronda
                    dataManager.playerGirat = 0;
                }
                break;

            case "Imatge2":
                dataManager.playerGirat++; // Incrementa el comptador de jugadors que han acabat
                if (dataManager.playerGirat == dataManager.jugadorsRestants) // Si tots els jugadors han acabat
                {
                    // Busca totes les instàncies de RuletaJugador i aplica el boost a cada jugador
                    RuletaJugador[] totesLesRuletes = FindObjectsOfType<RuletaJugador>();
                    foreach (RuletaJugador ruleta in totesLesRuletes)
                    {
                        if (ruleta.moveForward != null)
                        {
                            ruleta.moveForward.ApplyBoost(dataManager.multi, 1f);
                            dataManager.playerAplicat++;
                        }
                    }
                    // Reinicia el comptador per a la següent ronda
                    dataManager.playerGirat = 0;
                }
                break;

            case "Imatge3":
                dataManager.playerGirat++; // Incrementa el comptador de jugadors que han acabat
                if (dataManager.playerGirat == dataManager.jugadorsRestants) // Si tots els jugadors han acabat
                {
                    // Busca totes les instàncies de RuletaJugador i aplica el boost a cada jugador
                    RuletaJugador[] totesLesRuletes = FindObjectsOfType<RuletaJugador>();
                    foreach (RuletaJugador ruleta in totesLesRuletes)
                    {
                        if (ruleta.moveForward != null)
                        {
                            ruleta.moveForward.ApplyBoost(dataManager.multi, 1f);
                            dataManager.playerAplicat++;
                        }
                    }
                    // Reinicia el comptador per a la següent ronda
                    dataManager.playerGirat = 0;
                }
                break;

            case "Image4":
                //playerMove.ApplyBoost(2f, 1f); // Augmenta la velocitat en 2 durant 1 segon
                break;

            case "Image5":
                //playerMove.ApplyBoost(2f, 1f); // Augmenta la velocitat en 2 durant 1 segon
                break;

            case "Image6":
                //playerMove.ApplyBoost(2f, 1f); // Augmenta la velocitat en 2 durant 1 segon
                break;


            case "Retroces":
                Debug.Log("Acció: El jugador retrocedeix una unitat!");
                break;

            default:
                Debug.Log("Cap acció específica per aquest resultat.");
                break;
        }
        StartCoroutine(ReactivarImatgeInterrogant());
    }
    private IEnumerator ReactivarImatgeInterrogant()
    {
        yield return new WaitForSeconds(1f);
        imatgeInterrogant.SetActive(true); // Torna a activar la imatge d'interrogant
    }

}
