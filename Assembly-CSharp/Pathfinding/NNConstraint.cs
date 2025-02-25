namespace Pathfinding;

public class NNConstraint
{
	public int graphMask = -1;

	public bool constrainArea;

	public int area = -1;

	public bool constrainWalkability = true;

	public bool walkable = true;

	public bool distanceXZ;

	public bool constrainTags = true;

	public int tags = -1;

	public bool constrainDistance = true;

	public int pathWidth = 1;

	public static NNConstraint Default => new NNConstraint();

	public static NNConstraint None
	{
		get
		{
			NNConstraint nNConstraint = new NNConstraint();
			nNConstraint.constrainWalkability = false;
			nNConstraint.constrainArea = false;
			nNConstraint.constrainTags = false;
			nNConstraint.constrainDistance = false;
			nNConstraint.graphMask = -1;
			return nNConstraint;
		}
	}

	public virtual bool SuitableGraph(int graphIndex, NavGraph graph)
	{
		return ((graphMask >> graphIndex) & 1) != 0;
	}

	public virtual bool Suitable(GraphNode node)
	{
		if (constrainWalkability && node.Walkable != walkable)
		{
			return false;
		}
		if (constrainArea && area >= 0 && node.Area != area)
		{
			return false;
		}
		if (constrainTags && ((tags >> (int)node.Tag) & 1) == 0)
		{
			return false;
		}
		if (pathWidth > 1 && node is GridNode && ((GridNode)node).clearance < pathWidth)
		{
			return false;
		}
		return true;
	}
}
