using System;
using Game;
using Generation;
using Steamworks.NET;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Events.Events;

public class GameFlowManager : MonoBehaviour
{
    public static bool IsGameActive = true; // The game is currently running
    public static bool IsGameStarted = false; // Game has started
    public static bool FinishedGeneratingLevel = false; // Has finished generating the map

    [Header("Parameters")]
    [Tooltip("Duration of the fade-to-black at the end of the game")]
    public float endSceneLoadDelay = 3f;
    [Tooltip("The canvas group of the fade-to-black screen")]
    public CanvasGroup endGameFadeCanvasGroup;

    [Header("Start Game")]
    [Tooltip("The GameObjects to enable when the game starts")]
    public GameObject[] EnableOnStart;
    [Tooltip("The GameObjects to disable when the game starts")]
    public GameObject[] DisableOnStart;
    [Tooltip("This starting position for the player on each level")]
    public Transform PlayerStartingPosition;

    [Header("Win")]
    [Tooltip("This string has to be the name of the scene you want to load when winning")]
    public string winSceneName = "WinScene";
    [Tooltip("Duration of delay before the fade-to-black, if winning")]
    public float delayBeforeFadeToBlack = 4f;
    [Tooltip("Duration of delay before the win message")]
    public float delayBeforeWinMessage = 2f;
    [Tooltip("Sound played on win")]
    public AudioClip victorySound;
    [Tooltip("Prefab for the win game message")]
    public GameObject WinGameMessagePrefab;

    [Header("Lose")]
    [Tooltip("This string has to be the name of the scene you want to load when losing")]
    public string loseSceneName = "LoseScene";


    public bool gameIsEnding { get; private set; }

    PlayerCharacterController m_Player;
    NotificationHUDManager m_NotificationHUDManager;
    ObjectiveManager m_ObjectiveManager;
    float m_TimeLoadEndGameScene;
    string m_SceneToLoad;

    private void Awake()
    {
        OnGameStart += ToggleObjectsOnStart;
        OnLevelStart += SetPlayerStartingPosition;
        OnSceneStart += ResetStatics;
        OnLevelCompletelyFinishedGenerating += OnGenerationCompleted;
        OnLevelStart += OnGenerationStarted;
    }

    void Start()
    {
        OnOnSceneStart();

        IsGameActive = true;

        m_Player = FindObjectOfType<PlayerCharacterController>();
        DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, GameFlowManager>(m_Player, this);

        m_ObjectiveManager = FindObjectOfType<ObjectiveManager>();
		DebugUtility.HandleErrorIfNullFindObject<ObjectiveManager, GameFlowManager>(m_ObjectiveManager, this);

        AudioUtility.SetMasterVolume(1);

        ToggleOnGameStart(true, DisableOnStart);
        ToggleOnGameStart(false, EnableOnStart);
    }

    void Update()
    {
        if (IsGameActive || !IsGameStarted) return;

        if (gameIsEnding)
        {
            float timeRatio = 1 - (m_TimeLoadEndGameScene - Time.time) / endSceneLoadDelay;
            endGameFadeCanvasGroup.alpha = timeRatio;

            AudioUtility.SetMasterVolume(1 - timeRatio);

            // See if it's time to load the end scene (after the delay)
            if (Time.time >= m_TimeLoadEndGameScene && FinishedGeneratingLevel)
            {
                if (Stats.NPHasDied)
                {
                    OnOnGameEnd();
                    SceneManager.LoadScene(m_SceneToLoad);
                    return;
                }
                NextLevel();
                endGameFadeCanvasGroup.alpha = 0;
                gameIsEnding = false;
                m_TimeLoadEndGameScene = 0f;
            }
        }
        else
        {
            if (!FinishedGeneratingLevel) return;

            if (m_ObjectiveManager && m_ObjectiveManager.AreAllObjectivesCompleted())
                EndGame(true);

            // Test if player died
            if (m_Player && m_Player.isDead)
            {
                OnOnPlayerDeath();
                EndGame(false);
            }
        }
    }

    /// <summary>
    /// When the generation has started
    /// </summary>
    public void OnGenerationStarted()
    {
        FinishedGeneratingLevel = false;
    }

    /// <summary>
    /// When the world generation is completed
    /// </summary>
    public void OnGenerationCompleted()
    {
        AudioUtility.SetMasterVolume(1); // Reset the audio volume.
        FinishedGeneratingLevel = true;
    }

    /// <summary>
    /// Reset the statics for the game
    /// </summary>
    public void ResetStatics()
    {
        IsGameActive = true;
        IsGameStarted = false;
    }

    /// <summary>
    /// Called to start the game
    /// </summary>
    public static void StartGame()
    {
        IsGameStarted = true;
        OnOnGameStart();
        IsGameActive = false;
        OnOnLevelStart();
    }

    /// <summary>
    /// Loads the next level
    /// </summary>
    /// <returns></returns>
    public void NextLevel()
    {
        OnOnLevelFinish();
        IsGameActive = true;
        IsGameStarted = false;
        StartGame();
    }

    void EndGame(bool win)
    {
        // unlocks the cursor before leaving the scene, to be able to click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Remember that we need to load the appropriate end scene after a delay
        gameIsEnding = true;
        endGameFadeCanvasGroup.gameObject.SetActive(true);
        if (win)
        {
            m_TimeLoadEndGameScene = Time.time + endSceneLoadDelay + delayBeforeFadeToBlack;

            // play a sound on win
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = victorySound;
            audioSource.playOnAwake = false;
            audioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDVictory);
            audioSource.PlayScheduled(AudioSettings.dspTime + delayBeforeWinMessage);

            // create a game message
            var message = Instantiate(WinGameMessagePrefab).GetComponent<DisplayMessage>();
            if (message)
            {
                message.delayBeforeShowing = delayBeforeWinMessage;
                message.GetComponent<Transform>().SetAsLastSibling();
            }
        }
        else
        {
            m_SceneToLoad = loseSceneName;
            m_TimeLoadEndGameScene = Time.time + endSceneLoadDelay;
        }
    }

    /// <summary>
    /// Sets the players position on level start
    /// </summary>
    public void SetPlayerStartingPosition()
    {
        m_Player.TeleportCharacter(PlayerStartingPosition.position);
    }

    /// <summary>
    /// Toggles the starting objects
    /// </summary>
    public void ToggleObjectsOnStart()
    {
        ToggleOnGameStart(false, DisableOnStart);
        ToggleOnGameStart(true, EnableOnStart);
    }

    /// <summary>
    /// Toggles a given array of objects
    /// </summary>
    /// <param name="toggle"></param>
    /// <param name="gameObjects"></param>
    public void ToggleOnGameStart(bool toggle, GameObject[] gameObjects)
    {
        foreach (var obj in gameObjects)
        {
            obj.SetActive(toggle);
        }
    }

    private void OnDestroy()
    {
        OnGameStart -= ToggleObjectsOnStart;
        OnLevelStart -= SetPlayerStartingPosition;
        OnSceneStart -= ResetStatics;
        OnLevelCompletelyFinishedGenerating -= OnGenerationCompleted;
        OnLevelStart -= OnGenerationStarted;
    }
}
