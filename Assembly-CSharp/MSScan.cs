using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MSScan : MonoBehaviour
{
	private static MSScan mInstance;

	private IVxDataSource _dataSource;

	public IntVector3 centerPos;

	public int radius;

	public Material mat;

	public bool bRefresh;

	public bool bInScanning;

	private List<IntVector3> orderedOffsetList;

	public List<byte> mMatList;

	public static MSScan Instance => mInstance;

	private void Awake()
	{
		radius = 80;
		mInstance = this;
		mMatList = new List<byte>();
	}

	private void Update()
	{
		if (bRefresh)
		{
			bRefresh = false;
			_dataSource = VFVoxelTerrain.self.Voxels;
			if (null != GameUI.Instance.mMainPlayer)
			{
				centerPos = GameUI.Instance.mMainPlayer.position;
			}
			StartCoroutine(Scan());
		}
	}

	private void ScanOneChunk(IntVector4 cposlod, out List<Vector3> outputVerts, out List<Color32> outputCols)
	{
		outputVerts = new List<Vector3>();
		outputCols = new List<Color32>();
		VFVoxelChunkData vFVoxelChunkData = _dataSource.readChunk(cposlod.x, cposlod.y, cposlod.z, cposlod.w);
		if (vFVoxelChunkData == null)
		{
			return;
		}
		byte[] dataVT = vFVoxelChunkData.DataVT;
		switch (dataVT.Length)
		{
		case 0:
			return;
		case 2:
		{
			if (!mMatList.Contains(dataVT[1]))
			{
				return;
			}
			int capacity = 32768;
			outputVerts = new List<Vector3>(capacity);
			outputCols = new List<Color32>(capacity);
			Vector3 item = new Vector3(0f, 0f, 0f);
			Color32 item2 = MetalScanData.GetColorByType((byte)(dataVT[0] * 2));
			int num = 0;
			while (num < 32)
			{
				int num2 = 0;
				while (num2 < 32)
				{
					int num3 = 0;
					while (num3 < 32)
					{
						outputVerts.Add(item);
						outputCols.Add(item2);
						num3++;
						item.x += 1f;
					}
					item.x = 0f;
					num2++;
					item.y += 1f;
				}
				item.y = 0f;
				num++;
				item.z += 1f;
			}
			return;
		}
		}
		int num4 = 1261;
		for (int i = 0; i < 32; i++)
		{
			for (int j = 0; j < 32; j++)
			{
				int num5 = 0;
				while (num5 < 32)
				{
					int num6 = num4 << 1;
					if (dataVT[num6] >= 128)
					{
						byte b = dataVT[num6 + 1];
						Color colorByType = MetalScanData.GetColorByType(b);
						if (mMatList.Contains(b))
						{
							outputVerts.Add(new Vector3(num5, j, i));
							outputCols.Add(colorByType);
						}
					}
					num5++;
					num4++;
				}
				num4 += 3;
			}
			num4 += 105;
		}
	}

	private IEnumerator Scan()
	{
		bInScanning = true;
		Transform tParent = ResetGOs();
		IntVector4 vecChunkPosLD = new IntVector4(centerPos.x - radius >> 5, centerPos.y - radius >> 5, centerPos.z - radius >> 5, 0);
		int nAxis = 1 + (radius * 2 >> 5);
		for (int z = 0; z < nAxis; z++)
		{
			for (int y = 0; y < nAxis; y++)
			{
				for (int x = 0; x < nAxis; x++)
				{
					IntVector4 vecChunkPos = new IntVector4(vecChunkPosLD.x + x, vecChunkPosLD.y + y, vecChunkPosLD.z + z, 0);
					ScanOneChunk(vecChunkPos, out var outputVerts, out var outputCols);
					int nVerts = outputVerts.Count;
					if (nVerts > 0)
					{
						GameObject mineralPointsGo = new GameObject("MP_" + vecChunkPos);
						mineralPointsGo.layer = 24;
						mineralPointsGo.AddComponent<MeshRenderer>().sharedMaterial = mat;
						Mesh mesh = mineralPointsGo.AddComponent<MeshFilter>().mesh;
						mesh.vertices = outputVerts.ToArray();
						mesh.colors32 = outputCols.ToArray();
						int[] indices = new int[nVerts];
						Array.Copy(SurfExtractorsMan.s_indiceMax, indices, nVerts);
						mesh.SetIndices(indices, MeshTopology.Points, 0);
						mineralPointsGo.transform.parent = tParent;
						mineralPointsGo.transform.position = new Vector3(vecChunkPos.x << 5, vecChunkPos.y << 5, vecChunkPos.z << 5);
					}
					yield return 0;
				}
			}
		}
		bInScanning = false;
	}

	private Transform ResetGOs()
	{
		GameObject gameObject = GameObject.Find("Minerals");
		if (gameObject != null)
		{
			List<Transform> list = new List<Transform>();
			foreach (Transform item in gameObject.transform)
			{
				try
				{
					list.Add(item);
					UnityEngine.Object.Destroy(item.GetComponent<MeshFilter>().mesh);
				}
				catch
				{
				}
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(list[num].gameObject);
			}
		}
		else
		{
			gameObject = new GameObject("Minerals");
			gameObject.layer = 24;
		}
		return gameObject.transform;
	}

	public void MakeAScan(Vector3 pos, List<byte> matList, int rad = 80)
	{
		_dataSource = VFVoxelTerrain.self.Voxels;
		centerPos = pos;
		mMatList = matList;
		radius = rad;
		StopAllCoroutines();
		StartCoroutine(Scan());
	}
}
