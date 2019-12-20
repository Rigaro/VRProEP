using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class Instructions : State
    {
        public Instructions(GameMaster gm) : base(gm)
        {
            stateName = STATE.INSTRUCTIONS;
        }

        /// <summary>
        /// Runs only once at the start of the state.
        /// </summary>
        protected override void Enter()
        {
            Debug.Log("Instructions State");
            if (gm.SkipInstructions)
                stateStage = EVENT.EXIT;
            else
            {
                gm.InInstructions = false;
                base.Enter();
            }
        }

        /// <summary>
        /// Runs continuously while in this state
        /// </summary>
        protected override void Update()
        {
            // Make sure the coroutine is only started once.
            if (!gm.InInstructions)
            {
                gm.InInstructions = true;
                gm.StartInstructionsLoop();
            }

            // Once the GM has flagged the end, we can move on.
            if (gm.InstructionsDone)
                stateStage = EVENT.EXIT;
        }

        /// <summary>
        /// Runs as the state is exited.
        /// </summary>
        protected override void Exit()
        {
            gm.InInstructions = false; // Reset flag
            gm.HudManager.DisplayText("Ready for training!", 3.0f); // Prompt user
            nextState = new Training(gm); // Set next state
            base.Exit();
        }
    }
}