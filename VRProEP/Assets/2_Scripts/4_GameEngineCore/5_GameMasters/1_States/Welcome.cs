using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class Welcome : State
    {
        public Welcome(GameMaster gm) : base(gm)
        {
            stateName = STATE.WELCOME;
        }

        /// <summary>
        /// Runs only once at the start of the state.
        /// </summary>
        protected override void Enter()
        {
            gm.InWelcome = false; // Make sure we reset the inWelcome variable to start coroutine
            base.Enter();
        }

        /// <summary>
        /// Runs continuously while in this state
        /// </summary>
        protected override void Update()
        {
            // Make sure the coroutine is only started once.
            if (!gm.InWelcome)
            {
                gm.InWelcome = true;
                StartCoroutine(gm.WelcomeLoop());
            }

            // Once the user has flagged the end, we can move on.
            if (gm.WelcomeDone)
                stateStage = EVENT.EXIT;
        }

        /// <summary>
        /// Runs as the state is exited.
        /// </summary>
        protected override void Exit()
        {            
            gm.InWelcome = false; // Reset the inWelcome flag
            gm.HudManager.DisplayText("Let's get started!", 3.0f); // Prompt the user
            nextState = new InitialisingExperiment(gm); // Go to experiment initialisation
            base.Exit();
        }
    }
}