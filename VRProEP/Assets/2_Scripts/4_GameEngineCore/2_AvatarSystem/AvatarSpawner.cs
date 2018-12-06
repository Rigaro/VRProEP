using System.Collections;
using System.IO;
using UnityEngine;
using Valve.VR;

namespace VRProEP.GameEngineCore
{
    /// <summary>
    /// Handles avatar loading and spawning into game scene.
    /// Avatar objects should be placed within: /Resources/Avatars/
    /// </summary>
    public static class AvatarSpawner
    {
        private static AvatarObjectData activeResidualLimbData;
        private static AvatarObjectData activeSocketData;
        private static AvatarObjectData activeElbowData_Upper;
        private static AvatarObjectData activeElbowData_Lower;
        private static AvatarObjectData activeForearmData;
        private static AvatarObjectData activeHandData;

        //private const float objectGap = 0.0f; // Helps with the gap between certain objects to avoid overlapping issues.

        private static readonly string resourcesDataPath = Application.dataPath + "/Resources/Avatars";

        /// <summary>
        /// Spawns a transradial avatar for the given user.
        /// </summary>
        /// <param name="userData">The user's physical data.</param>
        /// <param name="avatarData">The user's avatar configuration data.</param>
        public static void SpawnTranshumeralAvatar(UserData userData, AvatarData avatarData)
        {
            // Load the different parts of the avatar
            LoadResidualLimb(avatarData.residualLimbType);
            LoadSocket(avatarData.socketType);
            LoadElbow(avatarData.elbowType, userData.upperArmLength);
            LoadForearm(avatarData.forearmType, userData.upperArmLength, userData.forearmLength);
            LoadHand(avatarData.handType, userData.upperArmLength, userData.forearmLength, userData.handLength);

            
            // Deactivate rendering of the markers
            // First get the objects
            GameObject shoulderMarkerGO = GameObject.Find("ShoulderJointMarker");
            GameObject elbowMarkerGO = GameObject.Find("ElbowJointMarker");
            GameObject trackerModelGO = GameObject.Find("TrackerModel");
            if (elbowMarkerGO == null || shoulderMarkerGO == null || trackerModelGO == null)
                throw new System.Exception("The joint markers were not found.");

            // Get their mesh renderer
            MeshRenderer shoulderMarkerMR = shoulderMarkerGO.GetComponent<MeshRenderer>();
            MeshRenderer elbowMarkerMR = elbowMarkerGO.GetComponent<MeshRenderer>();
            // Deactivate
            shoulderMarkerMR.enabled = false;
            elbowMarkerMR.enabled = false;
            trackerModelGO.SetActive(false);
            
        }

        /// <summary>
        /// Spawns a transradial avatar for the given user.
        /// </summary>
        /// <param name="userData">The user's physical data.</param>
        /// <param name="avatarData">The user's avatar configuration data.</param>
        public static void SpawnTransradialAvatar(UserData userData, AvatarData avatarData)
        {
            // Load
            // Customize
            throw new System.NotImplementedException("Transradial avatars not yet implemented.");
        }

        /// <summary>
        /// Spawns an able bodied (hand) avatar.
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="avatarData"></param>
        public static void SpawnAbleBodiedAvatar(UserData userData, AvatarData avatarData)
        {
            LoadAbleHand(userData.lefty);
        }

        /// <summary>
        /// Loads an able-bodied hand avatar compatible with the VRProEP interaction system.
        /// </summary>
        /// <param name="lefty">True if left handed.</param>
        /// <returns>The instantiated hand GameObject.</returns>
        private static GameObject LoadAbleHand(bool lefty)
        {
            GameObject playerGO = GameObject.Find("Player");

            if (playerGO == null)
                throw new System.Exception("The player GameObject was not found.");

            string side = "R";
            if (lefty)
                side = "L";

            // Load able bodied hand from avatar folder and check whether successfully loaded.
            GameObject handPrefab = Resources.Load<GameObject>("Avatars/Hands/ACESAble" + side);
            if (handPrefab == null)
                throw new System.Exception("The requested hand prefab was not found.");

            // Load Avatar object to set as parent.
            GameObject avatarGO = GameObject.FindGameObjectWithTag("Avatar");
            // Instantiate and set reference frame;
            GameObject handGO = Object.Instantiate(handPrefab, handPrefab.transform.position, handPrefab.transform.rotation, avatarGO.transform);
            handGO.GetComponent<SteamVR_TrackedObject>().origin = playerGO.transform;

            return handGO;
        }

