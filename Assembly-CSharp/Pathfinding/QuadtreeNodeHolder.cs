namespace Pathfinding;

public class QuadtreeNodeHolder
{
	public QuadtreeNode node;

	public QuadtreeNodeHolder c0;

	public QuadtreeNodeHolder c1;

	public QuadtreeNodeHolder c2;

	public QuadtreeNodeHolder c3;

	public void GetNodes(GraphNodeDelegateCancelable del)
	{
		if (node != null)
		{
			del(node);
			return;
		}
		c0.GetNodes(del);
		c1.GetNodes(del);
		c2.GetNodes(del);
		c3.GetNodes(del);
	}
}
