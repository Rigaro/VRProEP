using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VRProEP.GameEngineCore;

public class InstructionManager : ConsoleManager
{
    public TextMeshPro instructionArea;

    public override void DisplayText(string text, float time)
    {
        StartCoroutine(DisplayTextCoroutine(instructionArea, text, time));
    }

    public override void DisplayError(int errorCode, string text)
    {
        StartCoroutine(DisplayTextCoroutine(instructionArea, "Error # " + errorCode + ": " + text, 3.0f));
    }
}
