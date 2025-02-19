using System.Collections.Generic;
using UnityEngine;

public class VCEUIDecalList : VCEUIAssetList
{
	private new void Update()
	{
		base.Update();
	}

	public void RefreshDecalList()
	{
		ClearItems();
		foreach (KeyValuePair<ulong, VCDecalAsset> s_Decal in VCEAssetMgr.s_Decals)
		{
			GameObject gameObject = Object.Instantiate(m_ItemRes);
			Vector3 localScale = gameObject.transform.localScale;
			gameObject.name = "_" + s_Decal.Value.GUIDString;
			gameObject.transform.parent = m_ItemGroup.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = localScale;
			VCEUIDecalItem component = gameObject.GetComponent<VCEUIDecalItem>();
			component.m_GUID = s_Decal.Value.m_Guid;
			component.m_ParentList = this;
			m_AssetItems.Add(gameObject);
		}
		foreach (KeyValuePair<ulong, VCDecalAsset> s_TempDecal in VCEAssetMgr.s_TempDecals)
		{
			GameObject gameObject2 = Object.Instantiate(m_ItemRes);
			Vector3 localScale2 = gameObject2.transform.localScale;
			gameObject2.name = "_" + s_TempDecal.Value.GUIDString;
			gameObject2.transform.parent = m_ItemGroup.transform;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localScale = localScale2;
			VCEUIDecalItem component2 = gameObject2.GetComponent<VCEUIDecalItem>();
			component2.m_GUID = s_TempDecal.Value.m_Guid;
			component2.m_ParentList = this;
			m_AssetItems.Add(gameObject2);
		}
		RepositionGrid();
	}

	private void OnAddDecalClick()
	{
		VCEditor.SelectedDecal = null;
		VCEditor.Instance.m_UI.m_DecalWindow.ShowWindow();
	}
}
