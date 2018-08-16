using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace AStar{
		public abstract class AStar_Searcher<T> {
		public Queue<I> Solve<I>(WeightedGraph<T> Graph,System.Func<T,I> ReturnConverter,T Start, T Goal, bool IncludeStart = false,bool IncludeGoal = false,int MaxIterations = int.MaxValue){
			Dictionary<T,T> CameFrom = new Dictionary<T, T>(); // initialize dictionarys
			Dictionary<T,float> CostSoFar = new Dictionary<T, float>(); // initialize dictionaries

			PriorityQueue<AStarNode<T>> Frontier = new PriorityQueue<AStarNode<T>> ();
			Frontier.Insert (new AStarNode<T>(Start,0)); // insert start

			CameFrom [Start] = Start;
			CostSoFar [Start] = 0;
			int IterationCount = 0;
			bool FoundPath = false;

			while (Frontier.Count > 0) {  // while there are still items in the frontier
				T Current = Frontier.RemoveRoot().Data; // remove most prior item

				if (EqualsGoal(Current,Goal)) {Goal = Current; FoundPath = true;break;} // found the goal

				foreach (T Next in Graph.Neighbors(Current)) {
					float NewCost = CostSoFar [Current] + Graph.Cost (Current,Next); // calculate the new cost of the next node. Cost of last item + cost to go from last to next
					if (!CostSoFar.ContainsKey (Next) || NewCost < CostSoFar [Next]) { // if next item isnt already in the cost so far dictionary or new cost is lower the prev cost
						CostSoFar [Next] = NewCost; // update the new cost
						float priority = NewCost + Heuristic (Next,Goal); // new priority is Newcost + distance between next and goal (AKA heuristic)
						Frontier.Insert(new AStarNode<T>(Next,priority)); // add item to the frontiers
						CameFrom [Next] = Current; // the parent of next is the last one
					}
				}

				// max iterations
				IterationCount++;
				if (IterationCount >= MaxIterations) {return null;} // could not find path. Timed out
			}

			if(FoundPath){ // there is a path found
				// build back path
				Queue<I> Path = new Queue<I> ();
				T current = Goal;
				Path.Enqueue (ReturnConverter(current));

				int count = 0; // iteration count
				while (!Equals(current,Start) && count < CameFrom.Count) {
					current = CameFrom [current];
					if (!Equals (current, Start)) {
						Path.Enqueue (ReturnConverter (current));
					}
					count++; // add up iteration count
				}
					
				if (!Equals(current,Start)) {return null;} // if current isnt equal to the start. No path is found
				if (IncludeStart) {Path.Enqueue (ReturnConverter(Start));} // include start
				return Path; // return valid path
			}
			return null;
		}

		#region Abstracts
		public abstract bool EqualsGoal (T Node, T Goal);
		public abstract float Heuristic(T GraphNode, T Goal);
		#endregion

		#region Inner class
		private class AStarNode<T>:IComparable<AStarNode<T>>{
			private float _Cost;
			private T _Data;

			public AStarNode(T Data, float Cost){
				this._Data = Data;
				this._Cost = Cost;
			}

			public int CompareTo (AStarNode<T> other)
			{
				if (other == null)return 1; // other is null return self as highest

				return this.Cost.CompareTo (other.Cost); // return compare
			}

			#region Get / Set
			public float Cost{
				get{
					return this._Cost;
				}
			}
			public T Data{
				get{
					return _Data;
				}
			}
			#endregion
		}
		#endregion
	}
}
