using System.Collections.Generic;
using UnityEngine;
using WhiteCat;

public class VCEGL : MonoBehaviour
{
	public GameObject m_ParentObject;

	private Material m_LineMaterial;

	private void Awake()
	{
	}

	private void Start()
	{
		CreateLineMaterials();
	}

	public static int SortByOrder(GLBehaviour a, GLBehaviour b)
	{
		return a.m_RenderOrder - b.m_RenderOrder;
	}

	private void OnPostRender()
	{
		GLBehaviour[] componentsInChildren = m_ParentObject.GetComponentsInChildren<GLBehaviour>();
		List<GLBehaviour> list = new List<GLBehaviour>();
		GLBehaviour[] array = componentsInChildren;
		foreach (GLBehaviour item in array)
		{
			list.Add(item);
		}
		list.Sort(SortByOrder);
		foreach (GLBehaviour item2 in list)
		{
			if (!(item2 != null) || !item2.gameObject.activeInHierarchy || !item2.enabled)
			{
				continue;
			}
			Material material = item2.m_Material;
			if (material == null)
			{
				material = m_LineMaterial;
			}
			int passCount = material.passCount;
			for (int j = 0; j < passCount; j++)
			{
				GL.PushMatrix();
				if (material.SetPass(j))
				{
					item2.OnGL();
				}
				GL.PopMatrix();
			}
		}
	}

	private void CreateLineMaterials()
	{
		if (!m_LineMaterial)
		{
			m_LineMaterial = PEVCConfig.instance.handleMaterial;
			m_LineMaterial.hideFlags = HideFlags.HideAndDontSave;
			m_LineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}
}
