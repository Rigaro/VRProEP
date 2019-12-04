using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class Paused : State
    {
        public Paused(GameMaster gm) : base(gm)
        {
            stateName = STATE.PAUSED;
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