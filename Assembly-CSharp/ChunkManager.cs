using System;
using System.Collections.Generic;
using uLink;
using UnityEngine;

public class ChunkManager : UnityEngine.MonoBehaviour
{
	private static ChunkManager _instance;

	internal Dictionary<IntVector3, Dictionary<IntVector3, VFVoxel>> VoxelVolumes = new Dictionary<IntVector3, Dictionary<IntVector3, VFVoxel>>();

	internal Dictionary<int, Dictionary<IntVector3, B45Block>> _areaBlockList = new Dictionary<int, Dictionary<IntVector3, B45Block>>();

	public static ChunkManager Instance => _instance;

	private void Awake()
	{
		_instance = this;
	}

	public void AddCacheReq(Vector3 pos)
	{
		if (VoxelVolumes.ContainsKey(new IntVector3(pos)))
		{
			ActionDelegate action = new ActionDelegate(this, OnChunkAddedEvent, new ChunkEventArgs(pos));
			ActionManager.AddAction(action);
		}
	}

	private void OnChunkAddedEvent(object sender, EventArgs args)
	{
		if (null == VFVoxelTerrain.self)
		{
			return;
		}
		ChunkEventArgs chunkEventArgs = args as ChunkEventArgs;
		IntVector3 key = new IntVector3(chunkEventArgs._pos.x, chunkEventArgs._pos.y, chunkEventArgs._pos.z);
		if (!VoxelVolumes.ContainsKey(key))
		{
			return;
		}
		foreach (KeyValuePair<IntVector3, VFVoxel> item in VoxelVolumes[key])
		{
			VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(item.Key.x, item.Key.y, item.Key.z);
			voxel.Volume = item.Value.Volume;
			voxel.Type = item.Value.Type;
			VFVoxelTerrain.self.AlterVoxelInBuild(item.Key, voxel);
		}
	}

	internal static void Clear()
	{
		if (!(Instance == null))
		{
			Instance.VoxelVolumes.Clear();
			Instance._areaBlockList.Clear();
		}
	}

	public static void ApplyVoxelVolume(IntVector3 digPos, IntVector3 chunkPos, VFVoxel voxel)
	{
		if (!Instance.VoxelVolumes.ContainsKey(chunkPos))
		{
			Instance.VoxelVolumes[chunkPos] = new Dictionary<IntVector3, VFVoxel>();
		}
		Instance.VoxelVolumes[chunkPos][digPos] = new VFVoxel(voxel.Volume, voxel.Type);
	}

	public static void RPC_S2C_BlockDestroyInRange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] data = stream.Read<byte[]>(new object[0]);
		DigTerrainManager.BlockDestroyInRangeNetReturn(data);
	}

	public static void RPC_S2C_TerrainDestroyInRange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		float power = stream.Read<float>(new object[0]);
		float radius = stream.Read<float>(new object[0]);
		DigTerrainManager.TerrainDestroyInRangeNetReturn(pos, power, radius);
	}

	public static void RPC_S2C_VoxelData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<int>(new object[0]);
		byte[] data = stream.Read<byte[]>(new object[0]);
		DigTerrainManager.ApplyVoxelData(data);
	}

	public static void RPC_S2C_BlockData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.Read<int>(new object[0]);
		byte[] data = stream.Read<byte[]>(new object[0]);
		DigTerrainManager.ApplyBlockData(data);
	}

	public static void RPC_S2C_BuildBlock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] binData = stream.Read<byte[]>(new object[0]);
		DigTerrainManager.ApplyBSVoxelData(binData);
	}

	public static void RPC_SKDigTerrain(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		IntVector3 pos = stream.Read<IntVector3>(new object[0]);
		float durDec = stream.Read<float>(new object[0]);
		float radius = stream.Read<float>(new object[0]);
		float height = stream.Read<float>(new object[0]);
		bool bReturnItem = stream.Read<bool>(new object[0]);
		DigTerrainManager.DigTerrainNetReturn(pos, durDec, radius, height, bReturnItem);
	}

	public static void RPC_SKChangeTerrain(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		IntVector3 intPos = stream.Read<IntVector3>(new object[0]);
		float radius = stream.Read<float>(new object[0]);
		byte targetType = stream.Read<byte>(new object[0]);
		byte[] data = stream.Read<byte[]>(new object[0]);
		DigTerrainManager.ChangeTerrainNetReturn(intPos, radius, targetType, data);
	}
}
