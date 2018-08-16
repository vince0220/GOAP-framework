using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP{
public abstract class GOAP_Goal {
		#region Variables
		private GOAP_State _Goal; // the goal state of this GOAP_Goal
		private GOAP_Agent _Agent;
		private float _DelayTime;
		#endregion

		#region Constructor
		public GOAP_Goal(GOAP_Agent Agent){
			this._Agent = Agent; // set Goap agent
		}
		#endregion

		#region Abstract members
		/// <summary>
		/// Returns a value between 0 and 1 which stands for the relevancy at the moment
		/// </summary>
		/// <value>The evaluate relevancy (0 - 1).</value>
		public abstract float Priority();
		protected abstract void InitialGoal ();
		#endregion

		#region Protected inputs
		protected void DelayGoal(float DelaySeconds){
			_DelayTime = Time.time + DelaySeconds;
		}
		#endregion

		#region Virtual Members
		/// <summary>
		/// Gets a value indicating whether this goal is possible.
		/// </summary>
		/// <value><c>true</c> if this instance is goal possible; otherwise, goal isnt possible <c>false</c>.</value>
		public virtual bool IsGoalPossible(){
			return Time.time >= _DelayTime;
		}

		public virtual void OnFailed(){}

		/// <summary>
		/// Calculations to do just before comparing goal states
		/// </summary>
		public virtual void PreCalculations(){}
		#endregion

		#region public inputs
		public void InitializeGoal(){
			InitialGoal ();
		}
		#endregion

		#region Get / Set
		public GOAP_Agent Agent{
			get{
				return this._Agent;
			}
		}
		public GOAP_State Goal{
			get{
				if (_Goal == null) {
					_Goal = new GOAP_State ();
					InitialGoal ();
				}
				return _Goal;
			}
			set{
				_Goal = value;
			}
		}
		#endregion
	}
}
