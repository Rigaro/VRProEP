using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class Resting : State
    {
        public Resting(GameMaster gm) : base(gm)
        {
            stateName = STATE.RESTING;
        }

        protected override void Enter()
        {
            gm.HandleHUDColour(); // Make HUD Green
            base.Enter();
        }

        protected override void Update()
        {
            // Continuously display status to user
            gm.InstructionManager.DisplayText(gm.GetDisplayInfoText());

            // Check if experiment wants to skip to the next session or end the experiment.
            if (gm.UpdateNext())
            {
                nextState = new InitialisingNextSession(gm);
                stateStage = EVENT.EXIT;
            }
            else if (gm.UpdateEnd())
            {
                nextState = new End(gm);
                stateStage = EVENT.EXIT;
            }
            // Wait for the flag to be released to be able to initialise the next iteration
            else if (gm.WaitFlag)
            {
                gm.SetWaitFlag(5.0f);
                gm.HudManager.DisplayText("5 more seconds...", 3.0f);
                nextState = new InitialisingNextIteration(gm);
                stateStage = EVENT.EXIT;
            }
        }

        protected override void Exit()
        {
            base.Exit();
        }
    }
}