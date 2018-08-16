using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;
using System.Linq;

namespace GOAP{
	public sealed class GOAP_Planner {
		#region Singleton
		private static GOAP_Planner _Instance;
		public static GOAP_Planner I{
			get{
				if (_Instance == null) {
					_Instance = new GOAP_Planner ();
				}
				return _Instance;
			}
		}
		#endregion	

		#region Input voids
		public void Plan(GOAP_Goal Goal,GOAP_Agent Agent, System.Action<Queue<GOAP_ActionState>,GOAP_Goal> OnDone){
			GOAP_State GoalState = Goal.Goal.RemoveConditions (Agent.Memory.State.Conditions); // remove already satisfied goals based on agent memory
			if (IsAchievable (GoalState, Agent)) { // first check wheter with the given actions the disired goal can be achived before forcing it with a*
				GOAP_Searcher Searcher = new GOAP_Searcher(); // create new A* GOAP searches
				Queue<GOAP_ActionState> NewPlan = Searcher.Solve<GOAP_ActionState>(
					new GOAP_Graph(), // initialize new goap graph
					(GOAP_GraphNode Node)=>{
						return Node.ConnectionAction;
					},
					new GOAP_GraphNode(GoalState,Agent,null,null), // start state is the goal cause of backwards searching
					new GOAP_GraphNode(Agent.Memory.State,Agent,null,null), // goal state is the current state cuase of backwards searching
					false,
					false, // include start in the que
					500 // max 500 iterations before force exit
				); // solve plan finding

				if (NewPlan != null) {OnDone.Invoke (NewPlan, Goal);return;} // created plan invoke
			}

			// could not find a plan
			OnDone.Invoke(null,Goal);
		}
		#endregion

		#region public voids
		public bool IsAchievable(GOAP_State GoalState, GOAP_Agent Agent){
			if (GoalState.Conditions.Count <= 0) {return false;} // goalstate is already reached

			GOAP_State CopyState = new GOAP_State(GoalState);
			for (int i = 0; i < Agent.Actions.Length; i++) {
				if (CopyState.Count <= 0) {return true;} // if goal state count is lower the zero then its achievable

				Agent.Actions [i].UpdateEffectsAndConditions (true, CopyState); // pre calculations done before checking
				if(!Agent.Actions[i].CheckProceduralUsablity(CopyState)){continue;} // check if action is runnable or not
				CopyState.RemoveConditions(Agent.Actions[i].Effects.Conditions); // remove conditions of state based on effects of actions
			}
			return CopyState.Count <= 0;
		}
		#endregion
	}
}
