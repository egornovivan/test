using System;
using System.Collections.Generic;
using System.IO;
using Pathea.Maths;
using UnityEngine;

public class VArtifactCursor : MonoBehaviour
{
	public delegate void OnOutputVoxel(int x, int y, int z, VCVoxel voxel);

	[SerializeField]
	private Transform Bound;

	[SerializeField]
	private Transform OriginTransform;

	[SerializeField]
	private Transform SceneGroup;

	[SerializeField]
	private Transform ObjectGroup;

	[SerializeField]
	private VCMeshMgr TMeshMgr;

	[SerializeField]
	private VCMeshMgr WMeshMgr;

	[SerializeField]
	private VCEMovingGizmo MGizmo;

	[SerializeField]
	private VCERotatingGizmo RGizmo;

	public bool MGizmoEnabled;

	public bool RGizmoEnabled;

	private VCMCComputer Computer;

	private LineRenderer[] Edges;

	public VCIsoData ISO;

	private static string s_PrefabPath = "Artifact/Artifact Cursor";

	private Quaternion oldrot;

	public Vector3 Size => Bound.localScale;

	public Vector3 Origin => OriginTransform.position;

	public Vector3 XDir => OriginTransform.right;

	public Vector3 YDir => OriginTransform.up;

	public Vector3 ZDir => OriginTransform.forward;

	private void Start()
	{
		Edges = Bound.GetComponentsInChildren<LineRenderer>(includeInactive: true);
		MGizmo.OnMoving = OnMoving;
		MGizmo.OnDrop = OnMovingEnd;
		RGizmo.OnRotating = OnRotating;
		RGizmo.OnDragBegin = OnGizmoBegin;
	}

	private void Update()
	{
		MGizmo.gameObject.SetActive(MGizmoEnabled);
		RGizmo.gameObject.SetActive(RGizmoEnabled);
		if (ISO != null)
		{
			MGizmo.transform.position = base.transform.position;
			RGizmo.transform.position = base.transform.position;
			MGizmo.transform.rotation = Quaternion.identity;
			RGizmo.transform.rotation = Quaternion.identity;
		}
	}

