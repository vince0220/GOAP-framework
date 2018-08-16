using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AI;

namespace GOAP{
		public class GOAP_GraphNode {

		#region Private variables
		private GOAP_Agent _Agent;
		private GOAP_State _LeftConditions;
		private GOAP_State _State;
		private GOAP_GraphNode _Parent;
		private GOAP_ActionState _ConnectionAction;
		private float _Cost = 0;
		private float _H = 0;
		private List<KeyValuePair<string,GOAP_State.PriorityValue>> _LeftPreConditions = new List<KeyValuePair<string, GOAP_State.PriorityValue>>();
		#endregion

		#region Constructor
		public GOAP_GraphNode(GOAP_State Left, GOAP_Agent Agent, GOAP_GraphNode Parent,GOAP_Action ConnectionAction,List<KeyValuePair<string,GOAP_State.PriorityValue>> LeftPreConditions = default(List<KeyValuePair<string,GOAP_State.PriorityValue>>)){
			this._Agent = Agent; // the agent of which to take its actions
			this._LeftConditions = new GOAP_State(Left); // the goal state represents the state you want to achieve using the actions of the agent.
			this._Parent = Parent; // the parents effect is used to meet the preconditions of this node. If parent is equal to null that means its the beginning point of the search.
			this._LeftPreConditions = LeftPreConditions;

			// temp
			float G = 0;
			if (Parent != null && ConnectionAction != null) {
				G = ConnectionAction.Cost; // set new cost
				_LeftConditions.RemoveConditions (ConnectionAction.Effects.Conditions); // remove effects from left state
				_LeftConditions += ConnectionAction.PreConditions; // add the pre conditions of the connection action

				_State = Parent.State + ConnectionAction.Effects; // the new state is the state of the parrent with the connection action effects applied

				if(Parent.ConnectionAction != null && Parent.ConnectionAction.Action != null){
					_State.ReplaceGenData(Parent.ConnectionAction.Action.PreConditions); // replace pre conditions gen data
				}

			} else {
				_State = Agent.Memory.State; // if parent is equal to null. It means this is the start node. The state of the start node is always the memory of the agent.
			}

			this._ConnectionAction = new GOAP_ActionState(ConnectionAction,new GOAP_State(this._State)); // the connection action is the action used to get from the parent state to this state.

			_H = _LeftConditions.Count; // the heuristic is the count of conditions left to solve
			_Cost = G + _H;
		}
		#endregion

		#region Get / Set
		/// <summary>
		/// the connection action is the action used to get from the parent state to this state.
		/// </summary>
		/// <value>The connection action.</value>
		public GOAP_ActionState ConnectionAction{
			get{
				return _ConnectionAction;
			}
		}
		public float Heuristic{
			get{
				return _H;
			}
		}
		public GOAP_Agent Agent{
			get{
				return _Agent;
			}
		}
		public GOAP_State LeftConditions{
			get{
				return _LeftConditions;
			}
		}
		public float Cost{
			get{
				return _Cost + Heuristic;
			}
		}
		public GOAP_State State{
			get{
				return _State;
			}
		}
		public GOAP_State PreConditions{
			get{
				return (ConnectionAction.Action != null) ? ConnectionAction.Action.PreConditions : new GOAP_State ();
			}
		}
		public List<KeyValuePair<string,GOAP_State.PriorityValue>> LeftPreConditions{
			get{
				return _LeftPreConditions;
			}
		}
		#endregion
	}
}
