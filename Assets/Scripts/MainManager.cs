using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class MainManager : MonoBehaviour
{
    [Header("Game Settings")]
    public Brick BrickPrefab;
    public int LineCount = 6;
    public Rigidbody Ball;

    [Header("UI")]
    public TMP_Text PlayerNameText;
    public TMP_Text ScoreText;
    public TMP_Text BestScoreText;

    [Header("Start Menu")]
    public GameObject StartPanel;
    public TMP_InputField PlayerNameInput;

    [Header("Game Over")]
    public TMP_Text GameOverText;

    private bool m_Started = false;
    private bool m_GameOver = false;

    private int m_Points = 0;
    private string m_PlayerName = "Player";

    // Saved high score
    private int m_HighScore = 0;
    private string m_HighScorePlayer = "Name";

    // Space key
    private InputAction m_SpaceAction;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        m_SpaceAction = new InputAction(
            "Space",
            InputActionType.Button,
            "<Keyboard>/space"
        );

        // Load saved high score
        m_HighScore = PlayerPrefs.GetInt("HighScore", 0);

        m_HighScorePlayer = PlayerPrefs.GetString(
            "HighScorePlayer",
            "Name"
        );
    }


    // =========================================================
    // ENABLE / DISABLE
    // =========================================================

    private void OnEnable()
    {
        m_SpaceAction.Enable();
    }

    private void OnDisable()
    {
        m_SpaceAction.Disable();
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Show start menu
        StartPanel.SetActive(true);

        // Hide Game Over
        GameOverText.gameObject.SetActive(false);

        // Hide game information
        PlayerNameText.gameObject.SetActive(false);
        ScoreText.gameObject.SetActive(false);
        BestScoreText.gameObject.SetActive(false);

        // Stop ball
        Ball.isKinematic = true;
        Ball.linearVelocity = Vector3.zero;

        // Reset score
        m_Points = 0;

        // Update saved high score
        UpdateBestScoreUI();

        // Create bricks
        CreateBricks();
    }


    // =========================================================
    // CREATE BRICKS
    // =========================================================

    private void CreateBricks()
    {
        const float step = 0.6f;

        int perLine = Mathf.FloorToInt(4.0f / step);

        int[] pointCountArray =
        {
            1,
            1,
            2,
            2,
            5,
            5
        };

        for (int i = 0; i < LineCount; ++i)
        {
            for (int x = 0; x < perLine; ++x)
            {
                Vector3 position = new Vector3(
                    -1.5f + step * x,
                    2.5f + i * 0.3f,
                    0
                );

                Brick brick = Instantiate(
                    BrickPrefab,
                    position,
                    Quaternion.identity
                );

                brick.PointValue = pointCountArray[i];

                brick.onDestroyed.AddListener(AddPoint);
            }
        }
    }


    // =========================================================
    // START GAME
    // =========================================================

    public void StartGame()
    {
        // Get player name
        m_PlayerName = PlayerNameInput.text.Trim();

        // Name is required
        if (string.IsNullOrEmpty(m_PlayerName))
        {
            Debug.Log("Please enter your name.");

            PlayerNameInput.Select();
            PlayerNameInput.ActivateInputField();

            return;
        }

        // Show game information
        PlayerNameText.gameObject.SetActive(true);
        ScoreText.gameObject.SetActive(true);
        BestScoreText.gameObject.SetActive(true);

        // Set player name
        PlayerNameText.text = "Now Playing : " + m_PlayerName;

        // Hide start menu
        StartPanel.SetActive(false);

        // Hide Game Over
        GameOverText.gameObject.SetActive(false);

        // Start game
        m_Started = true;
        m_GameOver = false;

        // Reset score
        m_Points = 0;
        ScoreText.text = "Score : 0";

        // Make ball active
        Ball.isKinematic = false;
        Ball.linearVelocity = Vector3.zero;
        Ball.transform.SetParent(null);

        // Launch ball
        float randomDirection = Random.Range(-1.0f, 1.0f);

        Vector3 forceDirection = new Vector3(
            randomDirection,
            1.0f,
            0
        );

        forceDirection.Normalize();

        Ball.AddForce(
            forceDirection * 2.0f,
            ForceMode.VelocityChange
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // -----------------------------------------------------
        // GAME OVER
        // SPACE = RESTART
        // -----------------------------------------------------

        if (m_GameOver)
        {
            if (m_SpaceAction.WasPressedThisFrame())
            {
                RestartGame();
            }

            return;
        }


        // -----------------------------------------------------
        // START MENU
        // SPACE = START
        // -----------------------------------------------------

        if (!m_Started)
        {
            if (m_SpaceAction.WasPressedThisFrame())
            {
                StartGame();
            }
        }
    }


    // =========================================================
    // ADD POINT
    // =========================================================

    private void AddPoint(int point)
    {
        // Don't score after Game Over
        if (m_GameOver)
            return;


        // Add score
        m_Points += point;


        // Update current score immediately
        ScoreText.text =
            "Score : " + m_Points;


        // =====================================================
        // CHECK HIGH SCORE IN REAL TIME
        // =====================================================

        if (m_Points > m_HighScore)
        {
            // New high score
            m_HighScore = m_Points;

            // IMPORTANT:
            // The CURRENT PLAYER gets the high score name.
            m_HighScorePlayer = m_PlayerName;


            // Save immediately
            PlayerPrefs.SetInt(
                "HighScore",
                m_HighScore
            );

            PlayerPrefs.SetString(
                "HighScorePlayer",
                m_HighScorePlayer
            );

            PlayerPrefs.Save();


            // IMPORTANT:
            // Update UI immediately
            UpdateBestScoreUI();
        }
    }


    // =========================================================
    // GAME OVER
    // =========================================================

    public void GameOver()
    {
        // Prevent multiple GameOver calls
        if (m_GameOver)
            return;


        m_GameOver = true;
        m_Started = false;


        // Stop ball
        Ball.linearVelocity = Vector3.zero;
        Ball.isKinematic = true;


        // Show Game Over
        GameOverText.gameObject.SetActive(true);

        GameOverText.text =
            "GAME OVER\n" +
            "Score : " + m_Points;


        // -----------------------------------------------------
        // FINAL HIGH SCORE CHECK
        // -----------------------------------------------------

        if (m_Points > m_HighScore)
        {
            m_HighScore = m_Points;
            m_HighScorePlayer = m_PlayerName;

            PlayerPrefs.SetInt(
                "HighScore",
                m_HighScore
            );

            PlayerPrefs.SetString(
                "HighScorePlayer",
                m_HighScorePlayer
            );

            PlayerPrefs.Save();

            UpdateBestScoreUI();
        }
    }


    // =========================================================
    // BEST SCORE UI
    // =========================================================

    private void UpdateBestScoreUI()
    {
        BestScoreText.text =
            "Best Score : " +
            m_HighScorePlayer +
            " : " +
            m_HighScore;
    }


    // =========================================================
    // RESTART
    // =========================================================

    private void RestartGame()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }


    // =========================================================
    // QUIT
    // =========================================================

    public void QuitGame()
    {
        Debug.Log("Quit Game");

        Application.Quit();
    }

    public void ResetHighScore()
    {
        m_HighScore = 0;
        m_HighScorePlayer = "Name";

        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.DeleteKey("HighScorePlayer");
        PlayerPrefs.Save();

        UpdateBestScoreUI();
    }
}