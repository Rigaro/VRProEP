using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class Training : State
    {
        public Training(GameMaster gm) : base(gm)
        {
            stateName = STATE.TRAINING;
        }

        /// <summary>
        /// Runs only once at the start of the state.
        /// </summary>
        protected override void Enter()
        {

            if (gm.SkipTraining)
                stateStage = EVENT.EXIT;
            else
            {
                gm.InTraining = false;
                base.Enter();
            }
        }

        /// <summary>
        /// Runs continuously while in this state
        /// </summary>
        protected override void Update()
        {
            // Make sure the coroutine is only started once.
            if (!gm.InTraining)
            {
                gm.InTraining = true;
                gm.StartTrainingLoop();
            }

            // Once the GM has flagged the end, we can move on.
            if (gm.TrainingDone)
                stateStage = EVENT.EXIT;
        }

        /// <summary>
        /// Runs as the state is exited.
        /// </summary>
        protected override void Exit()
        {
            gm.InTraining = false; // Reset flag
            gm.HudManager.DisplayText("Ready to start!", 3.0f); // Prompt user
            nextState = new WaitingForStart(gm); // Set next state
            base.Exit();
        }
    }
}