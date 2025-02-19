using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Transvoxel.Lengyel;
using Transvoxel.SurfaceExtractor;
using UnityEngine;

public class TransvoxelGoCreator
{
	private Dictionary<IntVector4, VFVoxelChunkData> transRebuildList = new Dictionary<IntVector4, VFVoxelChunkData>();

	private Thread threadBuildTransVoxel;

	private IVxSurfExtractor surfExtractorTrans = new SurfExtractorTrans();

	private bool bTransVoxelEnabled;

	public ETransBuildStatus curStatus;

	public bool IsTransvoxelEnabled
	{
		get
		{
			return bTransVoxelEnabled;
		}
		set
		{
			bTransVoxelEnabled = value;
			if (!bTransVoxelEnabled)
			{
				ClearAllTrans();
				threadBuildTransVoxel = null;
				return;
			}
			PrepChunkList();
			if (threadBuildTransVoxel == null)
			{
				threadBuildTransVoxel = new Thread(ThreadBuildTransExec);
				threadBuildTransVoxel.Start();
			}
		}
	}

	public bool PrepChunkList()
	{
		if (curStatus == ETransBuildStatus.Status_Idle)
		{
			transRebuildList.Clear();
			Transform transform = VFVoxelTerrain.self.transform;
			foreach (Transform item in transform)
			{
				if (item.gameObject.activeSelf)
				{
					try
					{
						VFVoxelChunkGo component = item.GetComponent<VFVoxelChunkGo>();
						VFVoxelChunkData data = component.Data;
						transRebuildList.Add(data.ChunkPosLod, data);
					}
					catch
					{
					}
				}
			}
			curStatus = ETransBuildStatus.Status_ToBuild;
			return true;
		}
		return false;
	}

	public void ClearAllTrans()
	{
		Transform transform = VFVoxelTerrain.self.transform;
		foreach (Transform item in transform)
		{
			VFVoxelChunkGo component = item.GetComponent<VFVoxelChunkGo>();
			if (component != null && component.TransvoxelGo != null)
			{
				VFGoPool<VFTransVoxelGo>.FreeGo(component.TransvoxelGo);
				component.TransvoxelGo = null;
			}
		}
	}

	public void UpdateTransMesh()
	{
		if (bTransVoxelEnabled && curStatus == ETransBuildStatus.Status_FinBuild && surfExtractorTrans.OnFin() == 0)
		{
			curStatus = ETransBuildStatus.Status_Idle;
		}
	}

	public void ThreadBuildTransExec()
	{
		surfExtractorTrans.Init();
		List<IntVector4> list = new List<IntVector4>();
		List<VFVoxelChunkData> list2 = new List<VFVoxelChunkData>();
		while (bTransVoxelEnabled)
		{
			if (curStatus == ETransBuildStatus.Status_ToBuild)
			{
				list = transRebuildList.Keys.ToList();
				list2 = transRebuildList.Values.ToList();
				for (int i = 0; i < list2.Count; i++)
				{
					VFVoxelChunkData vFVoxelChunkData = list2[i];
					if (vFVoxelChunkData.LOD == LODOctreeMan._maxLod || (object)vFVoxelChunkData.ChunkGo == null)
					{
						continue;
					}
					int num = (((object)vFVoxelChunkData.ChunkGo.TransvoxelGo != null) ? vFVoxelChunkData.ChunkGo.TransvoxelGo._faceMask : 0);
					int num2 = 0;
					int lOD = vFVoxelChunkData.LOD;
					int num3 = -1 << lOD + 1;
					for (int j = 0; j < Tables.TransitionFaceDir.Length; j++)
					{
						IntVector3 intVector = Tables.TransitionFaceDir[j];
						IntVector4 item = new IntVector4((vFVoxelChunkData.ChunkPosLod.x + (intVector.x << lOD)) & num3, (vFVoxelChunkData.ChunkPosLod.y + (intVector.y << lOD)) & num3, (vFVoxelChunkData.ChunkPosLod.z + (intVector.z << lOD)) & num3, lOD + 1);
						if (list.Contains(item))
						{
							num2 |= 1 << (j & 0x1F);
						}
					}
					if (num2 != num)
					{
						surfExtractorTrans.AddSurfExtractReq(new SurfExtractReqTrans(num2, vFVoxelChunkData));
					}
				}
				surfExtractorTrans.Exec();
				curStatus = ETransBuildStatus.Status_FinBuild;
			}
			Thread.Sleep(16);
		}
		curStatus = ETransBuildStatus.Status_Idle;
	}

	public static int UnindexedVertex(TransVertices verts, List<int> indices, out Vector3[] vert, out Vector2[] norm01, out Vector2[] norm2t)
	{
		int count = indices.Count;
		vert = new Vector3[count];
		norm01 = new Vector2[count];
		norm2t = new Vector2[count];
		int num = 0;
		while (num < count)
		{
			int index = indices[num];
			ref Vector3 reference = ref vert[num];
			reference = verts.Position[index];
			Vector4 vector = verts.Normal_t[index];
			ref Vector2 reference2 = ref norm01[num];
			reference2 = new Vector2(0f - vector.x, 0f - vector.y);
			ref Vector2 reference3 = ref norm2t[num];
			reference3 = new Vector2(vector.z, 0f);
			float w = vector.w;
			num++;
			index = indices[num];
			ref Vector3 reference4 = ref vert[num];
			reference4 = verts.Position[index];
			vector = verts.Normal_t[index];
			ref Vector2 reference5 = ref norm01[num];
			reference5 = new Vector2(0f - vector.x, 0f - vector.y);
			ref Vector2 reference6 = ref norm2t[num];
			reference6 = new Vector2(vector.z, 0.1f);
			float w2 = vector.w;
			num++;
			index = indices[num];
			ref Vector3 reference7 = ref vert[num];
			reference7 = verts.Position[index];
			vector = verts.Normal_t[index];
			ref Vector2 reference8 = ref norm01[num];
			reference8 = new Vector2(0f - vector.x, 0f - vector.y);
			ref Vector2 reference9 = ref norm2t[num];
			reference9 = new Vector2(vector.z, 0.2f);
			float w3 = vector.w;
			num++;
			norm2t[num - 1].x += w * 4f + 2f;
			norm2t[num - 1].y += w2 * 256f + w3;
			norm2t[num - 2].x += w * 4f + 2f;
			norm2t[num - 2].y += w2 * 256f + w3;
			norm2t[num - 3].x += w * 4f + 2f;
			norm2t[num - 3].y += w2 * 256f + w3;
		}
		return count;
	}

	public static void CreateTransvoxelGo(VFVoxelChunkGo chunkGo, int faceMask)
	{
		TransVertices verts = new TransVertices();
		List<int> indices = new List<int>();
		float cellSize = 0.01f;
		TransvoxelExtractor2.BuildTransitionCells(faceMask, chunkGo.Data, cellSize, verts, indices);
		Vector3[] vert;
		Vector2[] norm;
		Vector2[] norm2t;
		int num = UnindexedVertex(verts, indices, out vert, out norm, out norm2t);
		SurfExtractReqTrans surfExtractReqTrans = new SurfExtractReqTrans(0, null);
		surfExtractReqTrans.vert = vert;
		surfExtractReqTrans.norm01 = norm;
		surfExtractReqTrans.norm2t = norm2t;
		surfExtractReqTrans.indice = new int[num];
		Array.Copy(SurfExtractorsMan.s_indiceMax, surfExtractReqTrans.indice, num);
		chunkGo.SetTransGo(surfExtractReqTrans, faceMask);
	}
}
