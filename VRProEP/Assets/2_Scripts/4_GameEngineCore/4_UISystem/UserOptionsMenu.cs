using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserOptionsMenu : MonoBehaviour {

    public GameObject mainMenu;
    public GameObject createUserMenu;
    public GameObject loadUserMenu;


    public void CreateUserMenu()
    {
        // Switch
        createUserMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void LoadUserMenu()
    {
        // Switch
        loadUserMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        // Return to main menu
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
