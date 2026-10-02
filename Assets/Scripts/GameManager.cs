using System;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Owns app-level configuration, quiz progress, and the small AR HUD.
/// Lesson data is loaded from StreamingAssets first so the app remains usable
/// offline; a successful remote refresh replaces it for the next session/round.
/// </summary>
public class GameManager : MonoBehaviour
{
    private const string DefaultConfigUrl = "https://sa.rtctek.com/AR_demo/config.json";

    public static GameManager instance;

    [HideInInspector] public bool WorldPlaced;
    [HideInInspector] public GameObject World;

    [SerializeField] private string configUrl = DefaultConfigUrl;
    [SerializeField] public Camera arCamera;
    [SerializeField] private GameObject[] Ants;
    [SerializeField] public TextMeshProUGUI helpText;

    public Data configData;
    public int currentQuestion = -1;

    private DataLoader dataLoader;
    private SceneScript scene;
    private Data pendingRemoteConfig;
    private bool configurationReady;
    private int correctAnswerCount;
    private int answersSubmitted;
    private Button resetPlacementButton;

    /// <summary>True once lesson content is valid and the world can be placed.</summary>
    public bool CanPlaceWorld
    {
        get { return configurationReady && !WorldPlaced; }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        World = null;
        WorldPlaced = false;

        if (arCamera == null)
            arCamera = Camera.main;

        dataLoader = GetComponent<DataLoader>();
    }