        /// <summary>
        /// Loads and instantiates a residual limb avatar prefab from Resources/Avatars/ResidualLimbs.
        /// The prefab must include the tag "ResidualLimbAvatar". Loads by name.
        /// </summary>
        /// <param name="rlType">The name of the prefab residual limb avatar to be loaded.</param>
        /// <returns>The instantiated residual limb GameObject.</returns>
        private static GameObject LoadResidualLimb(string rlType)
        {
            // Load Avatar object to set as parent.
            GameObject avatarGO = GameObject.FindGameObjectWithTag("Avatar");
                
            // Load residual limb from avatar folder and check whether successfully loaded.
            GameObject residualLimbPrefab = Resources.Load<GameObject>("Avatars/ResidualLimbs/" + rlType);
            if (residualLimbPrefab == null)
                throw new System.Exception("The requested residual limb prefab was not found.");

            // Load the residual object info.
            string objectPath = resourcesDataPath + "/ResidualLimbs/" + rlType + ".json";
            string objectDataAsJson = File.ReadAllText(objectPath);
            activeResidualLimbData = JsonUtility.FromJson<AvatarObjectData>(objectDataAsJson);
            if (activeResidualLimbData == null)
                throw new System.Exception("The requested residual limb information was not found.");

            // Instantiate with tracker as parent.
            GameObject residualLimbGO = Object.Instantiate(residualLimbPrefab, new Vector3(0.0f, -activeResidualLimbData.dimensions.x / 2.0f, 0.0f), Quaternion.identity, avatarGO.transform);
            
            // Make sure the loaded residual limb has a the follower script and set the offset
            ResidualLimbFollower follower = residualLimbGO.GetComponent<ResidualLimbFollower>();
            // If it wasn't found, then add it.
            if (follower == null)
                residualLimbGO.AddComponent<ResidualLimbFollower>();

            follower.offset = new Vector3(0.0f, -activeResidualLimbData.dimensions.x / 2.0f, 0.0f);

            return residualLimbGO;
        }

        /// <summary>
        /// Loads and instantiates a socket avatar prefab from Resources/Avatars/Sockets.
        /// The prefab must include the tag "Socket". Loads by name.
        /// </summary>
        /// <param name="socketType">The name of the prefab socket avatar to be loaded.</param>
        /// <returns>The instantiated socket GameObject.</returns>
        private static GameObject LoadSocket(string socketType)
        {
            // Need to attach to ResidualLimbAvatar, so find that first and get its Rigidbody.
            GameObject residualLimbGO = GameObject.FindGameObjectWithTag("ResidualLimbAvatar");
            Rigidbody residualLimbRB = residualLimbGO.GetComponent<Rigidbody>();

            // Load socket from avatar folder and check whether successfully loaded.
            GameObject socketPrefab = Resources.Load<GameObject>("Avatars/Sockets/" + socketType);
            if (socketPrefab == null)
                throw new System.Exception("The requested socket prefab was not found.");

            // Get parent prosthesis manager
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");

            // Load socket object info
            string objectPath = resourcesDataPath + "/Sockets/" + socketType + ".json";
            string objectDataAsJson = File.ReadAllText(objectPath);
            activeSocketData = JsonUtility.FromJson<AvatarObjectData>(objectDataAsJson);
            if (activeSocketData == null)
                throw new System.Exception("The requested socket information was not found.");

            // Instantiate with prosthesis manager as parent.
            GameObject socketGO = Object.Instantiate(socketPrefab, new Vector3(socketPrefab.transform.localPosition.x, -(activeResidualLimbData.dimensions.x + (activeSocketData.dimensions.x/2.0f)), socketPrefab.transform.localPosition.z), socketPrefab.transform.localRotation, prosthesisManagerGO.transform);
            
            // Attach the socket to the residual limb through a fixed joint.
            FixedJoint socketFixedJoint = socketGO.GetComponent<FixedJoint>();
            // If no fixed joint was found, then add it.
            if (socketFixedJoint == null)
                socketFixedJoint = socketGO.AddComponent<FixedJoint>();
            // Connect
            socketFixedJoint.connectedBody = residualLimbRB;
            return socketGO;
        }

