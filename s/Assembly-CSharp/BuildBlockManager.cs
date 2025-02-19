using UnityEngine;

public class BuildBlockManager : MonoBehaviour
{
	public const int c_MinItemProtoID = 268;

	public const int c_MaxItemProtoID = 282;

	public static int GetItemIDByMatIndex(int matIndex)
	{
		return matIndex + 1 + 30200000;
	}

	public static int GetBlockItemProtoID(byte matIndex)
	{
		return matIndex + 268;
	}

	public static int GetVoxelItemProtoID(byte matIndex)
	{
		return BSVoxelMatMap.GetItemID(matIndex);
	}
}