	public static VArtifactCursor Create(string full_path, int layer = 0)
	{
		GameObject gameObject = Resources.Load(s_PrefabPath) as GameObject;
		if (gameObject == null)
		{
			throw new Exception("Load artifact cursor prefab failed");
		}
		VArtifactCursor component = gameObject.GetComponent<VArtifactCursor>();
		if (component == null)
		{
			throw new Exception("Load artifact cursor prefab failed");
		}
		VCIsoData vCIsoData = LoadIso(full_path);
		if (vCIsoData == null)
		{
			throw new Exception("Load artifact file error");
		}
		VArtifactCursor vArtifactCursor = UnityEngine.Object.Instantiate(component);
		vArtifactCursor.ISO = vCIsoData;
		vArtifactCursor.SetBoundSize(new Vector3(vCIsoData.m_HeadInfo.xSize, vCIsoData.m_HeadInfo.ySize, vCIsoData.m_HeadInfo.zSize));
		vArtifactCursor.Computer = new VCMCComputer();
		vArtifactCursor.Computer.Init(new IntVector3(vCIsoData.m_HeadInfo.xSize, vCIsoData.m_HeadInfo.ySize, vCIsoData.m_HeadInfo.zSize), vArtifactCursor.TMeshMgr, for_editor: false);
		foreach (VCComponentData component2 in vCIsoData.m_Components)
		{
			component2.CreateEntity(for_editor: false, vArtifactCursor.ObjectGroup);
		}
		foreach (KeyValuePair<int, VCVoxel> voxel in vCIsoData.m_Voxels)
		{
			vArtifactCursor.Computer.AlterVoxel(voxel.Key, voxel.Value);
		}
		vArtifactCursor.Computer.ReqMesh();
		vArtifactCursor.gameObject.layer = layer;
		Transform[] componentsInChildren = vArtifactCursor.gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			transform.gameObject.layer = layer;
		}
		vArtifactCursor.gameObject.SetActive(value: true);
		vArtifactCursor.Bound.gameObject.SetActive(value: true);
		vArtifactCursor.SceneGroup.gameObject.SetActive(value: true);
		return vArtifactCursor;
	}

	private static VCIsoData LoadIso(string path)
	{
		try
		{
			VCIsoData vCIsoData = new VCIsoData();
			using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			byte[] array = new byte[(int)fileStream.Length];
			fileStream.Read(array, 0, (int)fileStream.Length);
			fileStream.Close();
			if (vCIsoData.Import(array, new VCIsoOption(editor: false)))
			{
				return vCIsoData;
			}
			return null;
		}
		catch (Exception ex)
		{
			Debug.LogError("Loading ISO Error : " + ex.ToString());
			return null;
		}
	}

	private void SetBoundSize(Vector3 size)
	{
		Edges = Bound.GetComponentsInChildren<LineRenderer>(includeInactive: true);
		Bound.localScale = size;
		float num = size.x + size.y + size.z;
		LineRenderer[] edges = Edges;
		foreach (LineRenderer lineRenderer in edges)
		{
			lineRenderer.SetWidth(num * 0.01f, num * 0.01f);
		}
		size.y = 0f;
		Bound.localPosition = -size * 0.5f;
		SceneGroup.localPosition = Bound.localPosition;
	}

	public void OutputVoxels(Vector3 offset, OnOutputVoxel output_function)
	{
		if (ISO == null || output_function == null)
		{
			return;
		}
		foreach (KeyValuePair<int, VCVoxel> voxel in ISO.m_Voxels)
		{
			Vector3 vector = new Vector3(voxel.Key & 0x3FF, voxel.Key >> 20, (voxel.Key >> 10) & 0x3FF);
			Vector3 vector2 = OriginTransform.position + vector.x * OriginTransform.right + vector.y * OriginTransform.up + vector.z * OriginTransform.forward;
			vector2 += offset;
			INTVECTOR3 iNTVECTOR = new INTVECTOR3(Mathf.FloorToInt(vector2.x), Mathf.FloorToInt(vector2.y), Mathf.FloorToInt(vector2.z));
			INTVECTOR3 iNTVECTOR2 = new INTVECTOR3(Mathf.CeilToInt(vector2.x), Mathf.CeilToInt(vector2.y), Mathf.CeilToInt(vector2.z));
			if (iNTVECTOR == iNTVECTOR2)
			{
				output_function(iNTVECTOR.x, iNTVECTOR.y, iNTVECTOR.z, voxel.Value);
				continue;
			}
			for (int i = iNTVECTOR.x; i <= iNTVECTOR2.x; i++)
			{
				for (int j = iNTVECTOR.y; j <= iNTVECTOR2.y; j++)
				{
					for (int k = iNTVECTOR.z; k <= iNTVECTOR2.z; k++)
					{
						float num = 1f - Mathf.Abs(vector2.x - (float)i);
						float num2 = 1f - Mathf.Abs(vector2.y - (float)j);
						float num3 = 1f - Mathf.Abs(vector2.z - (float)k);
						float num4 = num * num2 * num3;
						num4 = ((!(num4 < 0.5f)) ? (0.5f / (1.5f - num4)) : (num4 / (0.5f + num4)));
						VCVoxel value = voxel.Value;
						value.Volume = (byte)Mathf.CeilToInt((float)(int)value.Volume * num4);
						if (value.Volume > 1)
						{
							output_function(i, j, k, value);
						}
					}
				}
			}
		}
	}

	private void OnMoving(Vector3 ofs)
	{
		base.transform.position += ofs;
	}

	private void OnMovingEnd()
	{
		Vector3 position = base.transform.position;
		position.x = Mathf.RoundToInt(position.x);
		position.y = Mathf.RoundToInt(position.y);
		position.z = Mathf.RoundToInt(position.z);
		base.transform.position = position;
	}

	private void OnGizmoBegin()
	{
		oldrot = base.transform.rotation;
	}

	private void OnRotating(Vector3 axis, float angle)
	{
		base.transform.rotation = oldrot;
		base.transform.Rotate(axis, angle, Space.World);
	}
}
