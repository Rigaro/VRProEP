//======= Copyright (c) Melbourne Robotics Lab, All rights reserved. ===============
using System.IO;
using System.Collections.Generic;
using UnityEngine;

using VRProEP.ProsthesisCore;


namespace VRProEP.GameEngineCore
{
    /// <summary>
    /// Handles avatar data management and spawning.
    /// </summary>
    public static class AvatarSystem
    {
        //private static readonly string dataFolder = Application.dataPath + "/UserData/";
        private static readonly string dataFolder = Application.persistentDataPath + "/UserData/";
        private static AvatarData activeAvatarData;
        private static List<ISensor> activeSensors = new List<ISensor>();

        private static AvatarType activeAvatarType;
        private static bool isPlayerAvailable = false;
        private static bool isAvatarAvaiable = false;

        public static bool IsAvatarAvaiable { get => isAvatarAvaiable; }
        public static bool IsPlayerAvailable { get => isPlayerAvailable; }
        public static AvatarType AvatarType { get => activeAvatarType; }



        /// <summary>
        /// Creates an avatar configuration file.
        /// Object types are referenced by prefab name.
        /// </summary>
        /// <param name="userID">The user ID to generate the file for.</param>
        /// <param name="residualLimbType">The type of residual limb.</param>
        /// <param name="socketType">The type of socket.</param>
        /// <param name="elbowType">The type of elbow.</param>
        /// <param name="forearmType">The type of forearm.</param>
        /// <param name="handType">The type of hand.</param>
        /// <param name="avatarType">The type of avatar.</param>
        /// <returns></returns>
        public static AvatarData CreateAvatarCustomizationData(string userID, string residualLimbType, string socketType, string elbowType, string forearmType, string handType)
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
        public static AvatarData LoadAvatarCustomizationData(string userID, AvatarType avatarType = AvatarType.Transhumeral)
        {
            AvatarData loadedAvatarData;
            string loadFilePath = dataFolder + userID + "/avatarInfo_" + avatarType.ToString() + ".json";

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
        public static void SaveAvatarCustomizationData(string userID, AvatarData avatarData)
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
        public static void LoadAvatar(UserData userData, AvatarType avatarType, bool isNew = true)
        {
            if (!IsPlayerAvailable)
                throw new System.Exception("Player object has not been loaded. First load the player object.");

            // Destroy avatar gameobject if already available.
            GameObject avatarGO = GameObject.FindGameObjectWithTag("Avatar");
            if (avatarGO != null)
            {
                GameObject.DestroyImmediate(avatarGO);
                isAvatarAvaiable = false;
            }

            // Get avatar data for user
            activeAvatarData = LoadAvatarCustomizationData(userData.id, avatarType);

            // Load avatar holder object prefab and check validity
            GameObject avatarPrefab = Resources.Load<GameObject>("Avatars/Avatar");
            if (avatarPrefab == null)
                throw new System.Exception("The avatar prefab was not found.");
            // Instantiate
            Object.Instantiate(avatarPrefab);

            // Select avatar type, customize tracking frame and spawn avatar.
            if (avatarType == AvatarType.Transhumeral)
            {
                activeAvatarType = AvatarType.Transhumeral;
                LoadTrackerFrame(userData.type, avatarType);
                CustomizeTrackingFrame(userData, avatarType);
                AvatarSpawner.SpawnTranshumeralAvatar(userData, activeAvatarData);
            }
            else if (avatarType == AvatarType.Transradial)
            {
                activeAvatarType = AvatarType.Transradial;
                LoadTrackerFrame(userData.type, avatarType);
                CustomizeTrackingFrame(userData, avatarType);
                AvatarSpawner.SpawnTransradialAvatar(userData, activeAvatarData);
            }
            else if (avatarType == AvatarType.AbleBodied)
            {
                activeAvatarType = AvatarType.AbleBodied;
                AvatarSpawner.SpawnAbleBodiedAvatar(userData, activeAvatarData, isNew);
            }

            isAvatarAvaiable = true;
        }

        /// <summary>
        /// Customizes the residual limb tracking frame to a given user.
        /// </summary>
        /// <param name="userData">The user's data to use for customization.</param>
        /// <param name="avatarType">The type of avatar to customize.</param>
        public static void CustomizeTrackingFrame(UserData userData, AvatarType avatarType)
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
                upperArmCollider.transform.localScale = new Vector3(upperArmCollider.transform.localScale.x, userData.upperArmLength / 2.0f, upperArmCollider.transform.localScale.z);
                upperArmCollider.transform.localPosition = new Vector3(upperArmCollider.transform.localPosition.x, -userData.upperArmLength / 2.0f, upperArmCollider.transform.localPosition.z);
            }
            else if (avatarType == AvatarType.Transradial)
            {
                // Load elbow location and check.
                GameObject elbowLocation = GameObject.FindGameObjectWithTag("ElbowLocation");
                if (elbowLocation == null)
                    throw new System.Exception("The residual limb frame has not been loaded.");

                // Modify the location of the elbowLocation
                elbowLocation.transform.position = new Vector3(elbowLocation.transform.position.x, -userData.forearmLength / 2.0f, elbowLocation.transform.position.z);
            }
        }

