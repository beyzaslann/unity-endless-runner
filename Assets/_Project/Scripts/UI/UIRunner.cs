using TMPro;
using UnityEngine;

public class UIRunner : MonoBehaviour
{
    public static UIRunner Instance { get; private set; }

    public TMP_Text scoreText;
    public GameObject startPanel;
    public GameObject gameOverPanel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetScore(int score)
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void HideStart()
    {
        if (startPanel != null)
            startPanel.SetActive(false);
    }
}
