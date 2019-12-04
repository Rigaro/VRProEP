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
            base.Enter();
        }

        protected override void Update()
        {

        }

        protected override void Exit()
        {
            base.Exit();
        }
    }
}