    private void Start()
    {
        if (helpText != null)
        {
            helpText.richText = false;
            helpText.raycastTarget = false;
            helpText.alignment = TextAlignmentOptions.Center;
            helpText.enableAutoSizing = true;
            helpText.fontSizeMin = 16f;
            helpText.fontSizeMax = 26f;
            ShowStatus("Preparing the lesson…");
        }

        CreateResetPlacementButton();

        if (dataLoader == null)
        {
            ShowStatus("Setup error: the lesson loader is missing from the GameManager.");
            Debug.LogError("GameManager requires a DataLoader component on the same GameObject.");
            return;
        }

        dataLoader.LoadLocalData(OnLocalConfigLoaded, OnLocalConfigFailed);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private void OnLocalConfigLoaded(Data data)
    {
        ApplyConfiguration(data);
        ShowPlacementInstructions();

        if (!string.IsNullOrWhiteSpace(configUrl))
            dataLoader.LoadRemoteData(configUrl, OnRemoteConfigLoaded, OnRemoteConfigFailed);
    }

    private void OnLocalConfigFailed(string error)
    {
        Debug.LogWarning("Bundled lesson data could not be loaded: " + error);

        if (string.IsNullOrWhiteSpace(configUrl))
        {
            ShowConfigurationError(error);
            return;
        }

        dataLoader.LoadRemoteData(configUrl, OnRemoteConfigLoaded, delegate(string remoteError)
        {
            Debug.LogError("Neither bundled nor remote lesson data could be loaded. " + remoteError);
            ShowConfigurationError("The lesson could not be loaded. Check your connection or restore " +
                                   "Assets/StreamingAssets/config.json.");
        });
    }

    private void OnRemoteConfigLoaded(Data data)
    {
        // Do not change the question underneath a player. Apply refreshed content
        // at the next question boundary instead.
        if (WorldPlaced || currentQuestion >= 0)
        {
            pendingRemoteConfig = data;
            Debug.Log("A lesson update is ready and will be used on the next question.");
            return;
        }

        ApplyConfiguration(data);
        ShowPlacementInstructions();
    }

    private void OnRemoteConfigFailed(string error)
    {
        // The bundled version remains active. Network access is an enhancement,
        // not a requirement for the core game loop.
        Debug.LogWarning("Remote lesson refresh failed; continuing with bundled content. " + error);
    }

    private void ApplyConfiguration(Data data)
    {
        if (data == null)
        {
            ShowConfigurationError("The lesson data was empty.");
            return;
        }

        configData = data;
        configurationReady = true;
        ConfigureSpeechVoice();
    }

    private void ConfigureSpeechVoice()
    {
        if (TextToSpeech.instance == null || configData == null || configData.voice == null)
            return;

        TextToSpeech.instance.ConfigureVoice(
            configData.voice.language,
            configData.voice.gender,
            configData.voice.voice);
    }

    private void CreateResetPlacementButton()
    {
        if (helpText == null || helpText.canvas == null)
            return;

        GameObject buttonObject = new GameObject(
            "Reset Placement Button",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
        buttonObject.transform.SetParent(helpText.canvas.transform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, 76f);
        buttonRect.sizeDelta = new Vector2(220f, 56f);

        Image background = buttonObject.GetComponent<Image>();
        background.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        background.type = Image.Type.Sliced;
        background.color = new Color32(23, 91, 82, 235);
        background.raycastTarget = true;

        resetPlacementButton = buttonObject.GetComponent<Button>();
        resetPlacementButton.targetGraphic = background;
        resetPlacementButton.onClick.AddListener(ResetPlacement);
        resetPlacementButton.gameObject.SetActive(false);

        GameObject labelObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 4f);
        labelRect.offsetMax = new Vector2(-8f, -4f);

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = "REPOSITION WORLD";
        label.font = helpText.font;
        label.fontSize = 18f;
        label.fontStyle = FontStyles.Bold;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        label.enableWordWrapping = false;
    }

    /// <summary>Called by PlaceObject after the AR environment is instantiated.</summary>
    public bool StartGame()
    {
        if (!configurationReady || configData == null)
        {
            ShowStatus("The lesson is still loading. Please try again in a moment.");
            return false;
        }

        if (World == null)
        {
            ShowStatus("The AR world could not be placed. Tap a detected flat surface to try again.");
            return false;
        }

        scene = World.GetComponent<SceneScript>();
        if (scene == null)
        {
            ShowStatus("Setup error: the placed environment is missing its SceneScript component.");
            Debug.LogError("The configured world prefab must contain a SceneScript component.");
            return false;
        }

        GameObject antPrefab = GetRandomAntPrefab();
        if (antPrefab == null || !scene.SpawnAnt(antPrefab))
        {
            ShowStatus("Setup error: no valid ant prefab or animator was found.");
            Debug.LogError("Assign at least one valid ant prefab with an Animator to GameManager.");
            return false;
        }

        currentQuestion = -1;
        correctAnswerCount = 0;
        answersSubmitted = 0;
        WorldPlaced = true;

        if (resetPlacementButton != null)
            resetPlacementButton.gameObject.SetActive(true);

        scene.BeginGame();
        LoadNextQuestion();
        return true;
    }

    private GameObject GetRandomAntPrefab()
    {
        if (Ants == null || Ants.Length == 0)
            return null;

        int startingIndex = UnityEngine.Random.Range(0, Ants.Length);
        for (int offset = 0; offset < Ants.Length; offset++)
        {
            GameObject candidate = Ants[(startingIndex + offset) % Ants.Length];
            if (candidate != null)
                return candidate;
        }

        return null;
    }

    public void LoadNextQuestion()
    {
        if (pendingRemoteConfig != null)
        {
            Data refreshedData = pendingRemoteConfig;
            pendingRemoteConfig = null;
            ApplyConfiguration(refreshedData);
            currentQuestion = -1;
        }

        if (!configurationReady || configData == null || configData.Questions == null ||
            configData.Questions.Length == 0 || scene == null)
        {
            ShowStatus("No playable questions are available.");
            return;
        }

        currentQuestion = (currentQuestion + 1) % configData.Questions.Length;
        Question question = configData.Questions[currentQuestion];
        if (question == null || !scene.SetQuestion(question))
        {
            ShowStatus("This question could not be displayed. Please check the lesson configuration.");
            return;
        }

        string prompt = FormatQuestionPrompt(question);
        string progress = "Question " + (currentQuestion + 1) + " of " + configData.Questions.Length +
                         "\nScore: " + correctAnswerCount + "/" + answersSubmitted;
        ShowStatus(progress + "\n\n" + prompt + "\n\nTap one of the three answers.");
        TextToSpeech.Speak(prompt);
    }

    private string FormatQuestionPrompt(Question question)
    {
        if (configData == null || string.IsNullOrWhiteSpace(configData.QuestionText))
            return question.question;

        return configData.QuestionText
            .Replace("{Question}", question.question)
            .Replace("{question}", question.question);
    }

    /// <summary>Updates score and feedback after the ant reaches a selected answer.</summary>
    public void RecordAnswer(bool isCorrect)
    {
        answersSubmitted++;
        if (isCorrect)
            correctAnswerCount++;

        string feedback = isCorrect ? configData.CorrectText : configData.IncorrectText;
        if (string.IsNullOrWhiteSpace(feedback))
            feedback = isCorrect ? "Correct!" : "Not quite. Try the next one.";

        ShowStatus(feedback + "\n\nScore: " + correctAnswerCount + "/" + answersSubmitted);
        TextToSpeech.Speak(feedback);
    }

    /// <summary>Called by the HUD button and the placement controller.</summary>
    public void ResetPlacement()
    {
        if (PlaceObject.instance != null)
        {
            PlaceObject.instance.ResetPlacement();
            return;
        }

        OnPlacementReset();
    }

    public void OnPlacementReset()
    {
        WorldPlaced = false;
        World = null;
        scene = null;
        currentQuestion = -1;
        correctAnswerCount = 0;
        answersSubmitted = 0;

        if (resetPlacementButton != null)
            resetPlacementButton.gameObject.SetActive(false);

        ShowPlacementInstructions();
    }

    private void ShowPlacementInstructions()
    {
        if (!configurationReady)
            return;

        string lessonHelp = configData != null ? configData.HelpText : null;
        if (string.IsNullOrWhiteSpace(lessonHelp))
            lessonHelp = "Welcome to Ants AR.";

        ShowStatus(lessonHelp + "\n\nMove your camera slowly, then tap a flat surface to place the quiz.");
    }

    private void ShowConfigurationError(string details)
    {
        configurationReady = false;
        string message = "The lesson could not be loaded. Check the bundled config or reconnect and restart.";
        if (!string.IsNullOrWhiteSpace(details))
            Debug.LogError(details);
        ShowStatus(message);
    }

    private void ShowStatus(string message)
    {
        if (helpText == null)
            return;

        helpText.gameObject.SetActive(true);
        helpText.text = message ?? string.Empty;
    }
}
