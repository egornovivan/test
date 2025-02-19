using ItemAsset;
using UnityEngine;

public class CSUI_WorkRoom : MonoBehaviour
{
	public CSCommon m_RefCommon;

	public CSUI_Personnel m_PersonnelUI;

	public CSPersonnel m_RefNpc;

	[SerializeField]
	private UISlicedSprite m_IconSprite;

	[SerializeField]
	private UILabel m_NPCCntLb;

	[SerializeField]
	private UILabel m_BuildingNameLb;

	[SerializeField]
	private UIButton m_LivingBtn;

	[SerializeField]
	private Color m_NormalColor;

	[SerializeField]
	private Color m_LivingColor;

	private bool m_Active;

	public string IconSpriteName
	{
		get
		{
			return m_IconSprite.spriteName;
		}
		set
		{
			m_IconSprite.spriteName = value;
		}
	}

	public void Activate(bool active)
	{
		m_Active = active;
	}

	private void OnLivingRoomClick()
	{
		if (m_RefNpc != null && (m_RefNpc.m_Occupation == 1 || m_RefNpc.m_Occupation == 6) && m_RefCommon.WorkerCount < m_RefCommon.WorkerMaxCount && m_RefNpc.WorkRoom != m_RefCommon)
		{
			m_RefNpc.TrySetWorkRoom(m_RefCommon);
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mSetWorkRoom.GetString(), m_RefNpc.FullName, CSUtils.GetEntityName(m_RefCommon.m_Type)));
		}
	}

	private void Start()
	{
		if (m_RefNpc != null)
		{
			Activate(m_RefNpc.Running);
		}
	}

	private void Update()
	{
		if (m_RefNpc == null)
		{
			return;
		}
		if (m_RefNpc != null)
		{
			if (m_RefNpc.WorkRoom == m_RefCommon)
			{
				m_NPCCntLb.color = m_LivingColor;
				m_BuildingNameLb.color = m_LivingColor;
			}
			else
			{
				m_NPCCntLb.color = m_NormalColor;
				m_BuildingNameLb.color = m_NormalColor;
			}
		}
		if (m_RefCommon == null)
		{
			return;
		}
		int workerCount = m_RefCommon.WorkerCount;
		int workerMaxCount = m_RefCommon.WorkerMaxCount;
		m_NPCCntLb.text = "[" + m_RefCommon.WorkerCount + "/" + m_RefCommon.WorkerMaxCount + "]";
		ItemProto itemData = ItemProto.GetItemData(m_RefCommon.ItemID);
		if (itemData != null)
		{
			string[] icon = ItemProto.GetItemData(m_RefCommon.ItemID).icon;
			if (icon.Length != 0)
			{
				m_IconSprite.spriteName = icon[0];
			}
			else
			{
				m_IconSprite.spriteName = string.Empty;
			}
		}
		m_BuildingNameLb.text = m_RefCommon.Name;
		if (workerCount >= workerMaxCount || !m_Active)
		{
			m_LivingBtn.isEnabled = false;
		}
		else
		{
			m_LivingBtn.isEnabled = true;
		}
	}
}
