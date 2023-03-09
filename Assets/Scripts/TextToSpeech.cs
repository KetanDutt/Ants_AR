using UnityEngine;
using System.Collections.Generic;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;

public class TextToSpeech : MonoBehaviour
{
    public static TextToSpeech instance;

    private AudioSource audioSource;
    private List<TTSClip> ttsClips = new List<TTSClip>();
    public VoiceConfig voiceConfig = new VoiceConfig()
    {
        gender = Enumerators.SsmlVoiceGender.MALE,
        languageCode = Enumerators.LanguageCode.en_US.ToString(),
        name = "en-US-Wavenet-A"
    };

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {

        GCTextToSpeech.Instance.GetVoicesSuccessEvent += GetVoicesSuccess;
        GCTextToSpeech.Instance.SynthesizeSuccessEvent += SynthesizeSuccessEvent;

        // GCTextToSpeech.Instance.GetVoices(new GetVoicesRequest()
        // {
        //     languageCode = GCTextToSpeech.Instance.PrepareLanguage(Enumerators.LanguageCode.en_US)
        // });

        audioSource = GetComponent<AudioSource>();
    }

    public static void convert(string text, bool playOnSyynthesize = true)
    {
        instance.textToSpeach(text, playOnSyynthesize);
    }

    private void SynthesizeSuccessEvent(PostSynthesizeResponse response, long requestId)
    {
        AudioClip clip = GCTextToSpeech.Instance.GetAudioClipFromBase64(response.audioContent, Constants.DEFAULT_AUDIO_ENCODING);
        foreach (TTSClip tTSClip in ttsClips)
        {
            if (tTSClip.requestID == requestId)
            {
                tTSClip.clip = clip;
                if (tTSClip.playOnSyynthesize)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                    Debug.Log("success => " + tTSClip.text);
                    return;
                }
            }
        }
    }

    public void GetVoicesSuccess(GetVoicesResponse response, long requestId)
    {
        Debug.Log("success GetVoicesSuccess");
        FrostweepGames.Plugins.GoogleCloud.TextToSpeech.Voice[] _voices = response.voices;

        for (int i = 0; i < _voices.Length; i++)
        {
            Debug.Log(_voices[i].name);
        }
    }

    public void textToSpeach(string text, bool playOnSyynthesize = true)
    {
        TTSClip found = null;
        foreach (TTSClip tTSClip in ttsClips)
        {
            if (text.Equals(tTSClip.text))
            {
                found = tTSClip;
                break;
            }
        }
        if (found == null)
        {
            Debug.Log("sent => " + text);
            try
            {
                long requestID = GCTextToSpeech.Instance.Synthesize(text, voiceConfig, false, 1.0, 1.0, 16000, new string[0] { }, new Enumerators.TimepointType[] { Enumerators.TimepointType.TIMEPOINT_TYPE_UNSPECIFIED });
                ttsClips.Add(new TTSClip()
                {
                    requestID = requestID,
                    text = text,
                    playOnSyynthesize = playOnSyynthesize
                });
            }
            catch (System.Exception e)
            {
                Debug.Log(voiceConfig);
                Debug.Log(e.Data.ToString());
                throw;
            }
        }
        else if (playOnSyynthesize)
        {
            audioSource.clip = found.clip;
            audioSource.Play();
        }
    }
}


public class TTSClip
{
    public long requestID;
    public string text;
    public bool playOnSyynthesize;
    public AudioClip clip;
}