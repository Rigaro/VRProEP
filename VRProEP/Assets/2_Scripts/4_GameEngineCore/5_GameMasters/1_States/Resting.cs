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