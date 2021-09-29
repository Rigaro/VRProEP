using System.Collections;
using System.IO;
using UnityEngine;
using Valve.VR;
using VRProEP.ProsthesisCore;

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
        private static int motionTrackerNumber = 0;

        //private static readonly string resourcesDataPath = Application.dataPath + "/Resources/Avatars";

        /// <summary>
        /// Spawns a transradial avatar for the given user.
        /// </summary>
        /// <param name="userData">The user's physical data.</param>
        /// <param name="avatarData">The user's avatar configuration data.</param>
        public static void SpawnTranshumeralAvatar(UserData userData, AvatarData avatarData)
        {
            // Load the different parts of the avatar
            LoadResidualLimb(avatarData.residualLimbType, AvatarType.Transhumeral);
            LoadSocket(avatarData.socketType);
            LoadElbow(avatarData.elbowType, userData.upperArmLength);
            LoadForearm(avatarData.forearmType, userData.upperArmLength, userData.forearmLength);
            LoadHand(avatarData.handType, userData.upperArmLength, userData.forearmLength, userData.handLength, userData.lefty);

            
            // Deactivate rendering of the markers
            // First get the objects
            GameObject shoulderMarkerGO = GameObject.Find("ShoulderJointMarker");
            GameObject elbowMarkerGO = GameObject.Find("ElbowJointMarker");
            GameObject trackerModelGO = GameObject.Find("TrackerModel");
            GameObject handTrackerGO = GameObject.Find("handTracker");


            if (elbowMarkerGO == null || shoulderMarkerGO == null || trackerModelGO == null)
                throw new System.Exception("The joint markers were not found.");

            // Get their mesh renderer
            MeshRenderer shoulderMarkerMR = shoulderMarkerGO.GetComponent<MeshRenderer>();
            MeshRenderer elbowMarkerMR = elbowMarkerGO.GetComponent<MeshRenderer>();
            // Deactivate
            shoulderMarkerMR.enabled = false;
            elbowMarkerMR.enabled = false;
            trackerModelGO.SetActive(true);

            
        }

        /// <summary>
        /// Spawns a transradial avatar for the given user.
        /// </summary>
        /// <param name="userData">The user's physical data.</param>
        /// <param name="avatarData">The user's avatar configuration data.</param>
        public static void SpawnTransradialAvatar(UserData userData, AvatarData avatarData)
        {
            // Load
            LoadResidualLimb(avatarData.residualLimbType, AvatarType.Transradial);
            LoadSocket(avatarData.socketType);
            LoadForearm(avatarData.forearmType, userData.forearmLength);
            LoadHand(avatarData.handType, userData.forearmLength, userData.handLength);
            
            // Deactivate rendering of the markers
            GameObject elbowMarkerGO = GameObject.Find("ElbowJointMarker");
            if (elbowMarkerGO == null)
                throw new System.Exception("The joint markers were not found.");
            // Get their mesh renderer
            MeshRenderer elbowMarkerMR = elbowMarkerGO.GetComponent<MeshRenderer>();
            // Deactivate
            elbowMarkerMR.enabled = false;
        }

        /// <summary>
        /// Spawns an able bodied (hand) avatar.
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="avatarData"></param>
        public static void SpawnAbleBodiedAvatar(UserData userData, AvatarData avatarData, bool isNew = true)
        {
            LoadAbleHand(userData.lefty, userData.handLength);
            LoadAbleForearm(userData.forearmLength, isNew);
        }

        /// <summary>
        /// Loads an able-bodied hand avatar compatible with the VRProEP interaction system.
        /// </summary>
        /// <param name="lefty">True if left handed.</param>
        /// <returns>The instantiated hand GameObject.</returns>
        private static GameObject LoadAbleHand(bool lefty, float handLength)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");

            if (playerGO == null)
                throw new System.Exception("The player GameObject was not found.");

            string side = "R";
            float sign = 1.0f;
            if (lefty)
            {
                side = "L";
                sign = -1.0f;
            }

            // Load able bodied hand from avatar folder and check whether successfully loaded.
            GameObject handPrefab = Resources.Load<GameObject>("Avatars/Hands/ACESAble_" + side);
            if (handPrefab == null)
                throw new System.Exception("The requested hand prefab was not found.");

            // Load Avatar object to set as parent.
            GameObject avatarGO = GameObject.FindGameObjectWithTag("Avatar");
            // Instantiate and set reference frame;
            GameObject handGO = Object.Instantiate(handPrefab, handPrefab.transform.position, handPrefab.transform.rotation, avatarGO.transform);
            //handGO.GetComponent<SteamVR_TrackedObject>().origin = playerGO.transform; // Tracked object version
            handGO.GetComponent<SteamVR_Behaviour_Pose>().origin = playerGO.transform; // Behaviour pose version

            // Load hand object info
            string objectPath = "Avatars/Hands/ACESAble_" + side;
            string objectDataAsJson = Resources.Load<TextAsset>(objectPath).text;
            activeHandData = JsonUtility.FromJson<AvatarObjectData>(objectDataAsJson);
            if (activeHandData == null)
                throw new System.Exception("The requested hand information was not found.");
            
            // Scale hand to fit user's hand
            float scaleFactor = handLength / activeHandData.dimensions.x;
            handGO.transform.localScale = new Vector3(sign*scaleFactor, sign*scaleFactor, sign*scaleFactor);

            return handGO;
        }

        /// <summary>
        /// Loads and instantiates a residual limb avatar prefab from Resources/Avatars/ResidualLimbs.
        /// The prefab must include the tag "ResidualLimbAvatar". Loads by name.
        /// </summary>
        /// <param name="rlType">The name of the prefab residual limb avatar to be loaded.</param>
        /// <returns>The instantiated residual limb GameObject.</returns>
        private static GameObject LoadResidualLimb(string rlType, AvatarType avatarType)
        {
            // Load Avatar object to set as parent.
            GameObject avatarGO = GameObject.FindGameObjectWithTag("Avatar");
                
            // Load residual limb from avatar folder and check whether successfully loaded.
            GameObject residualLimbPrefab = Resources.Load<GameObject>("Avatars/ResidualLimbs/" + rlType);
            if (residualLimbPrefab == null)
                throw new System.Exception("The requested residual limb prefab was not found.");

            // Load the residual object info.
            //string objectPath = resourcesDataPath + "/ResidualLimbs/" + rlType + ".json";
            //string objectDataAsJson = File.ReadAllText(objectPath);
            string objectPath = "Avatars/ResidualLimbs/" + rlType;
            string objectDataAsJson = Resources.Load<TextAsset>(objectPath).text;
            activeResidualLimbData = JsonUtility.FromJson<AvatarObjectData>(objectDataAsJson);
            if (activeResidualLimbData == null)
                throw new System.Exception("The requested residual limb information was not found.");

            // Instantiate with tracker as parent.
            GameObject residualLimbGO = Object.Instantiate(residualLimbPrefab, new Vector3(0.0f, -activeResidualLimbData.dimensions.x / 1.0f, 0.0f), Quaternion.identity, avatarGO.transform);
            
            // Make sure the loaded residual limb has a the follower script and set the offset
            LimbFollower follower = residualLimbGO.GetComponent<LimbFollower>();
            // If it wasn't found, then add it.
            if (follower == null)
                follower = residualLimbGO.AddComponent<LimbFollower>();

            follower.avatarType = avatarType;
            follower.offset = new Vector3(0.0f, -activeResidualLimbData.dimensions.x / 1.0f, 0.0f);

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
            //string objectPath = resourcesDataPath + "/Sockets/" + socketType + ".json";
            //string objectDataAsJson = File.ReadAllText(objectPath);
            string objectPath = "Avatars/Sockets/" + socketType;
            string objectDataAsJson = Resources.Load<TextAsset>(objectPath).text;
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
            //string objectPath_Upper = resourcesDataPath + "/Elbows/" + elbowType + "_Upper.json";
            //string objectPath_Lower = resourcesDataPath + "/Elbows/" + elbowType + "_Lower.json";
            //string objectDataAsJson_Upper = File.ReadAllText(objectPath_Upper);
            //string objectDataAsJson_Lower = File.ReadAllText(objectPath_Lower);
            string objectPath_Upper = "Avatars/Elbows/" + elbowType + "_Upper";
            string objectPath_Lower = "Avatars/Elbows/" + elbowType + "_Lower";
            string objectDataAsJson_Upper = Resources.Load<TextAsset>(objectPath_Upper).text;
            string objectDataAsJson_Lower = Resources.Load<TextAsset>(objectPath_Lower).text;
            activeElbowData_Upper = JsonUtility.FromJson<AvatarObjectData>(objectDataAsJson_Upper);
            activeElbowData_Lower = JsonUtility.FromJson<AvatarObjectData>(objectDataAsJson_Lower);

            if (activeElbowData_Upper == null || activeElbowData_Lower == null)
                throw new System.Exception("The requested elbow information was not found.");

            // Instantiate with prosthesis manager as parent.
            //float elbowOffset = (upperArmLength - (activeElbowData_Upper.dimensions.x / 2.0f));
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
        /// Use this in Transhumeral avatars.
        /// </summary>
        /// <param name="forearmType">The name of the type of forearm to load.</param>
        /// <param name="upperArmLength">The user's upper-arm length.</param>
        /// <param name="lowerArmLength">The user's lower-arm length.</param>
        /// <returns>The loaded forearm.</returns>
        private static GameObject LoadForearm(string forearmType, float upperArmLength, float lowerArmLength)
        {

            // Load forearm from avatar folder and check whether successfully loaded.
            GameObject forearmPrefab = Resources.Load<GameObject>("Avatars/Forearms/" + forearmType);
            if (forearmPrefab == null)
                throw new System.Exception("The requested socket prefab was not found.");

            // Get parent prosthesis manager
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");
            
            // Load forearm object info
            //string objectPath = resourcesDataPath + "/Forearms/" + forearmType + ".json";
            //string objectDataAsJson = File.ReadAllText(objectPath);
            string objectPath = "Avatars/Forearms/" + forearmType;
            string objectDataAsJson = Resources.Load<TextAsset>(objectPath).text;
            activeForearmData = JsonUtility.FromJson<AvatarObjectData>(objectDataAsJson);
            if (activeForearmData == null)
                throw new System.Exception("The requested forearm information was not found.");

            // Need to attach to Elbow_Lower, so find that first and get its Rigidbody.
            GameObject elbowLowerGO = GameObject.FindGameObjectWithTag("Elbow_Lower");
            Rigidbody elbowLowerRB = elbowLowerGO.GetComponent<Rigidbody>();

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
        /// Loads and instantiates a forearm avatar prefab from Resources/Avatars/Forearms.
        /// The prefab must include the tag "Forearm". Loads by name.
        /// Use this in Transradial avatars.
        /// </summary>
        /// <param name="forearmType">The name of the type of forearm to load.</param>
        /// <param name="lowerArmLength">The user's lower-arm length.</param>
        /// <returns>The loaded forearm.</returns>
        private static GameObject LoadForearm(string forearmType, float lowerArmLength)
        {

            // Load forearm from avatar folder and check whether successfully loaded.
            GameObject forearmPrefab = Resources.Load<GameObject>("Avatars/Forearms/" + forearmType);
            if (forearmPrefab == null)
                throw new System.Exception("The requested socket prefab was not found.");

            // Get parent prosthesis manager
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");

            // Load forearm object info
            //string objectPath = resourcesDataPath + "/Forearms/" + forearmType + ".json";
            //string objectDataAsJson = File.ReadAllText(objectPath);
            string objectPath = "Avatars/Forearms/" + forearmType;
            string objectDataAsJson = Resources.Load<TextAsset>(objectPath).text;
            activeForearmData = JsonUtility.FromJson<AvatarObjectData>(objectDataAsJson);
            if (activeForearmData == null)
                throw new System.Exception("The requested forearm information was not found.");
            
            // Need to attach to Socket, so find that first and get its Rigidbody.
            GameObject socketGO = GameObject.FindGameObjectWithTag("Socket");
            Rigidbody socketRB = socketGO.GetComponent<Rigidbody>();

            // Instantiate with prosthesis manager as parent.
            float forearmOffset = lowerArmLength - (activeForearmData.dimensions.x / 2.0f);
            GameObject forearmGO = Object.Instantiate(forearmPrefab, new Vector3(forearmPrefab.transform.localPosition.x, -forearmOffset, forearmPrefab.transform.localPosition.z), forearmPrefab.transform.localRotation, prosthesisManagerGO.transform);

            // Attach the socket to the residual limb through a fixed joint.
            FixedJoint forearmFixedJoint = forearmGO.GetComponent<FixedJoint>();
            // If no fixed joint was found, then add it.
            if (forearmFixedJoint == null)
                forearmFixedJoint = forearmGO.AddComponent<FixedJoint>();
            // Connect
            forearmFixedJoint.connectedBody = socketRB;

            return forearmGO;
        }


        /// <summary>
        /// Loads and instantiates a forearm avatar prefab from Resources/Avatars/Forearms.
        /// The prefab must include the tag "Forearm". Loads by name.
        /// </summary>
        /// <param name="forearmType">The name of the prefab forearm avatar to be loaded.</param>
        /// <returns>The instantiated forearm GameObject.</returns>
        private static GameObject LoadAbleForearm(float lowerArmLength, bool newTracker = true)
        {
            // Add motion tracker and assign as forearm tracker, use as parent
            //GameObject llMotionTrackerGO = AvatarSystem.AddMotionTracker();
            GameObject llMotionTrackerGO = SpawnMotionTracker(newTracker);
            llMotionTrackerGO.tag = "ForearmTracker";
            llMotionTrackerGO.transform.GetChild(1).gameObject.SetActive(false); // Disable marker

            // Load forearm from avatar folder and check whether successfully loaded.
            GameObject forearmPrefab = Resources.Load<GameObject>("Avatars/Forearms/ForearmAble");
            if (forearmPrefab == null)
                throw new System.Exception("The requested socket prefab was not found.");

            // Get parent prosthesis manager
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");

            // Load forearm object info
            //string objectPath = resourcesDataPath + "/Forearms/ForearmAble.json";
            //string objectDataAsJson = File.ReadAllText(objectPath);
            string objectPath = "Avatars/Forearms/ForearmAble";
            string objectDataAsJson = Resources.Load<TextAsset>(objectPath).text;
            activeForearmData = JsonUtility.FromJson<AvatarObjectData>(objectDataAsJson);
            if (activeForearmData == null)
                throw new System.Exception("The requested forearm information was not found.");

            // Instantiate with tracker as parent.
            GameObject forearmGO = Object.Instantiate(forearmPrefab, Vector3.zero, forearmPrefab.transform.rotation, llMotionTrackerGO.transform);

            // Make sure the loaded forearm has a the follower script and correct setting
            LimbFollower follower = forearmGO.GetComponent<LimbFollower>();
            // If it wasn't found, then add it.
            if (follower == null)
                follower = forearmGO.AddComponent<LimbFollower>();

            follower.avatarType = AvatarType.AbleBodied;

            // Scale forearm to fit user's hand
            float scaleFactor = lowerArmLength / (2 * activeForearmData.dimensions.x);
            forearmGO.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            return forearmGO;
        }


        /// <summary>
        /// Loads and instantiates a hand avatar prefab from Resources/Avatars/Hands.
        /// The prefab must include the tag "Hand". Loads by name.
        /// Use this in Transhumeral avatars.
        /// </summary>
        /// <param name="handType">The name of the prefab forearm avatar to be loaded.</param>
        /// <param name="upperArmLength">The user's upper-arm length.</param>
        /// <param name="lowerArmLength">The user's lower-arm length.</param>
        /// <param name="handLength">The user's hand length.</param>
        /// <returns>The instantiated hand GameObject.</returns>
        private static GameObject LoadHand(string handType, float upperArmLength, float lowerArmLength, float handLength, bool lefty)
        {
            string side = "R";
            float sign = 1.0f;
            if (lefty)
            {
                side = "L";
                sign = -1.0f;
            }

            // Need to attach to Forearm, so find that first and get its Rigidbody.
            GameObject forearmGO = GameObject.FindGameObjectWithTag("Forearm");
            Rigidbody forearmRB = forearmGO.GetComponent<Rigidbody>();

            // Load hand from avatar folder and check whether successfully loaded.
            GameObject handPrefab = Resources.Load<GameObject>("Avatars/Hands/" + handType + "_" + side);
            if (handPrefab == null)
                throw new System.Exception("The requested hand prefab was not found.");

            // Get parent prosthesis manager
            GameObject prosthesisManagerGO = GameObject.FindGameObjectWithTag("ProsthesisManager");

            // Load hand object info
            //string objectPath = resourcesDataPath + "/Hands/" + handType + ".json";
            //string objectDataAsJson = File.ReadAllText(objectPath);
            string objectPath = "Avatars/Hands/" + handType + "_" + side;
            string objectDataAsJson = Resources.Load<TextAsset>(objectPath).text;
            activeHandData = JsonUtility.FromJson<AvatarObjectData>(objectDataAsJson);
            if (activeHandData == null)
                throw new System.Exception("The requested hand information was not found.");

            // Instantiate with prosthesis manager as parent.
            float handOffset = upperArmLength + lowerArmLength + (activeHandData.dimensions.x / 2.0f);
            GameObject handGO = Object.Instantiate(handPrefab, new Vector3(handPrefab.transform.localPosition.x, -handOffset, handPrefab.transform.localPosition.z), handPrefab.transform.localRotation, prosthesisManagerGO.transform);

            // Scale hand to fit user's hand
            float scaleFactor = handLength / activeHandData.dimensions.x;
            handGO.transform.localScale = new Vector3(sign*scaleFactor, sign*scaleFactor, sign*scaleFactor);

            // Attach the socket to the residual limb through a fixed joint.
            FixedJoint handFixedJoint = handGO.GetComponent<FixedJoint>();
            // If no fixed joint was found, then add it.
            if (handFixedJoint == null)
                handFixedJoint = forearmGO.AddComponent<FixedJoint>();
            // Connect
            handFixedJoint.connectedBody = forearmRB;

            return forearmGO;
        }

        /// <summary>
        /// Loads and instantiates a hand avatar prefab from Resources/Avatars/Hands.
        /// The prefab must include the tag "Hand". Loads by name.
        /// Use this in Transhumeral avatars.
        /// </summary>
        /// <param name="handType">The name of the prefab forearm avatar to be loaded.</param>
        /// <param name="lowerArmLength">The user's lower-arm length.</param>
        /// <param name="handLength">The user's hand length.</param>
        /// <returns>The instantiated hand GameObject.</returns>
        private static GameObject LoadHand(string handType, float lowerArmLength, float handLength)
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
            //string objectPath = resourcesDataPath + "/Hands/" + handType + ".json";
            //string objectDataAsJson = File.ReadAllText(objectPath);
            string objectPath = "Avatars/Hands/" + handType;
            string objectDataAsJson = Resources.Load<TextAsset>(objectPath).text;
            activeHandData = JsonUtility.FromJson<AvatarObjectData>(objectDataAsJson);
            if (activeHandData == null)
                throw new System.Exception("The requested hand information was not found.");

            // Instantiate with prosthesis manager as parent.
            float handOffset = lowerArmLength + (activeHandData.dimensions.x / 2.0f);
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

        public static GameObject SpawnMotionTracker(bool newTracker)
        {
            // Load prefab and check validity
            GameObject motionTrackerPrefab = Resources.Load<GameObject>("Trackers/VIVETracker");
            if (motionTrackerPrefab == null)
                throw new System.Exception("The requested motion tracker prefab was not found.");

            // Load Player object
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO == null)
                throw new System.Exception("The player GameObject was not found.");

            // Load Avatar object to set as parent.
            GameObject avatarGO = GameObject.FindGameObjectWithTag("Avatar");

            // Instantiate
            GameObject motionTrackerGO = Object.Instantiate(motionTrackerPrefab, avatarGO.transform);

            // Configure
            SteamVR_TrackedObject motionTrackerConfig = motionTrackerGO.GetComponent<SteamVR_TrackedObject>();
            if (AvatarSystem.AvatarType == AvatarType.AbleBodied)
                motionTrackerConfig.SetDeviceIndex(motionTrackerNumber + 4); // Set hardware device index to follow
            else
                motionTrackerConfig.SetDeviceIndex(motionTrackerNumber + 5); // Set hardware device index to follow
            motionTrackerConfig.origin = playerGO.transform; 
            if(newTracker)
                motionTrackerNumber++; 

            return motionTrackerGO;
        }
    }

}
