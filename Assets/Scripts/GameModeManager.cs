using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    private static GameModeManager instance;
    public static GameModeManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameModeManager");
                instance = go.AddComponent<GameModeManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    [Header("游戏模式配置")]
    public GameModeData[] gameModes;
    
    private GameMode selectedMode = GameMode.Endless;
    private const string SELECTED_MODE_KEY = "SelectedGameMode";
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        LoadSelectedMode();
    }
    
    public void SelectMode(GameMode mode)
    {
        selectedMode = mode;
        SaveSelectedMode();
        Debug.Log($"[游戏模式] 已选择模式: {mode}");
    }
    
    public GameMode GetSelectedMode()
    {
        return selectedMode;
    }
    
    public GameModeData GetSelectedModeData()
    {
        if (gameModes == null || gameModes.Length == 0)
        {
            Debug.LogError("No game modes configured!");
            return null;
        }
        
        foreach (var modeData in gameModes)
        {
            if (modeData != null && modeData.modeType == selectedMode)
            {
                return modeData;
            }
        }
        
        Debug.LogWarning($"Mode data not found for {selectedMode}, using first mode");
        return gameModes[0];
    }
    
    public GameModeData GetModeData(GameMode mode)
    {
        if (gameModes == null || gameModes.Length == 0)
        {
            return null;
        }
        
        foreach (var modeData in gameModes)
        {
            if (modeData != null && modeData.modeType == mode)
            {
                return modeData;
            }
        }
        
        return null;
    }
    
    void SaveSelectedMode()
    {
        PlayerPrefs.SetInt(SELECTED_MODE_KEY, (int)selectedMode);
        PlayerPrefs.Save();
    }
    
    void LoadSelectedMode()
    {
        selectedMode = (GameMode)PlayerPrefs.GetInt(SELECTED_MODE_KEY, 0);
    }
}
