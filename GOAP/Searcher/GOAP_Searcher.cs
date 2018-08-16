using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

namespace GOAP{
	public class GOAP_Searcher : AStar_Searcher<GOAP_GraphNode> {
		public override float Heuristic (GOAP_GraphNode GraphNode, GOAP_GraphNode Goal)
		{
			return GraphNode.Heuristic;
		}
			
		public override bool EqualsGoal (GOAP_GraphNode Node, GOAP_GraphNode Goal)
		{
			return (Node.LeftConditions.Count == 0); // once there are no more left conditions to satisfy
		}
	}
}
