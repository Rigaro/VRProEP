using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class PerformingTask : State
    {
        public PerformingTask(GameMaster gm) : base(gm)
        {
            stateName = STATE.PERFORMING_TASK;
        }

        protected override void Enter()
        {
            // Display status to user
            gm.InstructionManager.DisplayText(gm.GetDisplayInfoText());
            gm.HandleHUDColour(true); // Make HUD Blue
            base.Enter();
        }

        protected override void Update()
        {
            gm.HandleTaskDataLogging();
            gm.HandleInTaskBehaviour();
            if(gm.IsTaskDone())
            {
                gm.HandleTaskCompletion();
                stateStage = EVENT.EXIT;
            }
        }

        protected override void Exit()
        {
            // Display status to user
            gm.InstructionManager.DisplayText(gm.GetDisplayInfoText());
            nextState = new AnalysingResults(gm); // Set next state
            base.Exit();
        }
    }
}