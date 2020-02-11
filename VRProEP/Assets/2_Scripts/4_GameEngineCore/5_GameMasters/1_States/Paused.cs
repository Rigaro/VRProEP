using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class Paused : State
    {
        //private bool requestedUnpause = false;
        //private bool requestedEnd = false;

        public Paused(GameMaster gm) : base(gm)
        {
            stateName = STATE.PAUSED;
        }

        protected override void Enter()
        {
            // Update HUD colour
            gm.HandleHUDColour();
            // Reset flags
            //requestedUnpause = false;
            //requestedEnd = false;
            base.Enter();
        }

        protected override void Update()
        {
            // Display status to user
            gm.InstructionManager.DisplayText(gm.GetDisplayInfoText());

            // Check if un-pause requested
            if (gm.UpdatePause())
            {
                //requestedUnpause = true;
                nextState = new WaitingForStart(gm);
                stateStage = EVENT.EXIT;
            }
            else if (gm.UpdateNext())
            {
                nextState = new InitialisingNextSession(gm);
                stateStage = EVENT.EXIT;
            }
            else if(gm.UpdateEnd())
            {
                //requestedEnd = true;
                nextState = new End(gm);
                stateStage = EVENT.EXIT;
            }
        }

        protected override void Exit()
        {
            //if (requestedUnpause)
            //{
            //    nextState = new WaitingForStart(gm);
            //}
            //else if (requestedEnd)
            //{
            //    nextState = new End(gm);
            //}
            base.Exit();
        }
    }
}