using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class InitialisingNextIteration : State
    {
        public InitialisingNextIteration(GameMaster gm) : base(gm)
        {
            stateName = STATE.INITIALISING_NEXT_ITERATION;
        }

        protected override void Enter()
        {
            // Display status to user
            gm.InstructionManager.DisplayText(gm.GetDisplayInfoText());
            gm.HandleHUDColour(false);
            base.Enter();
        }

        protected override void Update()
        {
            // Wait for the flag to be released to be able to initialise the next iteration
            if(gm.WaitFlag)
            {
                gm.HandleIterationInitialisation();
                stateStage = EVENT.EXIT;
            }
        }

        protected override void Exit()
        {
            // Prompt user to go to start position
            gm.HudManager.DisplayText("Ready to start!", 2.0f);
            // Go to the next iteration
            nextState = new WaitingForStart(gm);
            base.Exit();
        }
    }
}