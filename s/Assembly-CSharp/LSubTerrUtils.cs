using UnityEngine;

public static class LSubTerrUtils
{
	public static int PosToIndex(int x_index, int z_index)
	{
		return z_index * 72 + x_index;
	}

	public static int WorldPosToIndex(Vector3 wpos)
	{
		return (int)(wpos.z / 256f) * 72 + (int)(wpos.x / 256f);
	}

	public static IntVector3 IndexToPos(int index)
	{
		return new IntVector3(index % 72, 0, index / 72);
	}

	public static Vector3 IndexToWorldPos(int index)
	{
		return new IntVector3(index % 72, 0, index / 72).ToVector3() * 256f;
	}

	public static IntVector3 WorldPosToPos(Vector3 wpos)
	{
		return new IntVector3((int)(wpos.x / 256f), 0, (int)(wpos.z / 256f));
	}

	public static Vector3 PosToWorldPos(IntVector3 ipos)
	{
		return ipos.ToVector3() * 256f;
	}

	public static int TreePosToKey(IntVector3 pos)
	{
		return (pos.x + 128) | (pos.y << 18) | (pos.z + 128 << 9);
	}

	public static IntVector3 KeyToTreePos(int key)
	{
		return new IntVector3((key & 0x1FF) - 128, key >> 18, ((key >> 9) & 0x1FF) - 128);
	}

	public static int TreeWorldPosTo32Key(Vector3 tree_world_pos)
	{
		return Mathf.FloorToInt(tree_world_pos.x / 32f) | (Mathf.FloorToInt(tree_world_pos.z / 32f) << 16);
	}

	public static IntVector3 Tree32KeyTo32Pos(int _32key)
	{
		return new IntVector3(_32key & 0xFFFF, 0, _32key >> 16);
	}

	public static int Tree32PosTo32Key(int x, int z)
	{
		return x | (z << 16);
	}

	public static Vector3 TreeTerrainPosToWorldPos(int tx, int tz, Vector3 tpos)
	{
		Vector3 result = tpos;
		result.x *= 256f;
		result.y *= 3000f;
		result.z *= 256f;
		result.x += (float)tx * 256f;
		result.z += (float)tz * 256f;
		return result;
	}

	public static Vector3 TreeWorldPosToTerrainPos(Vector3 wpos)
	{
		wpos.x /= 256f;
		wpos.y /= 3000f;
		wpos.z /= 256f;
		wpos.x -= Mathf.Floor(wpos.x);
		wpos.z -= Mathf.Floor(wpos.z);
		return wpos;
	}
}
