using UnityEngine;
using UnityEngine.SceneManagement;
using TikTokLiveSharp.Events;
using TikTokLiveUnity;
using TikTokLiveSharp.Events.Objects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Microsoft.Unity.VisualStudio.Editor;

public class TikTokLiveBasicInfo : MonoBehaviour
{
    public bool player1HaveChoosen = false;
    public bool player2HaveChoosen = false;
    public bool player3HaveChoosen = false;
    public ImageControllerManager imageControllerManager;
    public bool japuc = false;
    public VictoryManager victoryManager;
    public List<Donator> top3DonatorsFixed = new List<Donator>(); // Emmagatzema el Top 3 fix per State2

    private GameManager.GameState previousState; // Estat anterior per detectar canvis

    public GameManager gameManager;
    public long numberOfViewers;
    public TextMeshProUGUI topDonator1name;
    public TextMeshProUGUI topDonator2name;
    public TextMeshProUGUI topDonator3name;
    public TextMeshProUGUI messageDonator1;
    public TextMeshProUGUI messageDonator2;
    public TextMeshProUGUI messageDonator3;
    [SerializeField] private string hostUsername;
    [SerializeField] private TextMeshProUGUI participantText;
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private TextMeshProUGUI donatorCountText;
    [SerializeField] private TextMeshProUGUI numberViewersText;
    [SerializeField] private TextMeshProUGUI top3DonatorsText;
    [SerializeField] private int maxPlayers = 30;

    private TikTokLiveManager liveManager;
    public List<Donator> donatorList = new List<Donator>(); // Llista per emmagatzemar els donadors
    private List<Donator> tempDonatorList = new List<Donator>(); // Llista per emmagatzemar donacions en State2
    public static List<Donator> Top3Donators = new List<Donator>();
    private int simulatedUserCount = 1;

    // Diccionari per als valors dels diferents regals
    private Dictionary<string, int> giftValues = new Dictionary<string, int>()
    {
        // Afegeix més regals amb el seu valor corresponent
        { "Rose", 1 },
        { "TikTok", 5 },
        { "Legendary", 100 }
    };

    private enum State { WaitingRoom, Game }
    private State currentState = State.WaitingRoom;

    private void Start()
    {
        previousState = gameManager.currentState;
        liveManager = TikTokLiveManager.Instance;

        if (liveManager == null)
        {
            // Si no existeix, crea'l
            GameObject liveManagerObject = new GameObject("TikTokLiveManager");
            liveManager = liveManagerObject.AddComponent<TikTokLiveManager>();
            DontDestroyOnLoad(liveManagerObject); // No es destruirà entre escenes
        }

        // Evita duplicar subscripcions als esdeveniments
        liveManager.OnConnected -= OnConnected;
        liveManager.OnRoomUpdate -= OnRoomUpdate;
        liveManager.OnChatMessage -= OnChatMessage;
        liveManager.OnFollow -= OnFollow;
        liveManager.OnGift -= OnGift;

        liveManager.OnConnected += OnConnected;
        liveManager.OnRoomUpdate += OnRoomUpdate;
        liveManager.OnChatMessage += OnChatMessage;
        liveManager.OnFollow += OnFollow;
        liveManager.OnGift += OnGift;

        liveManager.ConnectToStreamAsync(hostUsername);

        UpdateStateText();
        UpdateDonatorCountText();
    }

