using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class AnalysingResults : State
    {
        //private enum NextStateRequest { End, Rest, Next, Continue, None };
        //private NextStateRequest nextRequest = NextStateRequest.None;

        public AnalysingResults(GameMaster gm) : base(gm)
        {
            //nextRequest = NextStateRequest.None;
            stateName = STATE.ANALYSING_RESULTS;
        }

        protected override void Enter()
        {
            // Display status to user
            gm.InstructionManager.DisplayText(gm.GetDisplayInfoText());
            gm.HandleHUDColour(false);
            // Encourage user
            gm.HudManager.DisplayText("Well done!", 2.0f);
            base.Enter();
        }

        protected override void Update()
        {
            gm.HandleResultAnalysis();
            if(gm.IsEndOfExperiment()) // Experiment end
            {
                gm.HudManager.DisplayText("Experiment end. Thank you!", 6.0f);
                //nextRequest = NextStateRequest.End;
                nextState = new End(gm);
                stateStage = EVENT.EXIT;
            }
            else if (gm.IsEndOfSession()) // End session and start a new one
            {
                gm.SetWaitFlag(3.0f);
                //nextRequest = NextStateRequest.Next;
                nextState = new InitialisingNextSession(gm);
                stateStage = EVENT.EXIT;
            }
            else if (gm.IsRestTime()) // Rest time
            {
                gm.HudManager.DisplayText("Take a " + gm.RestTime + " seconds rest.", 6.0f);
                gm.SetWaitFlag(gm.RestTime);
                //nextRequest = NextStateRequest.Rest;
                nextState = new Resting(gm);
                stateStage = EVENT.EXIT;
            }
            else // Continue with next iteration
            {
                gm.SetWaitFlag(3.0f);
                //nextRequest = NextStateRequest.Continue;
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