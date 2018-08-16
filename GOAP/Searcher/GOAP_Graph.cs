using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;
using System.Linq;

namespace GOAP{
	public class GOAP_Graph : WeightedGraph<GOAP_GraphNode> {
		public float Cost (GOAP_GraphNode Current, GOAP_GraphNode Next)
		{
			return Next.Cost; // return the cost of the node
		}

		public IEnumerable<GOAP_GraphNode> Neighbors (GOAP_GraphNode Item)
		{
			if (Item.LeftPreConditions == null || Item.LeftPreConditions.Count <= 0) { // solve left priorities
				foreach (GOAP_Action Action in PossibleActions(Item)) {
					var SortedConditions = Action.PreConditions.GetSortedList ();
					var LeftConditions = new GOAP_State(Item.LeftConditions);
					GOAP_State Combined = Item.State + Action.Effects; // the combined conditions of the current state of the parrent and the effects of the new action
					LeftConditions.RemoveConditions (Combined.Conditions);
				
					yield return new GOAP_GraphNode (LeftConditions, Item.Agent, Item, Action,SortedConditions); // this is a neighbor of the parent node
				}
			} else { // first solve preconditions
				foreach (GOAP_Action Action in PossibleActions(Item,Item.LeftPreConditions)) {
					GOAP_State Combined = Item.State + Action.Effects; // the combined conditions of the current state of the parrent and the effects of the new action
					var CopyList = new List<KeyValuePair<string,GOAP_State.PriorityValue>> (Item.LeftPreConditions); // copy left preconditions
					bool SubstractedPreconditions = Combined.RemoveSelfInOrderFromList (ref CopyList); // remove from combined state

					if (SubstractedPreconditions) { // combined effects satisfy the first or more preconditions of the previous action
						var SortedConditions = Action.PreConditions.GetSortedList (); // set sorted list of preconditios from this action
						SortedConditions.AddRange (CopyList); // add copy list to preconditions of action
						yield return new GOAP_GraphNode (Item.LeftConditions, Item.Agent, Item, Action,SortedConditions); // this is a neighbor of the parent node
					}
				}
			}
		}

		#region Private voids
		public IEnumerable<GOAP_Action> PossibleActions(GOAP_GraphNode Node,List<KeyValuePair<string,GOAP_State.PriorityValue>> LeftPreconditions){
			GOAP_Action[] Actions = Node.Agent.Actions;// get all possible actions of agent
			for (int i = 0; i < Actions.Length; i++) {
				var PossibleAction = Actions[i];
				GOAP_State State = new GOAP_State (LeftPreconditions); // create left state based on preconditions

				PossibleAction.UpdateEffectsAndConditions (true, State); // update action conditions

				if (PossibleAction.Effects.HasMatch (State) && PossibleAction.CheckProceduralUsablity (State)) { // if has matching effect and possible action is possible
					yield return PossibleAction; // this is a possible action
				}
			}
		}
		public IEnumerable<GOAP_Action> PossibleActions(GOAP_GraphNode Node){
			GOAP_Action[] Actions = Node.Agent.Actions;// get all possible actions of agent
			for (int i = 0; i < Actions.Length; i++) {
				var PossibleAction = Actions[i];
				PossibleAction.UpdateEffectsAndConditions (true, Node.LeftConditions); // update action conditions

				if (PossibleAction.Effects.HasMatch (Node.LeftConditions) && PossibleAction.CheckProceduralUsablity (Node.LeftConditions)) { // if has matching effect and possible action is possible
					yield return PossibleAction; // this is a possible action
				}
			}
		}
		#endregion
	}
}
