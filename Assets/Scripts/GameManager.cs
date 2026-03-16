using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI 设置")]
    public GameObject gameOverUI;

    private bool isGameOver;
    private float gameTime;
    private float finalScore;
    private PlayerController playerController;

    void Start()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        Time.timeScale = 1f;

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
        
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void OnGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (playerController != null)
        {
            finalScore = playerController.GetTotalDistance();
        }

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        Debug.Log($"游戏结束，得分 {finalScore:F1} 米");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        // 如果有主菜单场景，请修改为对应的场景名称
        // SceneManager.LoadScene("MainMenu");
        // 目前没有主菜单，暂时重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public float GetGameTime() => gameTime;
    public bool IsGameOver() => isGameOver;
    public float GetFinalScore() => finalScore;

    public float GetCurrentDistance()
    {
        return playerController != null ? playerController.GetTotalDistance() : 0f;
    }
}
