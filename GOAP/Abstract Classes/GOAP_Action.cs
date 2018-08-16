using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP{
	public abstract class GOAP_Action {
		#region variables
		private GOAP_State _PreConditions = new GOAP_State();
		private GOAP_State _Effects = new GOAP_State();
		private float _Cost = 1;
		private float _CurrentSkipDelay = 0f;
		private GOAP_Agent _Agent;

		// protected
		protected float SkipDelay = 0.5f;
		#endregion

		#region Constructor
		public GOAP_Action(GOAP_Agent Agent){
			this._Agent = Agent;
		}
		#endregion

		#region Get / Set
		public GOAP_State PreConditions{
			get{
				return _PreConditions;
			}
		}
		public GOAP_State Effects{
			get{
				return _Effects;
			}
		}
		public GOAP_Agent Agent{
			get{
				return _Agent;
			}
		}
		#endregion

		#region Inputs
		public void UpdateEffectsAndConditions(bool SetDefaults,GOAP_State GoalState){
			if(SetDefaults){SetDefaultValues ();} // set default values before pre calculations
			PreCalculations (GoalState);
		}
		#endregion

		#region abstract
		/// <summary>
		/// Determines wheter the action is done performing
		/// </summary>
		/// <value><c>true</c> if this instance is done; otherwise, <c>false</c>.</value>
		public abstract bool IsDone(GOAP_State GoalState);

		protected abstract void Enter(GOAP_State GoalState);
		protected abstract void Update(GOAP_State GoalState);
		protected abstract void Exit(GOAP_State GoalState);
        protected abstract void OnRecalculate();
        #endregion

        #region Inputs
        public void EnterAction(GOAP_State GoalState){
			_CurrentSkipDelay = 0f; // reset skip delay to call directly
			Enter (GoalState);
		}
		public void ExitAction(GOAP_State GoalState){
			Exit (GoalState);
		}
		public void UpdateAction(GOAP_State GoalState){
			_CurrentSkipDelay -= 1 * DeltaTime;

			if (_CurrentSkipDelay <= 0) {
				_CurrentSkipDelay = SkipDelay;
				SkipUpdate (GoalState); // call the skip update
			}

			Update (GoalState); // call abstract update
		}
		public void InterruptAction(GOAP_State GoalState){
			OnInterruption (GoalState);
		}
		#endregion

		#region Protected
		protected void Failed(){
			if (Agent.CurrentGoal != null) {
				Agent.CurrentGoal.OnFailed(); // call failed goal
			}
		}
		#endregion

		#region Virtuals
		/// <summary>
		/// Defines the cost of performing this actio.
		/// </summary>
		/// <value>The cost.</value>
		public virtual float Cost{
			get{
				return _Cost; // get default cost
			}
		}

		/// <summary>
		/// Returns wheter this action is interruptable at this given moment in time. Always called before changing to new state
		/// </summary>
		/// <returns><c>true</c>, if for interruption was asked, <c>false</c> otherwise.</returns>
		/// <param name="Agent">Agent.</param>
		/// <param name="GoalState">Goal state.</param>
		public virtual bool AskForInterruption (GOAP_State GoalState){
			return true;
		}

		protected virtual void OnInterruption(GOAP_State GoalState){}

		/// <summary>
		/// Calculations to do just before comparison state / effects comparisons
		/// </summary>
		/// <param name="Agent">Agent.</param>
		/// <param name="GoalState">Goal state.</param>
		protected virtual void PreCalculations(GOAP_State GoalState){}


		/// <summary>
		/// Skip update is called not every frame but based on the "SkipDelay" value
		/// </summary>
		/// <param name="Agent">Agent.</param>
		/// <param name="GoalState">Goal state.</param>
		protected virtual void SkipUpdate(GOAP_State GoalState){}

		/// <summary>
		/// Sets the action default values
		/// </summary>
		protected virtual void SetDefaultValues(){}

		/// <summary>
		/// Checks whether this action is usable during the planning of a goal
		/// </summary>
		/// <returns><c>true</c>, if procedural usablity was checked, <c>false</c> otherwise.</returns>
		/// <param name="Agent">Agent.</param>
		/// <param name="GoalState">Goal state.</param>
		public virtual bool CheckProceduralUsablity(GOAP_State GoalState){return true;}

		/// <summary>
		/// Checks the procedural precondition, determines wheter this action is still runnable or not.
		/// </summary>
		/// <returns><c>true</c>, if procedural precondition was checked, <c>false</c> otherwise.</returns>
		/// <param name="Agent">Agent.</param>
		/// <param name="GoalState">Goal state.</param>
		public virtual bool CheckProceduralPrecondition(GOAP_State GoalState){return true;}

		/// <summary>
		/// This check is performed when switching to this action. It is only called once.
		/// </summary>
		/// <returns><c>true</c>, if procedural switch was checked, <c>false</c> otherwise.</returns>
		/// <param name="Agent">Agent.</param>
		/// <param name="GoalState">Goal state.</param>
		public virtual bool CheckProceduralSwitch(GOAP_State GoalState){return true;}

		/// <summary>
		/// Determines the delta time in which this actions should exist
		/// </summary>
		/// <value>The delta time.</value>
		protected virtual float DeltaTime{
			get{
				return Time.deltaTime;
			}
		}
		#endregion
	}
}
