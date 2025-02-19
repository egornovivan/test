using UnityEngine;

public abstract class VCPartData : VCComponentData
{
	public override GameObject CreateEntity(bool for_editor, Transform parent)
	{
		if (m_Entity != null)
		{
			DestroyEntity();
		}
		m_Entity = Object.Instantiate(VCConfig.s_Parts[m_ComponentId].m_ResObj);
		m_Entity.name = VCUtils.Capital(VCConfig.s_Parts[m_ComponentId].m_Name, everyword: true);
		if (for_editor)
		{
			m_Entity.transform.parent = VCEditor.Instance.m_PartGroup.transform;
			VCEComponentTool component = m_Entity.GetComponent<VCEComponentTool>();
			component.m_IsBrush = false;
			component.m_InEditor = true;
			component.m_ToolGroup.SetActive(value: true);
			component.m_SelBound.enabled = false;
			component.m_SelBound.GetComponent<Collider>().enabled = false;
			component.m_SelBound.m_BoundColor = GLComponentBound.s_Blue;
			component.m_Data = this;
			Collider[] componentsInChildren = m_Entity.GetComponentsInChildren<Collider>(includeInactive: true);
			Collider[] array = componentsInChildren;
			foreach (Collider collider in array)
			{
				if (collider.gameObject != component.m_SelBound.gameObject)
				{
					collider.enabled = false;
				}
			}
		}
		else
		{
			m_Entity.transform.parent = parent;
			Transform[] componentsInChildren2 = m_Entity.GetComponentsInChildren<Transform>(includeInactive: true);
			Transform[] array2 = componentsInChildren2;
			foreach (Transform transform in array2)
			{
				transform.gameObject.layer = VCConfig.s_ProductLayer;
			}
		}
		UpdateEntity(for_editor);
		if (!for_editor)
		{
			UpdateComponent();
		}
		return m_Entity;
	}
}
