using System;
using System.Collections.Generic;
using UnityEngine;

public class Block45OctNode
{
	public static GenericArrayPool<byte> s_ChunkDataPool = new GenericArrayPool<byte>(2000);

	private Block45LODNodeData _nData;

	private Block45ChunkGo _goChunk;

	private byte[] _chkData;

	private int _stamp;

	public static readonly int[,] S_NearNodeOfs = new int[8, 3]
	{
		{ 0, 0, 0 },
		{ 1, 0, 0 },
		{ 0, 1, 0 },
		{ 1, 1, 0 },
		{ 0, 0, 1 },
		{ 1, 0, 1 },
		{ 0, 1, 1 },
		{ 1, 1, 1 }
	};

	public static readonly int S_MinNoDirtyIdx = 1;

	public static readonly int S_MaxNoDirtyIdx = 7;

	public IntVector4 _pos;

	public Block45OctNode _parent;

	public Block45OctNode[] _children;

	private List<BlockVec> _vecData;

	public GameObject cube;

	private GameObject cubeGO;

	private Vector3 Pos => _pos.ToVector3();

	public int LOD => _pos.w;

	public Block45LODNodeData NodeData => _nData;

	public Block45ChunkGo ChunkGo => _goChunk;

	public byte[] DataVT
	{
		get
		{
			if (_chkData == null && VecData != null)
			{
				_chkData = BlockVecs2ByteArray();
			}
			return _chkData;
		}
	}

	public int Signature => _pos.GetHashCode();

	public List<BlockVec> VecData => (_children != null) ? GetVecDataFromChildren() : _vecData;

	public Action<Block45OctNode> OnCreateNode { get; set; }

	public bool IsLeaf => _children == null;

	public int ScaledSize => 1 << _pos.w + 2;

	private Block45OctNode()
	{
	}

	public int GetStamp()
	{
		return (_nData != null) ? ((_nData.StampOfChnkUpdating << 12) | (_stamp & 0xFFF)) : _stamp;
	}

	public bool IsStampIdentical(int stamp)
	{
		return stamp == GetStamp();
	}

	public void AttachLODNode(Block45LODNodeData nData)
	{
		_nData = nData;
		if (_nData != null)
		{
			_nData.AddOctNode(this);
		}
	}

	public void DetachLODNode()
	{
		DestroyChunkGo();
		_nData = null;
	}

	public void AttachChunkGo(Block45ChunkGo b45Go)
	{
		Block45ChunkGo goChunk = _goChunk;
		_goChunk = null;
		if (b45Go != null)
		{
			float num = (float)(1 << _pos.w) * 0.5f;
			_goChunk = b45Go;
			_goChunk.name = "b45Chnk_" + _pos.x + "_" + _pos.y + "_" + _pos.z + "_" + _pos.w;
			_goChunk.transform.localScale = new Vector3(num, num, num);
			_goChunk.transform.localPosition = _pos.ToVector3();
			_goChunk._data = this;
			if (goChunk != null && goChunk._mc != null && goChunk._mc.sharedMesh != null)
			{
				_goChunk.OnSetCollider();
			}
		}
		if (goChunk != null)
		{
			VFGoPool<Block45ChunkGo>.FreeGo(goChunk);
			goChunk = null;
		}
		if (_nData != null)
		{
			_nData.EndUpdateOctNode(this);
		}
	}

	public void DestroyChunkGo()
	{
		if ((object)_goChunk != null)
		{
			if (_nData == null)
			{
				VFGoPool<Block45ChunkGo>.FreeGo(_goChunk);
			}
			else
			{
				VFGoPool<Block45ChunkGo>.ReqFreeGo(_goChunk);
			}
			_goChunk = null;
		}
	}

	public void FreeChkData()
	{
		if (_chkData != null)
		{
			s_ChunkDataPool.Free(_chkData);
			_chkData = null;
		}
	}

	public void Clear()
	{
		DetachLODNode();
		FreeChkData();
		_pos = null;
	}

