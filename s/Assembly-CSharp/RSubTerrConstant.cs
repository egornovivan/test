using UnityEngine;

public static class RSubTerrConstant
{
	public static int ChunkSize = 32;

	public static int ChunkSizeShift = 5;

	public static float ChunkSizeF = ChunkSize;

	public static Vector3 TerrainSize = new Vector3(288f, 512f, 288f);

	public static IntVec3 ChunkCountPerAxis = new IntVec3(9f, 1f, 9f);

	public static int ChunkHeight = (int)TerrainSize.y;

	public static float ChunkHeightF = TerrainSize.y;
}
