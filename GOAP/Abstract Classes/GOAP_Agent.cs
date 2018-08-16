using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GOAP{
	public abstract class GOAP_Agent : MonoBehaviour {
		#region Private variables
		private GOAP_Action[] _Actions; // a list of all base actions
		private GOAP_Goal[] _Goals; // a list of all base goals
		private GOAP_Goal _CurrentGoal; // the goal this agent is currently trying to fulfil
		private Queue<GOAP_ActionState> _CurrentPlan; // the current sequence of actions to take before reaching the current goal
		private Dictionary<GOAP_Goal,float> _GoalBlackList = new Dictionary<GOAP_Goal, float>(); // a list with all goals that are not possible
		private GOAP_Memory _Memory;
		private FSM.FSM _StateMachine = new FSM.FSM();

		// Temp values
		private float ReconsiderTimer; // the temp value that indicates wheter to reconsider again or not
		private bool IsPlanning = false;
		private float SkipTickUpdate = 0f;
		private float _SkipDelay = 0.5f;
		private bool _ReconsiderOnSkip = false;
		#endregion
		#region Core voids
		private void Awake(){
			foreach (GOAP_Goal Goal in AvailableGoals) {Goal.InitializeGoal ();} // init goals
			OnAwake (); // call on awake virtual
		}
		private void Start(){
			OnStart (); // call on start virtual
		}
		private void Update(){
			Memory.UpdateMemory(); // update memory before doing anything
			StateMachine.Update(); // update state machine
			Tick (); // update agent
			OnUpdate(); // call update virtual
		}
		private void OnDestroy(){
			Memory.DestroyMemory (); // destroy memory and sensors
			OnDestruct();
		}
		#endregion
		#region public Get / Set
		public GOAP_Action[] ActionPlan{
			get{
				GOAP_ActionState[] StatePlan = Plan;
				GOAP_Action[] result = new GOAP_Action[StatePlan.Length];

				for (int i = 0; i < result.Length; i++) {
					result [i] = StatePlan [i].Action;
				}
				return result;
			}
		}
		public GOAP_ActionState[] Plan{
			get{
				return (_CurrentPlan != null)?_CurrentPlan.ToArray():new GOAP_ActionState[0];
			}
		}
		public GOAP_Goal CurrentGoal{
			get{
				return _CurrentGoal;
			}
		}
		public GOAP_Action[] Actions{
			get{
				if(_Actions == null){
					_Actions = InitializeActions ();
				}
				return _Actions;
			}
		}
		public GOAP_Goal[] InitialGoals{
			get{
				if (_Goals == null) {
					_Goals = InitializeGoals ();
				}
				return _Goals;
			}
		}
		public List<GOAP_Goal> AvailableGoals{
			get{
				int[] AvailableIndexes = new int[InitialGoals.Length];
				int AvailableCount = 0;
				for (int i = 0; i < InitialGoals.Length; i++) {
					if (InitialGoals [i].IsGoalPossible ()) {
						AvailableIndexes[i] = 1; // set possible
						AvailableCount++;

					} else {
						AvailableIndexes [i] = 0; // set not possible
					}
				}

				GOAP_Goal[] _Available = new GOAP_Goal[AvailableCount];
				int Index = 0;
				for (int i = 0; i < AvailableIndexes.Length; i++) {
					if (AvailableIndexes [i] != 0) {
						_Available [Index] = InitialGoals [i];
						Index++;
					}
				}
				return new List<GOAP_Goal>(_Available);
			}
		}
		public List<GOAP_Goal> PrioritizedGoals{
			get{
				List<GOAP_Goal> _TempAvailableGoals = AvailableGoals;
				_TempAvailableGoals.Sort (delegate(GOAP_Goal x, GOAP_Goal y) {
					return y.Priority().CompareTo(x.Priority());
				}); // sort array based on Priority
				return _TempAvailableGoals; // return prioritized array
			}
		}
		public GOAP_Memory Memory{
			get{
				if (_Memory == null) {
					_Memory = new GOAP_Memory ();
					_Memory.SetSensors (InitializeSensors(),this);
				}
				return _Memory;
			}
		}
		public FSM.FSM StateMachine{
			get{
				return _StateMachine;
			}
		}
		public bool ReconsiderOnSkip{
			get{
				return _ReconsiderOnSkip;
			}
			set{
				_ReconsiderOnSkip = value;
			}
		}
        public float SkipDelay
        {
            get
            {
                return _SkipDelay;
            }
            set
            {
                _SkipDelay = value;
            }
        }
		private bool PursueCurrentPlan{
			get{
				return (_CurrentPlan != null && _CurrentPlan.Count > 0 && _CurrentGoal != null && _CurrentGoal.IsGoalPossible() && CheckPlanValidation());
			}
		}
		private bool CanCancelCurrentPlan{
			get{
				if (_CurrentPlan != null && _CurrentPlan.Count > 0 && _CurrentGoal != null) {
					if (!_CurrentPlan.Peek ().Action.AskForInterruption (_CurrentGoal.Goal) && !_CurrentPlan.Peek ().Action.IsDone (_CurrentGoal.Goal)) {
						return false;
					}
				}
				return true;
			}
		}
		#endregion
		#region Abstract Get / Set
		protected abstract GOAP_Action[] InitializeActions(); // initialize available actions
		protected abstract GOAP_Goal[] InitializeGoals(); // initialize agent goals
		protected abstract GOAP_Sensor[] InitializeSensors(); //  initialize the sensors
		#endregion
		#region Virtuals
		#region Voids
		protected virtual void OnAwake(){}
		protected virtual void OnStart(){}
		protected virtual void OnUpdate(){}
		protected virtual void OnDestruct(){}
		protected virtual void OnSkipUpdate(){}
		protected virtual void NoAvailableGoals(){}
		#endregion
		#region Values
		#endregion
		#endregion
		#region private voids
		// tick is the core update function of a GOAP agent
		private void Tick(){ // update the agent
			CheckCurrentPlan(); // update current plan to follow
			UpdateSkipTick ();
		}
		private void SkipTick(){
			if (ReconsiderOnSkip) {Reconsider ();}
			OnSkipUpdate (); // call protected skip update
		}
		private void UpdateSkipTick(){
			if(Time.time >= SkipTickUpdate){
				SkipTickUpdate = Time.time + SkipDelay;
				SkipTick (); // call skip tick
			}
		}

		// checks wheter the current plan is still valid and if so makes sure the current plan gets pursued
		private void CheckCurrentPlan(){
			if (!PursueCurrentPlan) { // there is no current plan or the current plan isnt possible anymore
				CreateNewPlan(); // set new goal and create a new plan for the newly chosen goal
			}
			PursuePlan(); // pursue planned plan
		}

		// pursues the completion of the current goal and plan
		private void PursuePlan(){
			if (PursueCurrentPlan) { // there is a plan and a goal to pursue
				if(!CheckGoalValidation()){return;} // if validation is not true stop here dont update

				_CurrentPlan.Peek().Action.UpdateAction(_CurrentPlan.Peek().GoalState); // update
				if (_CurrentPlan.Peek ().Action.IsDone(_CurrentPlan.Peek().GoalState)) { // if action is done or actions can not be
					GOAP_ActionState DequeAction = _CurrentPlan.Dequeue ();
					DequeAction.Action.ExitAction (_CurrentGoal.Goal); // exit action

					bool EnterPlan = OnEnterPlan (); // call on enter of current plan
					if(!EnterPlan){SetNewPlan(null,null);} // set empty plan
				}
			} else { // there is no goal and plan to pursue
				#if DEBUG
				Debug.LogError("Trying to pursue a plan but there isnt any: "+this.gameObject.name+". Create a new goal and plan. CurrentGoal: "+_CurrentGoal);
				#endif
			}
		}

		// finds the most valid goal at the moment and creates a plan to complete it
		private void CreateNewPlan(){
			if (_CurrentPlan != null && _CurrentPlan.Count > 0) {
				_CurrentPlan.Peek ().Action.ExitAction (_CurrentPlan.Peek ().GoalState);
			}

			List<GOAP_Goal> PriorGoals = PrioritizedGoals; // get current available goals sorted by priority
			if (PriorGoals != null && PriorGoals.Count > 0) { // there are available goals found
				PlanGoal(PriorGoals[0]); // get first prior goal and plan it
			} else { // no available goals are found
				_CurrentGoal = null;
				_CurrentPlan = null;
				#if DEBUG
				Debug.LogError("No available goals found in agent: "+ this.gameObject.name);
				#endif
				NoAvailableGoals (); // run no available goals function
			}
		}

		// plan a new plan
		private void PlanGoal(GOAP_Goal TargetGoal){
			IsPlanning = true; // set isplanning to true cause the Planner is planning a plan
			TargetGoal.PreCalculations(); // do pre calculations before calculating path to goal state
			GOAP_Planner.I.Plan(TargetGoal,this,OnDonePlanning); // plan a new plan
		}

		private bool CheckGoalValidation(){
			if (PursueCurrentPlan) {
				return CheckPlanValidation ();
			}
			return false;
		}
		private bool CheckPlanValidation(){
			if(!_CurrentPlan.Peek().Action.CheckProceduralPrecondition(_CurrentPlan.Peek().GoalState)){ // check current action is still valid and if not check if can cancel
				//CreateNewPlan(); // create a new plan, current plan isnt valid anymore
				return false;
			}
			return true;
		}

		// When done with planning
		private void OnDonePlanning(Queue<GOAP_ActionState> Plan,GOAP_Goal Goal){
			IsPlanning = false;
			if (Plan != null && Plan.Count > 0 && Goal != null) {
				SetNewPlan (Plan,Goal);
				return; // find a purpose
			}
			SetNewPlan (null,null);
		}

		private void SetNewPlan(Queue<GOAP_ActionState> Plan,GOAP_Goal Goal){
			_CurrentGoal = Goal; // set current goal
			_CurrentPlan = Plan; // set current plan
			OnEnterPlan();
		}

		private bool OnEnterPlan(){ // call on enter of current plan
			if (_CurrentPlan != null && _CurrentPlan.Count > 0) { // if still contains actions in current plan
				Memory.UpdateMemory(); // update memory first

				if (!_CurrentPlan.Peek ().Action.CheckProceduralSwitch (_CurrentPlan.Peek ().GoalState)) {return false;} // cant enter new action
				_CurrentPlan.Peek().Action.UpdateEffectsAndConditions(true,_CurrentPlan.Peek().GoalState); // update conditions before entering
				_CurrentPlan.Peek().Action.EnterAction(_CurrentPlan.Peek().GoalState); // call on enter function
			}
			return true;
		}
		#endregion
		#region Public voids
		/// <summary>
		/// Reconsider the current goal
		/// </summary>
		public void Reconsider(){
			if (_CurrentGoal == null && (_CurrentPlan == null || _CurrentPlan.Count <= 0)) {return;} // there is no current plan, there is nothing to reconsider. Next frame a plan will be formed
			if (_CurrentGoal != null && _CurrentGoal.Priority() >= PrioritizedGoals[0].Priority()){return;} // if the current goals priority isnt the highest priority anymore
			if (!CanCancelCurrentPlan) {return;} // current action is not interruptible

            if (_CurrentPlan != null && _CurrentPlan.Count > 0) { _CurrentPlan.Peek().Action.InterruptAction(_CurrentPlan.Peek().GoalState); }
			List<GOAP_Goal> PriorGoals = PrioritizedGoals;
			if (PriorGoals != null && PriorGoals.Count > 0) { // have prior goals. Only reconsider current goal if there are any goals to consider at all.
				for (int i = 0; i < PriorGoals.Count; i++) {
					if (_CurrentGoal != PriorGoals [i] && _CurrentGoal.Priority() <= PriorGoals [i].Priority()) { // check if new goal is more relevant then current goal
						CreateNewPlan (); // create a new plan cause there is a new more relevant goal or current goal is empty
						break;
					}
				}
			}
		}
		#endregion
	}
}
