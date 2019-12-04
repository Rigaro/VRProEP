using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class UpdatingApplication : State
    {
        public UpdatingApplication(GameMaster gm) : base(gm)
        {
            stateName = STATE.UPDATING_APPLICATION;
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