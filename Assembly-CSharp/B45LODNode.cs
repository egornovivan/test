using UnityEngine;

public class B45LODNode
{
	protected IntVector4 pos;

	protected B45LODNode[] children;

	protected B45LODNode parent;

	protected int childmaxLod;

	protected int octant;

	public GameObject cube;

	private GameObject cubeGO;

	public IntVector4 Pos => pos;

	protected bool isLeaf => children == null;

	public int physicalSize => (1 << pos.w) * 4;

	public int logicalSize => 1 << pos.w + 3;

	public B45LODNode(IntVector4 _pos, B45LODNode _parent, int _octant)
	{
		pos = new IntVector4(_pos);
		parent = _parent;
		octant = _octant;
	}

	public void split()
	{
		children = new B45LODNode[8];
		int w = pos.w;
		int num = logicalSize >> 1;
		for (int i = 0; i < 8; i++)
		{
			IntVector4 intVector = new IntVector4(pos);
			intVector.w = w - 1;
			intVector.x += (i & 1) * num;
			intVector.y += ((i >> 1) & 1) * num;
			intVector.z += ((i >> 2) & 1) * num;
			children[i] = new B45LODNode(intVector, this, i);
		}
	}

	public static B45LODNode readNode(IntVector3 atpos, B45LODNode root, int lod)
	{
		int num = 0;
		B45LODNode b45LODNode = root;
		IntVector3 zero = IntVector3.Zero;
		do
		{
			zero.x = b45LODNode.pos.x + b45LODNode.logicalSize / 2;
			zero.y = b45LODNode.pos.y + b45LODNode.logicalSize / 2;
			zero.z = b45LODNode.pos.z + b45LODNode.logicalSize / 2;
			num = ((atpos.x >= zero.x) ? 1 : 0) | ((atpos.y >= zero.y) ? 2 : 0) | ((atpos.z >= zero.z) ? 4 : 0);
			if (b45LODNode.isLeaf)
			{
				b45LODNode.split();
			}
			b45LODNode = b45LODNode.children[num];
		}
		while (b45LODNode.pos.w != 0);
		return b45LODNode;
	}

	public static void splitAt(B45LODNode root, IntVector3 atpos, int lod)
	{
		int num = 0;
		B45LODNode b45LODNode = root;
		IntVector3 zero = IntVector3.Zero;
		for (int i = 0; i < lod; i++)
		{
			zero.x = b45LODNode.pos.x + b45LODNode.logicalSize;
			zero.y = b45LODNode.pos.y + b45LODNode.logicalSize;
			zero.z = b45LODNode.pos.z + b45LODNode.logicalSize;
			num = ((atpos.x > zero.x) ? 1 : 0) | ((atpos.y > zero.y) ? 2 : 0) | ((atpos.z > zero.z) ? 4 : 0);
			if (b45LODNode.isLeaf)
			{
				b45LODNode.split();
			}
			b45LODNode = b45LODNode.children[num];
		}
	}

	public static void merge(B45LODNode node)
	{
		if (!node.isLeaf)
		{
			for (int i = 0; i < 8; i++)
			{
				if (node.children[i] != null)
				{
					merge(node.children[i]);
				}
				node.children[i] = null;
			}
		}
		node.children = null;
	}

	public void findNeighbour(int dirIdx)
	{
		for (int i = 0; i < 8; i++)
		{
		}
	}

	public void makeCube()
	{
		Vector3 vector = pos.XYZ.ToVector3() * 0.5f;
		vector.x += (float)physicalSize * 0.5f;
		vector.y += (float)physicalSize * 0.5f;
		vector.z += (float)physicalSize * 0.5f;
		if (cubeGO == null)
		{
		}
		cubeGO.transform.localScale = new Vector3(physicalSize, physicalSize, physicalSize);
	}

	public void removeCube()
	{
		if (cubeGO != null)
		{
			Object.DestroyImmediate(cubeGO);
			cubeGO = null;
		}
	}

	public static void makeCubeRec(B45LODNode node)
	{
		node.makeCube();
		if (!node.isLeaf)
		{
			for (int i = 0; i < 8; i++)
			{
				makeCubeRec(node.children[i]);
			}
		}
	}
}
