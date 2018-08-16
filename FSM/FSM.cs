using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM{
	public class FSM {
		private FSM_State CurrentState;

		public void Update(){
			if (CurrentState != null) {
				CurrentState.Update ();
			}
		}

		public void SetState(FSM_State state){
			if (CurrentState != null) {
				CurrentState.ExitStack ();
			}
			state.EnterStack (); // enter state
			CurrentState = state;
		}

		public FSM_State State{
			get{
				return CurrentState;
			}
		}
	}
}
