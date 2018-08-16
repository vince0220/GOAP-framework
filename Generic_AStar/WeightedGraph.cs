using System.Collections;
using System.Collections.Generic;

namespace AStar{
	public interface WeightedGraph<L> {
		float Cost (L Current, L Next);
		IEnumerable<L> Neighbors(L Item);
	}
}
