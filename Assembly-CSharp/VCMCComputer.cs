using System;
using UnityEngine;
using UnityEngine.Rendering;
using WhiteCat;

public class VCMCComputer : IVxChunkHelperProc
{
	private const int CHUNK_VOXEL_NUM = 32;

	private VFCreationDataSource m_DataSource;

	public VCMeshMgr m_MeshMgr;

	public int m_OutputLayer = VCConfig.s_EditorLayer;

	public bool m_ForEditor = true;

	public bool m_CreateBoxCollider;

	private static int SN;

	public bool Computing => !SurfExtractorsMan.VxSurfExtractor.IsAllClear;

	public IVxSurfExtractor SurfExtractor => SurfExtractorsMan.VxSurfExtractor;

	public int ChunkSig => 2;

	public void Init(IntVector3 editor_size, VCMeshMgr mesh_mgr, bool for_editor = true)
	{
		m_ForEditor = for_editor;
		int num = Mathf.CeilToInt((float)(editor_size.x + 1) / 32f);
		int num2 = Mathf.CeilToInt((float)(editor_size.y + 1) / 32f);
		int num3 = Mathf.CeilToInt((float)(editor_size.z + 1) / 32f);
		m_DataSource = new VFCreationDataSource(num, num2, num3);
		SN++;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				for (int k = 0; k < num3; k++)
				{
					VFVoxelChunkData vFVoxelChunkData = new VFVoxelChunkData(null, VFVoxelChunkData.S_ChunkDataAir);
					vFVoxelChunkData.HelperProc = this;
					IntVector3 intVector = PackSNToChunkPos(new IntVector3(i, j, k), SN);
					m_DataSource.writeChunk(intVector.x, intVector.y, intVector.z, vFVoxelChunkData);
				}
			}
		}
		m_MeshMgr = mesh_mgr;
	}

	public void InitClone(VFCreationDataSource dataSource, VCMeshMgr mesh_mgr, bool for_editor = true)
	{
		m_ForEditor = for_editor;
		m_DataSource = dataSource;
		m_MeshMgr = mesh_mgr;
	}

	public void Init(IntVector3 editor_size, VCMeshMgr mesh_mgr, bool for_editor, int meshIndex)
	{
		m_ForEditor = for_editor;
		int num = Mathf.CeilToInt((float)(editor_size.x + 1) / 32f);
		int num2 = Mathf.CeilToInt((float)(editor_size.y + 1) / 32f);
		int num3 = Mathf.CeilToInt((float)(editor_size.z + 1) / 32f);
		m_DataSource = new VFCreationDataSource(num, num2, num3);
		SN++;
		int num4 = Mathf.CeilToInt(0.5f * (float)num);
		int num5 = (mesh_mgr.m_DaggerMesh ? ((!mesh_mgr.m_LeftSidePos) ? num4 : 0) : 0);
		int num6 = ((!mesh_mgr.m_DaggerMesh) ? num : ((!mesh_mgr.m_LeftSidePos) ? num : num4));
		for (int i = num5; i < num6; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				for (int k = 0; k < num3; k++)
				{
					VFVoxelChunkData vFVoxelChunkData = new VFVoxelChunkData(null, VFVoxelChunkData.S_ChunkDataAir);
					vFVoxelChunkData.HelperProc = this;
					IntVector3 intVector = PackSNToChunkPos(new IntVector3(i, j, k), SN);
					m_DataSource.writeChunk(intVector.x, intVector.y, intVector.z, vFVoxelChunkData);
				}
			}
		}
		m_MeshMgr = mesh_mgr;
	}

	public void Clear()
	{
		if (m_DataSource == null)
		{
			return;
		}
		for (int i = 0; i < m_DataSource.ChunkNum.x; i++)
		{
			for (int j = 0; j < m_DataSource.ChunkNum.y; j++)
			{
				for (int k = 0; k < m_DataSource.ChunkNum.z; k++)
				{
					VFVoxelChunkData vFVoxelChunkData = m_DataSource.readChunk(i, j, k);
					if (vFVoxelChunkData != null)
					{
						vFVoxelChunkData.ClearMem();
						vFVoxelChunkData.SetDataVT(new byte[2]);
					}
				}
			}
		}
	}

	public void Destroy()
	{
		if (m_DataSource != null)
		{
			for (int i = 0; i < m_DataSource.ChunkNum.x; i++)
			{
				for (int j = 0; j < m_DataSource.ChunkNum.y; j++)
				{
					for (int k = 0; k < m_DataSource.ChunkNum.z; k++)
					{
						m_DataSource.readChunk(i, j, k)?.ClearMem();
					}
				}
			}
			m_DataSource = null;
		}
		m_MeshMgr = null;
	}

	public void AlterVoxel(int poskey, VCVoxel voxel)
	{
		IntVector3 intVector = VCIsoData.KeyToIPos(poskey);
		m_DataSource.Write(intVector.x + 1, intVector.y + 1, intVector.z + 1, new VFVoxel(voxel.Volume, voxel.Type));
	}

	public void AlterVoxel(int x, int y, int z, VCVoxel voxel)
	{
		m_DataSource.Write(x + 1, y + 1, z + 1, new VFVoxel(voxel.Volume, voxel.Type));
	}

	public bool InSide(int poskey, int xsize, bool left)
	{
		IntVector3 intVector = VCIsoData.KeyToIPos(poskey);
		int num = Mathf.CeilToInt(0.5f * (float)xsize);
		return (!left) ? (intVector.x > num) : (intVector.x <= num);
	}

	public void ReqMesh()
	{
		if (m_MeshMgr != null)
		{
			m_DataSource.SubmitReq();
		}
	}

	public ILODNodeData CreateLODNodeData(LODOctreeNode node)
	{
		return null;
	}

	public void ChunkProcPreSetDataVT(ILODNodeData ndata, byte[] data, bool bFromPool)
	{
	}

	public void ChunkProcPreLoadData(ILODNodeData ndata)
	{
	}

	public bool ChunkProcExtractData(ILODNodeData ndata)
	{
		VFVoxelChunkData vFVoxelChunkData = ndata as VFVoxelChunkData;
		byte b = vFVoxelChunkData.DataVT[0];
		byte b2 = vFVoxelChunkData.DataVT[1];
		byte[] array = VFVoxelChunkData.s_ChunkDataPool.Get();
		if (b != 0)
		{
			int num = 0;
			while (num < 85750)
			{
				array[num++] = b;
				array[num++] = b2;
			}
		}
		else
		{
			Array.Clear(array, 0, 85750);
		}
		vFVoxelChunkData.SetDataVT(array, bFromPool: true);
		return true;
	}

	public VFVoxel ChunkProcExtractData(ILODNodeData ndata, int x, int y, int z)
	{
		VFVoxelChunkData vFVoxelChunkData = ndata as VFVoxelChunkData;
		return new VFVoxel(vFVoxelChunkData.DataVT[0], vFVoxelChunkData.DataVT[1]);
	}

	public void ChunkProcPostGenMesh(IVxSurfExtractReq ireq)
	{
		if (m_MeshMgr == null)
		{
			return;
		}
		SurfExtractReqMC surfExtractReqMC = ireq as SurfExtractReqMC;
		if (surfExtractReqMC.IsInvalid)
		{
			Debug.Log(string.Concat("[VCSystem] RemoveChunkInSet", surfExtractReqMC._chunk.ChunkPosLod, ":", surfExtractReqMC._chunkStamp, "|", surfExtractReqMC._chunk.StampOfUpdating));
			return;
		}
		IntVector3 pos = SNChunkPosToChunkPos(new IntVector3(surfExtractReqMC._chunk.ChunkPosLod.x, surfExtractReqMC._chunk.ChunkPosLod.y, surfExtractReqMC._chunk.ChunkPosLod.z));
		int num = surfExtractReqMC.FillMesh(null);
		m_MeshMgr.Clamp(pos, num);
		int num2 = 0;
		while (num > 0)
		{
			GameObject gameObject = m_MeshMgr.QueryAtIndex(pos, num2);
			if (gameObject == null)
			{
				gameObject = new GameObject();
				gameObject.AddComponent<MeshFilter>();
				MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
				meshRenderer.material = m_MeshMgr.m_MeshMat;
				meshRenderer.receiveShadows = true;
				meshRenderer.shadowCastingMode = ShadowCastingMode.On;
				if (!m_ForEditor)
				{
					VCParticlePlayer vCParticlePlayer = gameObject.AddComponent<VCParticlePlayer>();
					vCParticlePlayer.FunctionTag = 1;
					vCParticlePlayer.LocalPosition = Vector3.zero;
				}
				m_MeshMgr.Set(pos, num2, gameObject);
			}
			MeshFilter component = gameObject.GetComponent<MeshFilter>();
			Mesh mesh = component.mesh;
			mesh.Clear();
			num = surfExtractReqMC.FillMesh(mesh);
			int vertexCount = mesh.vertexCount;
			Color32[] array = new Color32[vertexCount];
			Vector3[] array2 = new Vector3[vertexCount];
			Vector3[] vertices = mesh.vertices;
			for (int i = 0; i < vertexCount; i++)
			{
				ref Color32 reference = ref array[i];
				reference = VCIsoData.BLANK_COLOR;
			}
			for (int j = 0; j < vertices.Length; j += 3)
			{
				Vector3 normalized = Vector3.Cross(vertices[j] - vertices[j + 1], vertices[j] - vertices[j + 2]).normalized;
				array2[j] = normalized;
				array2[j + 1] = normalized;
				array2[j + 2] = normalized;
			}
			mesh.normals = array2;
			mesh.colors32 = array;
			m_MeshMgr.UpdateMeshColor(component);
			PostGenerate(component);
			num2++;
		}
	}

	public void OnBegUpdateNodeData(ILODNodeData ndata)
	{
	}

	public void OnEndUpdateNodeData(ILODNodeData ndata)
	{
	}

	public void OnDestroyNodeData(ILODNodeData ndata)
	{
	}

	private void PostGenerate(MeshFilter mf)
	{
		if (m_ForEditor)
		{
			m_MeshMgr.m_ColliderDirty = true;
		}
		else
		{
			if (m_CreateBoxCollider)
			{
				BoxCollider component = mf.gameObject.GetComponent<BoxCollider>();
				if (component != null)
				{
					UnityEngine.Object.DestroyImmediate(component);
				}
				RecalcCreationMeshBounds(mf.mesh);
				component = mf.gameObject.AddComponent<BoxCollider>();
				component.size *= PEVCConfig.instance.creationColliderScale;
				component.material = PEVCConfig.instance.physicMaterial;
			}
			VCParticlePlayer component2 = mf.GetComponent<VCParticlePlayer>();
			component2.LocalPosition = VCUtils.RandPosInBoundingBox(mf.mesh.bounds);
		}
		CreationController componentInParent = mf.GetComponentInParent<CreationController>();
		if ((bool)componentInParent)
		{
			componentInParent.OnNewMeshBuild(mf);
		}
	}

	private static void RecalcCreationMeshBounds(Mesh mesh)
	{
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		int num = mesh.triangles.Length;
		Vector3[] vertices = mesh.vertices;
		for (int i = 0; i < num; i++)
		{
			Vector3 vector3 = vertices[i];
			if (vector3.x < vector.x)
			{
				vector.x = vector3.x;
			}
			if (vector3.y < vector.y)
			{
				vector.y = vector3.y;
			}
			if (vector3.z < vector.z)
			{
				vector.z = vector3.z;
			}
			if (vector3.x > vector2.x)
			{
				vector2.x = vector3.x;
			}
			if (vector3.y > vector2.y)
			{
				vector2.y = vector3.y;
			}
			if (vector3.z > vector2.z)
			{
				vector2.z = vector3.z;
			}
		}
		mesh.bounds = new Bounds((vector + vector2) * 0.5f, vector2 - vector);
	}

	public static IntVector3 SNChunkPosToChunkPos(IntVector3 pos)
	{
		return new IntVector3(pos.x & 0x1F, pos.y & 0x1F, pos.z & 0x1F);
	}

	public static IntVector3 PackSNToChunkPos(IntVector3 pos, int sn)
	{
		int num = pos.x & 0x1F;
		int num2 = pos.y & 0x1F;
		int num3 = pos.z & 0x1F;
		int num4 = sn & 0x1F;
		int num5 = (sn >> 5) & 0x1F;
		int num6 = (sn >> 10) & 0x1F;
		return new IntVector3(num | (num4 << 5), num2 | (num5 << 5), num3 | (num6 << 5));
	}
}
