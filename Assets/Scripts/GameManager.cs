using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI设置")]
    public GameObject gameOverUI;
    
    private bool isGameOver = false;
    private float gameTime = 0f;
    private float finalScore = 0f;
    private PlayerController playerController;

    void Start()
    {
        // 隐藏游戏结束UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
        
        Time.timeScale = 1f;
        
        // 获取玩家控制器
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        if (!isGameOver)
        {
            gameTime += Time.deltaTime;
        }

        // 按R键重新开始游戏
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void OnGameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        
        // 获取最终得分（移动距离）
        if (playerController != null)
        {
            finalScore = playerController.GetTotalDistance();
        }
        
        // 显示游戏结束UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        
        Debug.Log($"游戏结束！得分: {finalScore:F1}米");
        Debug.Log("按 R 键重新开始游戏");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public float GetGameTime()
    {
        return gameTime;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public float GetFinalScore()
    {
        return finalScore;
    }

    public float GetCurrentDistance()
    {
        if (playerController != null)
        {
            return playerController.GetTotalDistance();
        }
        return 0f;
    }
}

