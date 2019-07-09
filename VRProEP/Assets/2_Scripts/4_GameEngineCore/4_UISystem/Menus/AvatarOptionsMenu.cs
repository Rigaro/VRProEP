using System.Collections;
using UnityEngine;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;

public class AvatarOptionsMenu : MonoBehaviour {

    public GameObject mainMenu;
    public GameObject experimentMenu;
    public GameObject transradialMenu;
    public LogManager logManager;
    public Camera mainCamera;

    public void LoadAbleBodiedAvatar()
    {
        try
        {
            AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.AbleBodied);
            AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.AbleBodied);

            KeepPlayerGameObjects();

            StartCoroutine(ResetCamera());
            StartCoroutine(DisplayInformationAndReturn(2.0f, "Successfully loaded able-bodied avatar."));
        }
        catch (System.Exception e)
        {
            StartCoroutine(DisplayInformationAndReturn(10.0f, e.Message));
        }
    }

    public void LoadTranshumeralAvatar()
    {
        try
        {
            AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.Transhumeral);
            AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.Transhumeral);

            //mainCamera.fieldOfView = 60;
            KeepPlayerGameObjects();

            StartCoroutine(ResetCamera());
            StartCoroutine(DisplayInformationAndReturn(2.0f, "Successfully loaded transhumeral avatar."));

            // Initialize prosthesis
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            ConfigurableElbowManager elbowManager = prosthesisManagerGO.AddComponent<ConfigurableElbowManager>();
            elbowManager.InitializeProsthesis(SaveSystem.ActiveUser.upperArmLength, (SaveSystem.ActiveUser.forearmLength + (SaveSystem.ActiveUser.handLength / 2.0f)));
            // Set the reference generator to jacobian-based.
            elbowManager.ChangeReferenceGenerator("VAL_REFGEN_JACOBIANSYN");
        }
        catch (System.Exception e)
        {
            StartCoroutine(DisplayInformationAndReturn(10.0f, e.Message));
        }
    }

    public void LoadTransradialAvatarMenu()
    {
        transradialMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public IEnumerator ResetCamera()
    {
        mainCamera.enabled = false;
        yield return new WaitForSeconds(0.01f);
        mainCamera.enabled = true;

    }

    public IEnumerator DisplayInformationAndReturn(float time, string info)
    {
        logManager.DisplayInformationOnLog(time, info);
        yield return new WaitForSecondsRealtime(time);
        ReturnToExperimentMenu();
    }

    public void ReturnToMainMenu()
    {
        // Clear log
        logManager.ClearLog();

        // Return to main menu
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ReturnToExperimentMenu()
    {
        // Return to main menu
        experimentMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    private void KeepPlayerGameObjects()
    {
        // Keep player and avatar objects
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        GameObject avatarGO = GameObject.FindGameObjectWithTag("Avatar");
        DontDestroyOnLoad(playerGO);
        DontDestroyOnLoad(avatarGO);
    }

}
