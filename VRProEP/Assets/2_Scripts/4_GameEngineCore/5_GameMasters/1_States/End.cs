using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class End : State
    {
        public End(GameMaster gm) : base(gm)
        {
            stateName = STATE.END;
        }

        protected override void Enter()
        {
            gm.HandleHUDColour(); // Make HUD Green
            // Handle experiment end
            gm.EndExperiment();
            base.Enter();
        }

        protected override void Update()
        {
            // Display status to user
            gm.InstructionManager.DisplayText(gm.GetDisplayInfoText());
            // Handle closing the application (return to main menu or quit).
            gm.UpdateCloseApplication();
        }

        protected override void Exit()
        {
            base.Exit();
        }
    }
}