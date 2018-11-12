using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class AvatarSpawner
    {
        public void SpawnDefaultAvatar()
        {
            // Load the residual limb and attach it to tracker
            if (LoadResidualLimb("ResidualLimbDefaultAvatar"))
                Debug.Log("Loaded ResidualLimbDefaultAvatar.");
            else
                throw new System.Exception("Could not load default avatar. Possible issues: couldn't find object with Avatar tag. Couldn't find the requested residual limb avatar.");
            // Load the socket and attach it to the residual limb avatar
            LoadSocket("SocketDefault");
        }

        public void SpawnTranshumeralAvatar(UserData userData, AvatarData avatarData)
        {
            // Load
            // Customize
            throw new System.NotImplementedException();
        }

        public void SpawnTransradialAvatar(UserData userData, AvatarData avatarData)
        {
            // Load
            // Customize
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Loads and instantiates a residual limb avatar prefab from Resources/Avatars/ResidualLimbs.
        /// The prefab must include the tag "ResidualLimbAvatar". Loads by name.
        /// </summary>
        /// <param name="rlType">The name of the prefab residual limb avatar to be loaded.</param>
        /// <returns>True if successfully loaded.</returns>
        private GameObject LoadResidualLimb(string rlType)
        {
            // Load Avatar object to set as parent.
            GameObject avatar = GameObject.FindGameObjectWithTag("Avatar");
                
            // Load residual limb from avatar folder and instantiate with tracker as parent.
            GameObject residualLimbPrefab = Resources.Load<GameObject>("Avatars/ResidualLimbs/" + rlType);
            GameObject residualLimbGO = Object.Instantiate(residualLimbPrefab, new Vector3(0.0f, -residualLimbPrefab.transform.localScale.y, 0.0f), Quaternion.identity, avatar.transform);
                
            // Make sure the loaded residual limb has a the follower script
            ResidualLimbFollower follower = residualLimbGO.GetComponent<ResidualLimbFollower>();
            // If it wasn't included, then add it.
            if (follower == null)
                residualLimbGO.AddComponent<ResidualLimbFollower>();

            return residualLimbGO;
        }

        private GameObject LoadSocket(string socketType)
        {
            // Need to attach to ResidualLimbAvatar, so find that first and get its Rigidbody.
            GameObject residualLimbAvatar = GameObject.FindGameObjectWithTag("ResidualLimbAvatar");
            Rigidbody rlAvatarRigidbody = residualLimbAvatar.GetComponent<Rigidbody>();
            // Get parent prosthesis manager
            GameObject prosthesisManager = GameObject.FindGameObjectWithTag("ProsthesisManager");
            // Load socket from avatar folder and instantiate with tracker as parent.
            GameObject socketPrefab = Resources.Load<GameObject>("Avatars/Sockets/" + socketType);
            GameObject socketGO = Object.Instantiate(socketPrefab, new Vector3(0.0f, -(residualLimbAvatar.transform.localScale.y + socketPrefab.transform.localScale.y + 0.01f), 0.0f), Quaternion.identity, prosthesisManager.transform);
            // Attach the socket to thre residual limb through a fixed joint.
            FixedJoint socketFixedJoint = socketGO.GetComponent<FixedJoint>();
            socketFixedJoint.connectedBody = rlAvatarRigidbody;
            return socketGO;
        }
    }

}
