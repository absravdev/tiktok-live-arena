using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VictoryManager : MonoBehaviour
{
    private string filePath;

    private void Awake()
    {
        filePath = @"D:\TikTokLiveUnityGame\Assets\PlayerData.json";
        Debug.Log("Ruta personalitzada del fitxer JSON: " + filePath);
    }

    public List<PlayerData> LoadPlayerData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<PlayerDataList>(json).Players;
        }
        else
        {
            return new List<PlayerData>();
        }
    }

    public void SavePlayerData(List<PlayerData> players)
    {
        PlayerDataList data = new PlayerDataList { Players = players };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
    }
}

[System.Serializable]
public class PlayerDataList
{
    public List<PlayerData> Players = new List<PlayerData>();
}
