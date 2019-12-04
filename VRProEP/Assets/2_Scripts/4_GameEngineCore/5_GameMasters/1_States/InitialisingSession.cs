using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class InitialisingSession : State
    {
        public InitialisingSession(GameMaster gm) : base(gm)
        {
            stateName = STATE.INITIALISING_SESSION;
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