    private void Update()
    {
        SortAndDisplayDonators();
        List<PlayerData> players = victoryManager.LoadPlayerData();

        // Combina noms i punts amb les victòries
        participantText.text = string.Join("\n", donatorList.Select(d =>
        {
            // Busca el jugador a la llista de victòries
            PlayerData playerData = players.Find(p => p.Name == d.Name);
            int victories = playerData != null ? playerData.Victories : 0; // Si no existeix, les victòries són 0

            // Retorna el text amb el format desitjat
            return $"{d.Name} ({victories} victòries) - {d.Points} punts";
        }));

        if (gameManager.currentState != previousState)
        {
            OnStateChange(previousState, gameManager.currentState);
            previousState = gameManager.currentState; // Actualitza l’estat anterior
        }
        UpdateViewersText();
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("a");
            SimulateGift();

        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            //ToggleState();
        }
    }

    private void ToggleState()
    {
        if (gameManager.currentState == GameManager.GameState.State1)
        {
            donatorList.Clear();
            donatorList.AddRange(tempDonatorList);
            tempDonatorList.Clear();

        }
        else if (gameManager.currentState == GameManager.GameState.State2)
        {
            //tempDonatorList.Clear();
        }

        UpdateStateText();
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene("Game");
    }

    private void SimulateGift()
    {
        string simulatedName = "SimulatedUser" + simulatedUserCount++;

        // Selecciona un regal aleatoriament del diccionari giftValues
        string simulatedGift = giftValues.Keys.ElementAt(Random.Range(0, giftValues.Count));
        int giftAmount = Random.Range(1, 5); // Aleatoriament entre 1 i 4 regals
        int randomValue = Random.Range(1, 1001); // UnityEngine.Random.Range és inclusiu al valor mínim però exclusiu al màxim
        // Multiplica pel nombre de regals enviats
        int giftValue = randomValue;

        Debug.Log($"{simulatedName} ha enviat {giftAmount} regals de tipus: {simulatedGift} amb valor total de {giftValue} punts.");

        UpdateDonator(simulatedName, giftValue);
        SortAndDisplayDonators();
    }

    private void UpdateDonator(string name, int giftValue)
    {
        // Cerca el donador a la llista i actualitza el seu puntatge
        var donator = donatorList.FirstOrDefault(d => d.Name == name);
        if (donator != null)
        {
            donator.Points += giftValue; // Actualitza punts si el donador ja existeix
        }
        else
        {
            donatorList.Add(new Donator { Name = name, Points = giftValue }); // Afegeix un nou donador si no existeix
            Debug.Log($"Nou donador afegit: {name} amb {giftValue} punts");
        }
    }
    private void SortAndDisplayDonators()
    {
        // Ordena la llista de donadors per punts en ordre descendent
        donatorList = donatorList.OrderByDescending(d => d.Points).ToList();

        // Selecciona només els 2 primers donadors disponibles
        var top2Donators = donatorList.Take(3).ToList();

        // Actualitza els textos de la UI amb els noms dels dos primers de donatorList
        if (gameManager.currentState == GameManager.GameState.State1)
        {
            topDonator1name.text = top2Donators.Count > 0 ? top2Donators[0].Name : "N/A";
            topDonator2name.text = top2Donators.Count > 1 ? top2Donators[1].Name : "N/A";
            topDonator3name.text = top2Donators.Count > 2 ? top2Donators[2].Name : "N/A";
            messageDonator1.text = topDonator1name.text; // No mostrar res al tercer lloc
            messageDonator2.text = topDonator2name.text; // No mostrar res al tercer lloc
            messageDonator3.text = topDonator3name.text; // No mostrar res al tercer lloc

            participantText.text = string.Join("\n", donatorList.Select(d => $"{d.Name} - {d.Points} punts"));
            top3DonatorsText.text = "";
        }
        else if (gameManager.currentState == GameManager.GameState.State2)
        {
            // Manté els dos primers fixos de State1
            if (top3DonatorsFixed.Count > 0)
            {
                //topDonator1name.text = top3DonatorsFixed[0].Name;
                //messageDonator1.text = $"{top3DonatorsFixed[0].Points} punts";
            }
            if (top3DonatorsFixed.Count > 1)
            {
                //topDonator2name.text = top3DonatorsFixed[1].Name;
                //messageDonator2.text = $"{top3DonatorsFixed[1].Points} punts";
            }
            if (top3DonatorsFixed.Count > 2)
            {
                //topDonator3name.text = top3DonatorsFixed[2].Name;
                //messageDonator3.text = $"{top3DonatorsFixed[2].Points} punts";
            } // No mostrar res al tercer lloc

            top3DonatorsText.text = string.Join("\n", top2Donators.Select(d => $"{d.Name}: {d.Points} punts"));
        }

        // Actualitza el text complet del Top 2 per mostrar a top3DonatorsText
        //op3DonatorsText.text = string.Join("\n", top2Donators.Select(d => $"{d.Name}: {d.Points} punts"));

        UpdateDonatorCountText();
    }

    private void UpdateDonatorCountText()
    {
        donatorCountText.text = $"Donadors actius: {donatorList.Count}";
    }

    private void UpdateStateText()
    {
        stateText.text = $"Estat: {currentState}";
    }
    private void UpdateViewersText()
    {
        numberViewersText.text = $"Estat: {numberOfViewers}";
    }
    private void OnDestroy()
    {
        if (liveManager != null)
        {
            liveManager.OnConnected -= OnConnected;
            liveManager.OnRoomUpdate -= OnRoomUpdate;
            liveManager.OnChatMessage -= OnChatMessage;
            liveManager.OnFollow -= OnFollow;
            liveManager.OnGift -= OnGift;

            // No destruïm el TikTokLiveManager perquè es vol que persisteixi entre escenes
            liveManager.DisconnectFromLivestreamAsync();
        }
    }

    private void OnConnected(TikTokLiveSharp.Client.TikTokLiveClient sender, bool isConnected)
    {
        Debug.Log(isConnected ? "Connectat al directe!" : "Error de connexió.");
    }

    private void OnRoomUpdate(TikTokLiveSharp.Client.TikTokLiveClient sender, TikTokLiveSharp.Events.RoomUpdate roomInfo)
    {
        Debug.Log($"Espectadors actuals: {roomInfo.NumberOfViewers}");
        numberOfViewers = roomInfo.NumberOfViewers;
    }
    private void OnChatMessage(TikTokLiveSharp.Client.TikTokLiveClient sender, Chat chatMessage)
    {
        string donatorName = chatMessage.Sender.NickName;
        string message = chatMessage.Message;

        // Llista de paraules prohibides
        string[] bannedWords = { "uwu", "paraulota2", "paraulota3" };

        // Comprova si el missatge conté paraules prohibides
        foreach (string bannedWord in bannedWords)
        {
            if (message.Contains(bannedWord, System.StringComparison.OrdinalIgnoreCase))
            {
                message = "Missatge anul·lat";
                break;
            }
        }

        if (donatorName == topDonator1name.text)
        {
            if (gameManager.currentState == GameManager.GameState.State2)
            {
                ShowMessageWithFade(messageDonator1, message, 1);
            }
            else if (gameManager.currentState == GameManager.GameState.State4 && ImageControllerManager.IsFirstTimerActive())
            {
                switch (message)
                {
                    case "1":
                        ImageControllerManager.AsignarImatgePerPosicio(1);
                        break;
                }
            }
        }
        else if (donatorName == topDonator2name.text)
        {
            if (gameManager.currentState == GameManager.GameState.State2)
            {
                ShowMessageWithFade(messageDonator1, message, 2);
            }
            else if (gameManager.currentState == GameManager.GameState.State4)
            {

            }
        }
        else if (donatorName == topDonator3name.text)
        {
            if (gameManager.currentState == GameManager.GameState.State2)
            {
                ShowMessageWithFade(messageDonator1, message, 3);
            }
            else if (gameManager.currentState == GameManager.GameState.State4 && ImageControllerManager.IsThirdTimerActive() && player3HaveChoosen == false)
            {
                switch (message)
                {
                    case "1":
                        ImageControllerManager.AsignarImatgePerPosicio(1);
                        player3HaveChoosen = true;
                        break;
                    case "2":
                        ImageControllerManager.AsignarImatgePerPosicio(2);
                        player3HaveChoosen = true;
                        break;
                }
            }
            Debug.Log($"Missatge de {chatMessage.Sender.NickName}: {chatMessage.Message}");
        }
    }
    private void ShowMessageWithFade(TextMeshProUGUI messageText, string message, int donatorIndex)
    {
        StopAllCoroutines(); // Atura qualsevol corutina prèvia del text
        StartCoroutine(ShowMessageWithFadeCoroutine(messageText, message, donatorIndex));
    }

    private IEnumerator ShowMessageWithFadeCoroutine(TextMeshProUGUI messageText, string message, int donatorIndex)
    {
        string originalText = messageText.text; // Guarda el text original

        // Fade out el text actual
        yield return StartCoroutine(FadeOutMessage(messageText));

        // Mostra el missatge del donador
        messageText.text = message;
        yield return StartCoroutine(FadeInMessage(messageText)); // Aplica el fade in
        yield return new WaitForSeconds(4f); // Espera 4 segons amb el text visible (canviat de 2 a 4)

        // Torna a fer fade out
        yield return StartCoroutine(FadeOutMessage(messageText));

        // Torna a mostrar el text original
        messageText.text = originalText;
        yield return StartCoroutine(FadeInMessage(messageText)); // Aplica el fade in
    }

    private IEnumerator FadeInMessage(TextMeshProUGUI messageText)
    {
        float fadeDuration = 1f; // Duració del fade in
        float startAlpha = 0f;
        float endAlpha = 1f;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t / fadeDuration);
            messageText.alpha = alpha;
            yield return null;
        }

        messageText.alpha = endAlpha; // Assegura que l'opacitat sigui 1 al final
    }

    private IEnumerator FadeOutMessage(TextMeshProUGUI messageText)
    {
        float fadeDuration = 1f; // Duració del fade out
        float startAlpha = 1f;
        float endAlpha = 0f;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t / fadeDuration);
            messageText.alpha = alpha;
            yield return null;
        }

        messageText.alpha = endAlpha; // Assegura que l'opacitat sigui 0 al final
    }
    private void OnFollow(TikTokLiveSharp.Client.TikTokLiveClient sender, Follow followEvent)
    {
        Debug.Log($"{followEvent.User.NickName} ha seguit el directe!");
    }

    private void OnGift(TikTokLiveSharp.Client.TikTokLiveClient sender, TikTokGift giftEvent)
    {
        string senderName = giftEvent.Sender.NickName;
        string giftName = giftEvent.Gift?.Name ?? "Regal desconegut";
        int giftAmount = (int)giftEvent.Amount;
        //int giftValue = giftValues.ContainsKey(giftName) ? giftValues[giftName] * giftAmount : giftAmount;
        int giftValue = giftEvent.Gift.DiamondCost;

        Debug.Log($"{senderName} ha enviat {giftAmount} regals de tipus {giftName} amb valor total de {giftValue} punts.");

        UpdateDonator(senderName, giftValue);
        SortAndDisplayDonators();
    }
    public void ClearDonatorList1()
    {
        donatorList.Clear();
        /*
        Top3Donators.Clear();
        topDonator1name.text = "N/A";
        topDonator2name.text = "N/A";
        topDonator3name.text = "N/A";
        messageDonator1.text = "";
        messageDonator2.text = "";
        messageDonator3.text = "";
        Debug.Log("Llista de participants netejada per la següent partida.");
        participantText.text = "";
        UpdateDonatorCountText();
        */
    }
    public void ClearDonatorList2()
    {
        //donatorList.Clear();
        Top3Donators.Clear();
        topDonator1name.text = "N/A";
        topDonator2name.text = "N/A";
        topDonator3name.text = "N/A";
        messageDonator1.text = "";
        messageDonator2.text = "";
        messageDonator3.text = "";
        participantText.text = "";
        UpdateDonatorCountText();
    }
    private void OnStateChange(GameManager.GameState oldState, GameManager.GameState newState)
    {
        Debug.Log($"Canvi d'estat detectat: {oldState} -> {newState}");

        // Debug abans de qualsevol operació
        Debug.Log($"Abans del canvi: donatorList = {donatorList.Count}, tempDonatorList = {tempDonatorList.Count}, top3DonatorsFixed = {top3DonatorsFixed.Count}");

        if (newState == GameManager.GameState.State1)
        {
            Debug.Log("Sincronitzant donatorList amb tempDonatorList per State1.");
        }
        else if (newState == GameManager.GameState.State2)
        {
            Debug.Log("Guardant el Top 3 de donadors i netejant donatorList.");

            // Desa el Top 3 abans de netejar
            top3DonatorsFixed = donatorList.OrderByDescending(d => d.Points).Take(3).ToList();
            japuc = true;
            // Neteja la llista de donadors
            donatorList.Clear();

            // Manté els noms fixes a la UI
            if (top3DonatorsFixed.Count > 0)
            {
                topDonator1name.text = top3DonatorsFixed[0].Name;
                messageDonator1.text = $"{top3DonatorsFixed[0].Points}";
            }
            if (top3DonatorsFixed.Count > 1)
            {
                topDonator2name.text = top3DonatorsFixed[1].Name;
                messageDonator2.text = $"{top3DonatorsFixed[1].Points}";
            }
            if (top3DonatorsFixed.Count > 2)
            {
                topDonator3name.text = top3DonatorsFixed[2].Name;
                messageDonator3.text = $"{top3DonatorsFixed[2].Points}";
            }
        }

        // Debug després de les operacions
        Debug.Log($"Després del canvi: donatorList = {donatorList.Count}, tempDonatorList = {tempDonatorList.Count}, top3DonatorsFixed = {top3DonatorsFixed.Count}");

        UpdateStateText();
    }

}

// Classe per representar un donador
public class Donator
{
    public string Name { get; set; }
    public int Points { get; set; }
    public int Victories; // Nou camp per guardar les victòries
}
[System.Serializable]
public class PlayerData
{
    public string Name;
    public int Victories;

    public PlayerData(string name, int victories)
    {
        Name = name;
        Victories = victories;
    }
}
