//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.IO;
using UnityEngine;


namespace VRProEP.GameEngineCore
{

    public class AvatarManager
    {
        private readonly string dataFolder = Application.dataPath + "/UserData/";
        private AvatarData activeAvatarData;
        private AvatarSpawner avatarSpawner = new AvatarSpawner();


        public AvatarData CreateAvatarCustomizationData(string userID, string residualLimbType, string socketType, string elbowType, string forearmType, string handType, AvatarType avatarType)
        {
            // Create avatar data object.
            AvatarData avatarData = new AvatarData();

            // Set the type of devices/avatar.
            avatarData.residualLimbType = residualLimbType;
            avatarData.socketType = socketType;
            avatarData.elbowType = elbowType;
            avatarData.forearmType = forearmType;
            avatarData.handType = handType;

            // Save customization data and set active.
            SaveAvatarCustomizationData(userID, avatarData);
            activeAvatarData = avatarData;

            return avatarData;
        }
        
        /// <summary>
        /// Loads the data used for avatar customization for the given user.
        /// </summary>
        /// <param name="userID">The user id of the user whose avatar wants to be loaded.</param>
        /// <returns>The loaded avatar customization data.</returns>
        public AvatarData LoadAvatarCustomizationData(string userID)
        {
            AvatarData loadedAvatarData;
            string loadFilePath = dataFolder + userID + "/userInfo.json";

            if (File.Exists(loadFilePath))
            {
                // Load data
                string userDataAsJson = File.ReadAllText(loadFilePath);
                loadedAvatarData = JsonUtility.FromJson<AvatarData>(userDataAsJson);
            }
            else
            {
                throw new System.Exception("The provided user does not exist in the user file directory.");
            }

            // Set as active user
            activeAvatarData = loadedAvatarData;
            
            return loadedAvatarData;
        }

        /// <summary>
        /// Saves the data used for avatar customization for the given user.
        /// </summary>
        /// <param name="userID">The user id of the user whose avatar wants to be loaded.</param>
        /// <param name="avatarData">The avatar customization data to save.</param>
        public void SaveAvatarCustomizationData(string userID, AvatarData avatarData)
        {
            string saveFolder = dataFolder + userID;

            // If the directory doesn't exist throw an exception. A new user must be created.
            if (!Directory.Exists(saveFolder))
                throw new System.Exception("The provided user does not exist in the user file directory. Please use use the save system to create a new user.");


            // Set file, format data as JSON, and save.
            string saveFilePath = saveFolder + "/avatarInfo.json";
            string avatarDataAsJson = JsonUtility.ToJson(avatarData);
            File.WriteAllText(saveFilePath, avatarDataAsJson);
        }


        public void LoadAvatar(UserData userData, AvatarType avatarType)
        {
            // Get avatar data for user
            activeAvatarData = LoadAvatarCustomizationData(userData.id);
            // Select method
            if (avatarType == AvatarType.Transhumeral)
                avatarSpawner.SpawnTranshumeralAvatar(userData, activeAvatarData);
            else if (avatarType == AvatarType.Transradial)
                avatarSpawner.SpawnTransradialAvatar(userData, activeAvatarData);
        }

        public void CustomizeTrackingFrame(UserData userData, AvatarType avatarType)
        {

        }
    }
}
