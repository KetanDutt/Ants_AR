using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Loads and validates lesson data from either the configured HTTPS endpoint or
/// the bundled StreamingAssets fallback. Local files are read through
/// UnityWebRequest so the same code works on Android, iOS, and desktop.
/// </summary>
public class DataLoader : MonoBehaviour
{
    private const int RequestTimeoutSeconds = 10;
    private const string LocalConfigFileName = "config.json";

    public void LoadRemoteData(string url, Action<Data> onLoaded, Action<string> onFailed)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            if (onFailed != null)
                onFailed("No remote configuration URL was provided.");
            return;
        }

        Uri parsedUri;
        if (!Uri.TryCreate(url, UriKind.Absolute, out parsedUri) || parsedUri.Scheme != Uri.UriSchemeHttps)
        {
            if (onFailed != null)
                onFailed("The remote configuration URL must be a valid HTTPS URL.");
            return;
        }

        StartCoroutine(DownloadConfiguration(url, onLoaded, onFailed));
    }

    public void LoadLocalData(Action<Data> onLoaded, Action<string> onFailed)
    {
        string path = Application.streamingAssetsPath + "/" + LocalConfigFileName;
        StartCoroutine(DownloadConfiguration(path, onLoaded, onFailed));
    }

    private IEnumerator DownloadConfiguration(string url, Action<Data> onLoaded, Action<string> onFailed)
    {
        UnityWebRequest request;
        try
        {
            request = UnityWebRequest.Get(url);
        }
        catch (Exception exception)
        {
            if (onFailed != null)
                onFailed("Could not create the configuration request: " + exception.Message);
            yield break;
        }

        using (request)
        {
            request.timeout = RequestTimeoutSeconds;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (onFailed != null)
                    onFailed(request.error ?? "The configuration request failed.");
                yield break;
            }

            Data data;
            string validationError;
            if (!TryParseAndValidate(request.downloadHandler.text, out data, out validationError))
            {
                if (onFailed != null)
                    onFailed(validationError);
                yield break;
            }

            if (onLoaded != null)
                onLoaded(data);
        }
    }

    private static bool TryParseAndValidate(string json, out Data data, out string error)
    {
        data = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(json))
        {
            error = "The configuration file is empty.";
            return false;
        }

        try
        {
            data = JsonUtility.FromJson<Data>(json);
        }
        catch (Exception exception)
        {
            error = "The configuration JSON could not be parsed: " + exception.Message;
            return false;
        }

        if (data == null || data.Questions == null || data.Questions.Length == 0)
        {
            error = "The configuration must contain at least one question.";
            return false;
        }

        for (int questionIndex = 0; questionIndex < data.Questions.Length; questionIndex++)
        {
            Question question = data.Questions[questionIndex];
            if (question == null || string.IsNullOrWhiteSpace(question.question))
            {
                error = "Question " + (questionIndex + 1) + " is missing its question text.";
                return false;
            }

            string[] answers = question.GetAnswers();
            if (answers == null || answers.Length < 3)
            {
                error = "Question " + (questionIndex + 1) + " must provide at least three answers.";
                return false;
            }

            for (int answerIndex = 0; answerIndex < 3; answerIndex++)
            {
                if (string.IsNullOrWhiteSpace(answers[answerIndex]))
                {
                    error = "Question " + (questionIndex + 1) + " has an empty answer choice.";
                    return false;
                }
            }

            string correctAnswer = question.GetCorrectAnswer(answers);
            if (string.IsNullOrWhiteSpace(correctAnswer) || !IsOneOfFirstThreeAnswers(correctAnswer, answers))
            {
                error = "Question " + (questionIndex + 1) +
                        " must identify a correctAnswer that matches one of its first three answers. " +
                        "Legacy content may use question text as the answer when it matches a choice.";
                return false;
            }
        }

        return true;
    }

    private static bool IsOneOfFirstThreeAnswers(string value, string[] answers)
    {
        string normalizedValue = value.Trim();
        for (int index = 0; index < 3; index++)
        {
            if (string.Equals(normalizedValue, answers[index].Trim(), StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}

[Serializable]
public class Question
{
    // Kept as "question" to remain compatible with the existing service contract.
    public string question;

    // The original service uses "Answers". The lowercase alias supports newer JSON.
    public string[] Answers;
    public string[] answers;

    // New content should explicitly state the correct choice.
    public string correctAnswer;

    public string[] GetAnswers()
    {
        return Answers != null && Answers.Length > 0 ? Answers : answers;
    }

    public string GetCorrectAnswer(string[] availableAnswers)
    {
        if (!string.IsNullOrWhiteSpace(correctAnswer))
            return correctAnswer.Trim();

        // Backward compatibility with the original project, which treated the
        // question string itself as the correct answer.
        if (!string.IsNullOrWhiteSpace(question) && availableAnswers != null)
        {
            for (int index = 0; index < Mathf.Min(3, availableAnswers.Length); index++)
            {
                if (string.Equals(question.Trim(), availableAnswers[index].Trim(), StringComparison.OrdinalIgnoreCase))
                    return availableAnswers[index].Trim();
            }
        }

        return string.Empty;
    }
}

[Serializable]
public class Ant
{
    public string name;
    public string skin;
}

[Serializable]
public class Voice
{
    public string language;
    public string gender;
    public string voice;
}

[Serializable]
public class Data
{
    public Voice voice;
    public string HelpText;
    public string WelcomeText;
    public string QuestionText;
    public string WhenAntSelectedText;
    public string CorrectText;
    public string IncorrectText;
    public string[] GameTexts;
    public Ant[] Ants;
    public Question[] Questions;
}
