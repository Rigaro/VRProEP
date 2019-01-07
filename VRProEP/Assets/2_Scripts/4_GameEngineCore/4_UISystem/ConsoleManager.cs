using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace VRProEP.GameEngineCore
{
    public abstract class ConsoleManager : MonoBehaviour
    {
        public abstract void DisplayText(string text, float time);
        public abstract void DisplayError(int errorCode, string text);

        protected IEnumerator DisplayTextCoroutine(TextMeshPro textArea, string text, float time)
        {
            textArea.text = text;
            yield return new WaitForSecondsRealtime(time);
            textArea.text = "";
        }
    }

}
