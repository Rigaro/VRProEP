using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;

public class TransradialOptionsMenu : MonoBehaviour
{
    private string ip;
    private int port = 0;

    public GameObject mainMenu;
    public GameObject experimentMenu;
    public GameObject avatarSelectionMenu;
    public GameObject noFeedbackButton;
    public GameObject feedbackButton;
    public GameObject loadFeedbackMenu;

    public LogManager logManager;
    public Camera mainCamera;
       
    public void UpdateIP(string ip)
    {
        this.ip = ip;
    }

    public void UpdatePort(string port)
    {
        this.port = int.Parse(port);
    }

    public void EnableFeedbackMenu()
    {
        noFeedbackButton.SetActive(false);
        feedbackButton.SetActive(false);
        loadFeedbackMenu.SetActive(true);
    }

    public void LoadNoFeedbackTransradialAvatar()
    {
        try
        {
            AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.Transradial);
            AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.Transradial);

            //mainCamera.fieldOfView = 60;
            KeepPlayerGameObjects();

            StartCoroutine(ResetCamera());
            StartCoroutine(DisplayInformationAndReturn(2.0f, "Successfully loaded transradial avatar."));

            // Initialize prosthesis
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            FakeEMGBoniHand prosthesisManager = prosthesisManagerGO.AddComponent<FakeEMGBoniHand>();
            //prosthesisManager.InitializeProsthesis();
        }
        catch (System.Exception e)
        {
            StartCoroutine(DisplayInformationAndReturn(10.0f, e.Message));
        }
    }

    public void LoadFeedbackTransradialAvatar()
    {
        if (ip == null || ip == string.Empty || port == 0)
            logManager.DisplayInformationOnLog(3.0f, "The provided sensor info is invalid.");

        try
        {
            AvatarSystem.LoadPlayer(SaveSystem.ActiveUser.type, AvatarType.Transradial);
            AvatarSystem.LoadAvatar(SaveSystem.ActiveUser, AvatarType.Transradial);

            //mainCamera.fieldOfView = 60;
            KeepPlayerGameObjects();

            StartCoroutine(ResetCamera());
            StartCoroutine(DisplayInformationAndReturn(2.0f, "Successfully loaded transradial avatar."));

            // Initialize prosthesis feedback system
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            FakeEMGBoniHand prosthesisManager = prosthesisManagerGO.AddComponent<FakeEMGBoniHand>();
            prosthesisManager.InitialiseFeedbackSystem(ip, port);
        }
        catch (System.Exception e)
        {
            StartCoroutine(DisplayInformationAndReturn(10.0f, e.Message));
        }
    }

    public void ReturnToAvatarSelection()
    {
        SetDefaultMenu();
        avatarSelectionMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        SetDefaultMenu();
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ReturnToExperimentMenu()
    {
        SetDefaultMenu();
        experimentMenu.SetActive(true);
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

    private void KeepPlayerGameObjects()
    {
        // Keep player and avatar objects
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        GameObject avatarGO = GameObject.FindGameObjectWithTag("Avatar");
        DontDestroyOnLoad(playerGO);
        DontDestroyOnLoad(avatarGO);
    }

    private void SetDefaultMenu()
    {
        noFeedbackButton.SetActive(true);
        feedbackButton.SetActive(true);
        loadFeedbackMenu.SetActive(false);
    }
}
