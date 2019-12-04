using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class WaitingForStart : State
    {
        public WaitingForStart(GameMaster gm) : base(gm)
        {
            stateName = STATE.WAITING_FOR_START;
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