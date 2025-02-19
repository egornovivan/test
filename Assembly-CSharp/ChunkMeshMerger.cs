using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkMeshMerger
{
	public struct MeshStruct
	{
		public Vector3[] vertices;

		public Vector3[] normals;

		public Vector2[] uv;

		public int[] triangles;
	}

	private List<MeshStruct> reorganizedMeshes;

	private int curMeshIdx;

	private int destBufferIdx;

	public void Init()
	{
		curMeshIdx = 0;
		destBufferIdx = 0;
		reorganizedMeshes = new List<MeshStruct>();
		addNewMesh();
	}

	public List<MeshStruct> GetReorganizedMeshes()
	{
		return reorganizedMeshes;
	}

	private void addNewMesh()
	{
		MeshStruct item = default(MeshStruct);
		item.vertices = new Vector3[64998];
		item.uv = new Vector2[64998];
		item.triangles = new int[64998];
		item.normals = new Vector3[64998];
		Array.Copy(Block45Kernel.indicesConst, 0, item.triangles, 0, 64998);
		reorganizedMeshes.Add(item);
	}

	public void Merge(List<IntVector4> chunkPosList, List<B45ChunkData> chunks, uint numChunks, List<Mesh> meshList)
	{
	}

	public void truncateLastMesh()
	{
		MeshStruct value = reorganizedMeshes[curMeshIdx];
		Vector3[] array = new Vector3[destBufferIdx];
		Vector2[] array2 = new Vector2[destBufferIdx];
		Vector3[] array3 = new Vector3[destBufferIdx];
		int[] array4 = new int[destBufferIdx];
		Array.Copy(value.vertices, 0, array, 0, destBufferIdx);
		Array.Copy(value.uv, 0, array2, 0, destBufferIdx);
		Array.Copy(value.triangles, 0, array4, 0, destBufferIdx);
		Array.Copy(value.normals, 0, array3, 0, destBufferIdx);
		value.vertices = array;
		value.uv = array2;
		value.triangles = array4;
		value.normals = array3;
		reorganizedMeshes[curMeshIdx] = value;
	}

	private void copy(MeshStruct src, int srcOfs, MeshStruct dest, int destOfs, int length, IntVector3 chunkpos)
	{
		Vector3 vector = new Vector3(chunkpos.x << 3, chunkpos.y << 3, chunkpos.z << 3);
		for (int i = 0; i < length; i++)
		{
			ref Vector3 reference = ref dest.vertices[destOfs + i];
			reference = src.vertices[srcOfs + i] + vector;
		}
		Array.Copy(src.uv, srcOfs, dest.uv, destOfs, length);
		Array.Copy(src.normals, srcOfs, dest.normals, destOfs, length);
	}
}