	public void WriteToByteArray(int x, int y, int z, byte b0, byte b1)
	{
		if (_nData != null && _nData.IsInReq)
		{
			if (_chkData == null)
			{
				_chkData = s_ChunkDataPool.Get();
				Array.Clear(_chkData, 0, _chkData.Length);
			}
			int num = Block45Kernel.OneIndexNoPrefix(x, y, z);
			_chkData[num] = b0;
			_chkData[num + 1] = b1;
			if (_nData.LOD == 0)
			{
				SceneChunkDependence.Instance.ValidListRemove(_nData.ChunkPosLod, EDependChunkType.ChunkBlkMask);
			}
			_nData.AddToBuildList(this);
		}
		else if (_chkData != null)
		{
			int num2 = Block45Kernel.OneIndexNoPrefix(x, y, z);
			_chkData[num2] = b0;
			_chkData[num2 + 1] = b1;
		}
	}

	public static Block45OctNode CreateNode(IntVector4 atpos, Block45OctNode parent)
	{
		Block45OctNode block45OctNode = new Block45OctNode();
		block45OctNode._pos = atpos;
		block45OctNode._parent = parent;
		if (parent != null)
		{
			block45OctNode.OnCreateNode = parent.OnCreateNode;
		}
		if (block45OctNode.OnCreateNode != null)
		{
			block45OctNode.OnCreateNode(block45OctNode);
		}
		return block45OctNode;
	}

	public static Block45OctNode CreateNode(IntVector4 atpos, Action<Block45OctNode> onCreateNode)
	{
		Block45OctNode block45OctNode = new Block45OctNode();
		block45OctNode._pos = atpos;
		block45OctNode._parent = null;
		block45OctNode.OnCreateNode = onCreateNode;
		if (block45OctNode.OnCreateNode != null)
		{
			block45OctNode.OnCreateNode(block45OctNode);
		}
		return block45OctNode;
	}

	public int GetChildStamp()
	{
		if (_children != null)
		{
			int num = 0;
			for (int i = 0; i < 8; i++)
			{
				num += _children[i].GetChildStamp();
			}
			return num;
		}
		return _stamp;
	}

	public List<BlockVec> GetVecDataFromChildren()
	{
		int childStamp = GetChildStamp();
		if (childStamp != _stamp)
		{
			GenVecDataFromChildren(this);
			_stamp = childStamp;
		}
		return _vecData;
	}

	public bool Covers(IntVector4 pos)
	{
		int num = Block45Constants.Size(_pos.w);
		if (_pos.x <= pos.x && _pos.x + num > pos.x && _pos.y <= pos.y && _pos.y + num > pos.y && _pos.z <= pos.z && _pos.z + num > pos.z)
		{
			return true;
		}
		return false;
	}

	public bool IsCenterInside(IntVector3 boundPos, int boundSize)
	{
		int num = Block45Constants.CenterOfs(_pos.w);
		if (boundPos.x <= _pos.x + num && boundPos.x + boundSize >= _pos.x + num && boundPos.y <= _pos.y + num && boundPos.y + boundSize >= _pos.y + num && boundPos.z <= _pos.z + num && boundPos.z + boundSize >= _pos.z + num)
		{
			return true;
		}
		return false;
	}

	public bool IsCenterInside(IntVector3 boundPos, IntVector3 boundSize)
	{
		int num = Block45Constants.CenterOfs(_pos.w);
		if (boundPos.x <= _pos.x + num && boundPos.x + boundSize.x >= _pos.x + num && boundPos.y <= _pos.y + num && boundPos.y + boundSize.y >= _pos.y + num && boundPos.z <= _pos.z + num && boundPos.z + boundSize.z >= _pos.z + num)
		{
			return true;
		}
		return false;
	}

	public bool IsWholeInside(IntVector3 boundPos, IntVector3 boundSize)
	{
		int num = Block45Constants.Size(_pos.w);
		if (boundPos.x <= _pos.x && boundPos.x + boundSize.x >= _pos.x + num && boundPos.y <= _pos.y && boundPos.y + boundSize.y >= _pos.y + num && boundPos.z <= _pos.z && boundPos.z + boundSize.z >= _pos.z + num)
		{
			return true;
		}
		return false;
	}

