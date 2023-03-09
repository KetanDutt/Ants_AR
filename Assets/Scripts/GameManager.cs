using UnityEngine;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool WorldPlaced = false;
    public GameObject World;

    [SerializeField] private string configUrl;
    [SerializeField] public Camera arCamera;
    [SerializeField] private GameObject[] Ants;
    [SerializeField] public TextMeshProUGUI helpText;

    public Data configData;
    public int currentQuestion = -1;
    private SceneScript scene;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        helpText.gameObject.SetActive(false);
        configUrl = "https://sa.rtctek.com/AR_demo/config.json";
        DataLoader dataLoader = gameObject.GetComponent<DataLoader>();
        dataLoader.LoadData(configUrl, (data) =>
        {
            configData = data;

            //Load Voice
            TextToSpeech.instance.voiceConfig.name = configData.voice.voice;
            TextToSpeech.instance.voiceConfig.languageCode = configData.voice.language;
            TextToSpeech.instance.voiceConfig.gender = Enumerators.SsmlVoiceGender.MALE;
            TextToSpeech.convert(configData.HelpText);

            helpText.gameObject.SetActive(true);
            helpText.text = configData.HelpText;
        });
    }

    public void StartGame()
    {
        helpText.gameObject.transform.parent.gameObject.SetActive(false);
        // TextToSpeech.convert(configData.WelcomeText, true);
        scene = World.GetComponent<SceneScript>();
        loadNextQuestion();
        scene.SpawnAnt(Ants[Random.Range(0, Ants.Length)]);
    }

    public void loadNextQuestion()
    {
        currentQuestion++;
        if (currentQuestion > configData.Questions.Length - 1)
            currentQuestion = 0;
        scene.setQuestion(configData.Questions[currentQuestion]);

        TextToSpeech.convert(configData.QuestionText.Replace("{Question}", configData.Questions[currentQuestion].question));
    }
}