        /// <summary>
        /// Loads and instantiates an elbow avatar prefab from Resources/Avatars/Elbows. Loads by name.
        /// The prefab is composed of 3 parts:
        /// The parent elbow empty GameObject which includes the tag "Elbow". 
        /// The upper arm part of the elbow device, which includes the tag "Elbow_Upper".
        /// The lower arm part of the elbow device, which includes the tag "Elbow_Lower".
        /// </summary>
        /// <param name="elbowType">The name of the prefab socket avatar to be loaded.</param>
        /// <returns>The instantiated socket GameObject.</returns>
        private static GameObject LoadElbow(string elbowType, float upperArmLength)
        {
            // Need to attach to Socket, so find that first and get its Rigidbody.
            GameObject socketGO = GameObject.FindGameObjectWithTag("Socket");
            Rigidbody socketRB = socketGO.GetComponent<Rigidbody>();

            // Load elbow components from avatar folder and check whether successfully loaded.
            GameObject elbowPrefab = Resources.Load<GameObject>("Avatars/Elbows/" + elbowType);
            if (elbowPrefab == null)
                throw new System.Exception("The requested elbow prefab was not found.");

            // Get parent prosthesis manager
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");

            // Load elbow objects info
            string objectPath_Upper = resourcesDataPath + "/Elbows/" + elbowType + "_Upper.json";
            string objectPath_Lower = resourcesDataPath + "/Elbows/" + elbowType + "_Lower.json";
            string objectDataAsJson_Upper = File.ReadAllText(objectPath_Upper);
            string objectDataAsJson_Lower = File.ReadAllText(objectPath_Lower);
            activeElbowData_Upper = JsonUtility.FromJson<AvatarObjectData>(objectDataAsJson_Upper);
            activeElbowData_Lower = JsonUtility.FromJson<AvatarObjectData>(objectDataAsJson_Upper);

            if (activeElbowData_Upper == null || activeElbowData_Lower == null)
                throw new System.Exception("The requested elbow information was not found.");

            // Instantiate with prosthesis manager as parent.
            float elbowOffset = (upperArmLength - (activeElbowData_Upper.dimensions.x / 2.0f));
            GameObject elbowGO = Object.Instantiate(elbowPrefab, new Vector3(elbowPrefab.transform.localPosition.x, -upperArmLength, elbowPrefab.transform.localPosition.z), elbowPrefab.transform.localRotation, prosthesisManagerGO.transform);
                       
            // Attach the socket to thre residual limb through a fixed joint.
            // Get the elbow upper part that needs to be attached to the socket
            FixedJoint elbowFixedJoint = elbowGO.GetComponentInChildren<FixedJoint>();
            // If no fixed joint was found, then add it.
            if (elbowFixedJoint == null)
            {
                GameObject elbow_Upper = GameObject.FindGameObjectWithTag("Elbow_Upper");
                elbowFixedJoint = elbow_Upper.AddComponent<FixedJoint>();

            }
            // Connect
            elbowFixedJoint.connectedBody = socketRB;
            return elbowGO;
        }


