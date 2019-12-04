using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class AnalysingResults : State
    {
        public AnalysingResults(GameMaster gm) : base(gm)
        {
            stateName = STATE.ANALYSING_RESULTS;
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