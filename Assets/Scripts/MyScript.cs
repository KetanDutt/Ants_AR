using UnityEngine;
using System.Collections;
using System;

public class MyScript : MonoBehaviour
{
    static string DebugIdentifier = "*******<KETAN DUTT>****** ";
    public static void Log(string str)
    {
        Debug.Log(DebugIdentifier + str);
    }

    public static IEnumerator waiter(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        callback();
    }
}
