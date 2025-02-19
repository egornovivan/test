using PETools;
using UnityEngine;

public class ModelLodTest : MonoBehaviour
{
	public GameObject asset;

	private GameObject m_Obj;

	private Vector3 position;

	private void Start()
	{
		Load();
	}

	private void Update()
	{
		if (m_Obj != null)
		{
			position = m_Obj.transform.position;
		}
	}

	private void Load()
	{
		if (asset != null)
		{
			m_Obj = Object.Instantiate(asset, base.transform.position, Quaternion.identity) as GameObject;
			m_Obj.transform.parent = base.transform;
			m_Obj.transform.localPosition = Vector3.zero;
		}
	}

	private void OnBorderEnter(IntVector2 node)
	{
		if (m_Obj != null && PEUtil.GetFixedPosition(LODOctreeMan.self.GetNodes(node), base.transform.position, out var fixedPos))
		{
			m_Obj.transform.position = fixedPos;
			m_Obj.SetActive(value: true);
		}
	}

	private void OnBorderExit(IntVector2 node)
	{
		if (m_Obj != null)
		{
			m_Obj.SetActive(value: false);
		}
	}

	private void OnColliderCreate(IntVector4 node)
	{
		if (m_Obj != null)
		{
			m_Obj.SetActive(value: true);
		}
	}

	private void OnColliderDestroy(IntVector4 node)
	{
		if (m_Obj != null)
		{
			m_Obj.SetActive(value: false);
		}
	}

	private void OnMeshCreated(IntVector4 node)
	{
	}

	private void OnMeshDestroy(IntVector4 node)
	{
		if (m_Obj != null)
		{
			Object.Destroy(m_Obj);
		}
	}
}
