using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI元素")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI restartHintText;

    private PlayerController playerController;
    private GameManager gameManager;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (gameManager != null)
        {
            bool isGameOver = gameManager.IsGameOver();
            
            // 游戏进行中显示速度和距离
            if (!isGameOver)
            {
                if (playerController != null && speedText != null)
                {
                    float speed = playerController.GetForwardSpeed();
                    speedText.text = $"speed: {speed:F1} m/s";
                }

                if (distanceText != null)
                {
                    float distance = gameManager.GetCurrentDistance();
                    distanceText.text = $"distance: {distance:F1} m";
                }
            }

            // 游戏结束时显示得分
            if (isGameOver && scoreText != null)
            {
                float score = gameManager.GetFinalScore();
                scoreText.text = $"Distance: {score:F1} m";
            }
            
            // 显示/隐藏游戏结束文本
            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(isGameOver);
            }
            
            if (scoreText != null)
            {
                scoreText.gameObject.SetActive(isGameOver);
            }
            
            if (restartHintText != null)
            {
                restartHintText.gameObject.SetActive(isGameOver);
            }
        }
    }
}

