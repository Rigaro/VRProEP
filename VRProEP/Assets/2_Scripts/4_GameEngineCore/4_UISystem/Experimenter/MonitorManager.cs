using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace VRProEP.GameEngineCore
{
    public class MonitorManager : ConsoleManager
    {
        public TextMeshPro generalArea;
        public TextMeshPro errorArea;

        public override void DisplayText(string text)
        {
            generalArea.text = text;
        }

        public override void DisplayText(string text, float time)
        {
            StartCoroutine(DisplayTextCoroutine(generalArea, text, time));
        }

        public override void DisplayError(int errorCode, string text)
        {
            StartCoroutine(DisplayTextCoroutine(errorArea, "Error # " + errorCode + ": " + text, 3.0f));
        }
    }
}