	public bool IsOverlapped(IntVector3 boundPos, IntVector3 boundSize)
	{
		int num = Block45Constants.Size(_pos.w);
		if (boundPos.x > _pos.x + num || boundPos.x + boundSize.x < _pos.x || boundPos.y > _pos.y + num || boundPos.y + boundSize.y < _pos.y || boundPos.z > _pos.z + num || boundPos.z + boundSize.z < _pos.z)
		{
			return false;
		}
		return true;
	}

	private void DoSplit(int octant, Block45OctNode node)
	{
		_children = new Block45OctNode[8];
		int w = _pos.w;
		int num = ScaledSize >> 1;
		for (int i = 0; i < 8; i++)
		{
			if (i == octant)
			{
				_children[octant] = node;
				continue;
			}
			IntVector4 intVector = new IntVector4(_pos);
			intVector.w = w - 1;
			intVector.x += (i & 1) * num;
			intVector.y += ((i >> 1) & 1) * num;
			intVector.z += ((i >> 2) & 1) * num;
			_children[i] = CreateNode(intVector, this);
		}
	}

	public void Split(int octant = -1, Block45OctNode node = null)
	{
		if (NodeData != null)
		{
			lock (NodeData)
			{
				DoSplit(octant, node);
				return;
			}
		}
		DoSplit(octant, node);
	}

	public Block45OctNode RerootToContainPos(IntVector4 pos)
	{
		if (!Covers(pos))
		{
			int num2;
			int num3;
			int num4;
			if (_pos.w < 4)
			{
				int num = (1 << 2 + _pos.w + 1) - 1;
				num2 = (((_pos.x & num) != 0 && pos.x < _pos.x) ? 1 : 0);
				num3 = (((_pos.y & num) != 0 && pos.y < _pos.y) ? 1 : 0);
				num4 = (((_pos.z & num) != 0 && pos.z < _pos.z) ? 1 : 0);
			}
			else
			{
				num2 = ((pos.x < _pos.x) ? 1 : 0);
				num3 = ((pos.y < _pos.y) ? 1 : 0);
				num4 = ((pos.z < _pos.z) ? 1 : 0);
			}
			int octant = num2 + (num3 << 1) + (num4 << 2);
			IntVector4 atpos = new IntVector4(_pos.x - num2 * ScaledSize, _pos.y - num3 * ScaledSize, _pos.z - num4 * ScaledSize, _pos.w + 1);
			Block45OctNode block45OctNode = (_parent = CreateNode(atpos, OnCreateNode));
			block45OctNode.Split(octant, this);
			return block45OctNode.RerootToContainPos(pos);
		}
		return this;
	}

	public Block45OctNode RerootToLOD(int lod)
	{
		if (LOD < lod)
		{
			int num = (1 << 2 + _pos.w + 1) - 1;
			int num2 = (((_pos.x & num) != 0) ? 1 : 0);
			int num3 = (((_pos.y & num) != 0) ? 1 : 0);
			int num4 = (((_pos.z & num) != 0) ? 1 : 0);
			int octant = num2 + (num3 << 1) + (num4 << 2);
			IntVector4 atpos = new IntVector4(_pos.x - num2 * ScaledSize, _pos.y - num3 * ScaledSize, _pos.z - num4 * ScaledSize, _pos.w + 1);
			Block45OctNode block45OctNode = (_parent = CreateNode(atpos, OnCreateNode));
			block45OctNode.Split(octant, this);
			return block45OctNode.RerootToLOD(lod);
		}
		return this;
	}

