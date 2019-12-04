using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class Training : State
    {
        public Training(GameMaster gm) : base(gm)
        {
            stateName = STATE.TRAINING;
        }

        /// <summary>
        /// Runs only once at the start of the state.
        /// </summary>
        protected override void Enter()
        {
            base.Enter();
        }

        /// <summary>
        /// Runs continuously while in this state
        /// </summary>
        protected override void Update()
        {

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