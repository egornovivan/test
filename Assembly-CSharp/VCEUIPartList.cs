using System.Collections.Generic;
using UnityEngine;

public class VCEUIPartList : VCEUIAssetList
{
	public UIPopupList m_TypePopupList;

	private string m_lastSelection;

	private new void Update()
	{
		base.Update();
		if (m_lastSelection != m_TypePopupList.selection)
		{
			m_lastSelection = m_TypePopupList.selection;
			RefreshPartList(m_TypePopupList.selection);
			VCEditor.SelectedPart = null;
			RepositionList();
		}
	}

	public void InitTypeList()
	{
		m_TypePopupList.items.Clear();
		m_TypePopupList.items.Add("All".ToLocalizationString());
		List<EVCComponent> partTypes = VCConfig.s_Categories[VCEditor.s_Scene.m_Setting.m_Category].m_PartTypes;
		bool flag = false;
		foreach (EVCComponent item in partTypes)
		{
			if (item != 0 && ((item != EVCComponent.cpVehicleFuelCell && item != EVCComponent.cpVtolFuelCell) || (!flag && (flag = true))))
			{
				m_TypePopupList.items.Add(VCUtils.Capital(VCConfig.s_PartTypes[item].m_ShortName, everyword: true).ToLocalizationString());
			}
		}
		m_TypePopupList.selection = "All".ToLocalizationString();
	}

	public void RefreshPartList(string filter)
	{
		ClearItems();
		List<EVCComponent> list = new List<EVCComponent>(2);
		List<EVCComponent> partTypes = VCConfig.s_Categories[VCEditor.s_Scene.m_Setting.m_Category].m_PartTypes;
		filter = filter.ToLower();
		foreach (EVCComponent item in partTypes)
		{
			if (filter == VCConfig.s_PartTypes[item].m_ShortName.ToLower())
			{
				list.Add(item);
			}
		}
		int num = 0;
		foreach (KeyValuePair<int, VCPartInfo> s_Part in VCConfig.s_Parts)
		{
			int num2 = -1;
			for (int i = 0; i < partTypes.Count; i++)
			{
				if (s_Part.Value.m_Type == partTypes[i])
				{
					num2 = i;
					break;
				}
			}
			if (num2 >= 0 && (list.Count == 0 || list.Contains(s_Part.Value.m_Type)))
			{
				GameObject gameObject = Object.Instantiate(m_ItemRes);
				Vector3 localScale = gameObject.transform.localScale;
				string text = num2.ToString("00");
				int num3 = ++num;
				gameObject.name = text + num3.ToString("000") + " " + s_Part.Value.m_Name;
				gameObject.transform.parent = m_ItemGroup.transform;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localScale = localScale;
				VCEUIPartItem component = gameObject.GetComponent<VCEUIPartItem>();
				component.m_HoverBtn.AddComponent<UIDragPanelContents>();
				component.m_ParentList = this;
				component.m_PartInfo = s_Part.Value;
				m_AssetItems.Add(gameObject);
			}
		}
		RepositionGrid();
	}
}