        /// <summary>
        /// Loads and instantiates a forearm avatar prefab from Resources/Avatars/Forearms.
        /// The prefab must include the tag "Forearm". Loads by name.
        /// </summary>
        /// <param name="forearmType">The name of the prefab forearm avatar to be loaded.</param>
        /// <returns>The instantiated forearm GameObject.</returns>
        private static GameObject LoadForearm(string forearmType, float upperArmLength, float lowerArmLength)
        {
            // Need to attach to Elbow_Lower, so find that first and get its Rigidbody.
            GameObject elbowLowerGO = GameObject.FindGameObjectWithTag("Elbow_Lower");
            Rigidbody elbowLowerRB = elbowLowerGO.GetComponent<Rigidbody>();

            // Load forearm from avatar folder and check whether successfully loaded.
            GameObject forearmPrefab = Resources.Load<GameObject>("Avatars/Forearms/" + forearmType);
            if (forearmPrefab == null)
                throw new System.Exception("The requested socket prefab was not found.");

            // Get parent prosthesis manager
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");

            // Load forearm object info
            string objectPath = resourcesDataPath + "/Forearms/" + forearmType + ".json";
            string objectDataAsJson = File.ReadAllText(objectPath);
            activeForearmData = JsonUtility.FromJson<AvatarObjectData>(objectDataAsJson);
            if (activeForearmData == null)
                throw new System.Exception("The requested forearm information was not found.");

            // Instantiate with prosthesis manager as parent.
            float forearmOffset = upperArmLength + lowerArmLength - (activeForearmData.dimensions.x / 2.0f);
            GameObject forearmGO = Object.Instantiate(forearmPrefab, new Vector3(forearmPrefab.transform.localPosition.x, -forearmOffset, forearmPrefab.transform.localPosition.z), forearmPrefab.transform.localRotation, prosthesisManagerGO.transform);
            
            // Attach the socket to the residual limb through a fixed joint.
            FixedJoint forearmFixedJoint = forearmGO.GetComponent<FixedJoint>();
            // If no fixed joint was found, then add it.
            if (forearmFixedJoint == null)
                forearmFixedJoint = forearmGO.AddComponent<FixedJoint>();
            // Connect
            forearmFixedJoint.connectedBody = elbowLowerRB;
            
            return forearmGO;
        }


        /// <summary>
        /// Loads and instantiates a hand avatar prefab from Resources/Avatars/Hands.
        /// The prefab must include the tag "Forearm". Loads by name.
        /// </summary>
        /// <param name="handType">The name of the prefab forearm avatar to be loaded.</param>
        /// <returns>The instantiated forearm GameObject.</returns>
        private static GameObject LoadHand(string handType, float upperArmLength, float lowerArmLength, float handLength)
        {
            // Need to attach to Forearm, so find that first and get its Rigidbody.
            GameObject forearmGO = GameObject.FindGameObjectWithTag("Forearm");
            Rigidbody forearmRB = forearmGO.GetComponent<Rigidbody>();

            // Load hand from avatar folder and check whether successfully loaded.
            GameObject handPrefab = Resources.Load<GameObject>("Avatars/Hands/" + handType);
            if (handPrefab == null)
                throw new System.Exception("The requested hand prefab was not found.");

            // Get parent prosthesis manager
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");

            // Load hand object info
            string objectPath = resourcesDataPath + "/Hands/" + handType + ".json";
            string objectDataAsJson = File.ReadAllText(objectPath);
            activeHandData = JsonUtility.FromJson<AvatarObjectData>(objectDataAsJson);
            if (activeHandData == null)
                throw new System.Exception("The requested hand information was not found.");

            // Instantiate with prosthesis manager as parent.
            float handOffset = upperArmLength + lowerArmLength + (activeHandData.dimensions.x / 2.0f);
            GameObject handGO = Object.Instantiate(handPrefab, new Vector3(handPrefab.transform.localPosition.x, -handOffset, handPrefab.transform.localPosition.z), handPrefab.transform.localRotation, prosthesisManagerGO.transform);

            // Scale hand to fit user's hand
            float scaleFactor = handLength / activeHandData.dimensions.x;
            handGO.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            // Attach the socket to the residual limb through a fixed joint.
            FixedJoint handFixedJoint = handGO.GetComponent<FixedJoint>();
            // If no fixed joint was found, then add it.
            if (handFixedJoint == null)
                handFixedJoint = forearmGO.AddComponent<FixedJoint>();
            // Connect
            handFixedJoint.connectedBody = forearmRB;

            return forearmGO;
        }
    }

}
