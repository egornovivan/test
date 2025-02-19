using UnityEngine;

public static class RSubTerrConstant
{
	public static int ChunkSize = 64;

	public static float ChunkSizeF = ChunkSize;

	public static int ChunkHeight = 512;

	public static float ChunkHeightF = ChunkHeight;

	public static IntVec3 ChunkCountPerAxis = new IntVec3(9f, 1f, 9f);

	public static Vector3 TerrainSize = new Vector3(ChunkSize * ChunkCountPerAxis.x, ChunkHeightF, ChunkSize * ChunkCountPerAxis.z);
}
