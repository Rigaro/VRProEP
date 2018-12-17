
using UnityEngine;
using TMPro;
using VRProEP.GameEngineCore;
using System;

public class CreateNewUserMenu : MonoBehaviour {

    public GameObject mainMenu;
    public GameObject userOptionsMenu;

    private UserData userData = new UserData();
    private int dataSet = 0;
    private bool userTypeSet = false;
       
    public void SetName(string name)
    {
        userData.name = name;
        dataSet += 1;
    }

    public void SetFamilyName(string familyName)
    {
        userData.familyName = familyName;
        dataSet += 1;
    }

    public void SetYOB(string yob)
    {
        int i_yob = int.Parse(yob);
        userData.yearOfBirth = i_yob;
        dataSet += 1;
    }

    public void SetUpperArmLength(string uaLength)
    {
        float f_uaLength = float.Parse(uaLength);
        userData.upperArmLength = f_uaLength;
        dataSet += 1;
    }

    public void SetUpperArmWidth(string uaWidth)
    {
        float f_uaWidth = float.Parse(uaWidth);
        userData.upperArmWidth = f_uaWidth;
        dataSet += 1;
    }

    public void SetForearmLength(string faLength)
    {
        float f_faLength = float.Parse(faLength);
        userData.forearmLength = f_faLength;
        dataSet += 1;
    }

    public void SetForearmWidth(string faWidth)
    {
        float f_faWidth = float.Parse(faWidth);
        userData.forearmWidth = f_faWidth;
        dataSet += 1;
    }

    public void SetHandLength(string handLegth)
    {
        float f_handLength = float.Parse(handLegth);
        userData.handLength = f_handLength;
        dataSet += 1;
    }

    public void SetUserType(Int32 userType)
    {
        if (userType != 4)
        {
            userData.type = (UserType)userType;
            dataSet += 1;
            userTypeSet = true;
        }
    }

    public void SetLefty(bool lefty = false)
    {
        userData.lefty = lefty;
        dataSet += 1;
    }

    public void CreateUser()
    {
        // Check that all data has been set.
        if (dataSet < 8)
            StartCoroutine(mainMenu.GetComponent<MainMenu>().DisplayInformationOnLog(3.0f, "Not all user information has been set."));
        else if (!userTypeSet)
            StartCoroutine(mainMenu.GetComponent<MainMenu>().DisplayInformationOnLog(3.0f, "Choose a valid user type."));
        else
        {
            // Generate user ID
            string userID = userData.name.ToCharArray()[0].ToString() + userData.familyName.ToCharArray()[0].ToString() + userData.yearOfBirth.ToString();
            userData.id = userID;
            // Create new user.
            try
            {
                SaveSystem.CreateNewUser(userData);

                // Create a default avatar.
                AvatarSystem.CreateAvatarCustomizationData(userID, "ResidualLimbUpperDefault", "UpperSocketDefault", "ElbowCustom", "ForearmCustom", "ACESHand");

                // Return to main menu
                mainMenu.GetComponent<MainMenu>().createdUser = true;
                ReturnToMainMenu();
            }
            catch (Exception e)
            {
                StartCoroutine(mainMenu.GetComponent<MainMenu>().DisplayInformationOnLog(3.0f, e.Message));                
            }
        }
    }

    public void ReturnToUserOptionsMenu()
    {
        userOptionsMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
