using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class ModeBestRecord
{
    public GameMode mode;
    public float bestDistance;
}

[Serializable]
public class RunRecord
{
    public long timestampUtc;
    public GameMode mode;
    public string characterId;
    public string characterName;
    public float distance;
}

[Serializable]
public class GameDataStore
{
    public string playerName = "玩家一";
    public List<RunRecord> runHistory = new List<RunRecord>();
    public List<ModeBestRecord> bestRecords = new List<ModeBestRecord>();
}

public class GameDataManager : MonoBehaviour
{
    private static GameDataManager instance;
    public static GameDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameDataManager");
                instance = go.AddComponent<GameDataManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private GameDataStore store;
    private string dataPath;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        dataPath = Path.Combine(Application.persistentDataPath, "game_data.json");
        Load();
    }

    public string GetPlayerName()
    {
        EnsureLoaded();
        return store.playerName;
    }

    public List<RunRecord> GetRunHistory()
    {
        EnsureLoaded();
        return store.runHistory;
    }

    public List<ModeBestRecord> GetBestRecords()
    {
        EnsureLoaded();
        return store.bestRecords;
    }

    public float GetBestDistance(GameMode mode)
    {
        EnsureLoaded();
        for (int i = 0; i < store.bestRecords.Count; i++)
        {
            if (store.bestRecords[i].mode == mode)
            {
                return store.bestRecords[i].bestDistance;
            }
        }
        return 0f;
    }

    public void RecordRun(GameMode mode, float distance, CharacterData character)
    {
        EnsureLoaded();

        RunRecord record = new RunRecord
        {
            timestampUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            mode = mode,
            characterId = character != null ? character.characterID : "",
            characterName = character != null ? character.characterName : "Unknown",
            distance = distance
        };

        store.runHistory.Add(record);
        UpdateBestDistance(mode, distance);
        Save();
    }

    void UpdateBestDistance(GameMode mode, float distance)
    {
        for (int i = 0; i < store.bestRecords.Count; i++)
        {
            if (store.bestRecords[i].mode == mode)
            {
                if (distance > store.bestRecords[i].bestDistance)
                {
                    store.bestRecords[i].bestDistance = distance;
                }
                return;
            }
        }

        store.bestRecords.Add(new ModeBestRecord
        {
            mode = mode,
            bestDistance = distance
        });
    }

    public void Load()
    {
        if (File.Exists(dataPath))
        {
            string json = File.ReadAllText(dataPath);
            store = JsonUtility.FromJson<GameDataStore>(json);
        }

        if (store == null)
        {
            store = new GameDataStore();
        }

        if (store.runHistory == null)
        {
            store.runHistory = new List<RunRecord>();
        }

        if (store.bestRecords == null)
        {
            store.bestRecords = new List<ModeBestRecord>();
        }
    }

    public void Save()
    {
        EnsureLoaded();
        string json = JsonUtility.ToJson(store, true);
        File.WriteAllText(dataPath, json);
    }

    void EnsureLoaded()
    {
        if (store == null)
        {
            Load();
        }
    }
}
