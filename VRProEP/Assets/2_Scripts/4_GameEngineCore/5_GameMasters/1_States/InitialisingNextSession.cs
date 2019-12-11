using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class InitialisingNextSession : State
    {
        public InitialisingNextSession(GameMaster gm) : base(gm)
        {
            stateName = STATE.INITIALISING_NEXT_SESSION;
        }

        protected override void Enter()
        {
            // Display status to user
            gm.InstructionManager.DisplayText(gm.GetDisplayInfoText());
            gm.HandleHUDColour();
            base.Enter();
        }

        protected override void Update()
        {
            // Wait for the flag to be released to be able to initialise the next session
            if (gm.WaitFlag)
            {
                gm.HandleSessionInitialisation();
                stateStage = EVENT.EXIT;
            }
        }

        protected override void Exit()
        {
            // Prompt user to go to start position
            gm.HudManager.DisplayText("Starting new session.", 3.0f);
            // Go to the next iteration
            nextState = new Instructions(gm);
            base.Exit();
        }
    }
}