using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BillboardTerrain : MonoBehaviour
{
	public int xPos;

	public int zPos;

	public Vector3 m_Center;

	private Dictionary<int, List<TreeInfo>> m_AllTrees = new Dictionary<int, List<TreeInfo>>();

	private Mesh _BMesh;

	public void SetTrees(List<TreeInfo> tree_list)
	{
		Reset();
		int num = 0;
		foreach (TreeInfo item in tree_list)
		{
			if (item.m_protoTypeIdx < LSubTerrainMgr.Instance.GlobalPrototypeBillboardList.Length && !(LSubTerrainMgr.Instance.GlobalPrototypeBillboardList[item.m_protoTypeIdx] == null))
			{
				if (!m_AllTrees.ContainsKey(item.m_protoTypeIdx))
				{
					m_AllTrees.Add(item.m_protoTypeIdx, new List<TreeInfo>());
				}
				m_AllTrees[item.m_protoTypeIdx].Add(item);
				num++;
			}
		}
		if (num < 1)
		{
			Reset();
			return;
		}
		Material[] array = new Material[m_AllTrees.Count];
		int num2 = 0;
		foreach (KeyValuePair<int, List<TreeInfo>> allTree in m_AllTrees)
		{
			array[num2++] = LSubTerrainMgr.Instance.GlobalPrototypeBillboardList[allTree.Key];
		}
		Vector3[] array2 = new Vector3[num * 4];
		Vector3[] array3 = new Vector3[num * 4];
		Vector2[] array4 = new Vector2[num * 4];
		List<int[]> list = new List<int[]>();
		int num3 = 0;
		foreach (KeyValuePair<int, List<TreeInfo>> allTree2 in m_AllTrees)
		{
			List<TreeInfo> value = allTree2.Value;
			int[] array5 = new int[value.Count * 6];
			list.Add(array5);
			int num4 = 0;
			foreach (TreeInfo item2 in value)
			{
				Bounds bounds = LSubTerrainMgr.Instance.GlobalPrototypeBounds[allTree2.Key];
				float num5 = Mathf.Max(bounds.extents.x, bounds.extents.z) * item2.m_widthScale;
				float num6 = bounds.extents.y * item2.m_heightScale;
				Vector3 center = bounds.center;
				center.x *= item2.m_widthScale;
				center.y *= item2.m_heightScale;
				center.z *= item2.m_widthScale;
				Vector3 vector = LSubTerrUtils.TreeTerrainPosToWorldPos(xPos, zPos, item2.m_pos) + center;
				Vector3 vector2 = vector - base.transform.position;
				Vector3 rhs = vector - m_Center;
				rhs.y = 0f;
				rhs.Normalize();
				Vector3 up = Vector3.up;
				Vector3 vector3 = Vector3.Cross(up, rhs);
				ref Vector3 reference = ref array2[num3 * 4];
				reference = vector2 + vector3 * num5 + up * num6;
				ref Vector3 reference2 = ref array2[num3 * 4 + 1];
				reference2 = vector2 - vector3 * num5 + up * num6;
				ref Vector3 reference3 = ref array2[num3 * 4 + 2];
				reference3 = vector2 - vector3 * num5 - up * num6;
				ref Vector3 reference4 = ref array2[num3 * 4 + 3];
				reference4 = vector2 + vector3 * num5 - up * num6;
				ref Vector3 reference5 = ref array3[num3 * 4];
				reference5 = Vector3.up;
				ref Vector3 reference6 = ref array3[num3 * 4 + 1];
				reference6 = Vector3.up;
				ref Vector3 reference7 = ref array3[num3 * 4 + 2];
				reference7 = Vector3.up;
				ref Vector3 reference8 = ref array3[num3 * 4 + 3];
				reference8 = Vector3.up;
				ref Vector2 reference9 = ref array4[num3 * 4];
				reference9 = new Vector2(0f, 1f);
				ref Vector2 reference10 = ref array4[num3 * 4 + 1];
				reference10 = new Vector2(1f, 1f);
				ref Vector2 reference11 = ref array4[num3 * 4 + 2];
				reference11 = new Vector2(1f, 0f);
				ref Vector2 reference12 = ref array4[num3 * 4 + 3];
				reference12 = new Vector2(0f, 0f);
				array5[num4 * 6] = num3 * 4;
				array5[num4 * 6 + 1] = num3 * 4 + 1;
				array5[num4 * 6 + 2] = num3 * 4 + 2;
				array5[num4 * 6 + 3] = num3 * 4 + 2;
				array5[num4 * 6 + 4] = num3 * 4 + 3;
				array5[num4 * 6 + 5] = num3 * 4;
				num3++;
				num4++;
			}
		}
		_BMesh = new Mesh();
		_BMesh.subMeshCount = array.Length;
		_BMesh.vertices = array2;
		_BMesh.normals = array3;
		_BMesh.uv = array4;
		for (int i = 0; i < list.Count; i++)
		{
			_BMesh.SetTriangles(list[i], i);
		}
		MeshFilter component = GetComponent<MeshFilter>();
		component.mesh = _BMesh;
		MeshRenderer component2 = GetComponent<MeshRenderer>();
		component2.materials = array;
	}

	public void Reset()
	{
		MeshFilter component = GetComponent<MeshFilter>();
		MeshRenderer component2 = GetComponent<MeshRenderer>();
		component.mesh = null;
		component.sharedMesh = null;
		component2.materials = new Material[0];
		component2.sharedMaterials = new Material[0];
		foreach (KeyValuePair<int, List<TreeInfo>> allTree in m_AllTrees)
		{
			if (allTree.Value != null)
			{
				allTree.Value.Clear();
			}
		}
		m_AllTrees.Clear();
		if (_BMesh != null)
		{
			Object.Destroy(_BMesh);
			_BMesh = null;
		}
	}

	private void OnDestroy()
	{
		Reset();
	}
}
