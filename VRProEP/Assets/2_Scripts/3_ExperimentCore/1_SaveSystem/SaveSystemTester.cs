﻿//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using VRProEP.ExperimentCore;
using UnityEngine;

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
    

    // Use this for initialization
    void Start () {
        saveSystem = new SaveSystem();
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
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void OnApplicationQuit()
    {
        saveSystem.GetActiveLogger(0).CloseLog();
    }
}