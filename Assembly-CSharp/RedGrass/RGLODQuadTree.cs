using System.Collections.Generic;

namespace RedGrass;

public class RGLODQuadTree
{
	public delegate void NodeDel(RGLODQuadTreeNode n);

	private List<Dictionary<int, RGLODQuadTreeNode>> mLODNodes;

	private Dictionary<int, Dictionary<int, RGLODQuadTreeNode>> mLODTailNodes;

	private EvniAsset mEvni;

	public RGLODQuadTree(EvniAsset evni)
	{
		mLODNodes = new List<Dictionary<int, RGLODQuadTreeNode>>();
		mLODTailNodes = new Dictionary<int, Dictionary<int, RGLODQuadTreeNode>>();
		mEvni = evni;
		int maxLOD = mEvni.MaxLOD;
		mLODNodes.Clear();
		for (int i = 0; i < maxLOD + 1; i++)
		{
			mLODNodes.Add(new Dictionary<int, RGLODQuadTreeNode>());
		}
	}

	public Dictionary<int, Dictionary<int, RGLODQuadTreeNode>> GetLODTailNode()
	{
		Dictionary<int, Dictionary<int, RGLODQuadTreeNode>> dictionary = new Dictionary<int, Dictionary<int, RGLODQuadTreeNode>>();
		foreach (KeyValuePair<int, RGLODQuadTreeNode> item in mLODNodes[mLODNodes.Count - 1])
		{
			TraversalTailNode(item.Value, dictionary);
		}
		return dictionary;
	}

	public Dictionary<int, RGLODQuadTreeNode> GetRootNodes()
	{
		return mLODNodes[mLODNodes.Count - 1];
	}

	private void TraversalTailNode(RGLODQuadTreeNode node)
	{
		if (node == null)
		{
			return;
		}
		if (node.isTail)
		{
			if (mLODTailNodes.ContainsKey(node.LOD))
			{
				int key = Utils.PosToIndex(node.xIndex, node.zIndex);
				mLODTailNodes[node.LOD].Add(key, node);
			}
			else
			{
				int key2 = Utils.PosToIndex(node.xIndex, node.zIndex);
				mLODTailNodes.Add(node.LOD, new Dictionary<int, RGLODQuadTreeNode>());
				mLODTailNodes[node.LOD].Add(key2, node);
			}
		}
		else
		{
			TraversalTailNode(node.node1);
			TraversalTailNode(node.node2);
			TraversalTailNode(node.node3);
			TraversalTailNode(node.node4);
		}
	}

	private void TraversalTailNode(RGLODQuadTreeNode node, Dictionary<int, Dictionary<int, RGLODQuadTreeNode>> tailNodes)
	{
		if (node == null)
		{
			return;
		}
		if (node.isTail)
		{
			if (tailNodes.ContainsKey(node.LOD))
			{
				int key = Utils.PosToIndex(node.xIndex, node.zIndex);
				tailNodes[node.LOD].Add(key, node);
			}
			else
			{
				int key2 = Utils.PosToIndex(node.xIndex, node.zIndex);
				tailNodes.Add(node.LOD, new Dictionary<int, RGLODQuadTreeNode>());
				tailNodes[node.LOD].Add(key2, node);
			}
		}
		else
		{
			TraversalTailNode(node.node1, tailNodes);
			TraversalTailNode(node.node2, tailNodes);
			TraversalTailNode(node.node3, tailNodes);
			TraversalTailNode(node.node4, tailNodes);
		}
	}

	public void TraversalNode(RGLODQuadTreeNode node, NodeDel del)
	{
		if (node.node1 != null)
		{
			TraversalNode(node.node1, del);
		}
		if (node.node2 != null)
		{
			TraversalNode(node.node2, del);
		}
		if (node.node3 != null)
		{
			TraversalNode(node.node3, del);
		}
		if (node.node4 != null)
		{
			TraversalNode(node.node4, del);
		}
		del(node);
	}

	public void TraversalNode4Req(RGLODQuadTreeNode node, RGScene scn)
	{
		if (node.node1 != null)
		{
			TraversalNode4Req(node.node1, scn);
		}
		if (node.node2 != null)
		{
			TraversalNode4Req(node.node2, scn);
		}
		if (node.node3 != null)
		{
			TraversalNode4Req(node.node3, scn);
		}
		if (node.node4 != null)
		{
			TraversalNode4Req(node.node4, scn);
		}
		scn.OnTraversalNode(node);
	}

