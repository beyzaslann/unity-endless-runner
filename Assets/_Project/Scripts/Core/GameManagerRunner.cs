using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerRunner : MonoBehaviour
{
    public static GameManagerRunner Instance { get; private set; }

    [Header("State")]
    public bool isRunning;
    public bool isGameOver;

    [Header("Score")]
    public int score;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 0f; // start panel açýkken dur
        isRunning = false;
        isGameOver = false;
        score = 0;
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        isRunning = true;
        isGameOver = false;

        if (UIRunner.Instance != null)
            UIRunner.Instance.HideStart();
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (UIRunner.Instance != null)
            UIRunner.Instance.SetScore(score);
    }

    public void GameOver()
    {
        isGameOver = true;
        isRunning = false;
        Time.timeScale = 0f;

        if (UIRunner.Instance != null)
            UIRunner.Instance.ShowGameOver();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

