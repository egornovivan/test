using System.Collections.Generic;
using UnityEngine;

public class VCEUIMaterialList : VCEUIAssetList
{
	public UIPopupList m_MatterPopupList;

	private string m_lastSelection;

	private VCMaterial m_TempSelected;

	private new void Update()
	{
		base.Update();
		if (m_lastSelection != m_MatterPopupList.selection)
		{
			m_lastSelection = m_MatterPopupList.selection;
			RefreshMaterialList(m_MatterPopupList.selection);
			VCEditor.SelectedMaterial = null;
			RepositionList();
		}
		foreach (GameObject assetItem in m_AssetItems)
		{
			VCEUIMaterialItem component = assetItem.GetComponent<VCEUIMaterialItem>();
			if (component.m_Material == VCEditor.SelectedMaterial)
			{
				component.m_SelectedGlow.SetActive(value: true);
			}
			else
			{
				component.m_SelectedGlow.SetActive(value: false);
			}
		}
	}

	public void InitMatterList()
	{
		m_MatterPopupList.items.Clear();
		m_MatterPopupList.items.Add("All".ToLocalizationString());
		foreach (KeyValuePair<int, VCMatterInfo> s_Matter in VCConfig.s_Matters)
		{
			m_MatterPopupList.items.Add(VCUtils.Capital(s_Matter.Value.Name, everyword: true).ToLocalizationString());
		}
		m_MatterPopupList.selection = "All".ToLocalizationString();
	}

	public void RefreshMaterialList(string filter)
	{
		ClearItems();
		int num = -1;
		foreach (KeyValuePair<int, VCMatterInfo> s_Matter in VCConfig.s_Matters)
		{
			if (filter.ToLower() == s_Matter.Value.Name.ToLower())
			{
				num = s_Matter.Value.ItemIndex;
				break;
			}
		}
		foreach (KeyValuePair<ulong, VCMaterial> s_Material in VCEAssetMgr.s_Materials)
		{
			if (num == -1 || num == s_Material.Value.m_MatterId)
			{
				GameObject gameObject = Object.Instantiate(m_ItemRes);
				Vector3 localScale = gameObject.transform.localScale;
				string text = "_Asset " + s_Material.Value.m_Name;
				gameObject.name = text;
				gameObject.transform.parent = m_ItemGroup.transform;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localScale = localScale;
				VCEUIMaterialItem component = gameObject.GetComponent<VCEUIMaterialItem>();
				gameObject.AddComponent<UIDragPanelContents>();
				component.m_ParentList = this;
				component.m_Material = s_Material.Value;
				component.m_MaterialIcon.gameObject.AddComponent<UIDragPanelContents>();
				m_AssetItems.Add(gameObject);
			}
		}
		foreach (KeyValuePair<ulong, VCMaterial> s_TempMaterial in VCEAssetMgr.s_TempMaterials)
		{
			if (num == -1 || num == s_TempMaterial.Value.m_MatterId)
			{
				GameObject gameObject2 = Object.Instantiate(m_ItemRes);
				Vector3 localScale2 = gameObject2.transform.localScale;
				string text2 = "_Iso " + s_TempMaterial.Value.m_Name;
				gameObject2.name = text2;
				gameObject2.transform.parent = m_ItemGroup.transform;
				gameObject2.transform.localPosition = Vector3.zero;
				gameObject2.transform.localScale = localScale2;
				VCEUIMaterialItem component2 = gameObject2.GetComponent<VCEUIMaterialItem>();
				gameObject2.AddComponent<UIDragPanelContents>();
				component2.m_ParentList = this;
				component2.m_Material = s_TempMaterial.Value;
				component2.m_MaterialIcon.gameObject.AddComponent<UIDragPanelContents>();
				m_AssetItems.Add(gameObject2);
			}
		}
		RepositionGrid();
	}

	public void ListFocusOn(VCMaterial focus_item)
	{
		if (focus_item != null)
		{
			Vector3 localPosition = m_Panel.transform.localPosition;
			foreach (GameObject assetItem in m_AssetItems)
			{
				VCEUIMaterialItem component = assetItem.GetComponent<VCEUIMaterialItem>();
				if (component.m_Material == focus_item)
				{
					localPosition.y = m_OriginY - assetItem.transform.localPosition.y - 8f;
				}
			}
			m_Panel.transform.localPosition = localPosition;
		}
		else
		{
			RepositionList();
		}
	}

	public void RefreshMaterialListThenFocusOnSelected()
	{
		RefreshMaterialList(m_MatterPopupList.selection);
		MaterialListFocusOnSelected();
	}

	public void MaterialListFocusOnSelected()
	{
		m_TempSelected = VCEditor.SelectedMaterial;
		if (m_MatterPopupList.selection != "All".ToLocalizationString() && VCEditor.SelectedMaterial != null)
		{
			m_MatterPopupList.selection = VCUtils.Capital(VCConfig.s_Matters[VCEditor.SelectedMaterial.m_MatterId].Name, everyword: true);
		}
		Invoke("MaterialListFocusOnSelectedInvoke", 0.2f);
	}

	private void MaterialListFocusOnSelectedInvoke()
	{
		VCEditor.SelectedMaterial = m_TempSelected;
		m_TempSelected = null;
		ListFocusOn(VCEditor.SelectedMaterial);
	}

	public void OnAddMaterialClick()
	{
		VCEditor.SelectedMaterial = null;
		VCEditor.Instance.m_UI.m_MaterialWindow.ShowWindow(null);
	}
}