	public void UpdateTree(int lod, int xIndex, int zIndex)
	{
		int maxLOD = mEvni.MaxLOD;
		if (mLODNodes.Count != maxLOD + 1)
		{
			mLODNodes.Clear();
			for (int i = 0; i < maxLOD + 1; i++)
			{
				mLODNodes.Add(new Dictionary<int, RGLODQuadTreeNode>());
			}
		}
		if (lod == maxLOD)
		{
			int num = 1 << maxLOD;
			int num2 = mEvni.LODExpandNum[lod] * num;
			Dictionary<int, RGLODQuadTreeNode> dictionary = new Dictionary<int, RGLODQuadTreeNode>();
			for (int j = xIndex - num2; j <= xIndex + num2; j += num)
			{
				for (int k = zIndex - num2; k <= zIndex + num2; k += num)
				{
					int key = Utils.PosToIndex(j, k);
					if (!mLODNodes[lod].ContainsKey(key))
					{
						RGLODQuadTreeNode rGLODQuadTreeNode = new RGLODQuadTreeNode(j, k, mEvni.MaxLOD);
						rGLODQuadTreeNode.isTail = true;
						rGLODQuadTreeNode.visible = true;
						dictionary.Add(key, rGLODQuadTreeNode);
					}
					else
					{
						mLODNodes[lod][key].isTail = true;
						mLODNodes[lod][key].visible = true;
						dictionary.Add(key, mLODNodes[lod][key]);
					}
				}
			}
			mLODNodes[lod] = dictionary;
			return;
		}
		int num3 = 1 << lod;
		int num4 = mEvni.LODExpandNum[lod] * num3;
		int num5 = lod + 1;
		int num6 = 1 << num5;
		Dictionary<int, RGLODQuadTreeNode> dictionary2 = new Dictionary<int, RGLODQuadTreeNode>();
		for (int l = xIndex - num4; l <= xIndex + num4; l += num3)
		{
			for (int m = zIndex - num4; m <= zIndex + num4; m += num3)
			{
				int key2 = Utils.PosToIndex(l, m);
				if (mLODNodes[lod].ContainsKey(key2))
				{
					RGLODQuadTreeNode parent = mLODNodes[lod][key2].parent;
					int key3 = Utils.PosToIndex(parent.node1.xIndex, parent.node1.zIndex);
					if (!dictionary2.ContainsKey(key3))
					{
						dictionary2.Add(Utils.PosToIndex(parent.node1.xIndex, parent.node1.zIndex), parent.node1);
						dictionary2.Add(Utils.PosToIndex(parent.node2.xIndex, parent.node2.zIndex), parent.node2);
						dictionary2.Add(Utils.PosToIndex(parent.node3.xIndex, parent.node3.zIndex), parent.node3);
						dictionary2.Add(Utils.PosToIndex(parent.node4.xIndex, parent.node4.zIndex), parent.node4);
					}
					parent.isTail = false;
					parent.node1.isTail = true;
					parent.node2.isTail = true;
					parent.node3.isTail = true;
					parent.node4.isTail = true;
					continue;
				}
				foreach (KeyValuePair<int, RGLODQuadTreeNode> item in mLODNodes[num5])
				{
					if (l >= item.Value.xIndex && l < item.Value.xIndex + num6 && m >= item.Value.zIndex && m < item.Value.zIndex + num6)
					{
						if (!item.Value.HasChild())
						{
							CreateChildNode(item.Value);
						}
						if (!dictionary2.ContainsKey(item.Key))
						{
							dictionary2.Add(Utils.PosToIndex(item.Value.node1.xIndex, item.Value.node1.zIndex), item.Value.node1);
							dictionary2.Add(Utils.PosToIndex(item.Value.node2.xIndex, item.Value.node2.zIndex), item.Value.node2);
							dictionary2.Add(Utils.PosToIndex(item.Value.node3.xIndex, item.Value.node3.zIndex), item.Value.node3);
							dictionary2.Add(Utils.PosToIndex(item.Value.node4.xIndex, item.Value.node4.zIndex), item.Value.node4);
						}
						item.Value.isTail = false;
						item.Value.node1.isTail = true;
						item.Value.node2.isTail = true;
						item.Value.node3.isTail = true;
						item.Value.node4.isTail = true;
						break;
					}
				}
			}
		}
		foreach (KeyValuePair<int, RGLODQuadTreeNode> item2 in mLODNodes[lod])
		{
			if (dictionary2.ContainsKey(item2.Key))
			{
				continue;
			}
			RGLODQuadTreeNode value = item2.Value;
			RGLODQuadTreeNode parent2 = value.parent.parent;
			bool isTail = true;
			while (parent2 != null)
			{
				if (parent2.isTail)
				{
					isTail = false;
				}
				parent2 = parent2.parent;
			}
			value.parent.isTail = isTail;
			value.isTail = false;
		}
		mLODNodes[lod] = dictionary2;
	}

	private void CreateChildNode(RGLODQuadTreeNode node)
	{
		int num = 1 << node.LOD - 1;
		node.node1 = new RGLODQuadTreeNode(node.xIndex, node.zIndex, node.LOD - 1);
		node.node2 = new RGLODQuadTreeNode(node.xIndex, node.zIndex + num, node.LOD - 1);
		node.node3 = new RGLODQuadTreeNode(node.xIndex + num, node.zIndex, node.LOD - 1);
		node.node4 = new RGLODQuadTreeNode(node.xIndex + num, node.zIndex + num, node.LOD - 1);
		node.node1.parent = node;
		node.node2.parent = node;
		node.node3.parent = node;
		node.node4.parent = node;
	}
}
