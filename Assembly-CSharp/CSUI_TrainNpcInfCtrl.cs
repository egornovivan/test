using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class CSUI_TrainNpcInfCtrl : MonoBehaviour
{
	[SerializeField]
	private GameObject m_InfoPage;

	[SerializeField]
	private GameObject m_InventoryPage;

	private CSPersonnel m_Npc;

	[SerializeField]
	private UIGrid mAbnormalGrid;

	[SerializeField]
	private CSUI_BuffItem mAbnormalPrefab;

	private bool mReposition;

	private List<CSUI_BuffItem> mAbnormalList = new List<CSUI_BuffItem>(1);

	public CSPersonnel Npc
	{
		get
		{
			return m_Npc;
		}
		set
		{
			if (m_Npc != null && null != m_Npc.m_Npc)
			{
				AbnormalConditionCmpt cmpt = m_Npc.m_Npc.GetCmpt<AbnormalConditionCmpt>();
				if (cmpt != null)
				{
					cmpt.evtStart -= AddNpcAbnormal;
					cmpt.evtEnd -= RemoveNpcAbnormal;
				}
			}
			m_Npc = value;
			if (m_Npc != null && null != m_Npc.m_Npc)
			{
				AbnormalConditionCmpt cmpt2 = m_Npc.m_Npc.GetCmpt<AbnormalConditionCmpt>();
				if (cmpt2 != null)
				{
					cmpt2.evtStart += AddNpcAbnormal;
					cmpt2.evtEnd += RemoveNpcAbnormal;
				}
			}
			RefreshNpcAbnormal();
		}
	}

	private void PageInfoOnActive(bool active)
	{
		m_InfoPage.SetActive(active);
	}

	private void PageInvetoryOnActive(bool active)
	{
		m_InventoryPage.SetActive(active);
	}

	private void Update()
	{
		GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.UpdateTraineeSkillsShow(m_Npc);
		GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.Reflashpackage();
		if (m_Npc == null)
		{
			GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.SetServantInfo("--", PeSex.Male, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
			GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.SetSprSex("null");
		}
		else
		{
			GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.SetServantInfo(m_Npc.FullName, m_Npc.Sex, (int)m_Npc.GetAttribute(AttribType.Hp), (int)m_Npc.GetAttribute(AttribType.HpMax), (int)m_Npc.GetAttribute(AttribType.Stamina), (int)m_Npc.GetAttribute(AttribType.StaminaMax), (int)m_Npc.GetAttribute(AttribType.Hunger), (int)m_Npc.GetAttribute(AttribType.HungerMax), (int)m_Npc.GetAttribute(AttribType.Comfort), (int)m_Npc.GetAttribute(AttribType.ComfortMax), (int)m_Npc.GetAttribute(AttribType.Oxygen), (int)m_Npc.GetAttribute(AttribType.OxygenMax), (int)m_Npc.GetAttribute(AttribType.Shield), (int)m_Npc.GetAttribute(AttribType.ShieldMax), (int)m_Npc.GetAttribute(AttribType.Energy), (int)m_Npc.GetAttribute(AttribType.EnergyMax), (int)m_Npc.GetAttribute(AttribType.Atk), (int)m_Npc.GetAttribute(AttribType.Def));
		}
	}

	private void RefreshNpcAbnormal()
	{
		RemoveAllAbnormal();
		if (m_Npc == null)
		{
			return;
		}
		List<PEAbnormalType> activeAbnormalList = m_Npc.m_Npc.Alnormal.GetActiveAbnormalList();
		if (activeAbnormalList.Count != 0)
		{
			for (int i = 0; i < activeAbnormalList.Count; i++)
			{
				AddNpcAbnormal(activeAbnormalList[i]);
			}
		}
	}

	private void AddNpcAbnormal(PEAbnormalType type)
	{
		AbnormalData data = AbnormalData.GetData(type);
		if (data != null && !(data.iconName == "0"))
		{
			CSUI_BuffItem cSUI_BuffItem = Object.Instantiate(mAbnormalPrefab);
			if (!cSUI_BuffItem.gameObject.activeSelf)
			{
				cSUI_BuffItem.gameObject.SetActive(value: true);
			}
			cSUI_BuffItem.transform.parent = mAbnormalGrid.transform;
			CSUtils.ResetLoacalTransform(cSUI_BuffItem.transform);
			cSUI_BuffItem.SetInfo(data.iconName, data.description);
			mAbnormalList.Add(cSUI_BuffItem);
			mReposition = true;
		}
	}

	private void RemoveNpcAbnormal(PEAbnormalType type)
	{
		AbnormalData data = AbnormalData.GetData(type);
		if (data != null && !(data.iconName == "0"))
		{
			CSUI_BuffItem cSUI_BuffItem = mAbnormalList.Find((CSUI_BuffItem i) => i._icon == data.iconName);
			if (!(cSUI_BuffItem == null))
			{
				Object.Destroy(cSUI_BuffItem.gameObject);
				mAbnormalList.Remove(cSUI_BuffItem);
				mReposition = true;
			}
		}
	}

	private void RemoveAllAbnormal()
	{
		if (mAbnormalList.Count != 0)
		{
			for (int i = 0; i < mAbnormalList.Count; i++)
			{
				Object.Destroy(mAbnormalList[i].gameObject);
				mAbnormalList.Remove(mAbnormalList[i]);
			}
		}
	}

	private void UpdateReposition()
	{
		if (mReposition)
		{
			mReposition = false;
			mAbnormalGrid.repositionNow = true;
		}
	}

	private void LateUpdate()
	{
		UpdateReposition();
	}
}
