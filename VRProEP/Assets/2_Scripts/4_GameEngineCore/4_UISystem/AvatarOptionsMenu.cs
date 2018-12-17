using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;

public class AvatarOptionsMenu : MonoBehaviour {

    public GameObject mainMenu;
    public TextMeshProUGUI logTMP;

    public void LoadAbleBodiedAvatar()
    {
        AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.AbleBodied);
        AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied);

        StartCoroutine(DisplayInformationAndReturn(3.0f, "Successfully loaded able-bodied avatar."));
    }

    public void LoadTranshumeralAvatar()
    {
        AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.Transhumeral);
        AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.Transhumeral);

        StartCoroutine(DisplayInformationAndReturn(3.0f, "Successfully loaded able-bodied avatar."));
    }

    public IEnumerator DisplayInformationAndReturn(float time, string info)
    {
        string defaultText = logTMP.text;
        logTMP.text += info;
        yield return new WaitForSecondsRealtime(time);
        logTMP.text = defaultText;
        ReturnToMainMenu();
    }

    public void ReturnToMainMenu()
    {
        // Clear log
        StopAllCoroutines();
        logTMP.text = "Log: \n";

        // Return to main menu
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }

}