	public void Write(int x, int y, int z, byte b0, byte b1)
	{
		x++;
		y++;
		z++;
		if (_vecData != null)
		{
			int i;
			for (i = 0; i < _vecData.Count; i++)
			{
				BlockVec value = _vecData[i];
				if (x == value.x && y == value.y && z == value.z)
				{
					if (b0 == 0)
					{
						_vecData.RemoveAt(i);
						break;
					}
					value._byte0 = b0;
					value._byte1 = b1;
					_vecData[i] = value;
					break;
				}
			}
			if (i >= _vecData.Count && b0 != 0)
			{
				_vecData.Add(new BlockVec(x, y, z, b0, b1));
			}
			_stamp++;
			WriteToByteArray(x, y, z, b0, b1);
		}
		else if (b0 != 0)
		{
			_vecData = new List<BlockVec>();
			_vecData.Add(new BlockVec(x, y, z, b0, b1));
			_stamp++;
			WriteToByteArray(x, y, z, b0, b1);
		}
	}

	public B45Block Read(int x, int y, int z)
	{
		if (_vecData == null)
		{
			return default(B45Block);
		}
		x++;
		y++;
		z++;
		for (int i = 0; i < _vecData.Count; i++)
		{
			BlockVec blockVec = _vecData[i];
			if (x == blockVec.x && y == blockVec.y && z == blockVec.z)
			{
				return new B45Block(blockVec._byte0, blockVec._byte1);
			}
		}
		return default(B45Block);
	}

	public byte[] BlockVecs2ByteArray()
	{
		byte[] array = s_ChunkDataPool.Get();
		Array.Clear(array, 0, array.Length);
		for (int i = 0; i < _vecData.Count; i++)
		{
			BlockVec blockVec = _vecData[i];
			int num = Block45Kernel.OneIndexNoPrefix(blockVec.x, blockVec.y, blockVec.z);
			array[num] = blockVec._byte0;
			array[num + 1] = blockVec._byte1;
		}
		return array;
	}

