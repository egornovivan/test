using System.Collections.Generic;
using uLink;
using UnityEngine;

namespace CustomData;

public class BuildVoxel
{
	public float duration;

	public int itemIDOrRadius;

	public byte opType;

	public bool dragHeight;

	public Vector3 chunkPos;

	public List<Vector3> buildPos;

	public static void WriteBuildVoxel(uLink.BitStream stream, object obj, params object[] codecOptions)
	{
		BuildVoxel buildVoxel = obj as BuildVoxel;
		stream.Write(buildVoxel.chunkPos);
		stream.Write(buildVoxel.duration);
		stream.Write(buildVoxel.itemIDOrRadius);
		stream.Write(buildVoxel.opType);
		stream.Write(buildVoxel.dragHeight);
		stream.Write(buildVoxel.buildPos.ToArray());
	}

	public static object ReadBuildVoxel(uLink.BitStream stream, params object[] codecOptions)
	{
		BuildVoxel buildVoxel = new BuildVoxel();
		buildVoxel.buildPos = new List<Vector3>();
		buildVoxel.chunkPos = stream.Read<Vector3>(new object[0]);
		buildVoxel.duration = stream.Read<float>(new object[0]);
		buildVoxel.itemIDOrRadius = stream.Read<int>(new object[0]);
		buildVoxel.opType = stream.Read<byte>(new object[0]);
		buildVoxel.dragHeight = stream.Read<bool>(new object[0]);
		Vector3[] collection = stream.Read<Vector3[]>(new object[0]);
		buildVoxel.buildPos.AddRange(collection);
		return buildVoxel;
	}
}
