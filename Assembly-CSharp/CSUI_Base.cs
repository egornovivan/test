using System;
using UnityEngine;

public class CSUI_Base : MonoBehaviour
{
	[Serializable]
	public class DurabilityPart
	{
		public UISlider m_Slider;

		public UILabel m_Val;
	}

	[Serializable]
	public class STBPart
	{
		public UISlider m_Slider;

		public UILabel m_TimeLb;

		public N_ImageButton m_Button;
	}

	public CSEntity m_Entity;

	[SerializeField]
	private DurabilityPart m_Durability;

	[SerializeField]
	private STBPart m_Repair;

	[SerializeField]
	private STBPart m_Delete;

	[SerializeField]
	private CSUI_QueryMat m_QueryMatUIPrefab;

	private CSUI_QueryMat m_QueryMatWnd;

	public Transform m_QueryMatRoot;

	private void UpdateRepair()
	{
		if (m_Entity.isRepairing)
		{
			float num = m_Entity.BaseData.m_RepairTime - m_Entity.BaseData.m_CurRepairTime;
			float sliderValue = m_Entity.BaseData.m_CurRepairTime / m_Entity.BaseData.m_RepairTime;
			m_Repair.m_Slider.sliderValue = sliderValue;
			m_Repair.m_TimeLb.text = CSUtils.GetRealTimeMS((int)num);
		}
		else
		{
			m_Repair.m_Slider.sliderValue = 0f;
			m_Repair.m_TimeLb.text = CSUtils.GetRealTimeMS(0);
		}
	}

	private void UpdateDelete()
	{
		if (m_Entity.isDeleting)
		{
			float num = m_Entity.BaseData.m_DeleteTime - m_Entity.BaseData.m_CurDeleteTime;
			float sliderValue = m_Entity.BaseData.m_CurDeleteTime / m_Entity.BaseData.m_DeleteTime;
			m_Delete.m_Slider.sliderValue = sliderValue;
			m_Delete.m_TimeLb.text = CSUtils.GetRealTimeMS((int)num);
		}
		else
		{
			m_Delete.m_Slider.sliderValue = 0f;
			m_Delete.m_TimeLb.text = CSUtils.GetRealTimeMS(0);
		}
	}

	private void UpdateDurability()
	{
		float sliderValue = m_Entity.BaseData.m_Durability / m_Entity.m_Info.m_Durability;
		m_Durability.m_Slider.sliderValue = sliderValue;
		string empty = string.Empty;
		empty += Mathf.RoundToInt(m_Entity.BaseData.m_Durability);
		empty += " / ";
		empty += Mathf.RoundToInt(m_Entity.m_Info.m_Durability);
		m_Durability.m_Val.text = empty;
	}

	private void OnRepairBtn()
	{
		if (m_QueryMatWnd == null)
		{
			m_QueryMatWnd = UnityEngine.Object.Instantiate(m_QueryMatUIPrefab);
			m_QueryMatWnd.transform.parent = m_QueryMatRoot;
			m_QueryMatWnd.transform.localPosition = new Vector3(0f, 0f, -30f);
			m_QueryMatWnd.transform.localRotation = Quaternion.identity;
			m_QueryMatWnd.transform.localScale = Vector3.one;
			m_QueryMatWnd.funcType = CSUI_QueryMat.FuncType.Repair;
			m_QueryMatWnd.m_Entity = m_Entity;
			if (CSUI_MainWndCtrl.Instance != null)
			{
				CSUI_MainWndCtrl.Instance.m_ChildWindowOfBed = m_QueryMatWnd.gameObject;
			}
		}
	}

	private void OnDeleteBtn()
	{
		if (m_QueryMatWnd == null)
		{
			m_QueryMatWnd = UnityEngine.Object.Instantiate(m_QueryMatUIPrefab);
			m_QueryMatWnd.transform.parent = m_QueryMatRoot;
			m_QueryMatWnd.transform.localPosition = new Vector3(0f, 0f, -15f);
			m_QueryMatWnd.transform.localRotation = Quaternion.identity;
			m_QueryMatWnd.transform.localScale = Vector3.one;
			m_QueryMatWnd.funcType = CSUI_QueryMat.FuncType.Delete;
			m_QueryMatWnd.m_Entity = m_Entity;
			if (CSUI_MainWndCtrl.Instance != null)
			{
				CSUI_MainWndCtrl.Instance.m_ChildWindowOfBed = m_QueryMatWnd.gameObject;
			}
		}
	}

	protected void Start()
	{
	}

	protected void Update()
	{
		if (m_Entity == null)
		{
			return;
		}
		UpdateRepair();
		UpdateDelete();
		UpdateDurability();
		m_Delete.m_Button.isEnabled = !m_Entity.isDeleting;
		if (m_Delete.m_Button.isEnabled)
		{
			if (m_Entity.m_Info.m_Durability <= (float)Mathf.CeilToInt(m_Entity.BaseData.m_Durability))
			{
				m_Repair.m_Button.isEnabled = false;
			}
			else
			{
				m_Repair.m_Button.isEnabled = !m_Entity.isRepairing;
			}
		}
		else
		{
			m_Repair.m_Button.isEnabled = false;
		}
	}
}
