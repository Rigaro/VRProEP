using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class InitialisingExperiment : State
    {
        public InitialisingExperiment(GameMaster gm) : base(gm)
        {
            stateName = STATE.INITIALISING_EXPERIMENT;
        }

        /// <summary>
        /// Runs only once at the start of the state.
        /// </summary>
        protected override void Enter()
        {
            //Debug.Log("InitialisingExperiment State");
            base.Enter();
        }

        /// <summary>
        /// Runs continuously while in this state
        /// </summary>
        protected override void Update()
        {
            gm.InitialiseExperiment(); // Run initialisation stuff.
            stateStage = EVENT.EXIT;
        }

        /// <summary>
        /// Runs as the state is exited.
        /// </summary>
        protected override void Exit()
        {
            nextState = new Instructions(gm);
            base.Exit();
        }
    }
}