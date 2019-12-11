using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class WaitingForStart : State
    {
        // Waiting for start management variables
        private WaitState waitState = WaitState.Waiting;
        private enum WaitState
        {
            Waiting,
            Countdown
        }
        // Countdown management
        private bool counting = false;
        private bool countdownDone = false;
        private Coroutine countdownCoroutine;

        // Waiting
        //private bool waitFlag = false;

        private Coroutine waitCoroutine;

        public WaitingForStart(GameMaster gm) : base(gm)
        {
            stateName = STATE.WAITING_FOR_START;
        }
        //private bool requestedPause = false;

        /// <summary>
        /// Runs only once at the start of the state.
        /// </summary>
        protected override void Enter()
        {
            // Reset flow control
            waitState = WaitState.Waiting;
            // Reset flags
            gm.StartEnable = false;
            counting = false;
            //requestedPause = false;
            base.Enter();
        }

        /// <summary>
        /// Runs continuously while in this state
        /// </summary>
        protected override void Update()
        {
            // Display status to user
            gm.InstructionManager.DisplayText(gm.GetDisplayInfoText());
            
            switch (waitState)
            {
                // Waiting for subject to get to start position.
                case WaitState.Waiting:
                    // Check if pause requested
                    if (gm.UpdatePause())
                    {
                        // Jump to pause state
                        gm.HudManager.DisplayText("Pausing experiment...", 3.0f);
                        nextState = new Paused(gm);
                        stateStage = EVENT.EXIT;
                        break;
                    }
                    if (gm.IsReadyToStart())
                    {
                        gm.StartEnable = true;
                        gm.PrepareForStart();
                        waitState = WaitState.Countdown;
                    }
                    break;
                // HUD countdown for reaching action.
                case WaitState.Countdown:
                    // START COUNTING
                    // If all is good and haven't started counting, start.
                    if (gm.StartEnable && !counting && !gm.CountdownDone)
                    {
                        gm.HudManager.ClearText();
                        gm.StopHUDCountDown();
                        counting = true;
                        gm.HUDCountDown(3);
                        break;
                    }
                    // COUNTDOWN DONE
                    // If all is good and the countdownDone flag is raised, switch to reaching.
                    else if (gm.CountdownDone)
                    {
                        // Reset flags
                        gm.StartEnable = false;
                        counting = false;
                        waitState = WaitState.Waiting;
                        // Continue to perform task
                        nextState = new PerformingTask(gm);
                        stateStage = EVENT.EXIT;
                        break;
                    }
                    // FAILED TO START
                    // If hand goes out of target reset countdown and wait for position
                    else if (!gm.IsReadyToStart() && !gm.CountdownDone)
                    {
                        gm.HandleHUDColour(false); // Make HUD Red
                        gm.StopHUDCountDown();
                        gm.StartEnable = false;
                        gm.StartFailureReset();
                        counting = false;
                        // Indicate to move back
                        gm.HudManager.DisplayText("Move to start", 2.0f);
                        waitState = WaitState.Waiting;
                        break;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Runs as the state is exited.
        /// </summary>
        protected override void Exit()
        {
            base.Exit();
        }
    }
}