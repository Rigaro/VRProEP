using System.Collections;
using TMPro;
using UnityEngine;
using VRProEP.GameEngineCore;
using Valve.VR;

public class MainMenu : MonoBehaviour {

    public GameObject createNewUserMenu;
    public GameObject loadUserMenu;
    public TextMeshProUGUI logTMP;
    public TextMeshProUGUI activeUserTMP;
    public bool createdUser = false;
    public bool loadedUser = false;


    public void OnEnable()
    {
        if (createdUser)
        {
            StartCoroutine(DisplayInformationOnLog(3.0f, "Created new user with ID " + SaveSystem.ActiveUser.id));
            createdUser = false;
        }

        if (loadedUser)
        {
            StartCoroutine(DisplayInformationOnLog(3.0f, "Loaded user with ID " + SaveSystem.ActiveUser.id));
            createdUser = false;
        }

        // Display active user name.
        if (SaveSystem.ActiveUser != null)
            activeUserTMP.text = "Active User: " + SaveSystem.ActiveUser.name + " " + SaveSystem.ActiveUser.familyName;

    }

    public void CreateNewUser()
    {
        createNewUserMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void LoadUser()
    {
        loadUserMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void LoadPlayground()
    {
        SteamVR_LoadLevel.Begin("DemoPlayground");
    }
    
    public IEnumerator DisplayInformationOnLog(float time, string info)
    {
        string defaultText = logTMP.text;
        logTMP.text += info;
        yield return new WaitForSecondsRealtime(time);
        logTMP.text = defaultText;
    }
}
