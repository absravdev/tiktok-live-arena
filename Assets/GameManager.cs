using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Necessari per utilitzar UI (per a la imatge de fade)
using TMPro; // Necessari per utilitzar TextMeshPro

public class GameManager : MonoBehaviour
{
    public CameraFollow2D cameraFollow2d;
    private bool victoryRecorded = false;
    public VictoryManager victoryManager;
    private Coroutine countdownCoroutine; // Referència a la Coroutine del temporitzador

    public TextMeshProUGUI countdownText; // Text per mostrar el temporitzador

    public TikTokLiveBasicInfo tikTokLiveBasicInfo;
    public DataManager dataManager;
    public string resRule1;
    // Enumeració per als estats del joc
    public enum GameState
    {
        State1, // Estat inicial
        State2, // Estat alternatiu
        State3,
        State4

    }

    public GameState currentState = GameState.State1; // Estat inicial

    // Referències al HUD i altres elements
    public VerticalRoulette verticalRoulette;
    public GameObject jugador1;
    public GameObject jugador2;
    public GameObject jugador3;
    public Transform jugador1a;
    public Transform jugador2a;
    public Transform jugador3a;
    public string jugador1name;
    public string jugador2name;
    public string jugador3name;
    public TextMeshProUGUI guanyador;
    public TextMeshProUGUI jugador1nameText;
    public TextMeshProUGUI jugador2nameText;
    public TextMeshProUGUI jugador3nameText;

    public GameObject hudState1; // HUD específic per a l'estat 1
    public GameObject hudState2; // HUD específic per a l'estat 2
    public GameObject hudState3; // HUD específic per a l'estat 2
    public GameObject hudState4;

    public TextMeshProUGUI text1; // Text per al jugador amb la Y més gran
    public TextMeshProUGUI text2; // Text per al segon jugador
    public TextMeshProUGUI text3; // Text per al jugador amb la Y més petita

    public Image fadeImage; // Imatge negra per al fade (assigna-la des de l'Inspector)

