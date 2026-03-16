using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("背景音乐设置")]
    public AudioClip backgroundMusic;
    
    [Range(0f, 1f)]
    public float volume = 0.5f;
    
    private AudioSource audioSource;
    private GameManager gameManager;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
        
        gameManager = FindObjectOfType<GameManager>();
        
        bool shouldPlayOwnMusic = true;
        
        if (GameModeManager.Instance != null)
        {
            GameModeData modeData = GameModeManager.Instance.GetSelectedModeData();
            if (modeData != null && modeData.backgroundMusic != null)
            {
                shouldPlayOwnMusic = false;
                Debug.Log("[MusicManager] 游戏模式系统已配置音乐，不播放MusicManager的音乐");
            }
        }
        
        if (shouldPlayOwnMusic && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.Play();
            Debug.Log("[MusicManager] 播放音乐: " + backgroundMusic.name);
        }
    }

    void Update()
    {
        if (gameManager != null && gameManager.IsGameOver())
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        
        audioSource.volume = volume;
    }
    
    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    public void PlayMusic()
    {
        if (audioSource != null && backgroundMusic != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
