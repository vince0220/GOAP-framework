using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FSM{
	public interface FSM_State {
		void EnterStack();
		void Update();
		void ExitStack ();
	}
}
