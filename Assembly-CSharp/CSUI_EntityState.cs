using ItemAsset;
using UnityEngine;

public class CSUI_EntityState : MonoBehaviour
{
	[SerializeField]
	private UISlicedSprite m_IconUI;

	[SerializeField]
	private UILabel m_NameUI;

	[SerializeField]
	private UILabel m_LifeUI;

	[SerializeField]
	private UISlider m_LifeProgressUI;

	public CSEntity m_RefCommon;

	private void Start()
	{
	}

	private void Update()
	{
		if (m_RefCommon == null)
		{
			return;
		}
		m_NameUI.text = m_RefCommon.Name;
		ItemProto itemData = ItemProto.GetItemData(m_RefCommon.ItemID);
		if (itemData != null)
		{
			string[] icon = ItemProto.GetItemData(m_RefCommon.ItemID).icon;
			if (icon.Length != 0)
			{
				m_IconUI.spriteName = icon[0];
			}
			else
			{
				m_IconUI.spriteName = string.Empty;
			}
		}
		float sliderValue = m_RefCommon.BaseData.m_Durability / m_RefCommon.m_Info.m_Durability;
		m_LifeProgressUI.sliderValue = sliderValue;
		string empty = string.Empty;
		empty += Mathf.RoundToInt(m_RefCommon.BaseData.m_Durability);
		empty += " / ";
		empty += Mathf.RoundToInt(m_RefCommon.m_Info.m_Durability);
		m_LifeUI.text = empty;
	}
}
