using UnityEngine;
using TikTokLiveSharp.Events.Objects;
using TikTokLiveSharp.Events;
using TikTokLiveUnity;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ChatMessageAnalyzer : MonoBehaviour
{
    public DataManager dataManager;
    public TextMeshProUGUI esultText1; // Text per a l'opció 1
    public TextMeshProUGUI esultText2; // Text per a l'opció 2
    public TextMeshProUGUI esultText3; // Text per a l'opció 3
    public GameManager gameManager;
    public TextMeshProUGUI resultText; // TextMeshPro per mostrar els resultats
    public TextMeshProUGUI resultText2;
    private TikTokLiveManager liveManager;

    private int count1 = 0; // Comptador per "1"
    private int count2 = 0; // Comptador per "2"
    private int count3 = 0; // Comptador per "3"
    public bool isAnalyzing = false; // Variable pública per controlar l'anàlisi des d'un altre script

    private HashSet<string> usersWhoVoted = new HashSet<string>(); // Llista d'usuaris que ja han votat

    private void Start()
    {
        liveManager = TikTokLiveManager.Instance;

        if (liveManager == null)
        {
            GameObject liveManagerObject = new GameObject("TikTokLiveManager");
            liveManager = liveManagerObject.AddComponent<TikTokLiveManager>();
            DontDestroyOnLoad(liveManagerObject);
        }
        liveManager.OnChatMessage -= HandleChatMessage;
        liveManager.OnChatMessage += HandleChatMessage;

        liveManager.ConnectToStreamAsync("hostUsername");
    }

    private void Update()
    {
        if (isAnalyzing)
        {
            UpdateResultText(); // Actualitza els textos de resultats en temps real
        }
        else
        {
            ClearResultTexts();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            StartChatAnalysis();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            count1++;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            count2++;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            count3++;
        }
    }

    public void StartChatAnalysis()
    {
        if (!isAnalyzing)
        {
            ResetData();
            isAnalyzing = true;
            StartCoroutine(AnalyzeChatForDuration(10));
        }
    }

    public void HandleChatMessage(TikTokLiveSharp.Client.TikTokLiveClient sender, Chat chatMessage)
    {
        if (!isAnalyzing) return;

        string username = chatMessage.Sender.NickName;
        string message = chatMessage.Message.Trim();

        if (usersWhoVoted.Contains(username))
        {
            Debug.Log($"Usuari {username} ja ha votat. Vot ignorat.");
            return;
        }

        // Obté les opcions vàlides
        List<string> votingOptions = GetVotingOptions();

        if (votingOptions.Contains(message))
        {
            switch (message)
            {
                case "1":
                    count1++;
                    break;
                case "2":
                    count2++;
                    break;
                case "3":
                    count3++;
                    break;
                case "1/2":
                    count1++;
                    break;
                case "2/3":
                    count2++;
                    break;
                case "3/1":
                    count3++;
                    break;
            }
            usersWhoVoted.Add(username);
            Debug.Log($"Vot acceptat: {username} -> {message}");
        }
        else
        {
            Debug.Log($"Vot no vàlid: {username} -> {message}");
        }
    }

    private IEnumerator AnalyzeChatForDuration(float duration)
    {
        float timeRemaining = duration;

        while (timeRemaining > 0)
        {
            resultText2.text = $"Temps restant: {timeRemaining:F1}s";
            yield return new WaitForSeconds(0.1f);
            timeRemaining -= 0.1f;
        }

        isAnalyzing = false;
        Debug.Log($"Anàlisi completada: 1: {count1}, 2: {count2}, 3: {count3}");
        StartCoroutine(ShowFinalTopOption());
    }

    private void ResetData()
    {
        count1 = 0;
        count2 = 0;
        count3 = 0;
        usersWhoVoted.Clear();
        Debug.Log("Dades resetejades.");
    }

    private void UpdateResultText()
    {
        switch (dataManager.resultatRuleta2)
        {
            case 4: // Opcions individuals per a cada jugador
                resultText.text = $"Resultats actuals:\n1: {count1}\n2: {count2}\n3: {count3}";
                esultText1.text = $"1:\n{count1}";
                esultText2.text = $"2:\n{count2}";
                esultText3.text = $"3:\n{count3}";
                break;

            case 3: // Opcions combinades per parelles
                resultText.text = $"Resultats actuals:\n1/2: {count1}\n2/3: {count2}\n3/1: {count3}";
                esultText1.text = $"1/2:\n{count1}";
                esultText2.text = $"2/3:\n{count2}";
                esultText3.text = $"3/1:\n{count3}";
                break;
        }
    }
    private IEnumerator ShowFinalTopOption()
    {
        string topOption = "";

        if (dataManager.resultatRuleta2 == 4) // Opcions individuals
        {
            int maxCount = Mathf.Max(count1, count2, count3);
            topOption = (maxCount == count1) ? "1" : (maxCount == count2) ? "2" : "3";
        }
        else if (dataManager.resultatRuleta2 == 3) // Opcions combinades
        {
            int maxCount = Mathf.Max(count1, count2, count3);
            topOption = (maxCount == count1) ? "1/2" : (maxCount == count2) ? "2/3" : "3/1";
        }

        resultText.text = $"Opció guanyadora: {topOption} amb {Mathf.Max(count1, count2, count3)} vots!";

        gameManager.ApplyEfect(topOption);

        yield return new WaitForSeconds(2f);
        resultText.text = "";
    }


    private void ClearResultTexts()
    {
        esultText1.text = "";
        esultText2.text = "";
        esultText3.text = "";
        resultText2.text = "";
    }

    private List<string> GetVotingOptions()
    {
        DataManager dataManager = FindObjectOfType<DataManager>();

        List<string> validOptions = new List<string>();

        switch (dataManager.resultatRuleta2)
        {
            case 4: // Opcions individuals per a cada jugador
                validOptions = new List<string> { "1", "2", "3" };
                break;

            case 3: // Opcions combinades per parelles
                validOptions = new List<string> { "1/2", "2/3", "3/1" };
                break;

            default:
                validOptions = new List<string>(); // Cap opció vàlida
                break;
        }

        Debug.Log($"Opcions de vot per al xat: {string.Join(", ", validOptions)}");

        return validOptions;
    }

}