    public bool reiniciarronda = true;
    private void Start()
    {
        UpdateHUD(); // Assegura't que el HUD inicial es configura correctament
        UpdatePlayerVisibility(); // Inicialitza la visibilitat dels jugadors
        //verticalRoulette.IniciarRuleta();
        // Assegura't que el fadeImage comença completament transparent
        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 0);
    }

    private void Update()
    {
        Debug.Log(jugador1name);

        if (dataManager.playerAplicat == dataManager.jugadorsRestants)
        {
            StartCoroutine(WaitAndStartRoulette());
            dataManager.playerAplicat = 0;
            dataManager.playerGirat = 0;
        }

        if (currentState == GameState.State2)
        {
            jugador1name = jugador1nameText.text;
            jugador2name = jugador2nameText.text;
            jugador3name = jugador3nameText.text;

            if (reiniciarronda)
            {
                verticalRoulette.IniciarRuleta();
                reiniciarronda = false;
            }

            // Només registra la victòria si encara no s'ha fet
            if (!victoryRecorded)
            {
                if ((!jugador1 || !jugador1.activeSelf) &&
                    (!jugador2 || !jugador2.activeSelf) &&
                    (jugador3 && jugador3.activeSelf))
                {
                    int victories = GetVictories(jugador3name);
                    guanyador.text = $"{jugador3name} - Victòries: {victories}";
                    StartCoroutine(UpdateWinnerTextWithDelay(jugador3name));
                    StartCoroutine(ChangeGameStateWithFadeEspecial(GameState.State3));
                    victoryRecorded = true; // Marca que ja s'ha registrat
                }
                else if ((!jugador1 || !jugador1.activeSelf) &&
                         (!jugador3 || !jugador3.activeSelf) &&
                         (jugador2 && jugador2.activeSelf))
                {
                    int victories = GetVictories(jugador2name);
                    guanyador.text = $"{jugador2name} - Victòries: {victories}";
                    StartCoroutine(UpdateWinnerTextWithDelay(jugador2name));
                    StartCoroutine(ChangeGameStateWithFadeEspecial(GameState.State3));
                    victoryRecorded = true;
                }
                else if ((!jugador2 || !jugador2.activeSelf) &&
                         (!jugador3 || !jugador3.activeSelf) &&
                         (jugador1 && jugador1.activeSelf))
                {
                    int victories = GetVictories(jugador1name);
                    guanyador.text = $"{jugador1name} - Victòries: {victories}";
                    StartCoroutine(UpdateWinnerTextWithDelay(jugador1name));
                    StartCoroutine(ChangeGameStateWithFadeEspecial(GameState.State3));
                    victoryRecorded = true;
                }

            }

            UpdatePlayerPositions();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            StartCoroutine(ChangeGameStateWithFadeNormal(GameState.State3));
            victoryRecorded = false; // Reinicia l'estat al canviar d'estat
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            StartCoroutine(ChangeGameStateWithFadeNormal(GameState.State1));
            victoryRecorded = false; // Reinicia l'estat al canviar d'estat
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartCoroutine(ChangeGameStateWithFadeNormal(GameState.State4));
            victoryRecorded = false; // Reinicia l'estat al canviar d'estat
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            reiniciarronda = true;
            StartCoroutine(ChangeGameStateWithFadeNormal(GameState.State2));
            victoryRecorded = false; // Reinicia l'estat al canviar d'estat
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            cameraFollow2d.DeactivatePlayer(jugador3a);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            cameraFollow2d.DeactivatePlayer(jugador2a);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            cameraFollow2d.DeactivatePlayer(jugador1a);
        }
    }

    private void UpdatePlayerPositions()
    {
        // Crea una llista amb els jugadors i els seus identificadors
        List<(GameObject jugador, int id)> jugadors = new List<(GameObject, int)>
    {
        (jugador1, 1),
        (jugador2, 2),
        (jugador3, 3)
    };

        // Elimina els jugadors que han estat destruïts
        jugadors.RemoveAll(j => j.jugador == null);

        // Ordena els jugadors restants per la seva posició en Y de major a menor
        jugadors.Sort((a, b) => b.jugador.transform.position.y.CompareTo(a.jugador.transform.position.y));

        // Assigna les posicions al DataManager segons l'ordre
        for (int i = 0; i < jugadors.Count; i++)
        {
            int posicio = i + 1; // La posició és 1 per al primer, 2 per al segon, etc.
            switch (jugadors[i].id)
            {
                case 1:
                    dataManager.posicióJugador1 = posicio;
                    break;
                case 2:
                    dataManager.posicióJugador2 = posicio;
                    break;
                case 3:
                    dataManager.posicióJugador3 = posicio;
                    break;
            }
        }

        // Si algun jugador ha estat eliminat, assigna una posició nul·la
        if (jugadors.FindIndex(j => j.id == 1) == -1)
            dataManager.posicióJugador1 = 0;
        if (jugadors.FindIndex(j => j.id == 2) == -1)
            dataManager.posicióJugador2 = 0;
        if (jugadors.FindIndex(j => j.id == 3) == -1)
            dataManager.posicióJugador3 = 0;

        // Mostra la classificació actual als textos
        if (jugadors.Count > 0 && jugadors[0].jugador != null)
        {
            text1.text = $"1r: {jugadors[0].jugador.name} - Y: {jugadors[0].jugador.transform.position.y:F2}";
        }
        else
        {
            text1.text = "1r: N/A";
        }

        if (jugadors.Count > 1 && jugadors[1].jugador != null)
        {
            text2.text = $"2n: {jugadors[1].jugador.name} - Y: {jugadors[1].jugador.transform.position.y:F2}";
        }
        else
        {
            text2.text = "2n: N/A";
        }

        if (jugadors.Count > 2 && jugadors[2].jugador != null)
        {
            text3.text = $"3r: {jugadors[2].jugador.name} - Y: {jugadors[2].jugador.transform.position.y:F2}";
        }
        else
        {
            text3.text = "3r: N/A";
        }

    }


    private IEnumerator ChangeGameStateWithFadeNormal(GameState newState)
    {
        // Fade out
        yield return StartCoroutine(FadeToBlack());

        if (newState == GameState.State2)
        {
            reiniciarronda = true;
        }
        if (currentState == GameState.State1 && newState == GameState.State2)
        {
            tikTokLiveBasicInfo.ClearDonatorList1();
        }
        if (currentState == GameState.State2 && newState == GameState.State1)
        {
            tikTokLiveBasicInfo.ClearDonatorList2();
        }
        // Canvia l'estat després del fade out
        currentState = newState;
        UpdateHUD(); // Actualitza el HUD segons el nou estat
        UpdatePlayerVisibility(); // Actualitza la visibilitat dels jugadors segons l'estat

        // Fade in
        yield return StartCoroutine(FadeToClear());
    }
    private IEnumerator ChangeGameStateWithFadeEspecial(GameState newState)
    {
        // Fade out
        yield return StartCoroutine(FadeToBlack());
        if (currentState == GameState.State1 && newState == GameState.State2)
        {
            tikTokLiveBasicInfo.ClearDonatorList1();
        }
        if (currentState == GameState.State2 && newState == GameState.State1)
        {
            tikTokLiveBasicInfo.ClearDonatorList2();
        }
        // Canvia l'estat després del fade out
        currentState = newState;
        UpdateHUD(); // Actualitza el HUD segons el nou estat
        //UpdatePlayerVisibility(); // Actualitza la visibilitat dels jugadors segons l'estat

        // Fade in
        yield return StartCoroutine(FadeToClear());
    }

    private IEnumerator FadeToBlack()
    {
        for (float t = 0; t <= 1; t += Time.deltaTime * 2) // Velocitat de fade
        {
            if (fadeImage != null)
                fadeImage.color = new Color(0, 0, 0, t); // Augmenta l'opacitat
            yield return null;
        }
    }

    private IEnumerator FadeToClear()
    {
        for (float t = 1; t >= 0; t -= Time.deltaTime * 2) // Velocitat de fade
        {
            if (fadeImage != null)
                fadeImage.color = new Color(0, 0, 0, t); // Redueix l'opacitat
            yield return null;
        }
    }

    private void UpdateHUD()
    {
        // Activa/desactiva els elements del HUD segons l'estat actual
        if (hudState1 != null) hudState1.SetActive(currentState == GameState.State1);
        if (hudState2 != null) hudState2.SetActive(currentState == GameState.State2);
        if (hudState3 != null) hudState3.SetActive(currentState == GameState.State3);
        if (hudState4 != null) hudState4.SetActive(currentState == GameState.State4);
        if (currentState == GameState.State1)
        {
            StartCountdown();
        }
        else if (currentState == GameState.State3)
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine); // Assegura que no hi hagi un compte enrere actiu
            }
            countdownCoroutine = StartCoroutine(CountdownAndChangeToState1()); // Inicia el compte enrere
        }
    }

    private void UpdatePlayerVisibility()
    {
        // Activa els jugadors només si estem en l'estat 2
        bool isActive = currentState == GameState.State2;
        Debug.Log(currentState);

        if (jugador1 != null)
        {
            jugador1.SetActive(isActive);
            if (isActive) jugador1.GetComponent<MoveForward>().ResetObject(); // Reinicia
        }

        if (jugador2 != null)
        {
            jugador2.SetActive(isActive);
            if (isActive) jugador2.GetComponent<MoveForward>().ResetObject(); // Reinicia
        }

        if (jugador3 != null)
        {
            jugador3.SetActive(isActive);
            if (isActive) jugador3.GetComponent<MoveForward>().ResetObject(); // Reinicia
        }
        if (verticalRoulette != null)
        {
            if (isActive)
            {
                //verticalRoulette.ResetRoulette(); // Reinicia visualment la ruleta
                //verticalRoulette.IniciarRuleta(); // Comença a girar
            }
        }
    }
    public void ApplyEfect(string topOption)
    {
        DataManager dataManager = FindObjectOfType<DataManager>();

        switch (topOption)
        {
            case "1/2": // Aplica l'efecte al jugador en 1r i 2n lloc
                jugador1.GetComponent<MoveForward>().ApplyBoost(2f, 1f);
                jugador2.GetComponent<MoveForward>().ApplyBoost(2f, 1f);
                break;

            case "2/3": // Aplica l'efecte al jugador en 1r i 3r lloc
                jugador2.GetComponent<MoveForward>().ApplyBoost(2f, 1f);
                jugador3.GetComponent<MoveForward>().ApplyBoost(2f, 1f);
                break;
            case "3/1": // Aplica l'efecte al jugador en 2n i 3r lloc
                jugador1.GetComponent<MoveForward>().ApplyBoost(2f, 1f);
                jugador3.GetComponent<MoveForward>().ApplyBoost(2f, 1f);
                break;

            case "1": // Aplica l'efecte al jugador 1
                jugador1.GetComponent<MoveForward>().ApplyBoost(2f, 1f);
                break;
            case "2": // Aplica l'efecte al jugador 2
                jugador2.GetComponent<MoveForward>().ApplyBoost(2f, 1f);
                break;
            case "3": // Aplica l'efecte al jugador 3
                jugador3.GetComponent<MoveForward>().ApplyBoost(2f, 1f);
                break;
            case "ALL": // Aplica l'efecte al jugador 3
                jugador1.GetComponent<MoveForward>().ApplyBoost(2f, 1f);
                jugador2.GetComponent<MoveForward>().ApplyBoost(2f, 1f);
                jugador3.GetComponent<MoveForward>().ApplyBoost(2f, 1f);
                break;
            case "NONE": // Aplica l'efecte al jugador 3
                break;
            case "player1": // Aplica l'efecte al jugador 3
                jugador1.GetComponent<MoveForward>().ApplyBoost(2f, 1f);
                break;
            case "player2": // Aplica l'efecte al jugador 3
                jugador2.GetComponent<MoveForward>().ApplyBoost(2f, 1f);
                break;
            case "player3": // Aplica l'efecte al jugador 3
                jugador3.GetComponent<MoveForward>().ApplyBoost(2f, 1f);
                break;
            default:
                Debug.LogWarning("Opció no vàlida o cap efecte aplicat.");
                break;
        }
        StartCoroutine(WaitAndStartRoulette());
    }
    private IEnumerator WaitAndStartRoulette()
    {
        yield return new WaitForSeconds(1f); // Espera 1 segon
        verticalRoulette.IniciarRuleta(); // Inicia la ruleta després del retard
    }
    private IEnumerator CountdownAndChangeState()
    {
        int countdown = 30; // Temps inicial del compte enrere en segons

        while (countdown > 0)
        {
            // Actualitza el text del temporitzador
            if (countdownText != null)
            {
                countdownText.text = $"Temps restant: {countdown}s";
            }

            yield return new WaitForSeconds(1); // Espera 1 segon
            countdown--; // Redueix el temps restant
        }
        while (tikTokLiveBasicInfo.donatorList.Count < 3)
        {
            if (countdownText != null)
            {
                countdownText.text = "Esperant mínim 3 donadors...";
            }

            yield return null; // Espera un frame abans de tornar a comprovar
        }

        // Quan s'acabi el compte enrere, canvia automàticament a State2
        StartCoroutine(ChangeGameStateWithFadeNormal(GameState.State2));
    }

    private void StartCountdown()
    {
        // Atura qualsevol compte enrere actiu
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        // Reinicia el compte enrere
        countdownCoroutine = StartCoroutine(CountdownAndChangeState());
    }
    private void UpdateVictories(string playerName)
    {
        // Carrega les dades existents
        List<PlayerData> players = victoryManager.LoadPlayerData();

        // Busca si el jugador ja hi és
        PlayerData player = players.Find(p => p.Name == playerName);

        if (player != null)
        {
            // Si existeix, incrementa les victòries
            player.Victories++;
        }
        else
        {
            // Si no existeix, afegeix-lo amb 1 victòria
            players.Add(new PlayerData(playerName, 1));
        }

        // Desa les dades actualitzades
        victoryManager.SavePlayerData(players);

        Debug.Log($"Jugador {playerName} té ara {player?.Victories ?? 1} victòries.");
    }
    private IEnumerator CountdownAndChangeToState1()
    {
        int countdown = 10; // Temps inicial del compte enrere en segons

        while (countdown > 0)
        {
            // Actualitza el text del temporitzador
            if (countdownText != null)
            {
                countdownText.text = $"Temps restant: {countdown}s";
            }

            yield return new WaitForSeconds(1); // Espera 1 segon
            countdown--; // Redueix el temps restant
        }

        // Quan s'acabi el compte enrere, canvia a State1
        StartCoroutine(ChangeGameStateWithFadeNormal(GameState.State1));
    }
    private int GetVictories(string playerName)
    {
        List<PlayerData> players = victoryManager.LoadPlayerData();
        PlayerData player = players.Find(p => p.Name == playerName);
        return player != null ? player.Victories : 0;
    }

    private IEnumerator UpdateWinnerTextWithDelay(string playerName)
    {
        yield return new WaitForSeconds(3); // Espera 2 segons

        UpdateVictories(playerName); // Incrementa les victòries
        int updatedVictories = GetVictories(playerName); // Obté el nou nombre de victòries
        guanyador.text = $"{playerName} - Victòries: {updatedVictories}"; // Actualitza el text del guanyador
    }

}
