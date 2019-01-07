using System.Collections;
using UnityEngine;
using TMPro;

public class LogManager : MonoBehaviour
{
    public TextMeshProUGUI logTMP;

    public void DisplayInformationOnLog(float time, string info)
    {
        StartCoroutine(Display(time, info));
    }

    private IEnumerator Display(float time, string info)
    {
        string defaultText = logTMP.text;
        logTMP.text += info;
        yield return new WaitForSecondsRealtime(time);
        logTMP.text = defaultText;
    }

    public void ClearLog()
    {
        // Clear log
        StopAllCoroutines();
        logTMP.text = "Log: \n";
    }
}
