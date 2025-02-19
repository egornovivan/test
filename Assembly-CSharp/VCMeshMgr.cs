using System.Collections.Generic;
using UnityEngine;

public class VCMeshMgr : MonoBehaviour
{
	public float m_VoxelSize;

	public Material m_MeshMat;

	public bool m_DaggerMesh;

	public bool m_LeftSidePos;

	public Dictionary<int, Color32> m_ColorMap;

	private Dictionary<int, List<GameObject>> m_MeshGOs;

	public bool m_ColliderDirty;

	public static int ChunkPosToKey(IntVector3 pos)
	{
		return pos.x | (pos.z << 10) | (pos.y << 20);
	}

	public static int ChunkPosToKey(int x, int y, int z)
	{
		return x | (z << 10) | (y << 20);
	}

	public static IntVector3 KeyToChunkPos(int key)
	{
		return new IntVector3(key & 0x3FF, key >> 20, (key >> 10) & 0x3FF);
	}

	public void Init()
	{
		m_MeshGOs = new Dictionary<int, List<GameObject>>();
	}

	public void Set(IntVector3 pos, int index, GameObject go)
	{
		if (index < 0 || index > 255)
		{
			Debug.LogError("[VCMeshMgr] set index error");
			return;
		}
		int key = ChunkPosToKey(pos);
		if (!m_MeshGOs.ContainsKey(key))
		{
			m_MeshGOs.Add(key, new List<GameObject>());
		}
		List<GameObject> list = m_MeshGOs[key];
		for (int i = list.Count; i <= index; i++)
		{
			list.Add(null);
		}
		if (list[index] != null && list[index] != go)
		{
			Debug.LogWarning("This position already have a gameobject, will destroy it!");
			MeshFilter component = list[index].GetComponent<MeshFilter>();
			if (component != null)
			{
				Object.Destroy(component.mesh);
			}
			Object.Destroy(list[index]);
		}
		list[index] = go;
		SetGO(pos, index);
	}

	public void SetGO(IntVector3 pos, int index)
	{
		int key = ChunkPosToKey(pos);
		if (m_MeshGOs.ContainsKey(key))
		{
			List<GameObject> list = m_MeshGOs[key];
			if (list != null && index < list.Count)
			{
				GameObject gameObject = list[index];
				float num = m_VoxelSize * 32f;
				gameObject.name = "vc_" + pos.x + pos.y + pos.z + "_" + index;
				gameObject.transform.parent = base.transform;
				gameObject.transform.localScale = m_VoxelSize * Vector3.one;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localPosition = new Vector3((float)pos.x * num, (float)pos.y * num, (float)pos.z * num) - 0.5f * m_VoxelSize * Vector3.one;
				gameObject.layer = base.gameObject.layer;
			}
		}
	}

	public List<GameObject> Query(IntVector3 pos)
	{
		int key = ChunkPosToKey(pos);
		if (m_MeshGOs.ContainsKey(key))
		{
			return m_MeshGOs[key];
		}
		return null;
	}

	public GameObject QueryAtIndex(IntVector3 pos, int index)
	{
		int key = ChunkPosToKey(pos);
		if (m_MeshGOs.ContainsKey(key))
		{
			List<GameObject> list = m_MeshGOs[key];
			if (list != null && index < list.Count)
			{
				return list[index];
			}
		}
		return null;
	}

	public IntVector3 QueryPos(GameObject go)
	{
		if (go == null)
		{
			return null;
		}
		foreach (KeyValuePair<int, List<GameObject>> meshGO in m_MeshGOs)
		{
			if (meshGO.Value == null)
			{
				continue;
			}
			foreach (GameObject item in meshGO.Value)
			{
				if (item == go)
				{
					return KeyToChunkPos(meshGO.Key);
				}
			}
		}
		return null;
	}

