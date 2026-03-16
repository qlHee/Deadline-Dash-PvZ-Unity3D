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
            Debug.LogError($"Invalid character index: {index}");
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
            Debug.LogError("No characters available!");
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
    }
}
