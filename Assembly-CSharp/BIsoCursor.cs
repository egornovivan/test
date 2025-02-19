using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BIsoCursor : MonoBehaviour
{
	public delegate void OnOutputVoxel(Dictionary<int, BSVoxel> voxels, Vector3 originalPos);

	[SerializeField]
	private Transform Bound;

	public Transform OriginTransform;

	[SerializeField]
	private Transform BlockGroup;

	public BSGizmoTriggerEvent gizmoTrigger;

	public BSIsoData ISO;

	private BSB45Computer Computer;

	public BSIsoHeadData testHead;

	private static string s_PrefabPath = "Prefab/Iso Cursor";

	public static BIsoCursor CreateIsoCursor(string full_path, int layer = 0)
	{
		GameObject gameObject = Resources.Load(s_PrefabPath) as GameObject;
		if (gameObject == null)
		{
			throw new Exception("Load iso cursor prefab failed");
		}
		BIsoCursor component = gameObject.GetComponent<BIsoCursor>();
		if (component == null)
		{
			throw new Exception("Load iso cursor prefab failed");
		}
		BIsoCursor bIsoCursor = UnityEngine.Object.Instantiate(component);
		BSIsoData bSIsoData = (bIsoCursor.ISO = LoadISO(full_path));
		if (bSIsoData.m_HeadInfo.Mode == EBSVoxelType.Block)
		{
			Vector3 size = new Vector3(bSIsoData.m_HeadInfo.xSize, bSIsoData.m_HeadInfo.ySize, bSIsoData.m_HeadInfo.zSize);
			bIsoCursor.SetBoundSizeOfBlock(size, bIsoCursor.gameObject);
			bIsoCursor.Computer = bIsoCursor.BlockGroup.gameObject.AddComponent<BSB45Computer>();
			foreach (KeyValuePair<int, BSVoxel> voxel in bIsoCursor.ISO.m_Voxels)
			{
				IntVector3 intVector = BSIsoData.KeyToIPos(voxel.Key);
				bIsoCursor.Computer.AlterBlockInBuild(intVector.x, intVector.y, intVector.z, voxel.Value.ToBlock());
			}
			bIsoCursor.Computer.RebuildMesh();
		}
		else if (bSIsoData.m_HeadInfo.Mode == EBSVoxelType.Voxel)
		{
			Debug.LogError("Cant Support the iso voxel");
			UnityEngine.Object.Destroy(gameObject);
			UnityEngine.Object.Destroy(bIsoCursor.gameObject);
			return null;
		}
		Transform[] componentsInChildren = bIsoCursor.gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			transform.gameObject.layer = layer;
		}
		bIsoCursor.gameObject.SetActive(value: true);
		bIsoCursor.BlockGroup.gameObject.SetActive(value: true);
		bIsoCursor.Bound.gameObject.SetActive(value: true);
		bIsoCursor.testHead = bSIsoData.m_HeadInfo;
		return bIsoCursor;
	}

	private static BSIsoData LoadISO(string full_path)
	{
		try
		{
			BSIsoData bSIsoData = new BSIsoData();
			using FileStream fileStream = new FileStream(full_path, FileMode.Open, FileAccess.Read);
			byte[] array = new byte[(int)fileStream.Length];
			fileStream.Read(array, 0, (int)fileStream.Length);
			fileStream.Close();
			if (bSIsoData.Import(array))
			{
				return bSIsoData;
			}
			return null;
		}
		catch (Exception ex)
		{
			Debug.LogError("Loading ISO Error : " + ex.ToString());
			return null;
		}
	}

	public void SetOriginOffset(Vector3 offset)
	{
		OriginTransform.localPosition = offset;
		Vector3 vector = Vector3.zero;
		if (ISO != null)
		{
			vector = new Vector3(ISO.m_HeadInfo.xSize % 2, 0f, ISO.m_HeadInfo.zSize % 2) * BSBlock45Data.s_Scale;
		}
		BlockGroup.localPosition = offset;
		Bound.localPosition = offset + vector;
	}

	public void OutputVoxels(Vector3 offset, OnOutputVoxel output_function)
	{
		if (ISO != null)
		{
			output_function?.Invoke(ISO.m_Voxels, OriginTransform.position);
		}
	}

	private void Start()
	{
	}

	private void SetBoundSizeOfBlock(Vector3 size, GameObject go)
	{
		Vector3 localScale = size * BSBlock45Data.s_Scale;
		Bound.localScale = localScale;
	}
}
