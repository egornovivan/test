using System.Collections.Generic;
using UnityEngine;

public class VCEUICostList : VCEUIAssetList
{
	public bool m_IsEditor = true;

	[HideInInspector]
	public CreationAttr m_NonEditorAttr;

	public bool IsEnough
	{
		get
		{
			if (m_AssetItems == null)
			{
				return false;
			}
			if (m_AssetItems.Count == 0)
			{
				return false;
			}
			foreach (GameObject assetItem in m_AssetItems)
			{
				VCEUICostItem component = assetItem.GetComponent<VCEUICostItem>();
				if (component != null && !component.m_IsEnough)
				{
					return false;
				}
			}
			return true;
		}
	}

	private new void Update()
	{
		base.Update();
	}

	private new void OnEnable()
	{
		RefreshCostList();
	}

	public void RefreshCostList()
	{
		ClearItems();
		CreationAttr creationAttr = null;
		creationAttr = ((!m_IsEditor) ? m_NonEditorAttr : VCEditor.s_Scene.m_CreationAttr);
		if (creationAttr != null)
		{
			foreach (KeyValuePair<int, int> item in creationAttr.m_Cost)
			{
				if (item.Value != 0)
				{
					GameObject gameObject = Object.Instantiate(m_ItemRes);
					Vector3 localScale = gameObject.transform.localScale;
					gameObject.name = item.Key.ToString();
					gameObject.transform.parent = m_ItemGroup.transform;
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localScale = localScale;
					VCEUICostItem component = gameObject.GetComponent<VCEUICostItem>();
					component.m_GameItemId = item.Key;
					component.m_GameItemCost = item.Value;
					m_AssetItems.Add(gameObject);
				}
			}
		}
		RepositionGrid();
	}
}