        /// <summary>
        /// Enables forearm and hand colliders
        /// </summary>
        public static void EnableAvatarColliders()
        {
            // Enable Forearm and hand colliders
            // Enable forearm collider
            GameObject forearmGO = GameObject.FindGameObjectWithTag("Forearm");
            if (forearmGO != null)
                forearmGO.GetComponent<Collider>().enabled = true;

            // Enable hand colliders
            GameObject handGO = GameObject.FindGameObjectWithTag("Hand");
            if (handGO != null)
            {
                Transform handColliders = handGO.transform.GetChild(0);
                handColliders.gameObject.SetActive(true);
            }

        }

        /// <summary>
        /// Loads a VR Player object and assigns a tracking frame to it.
        /// </summary>
        /// <param name="userType">The type of player to load: able-bodied, TR, TH.</param>
        /// <param name="avatarType">The type of avatar to load: able-bodied, TR, TH.</param>
        public static void LoadPlayer(UserType userType, AvatarType avatarType)
        {
            // Destroy player gameobject if already available.
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                Object.DestroyImmediate(playerGO);
                isPlayerAvailable = false;
            }

            switch (userType)
            {
                case UserType.AbleBodied:
                    switch (avatarType)
                    {
                        case AvatarType.AbleBodied:
                            // Load prefab and check validity
                            GameObject playerAblePrefab = Resources.Load<GameObject>("Players/PlayerAble");
                            if (playerAblePrefab == null)
                                throw new System.Exception("The requested player prefab was not found.");

                            // Instantiate
                            Object.Instantiate(playerAblePrefab);
                            isPlayerAvailable = true;
                            break;
                        case AvatarType.Transhumeral:
                            // Load prefab and check validity
                            GameObject playerAbleTHPrefab = Resources.Load<GameObject>("Players/PlayerAbleTH");
                            if (playerAbleTHPrefab == null)
                                throw new System.Exception("The requested player prefab was not found.");

                            // Instantiate
                            Object.Instantiate(playerAbleTHPrefab);
                            // Make sure the correct VIVE Tracker is selected for the upper arm
                            isPlayerAvailable = true;
                            break;
                        case AvatarType.Transradial:
                            // Load prefab and check validity
                            GameObject playerAbleTRPrefab = Resources.Load<GameObject>("Players/PlayerAbleTR");
                            if (playerAbleTRPrefab == null)
                                throw new System.Exception("The requested player prefab was not found.");

                            // Instantiate
                            Object.Instantiate(playerAbleTRPrefab);
                            // Make sure the correct VIVE Tracker is selected for the upper arm
                            isPlayerAvailable = true;
                            break;
                        default:
                            throw new System.Exception("The given user and avatar type combination is not available.");
                    }
                    break;
                case UserType.Transhumeral:
                    break;
                case UserType.Transradial:
                    break;
                default:
                    throw new System.Exception("The given user type is not available.");
            }
        }

        /// <summary>
        /// Loads the tracking frame for a given avatar type and attaches it to the residual limb tracker.
        /// </summary>
        /// <param name="avatarType"></param>
        private static void LoadTrackerFrame(UserType userType, AvatarType avatarType)
        {
            GameObject trackerGO = GameObject.FindGameObjectWithTag("ResidualLimbTracker");

            if (trackerGO == null)
                throw new System.Exception("The residual limb traker GameObject was not found.");

            //
            // Able-bodied subject with transhumeral frame
            //
            if (userType == UserType.AbleBodied && avatarType == AvatarType.Transhumeral)
            {
                // Load TH tracking frame from Frames folder and check whether successfully loaded.
                GameObject ableBodiedTHFramePrefab = Resources.Load<GameObject>("Frames/AbleBodiedFrameTH");
                if (ableBodiedTHFramePrefab == null)
                    throw new System.Exception("The requested tracker frame prefab was not found.");

                //GameObject residualLimbGO = Object.Instantiate(ableBodiedTHFramePrefab, trackerGO.transform, false);
                Object.Instantiate(ableBodiedTHFramePrefab, trackerGO.transform, false);
            }
            //
            // Able-bodied subject with transradial frame
            //
            else if (userType == UserType.AbleBodied && avatarType == AvatarType.Transradial)
            {
                // Load TR tracking frame from Frames folder and check whether successfully loaded.
                GameObject ableBodiedTRFramePrefab = Resources.Load<GameObject>("Frames/AbleBodiedFrameTR");
                if (ableBodiedTRFramePrefab == null)
                    throw new System.Exception("The requested tracker frame prefab was not found.");

                //GameObject residualLimbGO = Object.Instantiate(ableBodiedTHFramePrefab, trackerGO.transform, false);
                Object.Instantiate(ableBodiedTRFramePrefab, trackerGO.transform, false);
            }
        }
        
        /// <summary>
        /// Adds a sensors to the list of available sensors.
        /// </summary>
        /// <param name="sensor">The given sensors.</param>
        public static void AddActiveSensor(ISensor sensor)
        {
            if (sensor == null)
                throw new System.ArgumentNullException("The provided sensor is empty.");

            activeSensors.Add(sensor);
        }

        /// <summary>
        /// Returns a list with the available sensors.
        /// </summary>
        /// <returns>The list of sensors.</returns>
        public static List<ISensor> GetActiveSensors()
        {
            return activeSensors;
        }

        /// <summary>
        /// Adds a motion tracker to the avatar using VIVE Trackers.
        /// </summary>
        /// <returns></returns>
        public static GameObject AddMotionTracker()
        {
            if (!IsPlayerAvailable)
                throw new System.Exception("Player object has not been loaded. First load the player object.");

            return AvatarSpawner.SpawnMotionTracker(true);
        }
    }
}