	public bool Exist(GameObject go)
	{
		foreach (KeyValuePair<int, List<GameObject>> meshGO in m_MeshGOs)
		{
			if (meshGO.Value == null)
			{
				continue;
			}
			foreach (GameObject item in meshGO.Value)
			{
				if (item == go)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool Exist(IntVector3 pos, GameObject go)
	{
		int key = ChunkPosToKey(pos);
		if (!m_MeshGOs.ContainsKey(key))
		{
			return false;
		}
		List<GameObject> list = m_MeshGOs[key];
		if (list != null)
		{
			foreach (GameObject item in list)
			{
				if (item == go)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Clear(IntVector3 pos)
	{
		int key = ChunkPosToKey(pos);
		if (!m_MeshGOs.ContainsKey(key))
		{
			return;
		}
		List<GameObject> list = m_MeshGOs[key];
		if (list != null)
		{
			foreach (GameObject item in list)
			{
				if (item != null)
				{
					MeshFilter component = item.GetComponent<MeshFilter>();
					if (component != null)
					{
						Object.Destroy(component.mesh);
					}
					Object.Destroy(item);
				}
			}
			list.Clear();
		}
		m_MeshGOs.Remove(key);
	}

	public void Clamp(IntVector3 pos, int target_count)
	{
		int key = ChunkPosToKey(pos);
		if (!m_MeshGOs.ContainsKey(key))
		{
			return;
		}
		List<GameObject> list = m_MeshGOs[key];
		if (list != null)
		{
			while (list.Count > target_count)
			{
				if (list[target_count] != null)
				{
					MeshFilter component = list[target_count].GetComponent<MeshFilter>();
					if (component != null)
					{
						Object.Destroy(component.mesh);
					}
					Object.Destroy(list[target_count]);
				}
				list.RemoveAt(target_count);
			}
		}
		if (target_count == 0)
		{
			m_MeshGOs.Remove(key);
		}
	}

	public void FreeGameObjects()
	{
		foreach (KeyValuePair<int, List<GameObject>> meshGO in m_MeshGOs)
		{
			if (meshGO.Value == null)
			{
				continue;
			}
			foreach (GameObject item in meshGO.Value)
			{
				if (item != null)
				{
					MeshFilter component = item.GetComponent<MeshFilter>();
					if (component != null && component.mesh != null)
					{
						Object.Destroy(component.mesh);
						component.mesh = null;
					}
					Object.Destroy(item);
				}
			}
			meshGO.Value.Clear();
		}
		m_MeshGOs.Clear();
	}

	public void SetMeshMat(Material mat)
	{
		foreach (KeyValuePair<int, List<GameObject>> meshGO in m_MeshGOs)
		{
			if (meshGO.Value == null)
			{
				continue;
			}
			foreach (GameObject item in meshGO.Value)
			{
				if (item != null)
				{
					MeshRenderer component = item.GetComponent<MeshRenderer>();
					if (component != null)
					{
						component.material = mat;
					}
				}
			}
		}
	}

	public static int MeshVertexToColorKey(Vector3 vertex, int cx, int cy, int cz)
	{
		return VCIsoData.IPosToColorKey(new Vector3(vertex.x + (float)(cx * 32) - 0.5f, vertex.y + (float)(cy * 32) - 0.5f, vertex.z + (float)(cz * 32) - 0.5f));
	}

	public static int WorldPosToColorKey(Vector3 worldpos)
	{
		return VCIsoData.IPosToColorKey(worldpos / VCEditor.s_Scene.m_Setting.m_VoxelSize);
	}

	public void UpdateAllMeshColor()
	{
		if (m_ColorMap == null || m_MeshGOs == null)
		{
			return;
		}
		foreach (KeyValuePair<int, List<GameObject>> meshGO in m_MeshGOs)
		{
			List<GameObject> value = meshGO.Value;
			if (value == null)
			{
				continue;
			}
			foreach (GameObject item in value)
			{
				if (item != null)
				{
					MeshFilter component = item.GetComponent<MeshFilter>();
					if (component != null)
					{
						UpdateMeshColor(component);
					}
				}
			}
		}
	}

	public void UpdateMeshColor(MeshFilter mf)
	{
		if (m_ColorMap == null)
		{
			return;
		}
		Mesh mesh = mf.mesh;
		if (mesh == null)
		{
			return;
		}
		IntVector3 intVector = QueryPos(mf.gameObject);
		if (intVector == null)
		{
			return;
		}
		Vector3[] vertices = mesh.vertices;
		Color[] colors = mesh.colors;
		int num = vertices.Length;
		Color32 bLANK_COLOR = VCIsoData.BLANK_COLOR;
		for (int i = 0; i < num; i++)
		{
			int key = MeshVertexToColorKey(vertices[i], intVector.x, intVector.y, intVector.z);
			if (m_ColorMap.TryGetValue(key, out var value))
			{
				ref Color reference = ref colors[i];
				reference = value;
			}
			else
			{
				ref Color reference2 = ref colors[i];
				reference2 = bLANK_COLOR;
			}
		}
		mesh.colors = colors;
	}

	public void PrepareMeshColliders()
	{
		foreach (KeyValuePair<int, List<GameObject>> meshGO in m_MeshGOs)
		{
			List<GameObject> value = meshGO.Value;
			if (value == null)
			{
				continue;
			}
			foreach (GameObject item in value)
			{
				if (!(item != null))
				{
					continue;
				}
				MeshCollider component = item.GetComponent<MeshCollider>();
				MeshFilter component2 = item.GetComponent<MeshFilter>();
				if (!(component2 != null))
				{
					continue;
				}
				if (component != null)
				{
					if (component.sharedMesh == null)
					{
						component.sharedMesh = component2.mesh;
					}
				}
				else
				{
					item.AddComponent<MeshCollider>().sharedMesh = component2.mesh;
					Debug.LogWarning("In design, this cannot be happen");
				}
			}
		}
		m_ColliderDirty = false;
	}

	private void Awake()
	{
		Init();
	}

	private void OnDestroy()
	{
		FreeGameObjects();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
