using UnityEngine;

public class LODOctree
{
	private LODOctreeMan _parentMan;

	private IntVector3 _idxPos;

	private IntVector3 _pos;

	public LODOctreeNode _root;

	private Vector3 _rootCenter;

	private int _rootHalfLen;

	public LODOctree(LODOctreeMan man, int maxlod, IntVector3 idxPos, IntVector3 sideLen)
	{
		_parentMan = man;
		_idxPos = idxPos;
		_pos = idxPos * sideLen;
		Vector3 initPos = LODOctreeMan.InitPos;
		int posx = (int)initPos.x;
		Vector3 initPos2 = LODOctreeMan.InitPos;
		int posy = (int)initPos2.y;
		Vector3 initPos3 = LODOctreeMan.InitPos;
		_root = new LODOctreeNode(null, maxlod, posx, posy, (int)initPos3.z);
		_rootHalfLen = LODOctreeNode._halfLens[_root.Lod];
		_rootCenter = _root.Center;
	}

	public void OnDestroy()
	{
		if (_root != null)
		{
			LODOctreeNode.DestroyNodeData(_root);
		}
	}

	public bool RefreshPos(Vector3 camPos)
	{
		if (!_parentMan._viewBounds.Contains(_rootCenter))
		{
			Vector3 vector = camPos + _parentMan._viewBounds.extents;
			vector.x -= _pos.x + _rootHalfLen;
			vector.y -= _pos.y + _rootHalfLen;
			vector.z -= _pos.z + _rootHalfLen;
			int newPosx = Mathf.FloorToInt(vector.x / (float)_parentMan._viewBoundsSize.x) * _parentMan._viewBoundsSize.x + _pos.x;
			int newPosy = Mathf.FloorToInt(vector.y / (float)_parentMan._viewBoundsSize.y) * _parentMan._viewBoundsSize.y + _pos.y;
			int newPosz = Mathf.FloorToInt(vector.z / (float)_parentMan._viewBoundsSize.z) * _parentMan._viewBoundsSize.z + _pos.z;
			_root.Reposition(newPosx, newPosy, newPosz);
			_rootCenter = _root.Center;
			return true;
		}
		return false;
	}

	public void FillTreeNodeArray(ref LODOctreeNode[][,,] lodTreeNodes)
	{
		TraverseFillTreeNodeArray(_root, ref lodTreeNodes, _idxPos);
	}

	private void TraverseFillTreeNodeArray(LODOctreeNode node, ref LODOctreeNode[][,,] lodTreeNodes, IntVector3 startIdx)
	{
		lodTreeNodes[node.Lod][startIdx.x, startIdx.y, startIdx.z] = node;
		if (node.Lod > 0)
		{
			for (int i = 0; i < 8; i++)
			{
				IntVector3 startIdx2 = new IntVector3((startIdx.x << 1) + (i & 1), (startIdx.y << 1) + ((i >> 1) & 1), (startIdx.z << 1) + ((i >> 2) & 1));
				TraverseFillTreeNodeArray(node._child[i], ref lodTreeNodes, startIdx2);
			}
		}
	}
}
