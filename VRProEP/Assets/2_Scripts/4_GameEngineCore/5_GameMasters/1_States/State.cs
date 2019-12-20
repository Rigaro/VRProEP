using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRProEP.GameEngineCore
{
    public class State
    {
        public enum STATE
        {
            WELCOME,
            INITIALISING_EXPERIMENT,
            INSTRUCTIONS,
            TRAINING,
            WAITING_FOR_START,
            PERFORMING_TASK,
            ANALYSING_RESULTS,
            INITIALISING_NEXT_ITERATION,
            INITIALISING_NEXT_SESSION,
            RESTING,
            PAUSED,
            END
        };

        public enum EVENT
        {
            ENTER, UPDATE, EXIT
        };

        protected GameMaster gm;
        protected STATE stateName;
        protected EVENT stateStage;
        protected State nextState;
        protected State previousState;

        public STATE StateName { get => stateName; }

        public State(GameMaster gm)
        {
            if (gm == null) throw new System.ArgumentNullException("The provided GameMaster is empty");
            
            this.gm = gm;
            stateStage = EVENT.ENTER;
        }

        /// <summary>
        /// Code that runs only once.
        /// </summary>
        protected virtual void Enter() { stateStage = EVENT.UPDATE; }
        /// <summary>
        /// Code that runs at every iteration.
        /// </summary>
        protected virtual void Update() { stateStage = EVENT.UPDATE; }
        /// <summary>
        /// Code that handles the end of this state.
        /// </summary>
        protected virtual void Exit() { stateStage = EVENT.EXIT; }

        public State Process()
        {
            if (stateStage == EVENT.ENTER) Enter();
            if (stateStage == EVENT.UPDATE) Update();
            if (stateStage == EVENT.EXIT) { Exit(); return nextState; };
            return this;
        }
    }
}
