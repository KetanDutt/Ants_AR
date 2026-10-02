using System;
using System.Collections.Generic;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TextToSpeech : MonoBehaviour
{
    private const int MaximumCachedClips = 24;

    public static TextToSpeech instance;

    private AudioSource audioSource;
    private GCTextToSpeech client;
    private bool isSubscribed;
    private bool hasWarnedAboutMissingKey;

    private readonly Dictionary<string, AudioClip> clipsByKey = new Dictionary<string, AudioClip>();
    private readonly List<string> cacheOrder = new List<string>();
    private readonly Dictionary<long, PendingSpeech> pendingByRequest = new Dictionary<long, PendingSpeech>();
    private readonly Dictionary<string, PendingSpeech> pendingByKey = new Dictionary<string, PendingSpeech>();

    public VoiceConfig voiceConfig = new VoiceConfig
    {
        gender = Enumerators.SsmlVoiceGender.MALE,
        languageCode = "en-US",
        name = "en-US-Wavenet-A"
    };

    private sealed class PendingSpeech
    {
        public long RequestId;
        public string CacheKey;
        public string Text;
        public bool PlayWhenReady;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
            audioSource.playOnAwake = false;
    }

    private void Start()
    {
        EnsureClient();
    }

    private void OnDisable()
    {
        UnsubscribeFromClient();
    }

    private void OnDestroy()
    {
        UnsubscribeFromClient();

        foreach (AudioClip clip in clipsByKey.Values)
        {
            if (clip != null)
                Destroy(clip);
        }

        clipsByKey.Clear();
        cacheOrder.Clear();
        pendingByRequest.Clear();
        pendingByKey.Clear();

        if (instance == this)
            instance = null;
    }

    public static void Speak(string text, bool playWhenReady = true)
    {
        if (instance == null)
        {
            Debug.LogWarning("Text-to-speech is not available in this scene.");
            return;
        }

        instance.RequestSpeech(text, playWhenReady);
    }

    public void ConfigureVoice(string languageCode, string gender, string voiceName)
    {
        if (voiceConfig == null)
            voiceConfig = new VoiceConfig();

        if (!string.IsNullOrWhiteSpace(languageCode))
            voiceConfig.languageCode = languageCode.Trim().Replace('_', '-');

        if (!string.IsNullOrWhiteSpace(voiceName))
            voiceConfig.name = voiceName.Trim();

        Enumerators.SsmlVoiceGender parsedGender;
        if (!string.IsNullOrWhiteSpace(gender) &&
            Enum.TryParse(gender.Trim(), true, out parsedGender) &&
            parsedGender != Enumerators.SsmlVoiceGender.SSML_VOICE_GENDER_UNSPECIFIED)
        {
            voiceConfig.gender = parsedGender;
        }
    }

    private void RequestSpeech(string text, bool playWhenReady)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (!EnsureClient())
            return;

        if (string.IsNullOrWhiteSpace(client.apiKey))
        {
            if (!hasWarnedAboutMissingKey)
            {
                Debug.LogWarning("Google Cloud Text-to-Speech is disabled because no API key is configured. " +
                                 "Use a secured backend proxy for production builds.");
                hasWarnedAboutMissingKey = true;
            }
            return;
        }

        string cacheKey = CreateCacheKey(text);
        AudioClip cachedClip;
        if (clipsByKey.TryGetValue(cacheKey, out cachedClip) && cachedClip != null)
        {
            if (playWhenReady)
                Play(cachedClip);
            return;
        }

        PendingSpeech existingRequest;
        if (pendingByKey.TryGetValue(cacheKey, out existingRequest))
        {
            existingRequest.PlayWhenReady |= playWhenReady;
            return;
        }

        try
        {
            // Use the stable v1 API overload. Timepoint support is not used by
            // this app and would unnecessarily select the beta request type.
            long requestId = client.Synthesize(
                text,
                voiceConfig,
                false,
                1.0,
                1.0,
                Constants.DEFAULT_SAMPLE_RATE,
                new string[0]);

            PendingSpeech pending = new PendingSpeech
            {
                RequestId = requestId,
                CacheKey = cacheKey,
                Text = text,
                PlayWhenReady = playWhenReady
            };
            pendingByRequest[requestId] = pending;
            pendingByKey[cacheKey] = pending;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not request speech synthesis: " + exception.Message);
        }
    }

    private bool EnsureClient()
    {
        if (client == null)
            client = GCTextToSpeech.Instance;

        if (client == null)
            return false;

        if (!isSubscribed)
        {
            client.SynthesizeSuccessEvent += OnSynthesisSucceeded;
            client.SynthesizeFailedEvent += OnSynthesisFailed;
            isSubscribed = true;
        }

        return true;
    }

    private void UnsubscribeFromClient()
    {
        if (client != null && isSubscribed)
        {
            client.SynthesizeSuccessEvent -= OnSynthesisSucceeded;
            client.SynthesizeFailedEvent -= OnSynthesisFailed;
        }

        isSubscribed = false;
    }

    private void OnSynthesisSucceeded(PostSynthesizeResponse response, long requestId)
    {
        PendingSpeech pending;
        if (!pendingByRequest.TryGetValue(requestId, out pending))
            return;

        RemovePending(pending);
        if (response == null || string.IsNullOrEmpty(response.audioContent))
        {
            Debug.LogWarning("Speech synthesis returned no audio content.");
            return;
        }

        AudioClip clip;
        try
        {
            clip = client.GetAudioClipFromBase64(response.audioContent, Constants.DEFAULT_AUDIO_ENCODING);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not decode synthesized audio: " + exception.Message);
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("Speech synthesis returned an invalid audio clip.");
            return;
        }

        CacheClip(pending.CacheKey, clip);
        if (pending.PlayWhenReady)
            Play(clip);
    }

    private void OnSynthesisFailed(string error, long requestId)
    {
        PendingSpeech pending;
        if (pendingByRequest.TryGetValue(requestId, out pending))
            RemovePending(pending);

        Debug.LogWarning("Speech synthesis failed: " + error);
    }

    private void RemovePending(PendingSpeech pending)
    {
        pendingByRequest.Remove(pending.RequestId);
        pendingByKey.Remove(pending.CacheKey);
    }

    private string CreateCacheKey(string text)
    {
        string language = voiceConfig != null ? voiceConfig.languageCode : string.Empty;
        string name = voiceConfig != null ? voiceConfig.name : string.Empty;
        string gender = voiceConfig != null ? voiceConfig.gender.ToString() : string.Empty;
        return language + "|" + name + "|" + gender + "|" + text;
    }

    private void CacheClip(string key, AudioClip clip)
    {
        AudioClip previousClip;
        if (clipsByKey.TryGetValue(key, out previousClip))
        {
            if (previousClip != null && previousClip != clip)
                Destroy(previousClip);
            clipsByKey[key] = clip;
            return;
        }

        clipsByKey.Add(key, clip);
        cacheOrder.Add(key);

        while (cacheOrder.Count > MaximumCachedClips)
        {
            string oldestKey = cacheOrder[0];
            cacheOrder.RemoveAt(0);

            AudioClip oldestClip;
            if (clipsByKey.TryGetValue(oldestKey, out oldestClip))
            {
                clipsByKey.Remove(oldestKey);
                if (oldestClip != null && oldestClip != audioSource.clip)
                    Destroy(oldestClip);
            }
        }
    }

    private void Play(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
}
