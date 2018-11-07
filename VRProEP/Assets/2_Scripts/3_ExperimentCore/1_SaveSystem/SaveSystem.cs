﻿//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VRProEP.ExperimentCore
{
    public class SaveSystem
    {
        private UserData activeUser;
        private string activeSaveFolder;
        private List<IExperimentLogger> activeLoggers;

        // Encapsulation
        public UserData ActiveUser
        {
            get
            {
                return activeUser;
            }

            set
            {
                activeUser = value;
            }
        }
        public string ActiveSaveFolder
        {
            get
            {
                return activeSaveFolder;
            }
        }


        /// <summary>
        /// Creates a new user data object with the given data, creates a folder for its data, saves the data in a file, and sets as the active user
        /// </summary>
        /// <param name="name">The user's name.</param>
        /// <param name="familyName">The user's family name.</param>
        /// <param name="yob">The user's year of birth.</param>
        /// <param name="uArmL">The user's upper arm length.</param>
        /// <param name="uArmW">The user's upper arm width.</param>
        /// <param name="fArmL">The user's forearm length.</param>
        /// <param name="fArmW">The user's forearm width.</param>
        /// <param name="handL">The user's hand length.</param>
        /// <param name="userType">The user type according to the enum UserType.</param>
        /// <returns>The created UserData.</returns>
        public UserData CreateNewUser(string name, string familyName, int yob, float uArmL, float uArmW, float fArmL, float fArmW, float handL, UserType userType)
        {
            // Generate user ID
            string userID = name.ToCharArray()[0].ToString() + familyName.ToCharArray()[0] + yob.ToString();
            // Create new UserData
            UserData newUser = new UserData();
            newUser.name = name;
            newUser.familyName = familyName;
            newUser.yearOfBirth = yob;
            newUser.id = userID;
            newUser.upperArmLength = uArmL;
            newUser.upperArmWidth = uArmW;
            newUser.foreArmLength = fArmL;
            newUser.foreArmWidth = fArmW;
            newUser.handLength = handL;
            newUser.type = userType;

            // Create new folder for user or throw exception if it already exists.
            try
            {
                CreateNewUserFolder(newUser);
            }
            catch
            {
                throw new System.Exception("The provided user data already exists in the user file directory.");
            }

            // If successfully created then we can proceed to make it the active user.
            ActiveUser = newUser;
            activeSaveFolder = Application.dataPath + "/UserData/" + ActiveUser.id.ToString();
            // And save its data
            SaveActiveUserData();

            return ActiveUser;
        }

        /// <summary>
        /// Creates a new folder for the given user data data, saves the data in a file, and sets as the active user.
        /// </summary>
        /// <param name="newUser">The user data to save.</param>
        /// <returns>The UserData.</returns>
        public UserData CreateNewUser(UserData newUser)
        {
            // Create new folder for user or throw exception if it already exists.
            try
            {
                CreateNewUserFolder(newUser);
            }
            catch
            {
                throw new System.Exception("The provided user data already exists in the user file directory.");
            }

            // If successfully created then we can proceed to make it the active user.
            ActiveUser = newUser;
            activeSaveFolder = Application.dataPath + "/UserData/" + ActiveUser.id.ToString();
            // And save its data
            SaveActiveUserData();

            return ActiveUser;
        }

        /// <summary>
        /// Creates a folder for the given user data.
        /// </summary>
        /// <param name="userData">The given user data.</param>
        private void CreateNewUserFolder(UserData userData)
        {
            string userFolder = "/UserData/" + userData.id.ToString();
            string userPath = Application.dataPath + userFolder;

            if (Directory.Exists(userPath))
                throw new System.Exception("The provided user already exists in the user file directory.");
            else
                Directory.CreateDirectory(userPath);
        }

        /// <summary>
        /// Saves the provided user data. If data exists, it is overwritten.
        /// </summary>
        /// <param name="userData">The user data to be saved.</param>
        public void SaveUserData(UserData userData)
        {
            string userFolder = "/UserData/" + userData.id.ToString();
            string userPath = Application.dataPath + userFolder;

            // If the directory doesn't exist throw an exception. A new user must be created.
            if (!Directory.Exists(userPath))
                throw new System.Exception("The provided user does not exist in the user file directory. Please use CreateNewUser function.");
            
            // Set file, format data as JSON, and save.
            string saveFilePath = userPath + "/" + "userInfo.json";
            string userDataAsJson = JsonUtility.ToJson(userData);
            File.WriteAllText(saveFilePath, userDataAsJson);
        }

        /// <summary>
        /// Saves the data of the current active user.
        /// </summary>
        public void SaveActiveUserData()
        {
            // Set file, format data as JSON, and save.
            string saveFilePath = activeSaveFolder + "/" + "userInfo.json";
            string userDataAsJson = JsonUtility.ToJson(ActiveUser);
            File.WriteAllText(saveFilePath, userDataAsJson);
        }

        /// <summary>
        /// Loads the user data for the given userID.
        /// </summary>
        /// <param name="userID">The ID of the user to load the data for.</param>
        /// <returns>The UserData for the requested user.</returns>
        public UserData LoadUserData(string userID)
        {
            UserData loadedUserData;
            // Get the folder for the given user ID
            string userPath = Application.dataPath + "/UserData/" + userID;
            string loadFilePath = userPath + "/userInfo.json";

            if (File.Exists(loadFilePath))
            {
                // Load data
                string userDataAsJson = File.ReadAllText(loadFilePath);
                loadedUserData = JsonUtility.FromJson<UserData>(userDataAsJson);
            }
            else
            {
                throw new System.Exception("The provided user does not exist in the user file directory.");
            }
            // Set as active user
            ActiveUser = loadedUserData;
            activeSaveFolder = userPath;

            // Re-initialize the experiment logger for the new active user.
            foreach (IExperimentLogger logger in activeLoggers)
                logger.InitializeLog(activeSaveFolder);

            return loadedUserData;
        }

        /// <summary>
        /// Initializes and adds an experiment logger to the save system.
        /// </summary>
        /// <param name="logger">The logger to be initialized and added.</param>
        public void InitializeExperimentLogger(IExperimentLogger logger)
        {
            if (logger == null)
                throw new System.ArgumentNullException("The provided logger is empty.");

            logger.InitializeLog(activeSaveFolder);
            activeLoggers.Add(logger);
        }
    }

}
