using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VRProEP.GameEngineCore;

public class LoadUser : MonoBehaviour {

    public GameObject mainMenu;
    public Dropdown availableUserDropdown;

    private List<string> userList = new List<string>();
    private int selectedUser = 0;
    private string userDataFolder;

    private void OnEnable()
    {
        // Get the data folder
        userDataFolder =  Application.dataPath + "/UserData";
        // Empty all options.
        availableUserDropdown.ClearOptions();

        // Get all available user IDs
        string[] availableUsers = Directory.GetDirectories(userDataFolder);
        // Add an empty one as default to force selection.
        userList.Add(string.Empty);

        // Add them to the user list
        foreach (string user in availableUsers)
        {
            userList.Add(user.Substring(userDataFolder.Length + 1));
        }
        // Add the options to the dropdown
        availableUserDropdown.AddOptions(userList);
        
    }

    public void UpdatedSelectedUser(int selectedUser)
    {
        this.selectedUser = selectedUser;
    }

    public void LoadSelectedUser()
    {
        if (selectedUser != 0)
        {
            SaveSystem.LoadUserData(userList[selectedUser]);

            // Return to main menu
            mainMenu.GetComponent<MainMenu>().loadedUser = true;
            ReturnToMainMenu();
        }
        else
            StartCoroutine(mainMenu.GetComponent<MainMenu>().DisplayInformationOnLog(3.0f, "Select a valid user."));
    }

    public void ReturnToMainMenu()
    {
        // Return to main menu
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
