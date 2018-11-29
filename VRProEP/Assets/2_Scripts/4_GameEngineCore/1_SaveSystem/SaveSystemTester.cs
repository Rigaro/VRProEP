//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using VRProEP.ExperimentCore;
using VRProEP.GameEngineCore;
using VRProEP.ProsthesisCore;
using UnityEngine;
using Valve.VR;
using UnityEngine.XR;
using System.IO;

public class SaveSystemTester : MonoBehaviour {

    public string userName = "Ricardo";
    public string familyName = "Garcia";
    public int yob = 1988;
    public float upperArmLength = 0.31f;
    public float upperArmWidth = 0.10f;
    public float foreArmLength = 0.29f;
    public float foreArmWidth = 0.08f;
    public float handLength = 0.18f;
    public UserType type = UserType.AbleBodied;

    private UserData activeUserData;
    private SaveSystem saveSystem;

    private AvatarSystem avatarSystem;

    private bool collidersEnabled = false;

    // Use this for initialization
    void Start ()
    {
        // Start XR Tracking
        //XRSettings.enabled = true;

        saveSystem = new SaveSystem();
        avatarSystem = new AvatarSystem();
        //saveSystem.CreateNewUser(userName, familyName, yob, upperArmLength, upperArmWidth, foreArmLength, foreArmWidth, handLength, type);
        //Debug.Log("The loaded user is: " + saveSystem.ActiveUser.name + " " + saveSystem.ActiveUser.familyName);
        activeUserData = saveSystem.LoadUserData("RG1988");
        Debug.Log("The loaded user is: " + saveSystem.ActiveUser.name + " " + saveSystem.ActiveUser.familyName);
        Debug.Log("The user's hand length is " + saveSystem.ActiveUser.handLength);
        /*
        UserData newUser = new UserData();
        string userID = userName.ToCharArray()[0].ToString() + familyName.ToCharArray()[0] + yob.ToString();
        newUser.id = userID;
        newUser.name = userName;
        newUser.familyName = familyName;
        newUser.yearOfBirth = yob;
        newUser.upperArmLength = upperArmLength;
        newUser.upperArmWidth = upperArmWidth;
        newUser.foreArmLength = foreArmLength;
        newUser.foreArmWidth = foreArmWidth;
        newUser.handLength = handLength;
        newUser.type = type;
        saveSystem.CreateNewUser(newUser);
        */

        // Create a data stream logger
        DataStreamLogger logger = new DataStreamLogger("FSF", "Motion");
        Debug.Log(logger.IsConfigured);
        Debug.Log(logger.GetActiveExperiment() + " " + logger.GetActiveLogType());

        saveSystem.AddExperimentLogger(logger);

        Debug.Log(logger.IsConfigured);

        IExperimentLogger tempLogger = saveSystem.GetActiveLogger(0);

        logger.ConfigureLogger("Test", "Score");
        Debug.Log(tempLogger.GetActiveExperiment() + " " + tempLogger.GetActiveLogType());

        Debug.Log(logger.IsInitialized.ToString() + logger.IsConfigured.ToString());
        logger.AddNewLogFile(1, 1, "test, format, does, it, work");
        logger.AppendData("it, is, fucking, working, yeah");
        logger.SaveLog();
        logger.AddNewLogFile(1, 2, "did, it, overwrite, the, data");
        saveSystem.GetActiveLogger(0).CloseLog();

        // Create avatar configuration for user
        //avatarSystem.CreateAvatarCustomizationData(saveSystem.ActiveUser.id, "ResidualLimbUpperDefault", "SocketDefault", "ElbowDefault", "ForearmDefault", "HandDefault", AvatarType.Transhumeral);
        avatarSystem.LoadAvatar(saveSystem.ActiveUser, AvatarType.Transhumeral);

        // Initialize prosthesis
        GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
        ConfigurableElbowManager elbowManager = prosthesisManagerGO.AddComponent<ConfigurableElbowManager>();
        elbowManager.InitializeProsthesis(activeUserData.upperArmLength, (activeUserData.forearmLength + activeUserData.handLength/2.0f));

        //elbowManager.ChangeSensor("VAL_SENSOR_VIVECONTROLLER");
        //elbowManager.ChangeReferenceGenerator("VAL_REFGEN_LINKINSYN");
        elbowManager.ChangeReferenceGenerator("VAL_REFGEN_JACOBIANSYN");

    }

    // Update is called once per frame
    void Update () {
        if (SteamVR_Input.vrproep.inActions.InterfaceEnableButton.GetStateDown(SteamVR_Input_Sources.Any) && !collidersEnabled)
        {
            avatarSystem.EnableAvatarColliders();
            collidersEnabled = true;
        }
	}

    private void OnApplicationQuit()
    {
        saveSystem.GetActiveLogger(0).CloseLog();
    }
}