	public List<BlockVec> ByteArray2BlockVecs()
	{
		List<BlockVec> list = new List<BlockVec>();
		if (_chkData != null)
		{
			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					for (int k = 0; k < 10; k++)
					{
						int num = Block45Kernel.OneIndexNoPrefix(k, j, i);
						if (_chkData[num] != 0)
						{
							list.Add(new BlockVec(k, j, i, _chkData[num], _chkData[num + 1]));
						}
					}
				}
			}
		}
		return list;
	}

	public static Block45OctNode GetNodeRO(IntVector4 poslod, Block45OctNode root)
	{
		if (root == null || !root.Covers(poslod))
		{
			return null;
		}
		if (root._pos.w < poslod.w)
		{
			return null;
		}
		if (root._pos.w == poslod.w)
		{
			return root;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		Block45OctNode block45OctNode = root;
		while (block45OctNode._pos.w > poslod.w)
		{
			if (block45OctNode.IsLeaf)
			{
				return null;
			}
			int num5 = Block45Constants.CenterOfs(block45OctNode._pos.w);
			num2 = block45OctNode._pos.x + num5;
			num3 = block45OctNode._pos.y + num5;
			num4 = block45OctNode._pos.z + num5;
			num = ((poslod.x >= num2) ? 1 : 0) | ((poslod.y >= num3) ? 2 : 0) | ((poslod.z >= num4) ? 4 : 0);
			block45OctNode = block45OctNode._children[num];
		}
		return block45OctNode;
	}

	public static Block45OctNode GetNodeRW(IntVector4 poslod, ref Block45OctNode root)
	{
		root = root.RerootToContainPos(poslod);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		Block45OctNode block45OctNode = root;
		while (block45OctNode._pos.w > poslod.w)
		{
			if (block45OctNode.IsLeaf)
			{
				block45OctNode.Split();
			}
			int num5 = Block45Constants.CenterOfs(block45OctNode._pos.w);
			num2 = block45OctNode._pos.x + num5;
			num3 = block45OctNode._pos.y + num5;
			num4 = block45OctNode._pos.z + num5;
			num = ((poslod.x >= num2) ? 1 : 0) | ((poslod.y >= num3) ? 2 : 0) | ((poslod.z >= num4) ? 4 : 0);
			block45OctNode = block45OctNode._children[num];
		}
		return block45OctNode;
	}

	public static void SplitAt(Block45OctNode root, IntVector3 atpos, int lod)
	{
		int num = 0;
		Block45OctNode block45OctNode = root;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < lod; i++)
		{
			int num5 = Block45Constants.CenterOfs(block45OctNode._pos.w);
			num2 = block45OctNode._pos.x + num5;
			num3 = block45OctNode._pos.y + num5;
			num4 = block45OctNode._pos.z + num5;
			num = ((atpos.x > num2) ? 1 : 0) | ((atpos.y > num3) ? 2 : 0) | ((atpos.z > num4) ? 4 : 0);
			if (block45OctNode.IsLeaf)
			{
				block45OctNode.Split();
			}
			block45OctNode = block45OctNode._children[num];
		}
	}

	public static void Merge(Block45OctNode node)
	{
		if (!node.IsLeaf)
		{
			for (int i = 0; i < 8; i++)
			{
				if (node._children[i] != null)
				{
					Merge(node._children[i]);
				}
				node._children[i] = null;
			}
		}
		node._children = null;
	}

	public static void FindNodesCenterInside(IntVector3 boundPos, int boundSize, int lod, Block45OctNode root, ref List<Block45OctNode> outNodesList)
	{
		if (root._pos.w == lod)
		{
			if (root.IsCenterInside(boundPos, boundSize))
			{
				outNodesList.Add(root);
			}
		}
		else if (root._pos.w > lod && !root.IsLeaf)
		{
			for (int i = 0; i < 8; i++)
			{
				FindNodesCenterInside(boundPos, boundSize, lod, root._children[i], ref outNodesList);
			}
		}
	}

	public static void Clear(Block45OctNode node)
	{
		if (node._vecData != null)
		{
			node._vecData = null;
		}
		node.FreeChkData();
		if (node._goChunk != null)
		{
			VFGoPool<Block45ChunkGo>.FreeGo(node._goChunk);
			node._goChunk = null;
		}
		if (!node.IsLeaf)
		{
			for (int i = 0; i < 8; i++)
			{
				Clear(node._children[i]);
			}
		}
	}

	public static void GenVecDataFromChildren(Block45OctNode node)
	{
		Dictionary<int, BlockVec> dictionary = new Dictionary<int, BlockVec>();
		for (int i = 0; i < 8; i++)
		{
			if (node._children[i]._vecData == null && !node._children[i].IsLeaf)
			{
				GenVecDataFromChildren(node._children[i]);
			}
			List<BlockVec> vecData = node._children[i]._vecData;
			if (vecData != null)
			{
				int num = BlockVec.ToXYZ(S_NearNodeOfs[i, 0] << 3, S_NearNodeOfs[i, 1] << 3, S_NearNodeOfs[i, 2] << 3);
				int count = vecData.Count;
				for (int j = 0; j < count; j++)
				{
					int num2 = (vecData[j]._xyz + num >> 1) & 0x7F7F7F;
					dictionary[num2] = new BlockVec(num2, vecData[j]._byte0, vecData[j]._byte1);
				}
			}
		}
		node._vecData = new List<BlockVec>(dictionary.Values);
		node.FreeChkData();
	}

	public void MakeCube()
	{
		int scaledSize = ScaledSize;
		int num = scaledSize >> 1;
		Vector3 vector = _pos.ToVector3();
		vector.x += num;
		vector.y += num;
		vector.z += num;
		if (cubeGO == null)
		{
		}
		cubeGO.transform.localScale = new Vector3(scaledSize, scaledSize, scaledSize);
	}

	public void RemoveCube()
	{
		if (cubeGO != null)
		{
			UnityEngine.Object.DestroyImmediate(cubeGO);
			cubeGO = null;
		}
	}

	public static void MakeCubeRec(Block45OctNode node)
	{
		node.MakeCube();
		if (!node.IsLeaf)
		{
			for (int i = 0; i < 8; i++)
			{
				MakeCubeRec(node._children[i]);
			}
		}
	}
}
