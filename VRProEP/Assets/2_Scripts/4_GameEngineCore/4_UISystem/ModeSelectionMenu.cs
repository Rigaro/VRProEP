using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ModeSelectionMenu : MonoBehaviour {

    public GameObject mainMenu;

    public void LoadPlayground()
    {
        // Keep player and avatar objects
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        GameObject avatarGO = GameObject.FindGameObjectWithTag("Avatar");
        DontDestroyOnLoad(playerGO);
        DontDestroyOnLoad(avatarGO);
        // Load level.
        SteamVR_LoadLevel.Begin("DemoPlayground");
    }

    public void ReturnToMainMenu()
    {
        // Return to main menu
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
