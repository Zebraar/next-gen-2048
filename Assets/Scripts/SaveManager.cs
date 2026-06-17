using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SaveManager
{
    public static void Save(int _highScore)
    {
        PlayerData playerData = new PlayerData();
        playerData.highScore = _highScore;
        playerData.playerSettings.Add("MaxTile", PlayerPrefs.GetInt("MaxTile", 2048));
        playerData.playerSettings.Add("Rows", PlayerPrefs.GetInt("Rows", 4));
        playerData.playerSettings.Add("Columns", PlayerPrefs.GetInt("Columns", 4));
        playerData.playerSettings.Add("FPS", PlayerPrefs.GetInt("FPS", 60));
        playerData.playerSettings.Add("AnimDuration", PlayerPrefs.GetFloat("AnimDuration", 0.05f));

        string json = JsonUtility.ToJson(playerData);
        File.WriteAllText(Application.persistentDataPath + "/save.json", json);
    }

    public static PlayerData GetPlayerData()
    {
        if (!File.Exists(Application.persistentDataPath + "/save.json"))
        {
            Debug.Log("No save file");
            return new PlayerData();
        }
        string json = File.ReadAllText(Application.persistentDataPath + "/save.json");
        PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);
        return playerData;
    }
}
