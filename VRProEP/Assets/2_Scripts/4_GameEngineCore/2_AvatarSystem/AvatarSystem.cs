//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.IO;
using UnityEngine;


namespace VRProEP.GameEngineCore
{
    /// <summary>
    /// 
    /// </summary>
    public class AvatarSystem
    {
        private readonly string dataFolder = Application.dataPath + "/UserData/";
        private AvatarData activeAvatarData;
        private AvatarSpawner avatarSpawner = new AvatarSpawner();
               
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="residualLimbType"></param>
        /// <param name="socketType"></param>
        /// <param name="elbowType"></param>
        /// <param name="forearmType"></param>
        /// <param name="handType"></param>
        /// <param name="avatarType"></param>
        /// <returns></returns>
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
            string loadFilePath = dataFolder + userID + "/avatarInfo.json";

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

        /// <summary>
        /// Loads an avatar for the given user of the given type.
        /// </summary>
        /// <param name="userData">The user's data to be used for avatar loading.</param>
        /// <param name="avatarType">The type of avatar to load.</param>
        public void LoadAvatar(UserData userData, AvatarType avatarType)
        {
            // Get avatar data for user
            activeAvatarData = LoadAvatarCustomizationData(userData.id);
            // Select avatar type, customize tracking frame and spawn avatar.
            if (avatarType == AvatarType.Transhumeral)
            {
                CustomizeTrackingFrame(userData, avatarType);
                avatarSpawner.SpawnTranshumeralAvatar(userData, activeAvatarData);
            }
            else if (avatarType == AvatarType.Transradial)
            {
                CustomizeTrackingFrame(userData, avatarType);
                avatarSpawner.SpawnTransradialAvatar(userData, activeAvatarData);
            }
        }

        /// <summary>
        /// Customizes the residual limb tracking frame to a given user.
        /// </summary>
        /// <param name="userData">The user's data to use for customization.</param>
        /// <param name="avatarType">The type of avatar to customize.</param>
        public void CustomizeTrackingFrame(UserData userData, AvatarType avatarType)
        {
            if (avatarType == AvatarType.Transhumeral)
            {
                // Load elbow location, shoulder location and upper arm collider objects.
                GameObject elbowLocation = GameObject.FindGameObjectWithTag("ElbowLocation");
                GameObject shoulderMarker = GameObject.FindGameObjectWithTag("ShoulderLocation");
                GameObject upperArmCollider = GameObject.FindGameObjectWithTag("UpperArmCollider");

                if (elbowLocation == null || shoulderMarker == null || upperArmCollider == null)
                    throw new System.Exception("The residual limb frame has not been loaded.");

                // Modify position of shoulder marker
                shoulderMarker.transform.position = new Vector3(shoulderMarker.transform.position.x, userData.upperArmLength + elbowLocation.transform.localPosition.y, shoulderMarker.transform.position.z);
                // Change size and position of collider
                upperArmCollider.transform.localScale = new Vector3(upperArmCollider.transform.localScale.x, userData.upperArmLength/2.0f, upperArmCollider.transform.localScale.z);
                upperArmCollider.transform.localPosition = new Vector3(upperArmCollider.transform.localPosition.x, -userData.upperArmLength / 2.0f, upperArmCollider.transform.localPosition.z);
            }
            else if (avatarType == AvatarType.Transradial)
            {
                throw new System.NotImplementedException("Transradial avatars not yet implemented.");
            }
        }
    }
}
