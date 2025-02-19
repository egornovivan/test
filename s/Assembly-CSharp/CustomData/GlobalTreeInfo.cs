using uLink;
using UnityEngine;

namespace CustomData;

public class GlobalTreeInfo
{
	public int _terrainIndex;

	public TreeInfo _treeInfo;

	public Vector3 WorldPos
	{
		get
		{
			IntVector3 intVector = LSubTerrUtils.IndexToPos(_terrainIndex);
			return LSubTerrUtils.TreeTerrainPosToWorldPos(intVector.x, intVector.z, _treeInfo.m_pos);
		}
	}

	public GlobalTreeInfo(int index, TreeInfo treeinfo)
	{
		_terrainIndex = index;
		_treeInfo = treeinfo;
	}

	public GlobalTreeInfo(int xindex, int zindex, TreeInfo treeinfo)
	{
		_terrainIndex = LSubTerrUtils.PosToIndex(xindex, zindex);
		_treeInfo = treeinfo;
	}

	public GlobalTreeInfo()
	{
		_treeInfo = new TreeInfo();
	}

	public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
	{
		GlobalTreeInfo globalTreeInfo = new GlobalTreeInfo();
		globalTreeInfo._terrainIndex = stream.Read<int>(new object[0]);
		globalTreeInfo._treeInfo.m_clr = stream.Read<Color>(new object[0]);
		globalTreeInfo._treeInfo.m_heightScale = stream.Read<float>(new object[0]);
		globalTreeInfo._treeInfo.m_pos = stream.Read<Vector3>(new object[0]);
		globalTreeInfo._treeInfo.m_protoTypeIdx = stream.Read<int>(new object[0]);
		globalTreeInfo._treeInfo.m_widthScale = stream.Read<float>(new object[0]);
		globalTreeInfo._treeInfo.m_lightMapClr = stream.Read<Color>(new object[0]);
		return globalTreeInfo;
	}

	public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		GlobalTreeInfo globalTreeInfo = (GlobalTreeInfo)value;
		stream.Write(globalTreeInfo._terrainIndex);
		stream.Write(globalTreeInfo._treeInfo.m_clr);
		stream.Write(globalTreeInfo._treeInfo.m_heightScale);
		stream.Write(globalTreeInfo._treeInfo.m_pos);
		stream.Write(globalTreeInfo._treeInfo.m_protoTypeIdx);
		stream.Write(globalTreeInfo._treeInfo.m_widthScale);
		stream.Write(globalTreeInfo._treeInfo.m_lightMapClr);
	}
}
