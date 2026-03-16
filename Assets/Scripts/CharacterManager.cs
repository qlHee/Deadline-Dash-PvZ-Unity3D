using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    private static CharacterManager instance;
    public static CharacterManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("CharacterManager");
                instance = go.AddComponent<CharacterManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    [Header("角色数据配置")]
    public CharacterData[] characters;
    
    private int selectedCharacterIndex = 0;
    private const string SELECTED_CHARACTER_KEY = "SelectedCharacter";
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        LoadSelectedCharacter();
    }
    
    public void SelectCharacter(int index)
    {
        if (characters == null || index < 0 || index >= characters.Length)
        {
            Debug.LogError($"角色索引无效: {index}");
            return;
        }

        if (!SharedUnlockIO.IsCharacterUnlocked(characters[index].characterID))
        {
            Debug.LogWarning($"角色未解锁: {characters[index].characterName}");
            return;
        }
        
        selectedCharacterIndex = index;
        SaveSelectedCharacter();
        Debug.Log($"[角色选择] 已选择角色: {characters[index].characterName}");
    }
    
    public CharacterData GetSelectedCharacter()
    {
        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("没有可用角色！");
            return null;
        }
        
        if (selectedCharacterIndex < 0 || selectedCharacterIndex >= characters.Length)
        {
            selectedCharacterIndex = 0;
        }
        
        return characters[selectedCharacterIndex];
    }
    
    public int GetSelectedCharacterIndex()
    {
        return selectedCharacterIndex;
    }
    
    public int GetCharacterCount()
    {
        return characters != null ? characters.Length : 0;
    }
    
    public CharacterData GetCharacterByIndex(int index)
    {
        if (characters == null || index < 0 || index >= characters.Length)
        {
            return null;
        }
        return characters[index];
    }
    
    void SaveSelectedCharacter()
    {
        PlayerPrefs.SetInt(SELECTED_CHARACTER_KEY, selectedCharacterIndex);
        PlayerPrefs.Save();
    }
    
    void LoadSelectedCharacter()
    {
        selectedCharacterIndex = PlayerPrefs.GetInt(SELECTED_CHARACTER_KEY, 0);
        
        if (characters != null && selectedCharacterIndex >= characters.Length)
        {
            selectedCharacterIndex = 0;
        }

        if (characters != null && characters.Length > 0)
        {
            if (!SharedUnlockIO.IsCharacterUnlocked(characters[selectedCharacterIndex].characterID))
            {
                int fallback = GetFirstUnlockedIndex();
                selectedCharacterIndex = fallback >= 0 ? fallback : 0;
            }
        }
    }

    int GetFirstUnlockedIndex()
    {
        if (characters == null)
        {
            return -1;
        }

        for (int i = 0; i < characters.Length; i++)
        {
            if (SharedUnlockIO.IsCharacterUnlocked(characters[i].characterID))
            {
                return i;
            }
        }

        return -1;
    }
}
