using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.IO;

public class DataLoader : MonoBehaviour
{
    public LocalData LoadLocalData()
    {
        string jsonStr = File.ReadAllText(Application.streamingAssetsPath + "/config.json");
        LocalData localData = JsonUtility.FromJson<LocalData>(jsonStr);
        return localData;
    }

    public void LoadData(string url, Action<Data> callback)
    {
        StartCoroutine(GetConfig(url, callback));
    }

    IEnumerator GetConfig(string url, Action<Data> callback)
    {
        using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(url))
        {
            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonStr = unityWebRequest.downloadHandler.text;
                callback(JsonUtility.FromJson<Data>(jsonStr));
            }
            else
            {
                Debug.Log(unityWebRequest.error);
            }
        }
    }
}
[System.Serializable]
public class LocalData
{
    public string configUrl;

}
[System.Serializable]
public class Question
{
    public string question;
    public string[] Answers;

}

[System.Serializable]
public class Ant
{
    public string name;
    public string skin;

}
[System.Serializable]
public class Voice
{
    public string language;
    public string gender;
    public string voice;

}

[System.Serializable]
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