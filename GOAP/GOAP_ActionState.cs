using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP{
	public class GOAP_ActionState {
		public GOAP_Action _Action;
		public GOAP_State _GoalState;

		public GOAP_ActionState(GOAP_Action Action, GOAP_State GoalState){
			this._Action = Action;
			this._GoalState = GoalState;
		}

		// get / setters
		public GOAP_Action Action{
			get{
				return _Action;
			}
		}
		public GOAP_State GoalState{
			get{
				return _GoalState;
			}
		}
	}
}
