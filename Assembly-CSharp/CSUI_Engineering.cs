using System;
using System.Collections.Generic;
using ItemAsset;
using ItemAsset.PackageHelper;
using Pathea;
using UnityEngine;

public class CSUI_Engineering : MonoBehaviour
{
	[Serializable]
	public class MenuPart
	{
		public UICheckbox m_EnhanceCB;

		public UICheckbox m_RepairCB;

		public UICheckbox m_RecyleCB;

		public UIGrid m_Root;
	}

	[Serializable]
	public class MaterilPart
	{
		public CSUI_MaterialGrid m_Prefab;

		public UIGrid m_Root;
	}

	[Serializable]
	public class HandlePart
	{
		public N_ImageButton m_OKBtn;

		public N_ImageButton m_ResetBtn;
	}

	private CSEnhance m_Enhance;

	private CSRepair m_Repair;

	private CSRecycle m_Recycle;

	private int m_CurType;

	private PeEntity player;

	private PlayerPackageCmpt playerPackageCmpt;

	public UIScrollBar mListScrolBar;

	[SerializeField]
	private MenuPart m_MenuPart;

	[SerializeField]
	public CSUI_SubEngneering m_SubEngneering;

	[SerializeField]
	private MaterilPart m_MatPart;

	private List<CSUI_MaterialGrid> m_MatList = new List<CSUI_MaterialGrid>();

	[SerializeField]
	private HandlePart m_Handle;

	[SerializeField]
	private GameObject m_SkillLock;

	public bool IsEmpty()
	{
		return m_Enhance == null && m_Repair == null && m_Recycle == null;
	}

	public void CloseLock()
	{
		if (m_SkillLock != null)
		{
			m_SkillLock.gameObject.SetActive(value: false);
		}
	}

	public void Replace(List<CSEntity> commons, CSEntity selectEntity)
	{
		List<CSCommon> list = new List<CSCommon>();
		if (m_Enhance != null)
		{
			list.Add(m_Enhance);
		}
		if (m_Repair != null)
		{
			list.Add(m_Repair);
		}
		if (m_Recycle != null)
		{
			list.Add(m_Recycle);
		}
		bool flag = false;
		if (commons.Count != list.Count)
		{
			flag = true;
		}
		else
		{
			for (int i = 0; i < commons.Count; i++)
			{
				if (!list.Contains(commons[i] as CSCommon))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			if (m_Enhance != null)
			{
				m_MenuPart.m_EnhanceCB.gameObject.SetActive(value: false);
				m_Enhance.onEnhancedTimeUp = null;
				m_Enhance = null;
			}
			if (m_Repair != null)
			{
				m_MenuPart.m_RepairCB.gameObject.SetActive(value: false);
				m_Repair.onRepairedTimeUp = null;
				m_Repair = null;
			}
			if (m_Recycle != null)
			{
				m_MenuPart.m_RecyleCB.gameObject.SetActive(value: false);
				m_Recycle = null;
			}
			foreach (CSCommon common in commons)
			{
				AddMachine(common);
			}
			CSUI_CommonIcon component = m_MenuPart.m_EnhanceCB.gameObject.GetComponent<CSUI_CommonIcon>();
			component.Common = m_Enhance;
			component = m_MenuPart.m_RepairCB.gameObject.GetComponent<CSUI_CommonIcon>();
			component.Common = m_Repair;
			component = m_MenuPart.m_RecyleCB.gameObject.GetComponent<CSUI_CommonIcon>();
			component.Common = m_Recycle;
		}
		if (selectEntity == null)
		{
			if (m_Enhance != null)
			{
				m_CurType = 4;
			}
			else if (m_Recycle != null)
			{
				m_CurType = 6;
			}
			else if (m_Repair != null)
			{
				m_CurType = 5;
			}
		}
		else if (m_Enhance != null && selectEntity == m_Enhance)
		{
			m_CurType = 4;
		}
		else if (m_Recycle != null && selectEntity == m_Recycle)
		{
			m_CurType = 6;
		}
		else if (m_Repair != null && selectEntity == m_Repair)
		{
			m_CurType = 5;
		}
		SelectByType();
	}

	public void AddMachine(CSEntity entity)
	{
		if (entity.m_Type == 4)
		{
			if (m_Enhance != null && !entity.IsRunning)
			{
				Debug.LogWarning("The enhanced machine is already exist!");
				return;
			}
			m_Enhance = entity as CSEnhance;
			m_MenuPart.m_EnhanceCB.gameObject.SetActive(value: true);
			if (m_MenuPart.m_EnhanceCB.isChecked)
			{
				m_SubEngneering.SetEntity(m_Enhance);
			}
			m_Enhance.onEnhancedTimeUp = OnEnhancedTimeUp;
		}
		else if (entity.m_Type == 5)
		{
			if (m_Repair != null && !entity.IsRunning)
			{
				Debug.LogWarning("The repair machine is already exist!");
				return;
			}
			m_Repair = entity as CSRepair;
			m_MenuPart.m_RepairCB.gameObject.SetActive(value: true);
			if (m_MenuPart.m_RepairCB.isChecked)
			{
				m_SubEngneering.SetEntity(m_Repair);
			}
			m_Repair.onRepairedTimeUp = OnRepairedTimeUp;
		}
		else if (entity.m_Type == 6)
		{
			if (m_Recycle != null && !entity.IsRunning)
			{
				Debug.LogWarning("The recycle machine is already exist!");
				return;
			}
			m_Recycle = entity as CSRecycle;
			m_MenuPart.m_RecyleCB.gameObject.SetActive(value: true);
			if (m_MenuPart.m_RecyleCB.isChecked)
			{
				m_SubEngneering.SetEntity(m_Recycle);
			}
			m_Recycle.onRecylced = OnRecycled;
		}
		m_MenuPart.m_Root.repositionNow = true;
	}

	public void RemoveMachine(CSEntity entity)
	{
		if (entity == m_Enhance)
		{
			if (m_MenuPart.m_EnhanceCB.isChecked)
			{
				if (m_MenuPart.m_RepairCB.gameObject.activeSelf)
				{
					m_CurType = 5;
				}
				else if (m_MenuPart.m_RecyleCB.gameObject.activeSelf)
				{
					m_CurType = 6;
				}
				else
				{
					m_CurType = 12121;
				}
				OnEnable();
			}
			m_MenuPart.m_EnhanceCB.gameObject.SetActive(value: false);
			m_Enhance.onEnhancedTimeUp = null;
			m_Enhance = null;
		}
		else if (entity == m_Repair)
		{
			if (m_MenuPart.m_RepairCB.isChecked)
			{
				if (m_MenuPart.m_EnhanceCB.gameObject.activeSelf)
				{
					m_CurType = 4;
				}
				else if (m_MenuPart.m_RecyleCB.gameObject.activeSelf)
				{
					m_CurType = 6;
				}
				else
				{
					m_CurType = 12121;
				}
				OnEnable();
			}
			m_MenuPart.m_RepairCB.gameObject.SetActive(value: false);
			m_Repair.onRepairedTimeUp = null;
			m_Repair = null;
		}
		else if (entity == m_Recycle)
		{
			if (m_MenuPart.m_RecyleCB.isChecked)
			{
				if (m_MenuPart.m_EnhanceCB.gameObject.activeSelf)
				{
					m_CurType = 4;
				}
				else if (m_MenuPart.m_RepairCB.gameObject.activeSelf)
				{
					m_CurType = 5;
				}
				else
				{
					m_CurType = 12121;
				}
				if (base.gameObject.activeInHierarchy)
				{
					OnEnable();
				}
			}
			m_MenuPart.m_RecyleCB.gameObject.SetActive(value: false);
			m_Recycle = null;
		}
		m_MenuPart.m_Root.repositionNow = true;
	}

	private void OnRecycled()
	{
		if (m_CurType == m_Recycle.m_Type)
		{
			m_SubEngneering.SetItem(null);
		}
	}

	private bool IsUnLock(int type)
	{
		if (GameUI.Instance == null || GameUI.Instance.mSkillWndCtrl == null || GameUI.Instance.mSkillWndCtrl._SkillMgr == null)
		{
			return true;
		}
		return GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckUnlockColony(type);
	}

	private void OnEnhanceActivate(bool active)
	{
		if (active)
		{
			m_SubEngneering.SetEntity(m_Enhance);
			m_CurType = m_Enhance.m_Type;
			m_SkillLock.gameObject.SetActive(!IsUnLock(m_CurType));
		}
	}

	private void OnRepairActivate(bool active)
	{
		if (active)
		{
			m_SubEngneering.SetEntity(m_Repair);
			m_CurType = m_Repair.m_Type;
			m_SkillLock.gameObject.SetActive(!IsUnLock(m_CurType));
		}
	}

	private void OnRecycleActivate(bool active)
	{
		if (active)
		{
			m_SubEngneering.SetEntity(m_Recycle);
			m_CurType = m_Recycle.m_Type;
			m_SkillLock.gameObject.SetActive(!IsUnLock(m_CurType));
		}
	}

	private void OnOKBtn()
	{
		if (m_CurType == 4)
		{
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000097), StartToWork);
		}
		else if (m_CurType == 5)
		{
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000098), StartToWork);
		}
		else if (m_CurType == 6)
		{
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000099), StartToWork);
		}
	}

	private void StartToWork()
	{
		if (!GameConfig.IsMultiMode)
		{
			if (PeSingleton<PeCreature>.Instance == null || !(PeSingleton<PeCreature>.Instance.mainPlayer != null))
			{
				return;
			}
			PlayerPackageCmpt playerPackageCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.packageCmpt as PlayerPackageCmpt;
			if (!(playerPackageCmpt != null))
			{
				return;
			}
			ItemPackage playerPak = playerPackageCmpt.package._playerPak;
			if (m_CurType == 4)
			{
				if (m_Enhance == null || m_Enhance.m_Item == null || m_Enhance.m_Item.protoData == null)
				{
					return;
				}
				foreach (CSUI_MaterialGrid mat in m_MatList)
				{
					playerPak.Destroy(mat.ItemID, mat.NeedCnt);
				}
				m_Enhance.StartCounter();
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToEnhance.GetString(), m_Enhance.m_Item.protoData.GetName()));
				{
					foreach (CSUI_MaterialGrid mat2 in m_MatList)
					{
						Vector3 position = mat2.transform.position;
						CSUI_MainWndCtrl.CreatePopupHint(position, base.transform, new Vector3(10f, -2f, -8f), " - " + mat2.NeedCnt, bGreen: false);
					}
					return;
				}
			}
			if (m_CurType == 5)
			{
				if (m_Repair == null || m_Repair.m_Item == null || m_Repair.m_Item.protoData == null)
				{
					return;
				}
				foreach (CSUI_MaterialGrid mat3 in m_MatList)
				{
					playerPak.Destroy(mat3.ItemID, mat3.NeedCnt);
				}
				m_Repair.StartCounter();
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToRepair.GetString(), m_Repair.m_Item.protoData.GetName()));
			}
			else if (m_CurType == 6 && m_Recycle != null && m_Recycle.m_Item != null && m_Recycle.m_Item.protoData != null)
			{
				m_Recycle.StartCounter();
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToRecycle.GetString(), m_Recycle.m_Item.protoData.GetName()));
			}
		}
		else if (m_CurType == 4)
		{
			m_Enhance._ColonyObj._Network.EHN_Start();
		}
		else if (m_CurType == 5)
		{
			m_Repair._ColonyObj._Network.RPA_Start();
		}
		else if (m_CurType == 6)
		{
			m_Recycle._ColonyObj._Network.RCY_Start();
		}
	}

	private void OnResetBtn()
	{
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000100), StopWork);
	}

	private void StopWork()
	{
		if (!GameConfig.IsMultiMode)
		{
			if (m_CurType == 4)
			{
				m_Enhance.StopCounter();
			}
			else if (m_CurType == 5)
			{
				m_Repair.StopCounter();
			}
			else if (m_CurType == 6)
			{
				m_Recycle.StopCounter();
			}
		}
		else if (m_CurType == 4)
		{
			m_Enhance._ColonyObj._Network.EHN_Stop();
		}
		else if (m_CurType == 5)
		{
			m_Repair._ColonyObj._Network.RPA_Stop();
		}
		else if (m_CurType == 6)
		{
			m_Recycle._ColonyObj._Network.RCY_Stop();
		}
	}

	private void OnEnhanceItemChanged(ItemObject item)
	{
		ClearMaterials();
		if (item == null)
		{
			return;
		}
		Dictionary<int, int> costsItems = m_Enhance.GetCostsItems();
		if (costsItems == null)
		{
			return;
		}
		foreach (KeyValuePair<int, int> item2 in costsItems)
		{
			if (item2.Value != 0)
			{
				AddMaterials(item2.Key, item2.Value);
			}
		}
		m_MatPart.m_Root.repositionNow = true;
	}

	private void OnRepairItemChanged(ItemObject item)
	{
		ClearMaterials();
		if (item == null)
		{
			return;
		}
		Dictionary<int, int> costsItems = m_Repair.GetCostsItems();
		if (costsItems == null)
		{
			return;
		}
		foreach (KeyValuePair<int, int> item2 in costsItems)
		{
			if (item2.Value != 0)
			{
				AddMaterials(item2.Key, item2.Value);
			}
		}
		m_MatPart.m_Root.repositionNow = true;
	}

	private void OnRecycleItemChanged(ItemObject item)
	{
		ClearMaterials();
		if (item == null)
		{
			return;
		}
		Dictionary<int, int> recycleItems = m_Recycle.GetRecycleItems();
		if (recycleItems == null)
		{
			return;
		}
		foreach (KeyValuePair<int, int> item2 in recycleItems)
		{
			if (item2.Value != 0)
			{
				AddMaterials(item2.Key, item2.Value, bUseColors: false);
			}
		}
		m_MatPart.m_Root.repositionNow = true;
	}

	private void OnEnhancedTimeUp(Strengthen item)
	{
		m_SubEngneering.UpdatePopupHintInfo(m_Enhance);
		m_SubEngneering.ItemGrid.PlayGlow(forward: true);
	}

	private void OnRepairedTimeUp(Repair item)
	{
		m_SubEngneering.UpdatePopupHintInfo(m_Repair);
	}

	private void ClearMaterials()
	{
		foreach (CSUI_MaterialGrid mat in m_MatList)
		{
			if (mat.gameObject != null)
			{
				UnityEngine.Object.DestroyImmediate(mat.gameObject);
			}
		}
		m_MatList.Clear();
		mListScrolBar.scrollValue = 0f;
	}

	private void AddMaterials(int item_id, int item_count, bool bUseColors = true)
	{
		CSUI_MaterialGrid cSUI_MaterialGrid = UnityEngine.Object.Instantiate(m_MatPart.m_Prefab);
		cSUI_MaterialGrid.transform.parent = m_MatPart.m_Root.transform;
		cSUI_MaterialGrid.transform.localPosition = Vector3.zero;
		cSUI_MaterialGrid.transform.localRotation = Quaternion.identity;
		cSUI_MaterialGrid.transform.localScale = Vector3.one;
		cSUI_MaterialGrid.ItemID = item_id;
		cSUI_MaterialGrid.NeedCnt = item_count;
		cSUI_MaterialGrid.bUseColors = bUseColors;
		m_MatList.Add(cSUI_MaterialGrid);
	}

	private void OnEnable()
	{
		SelectByType();
	}

	private void SelectByType()
	{
		if (m_CurType == 4)
		{
			m_MenuPart.m_EnhanceCB.isChecked = true;
			OnEnhanceActivate(active: true);
		}
		else if (m_CurType == 5)
		{
			m_MenuPart.m_RepairCB.isChecked = true;
			OnRepairActivate(active: true);
		}
		else if (m_CurType == 6)
		{
			m_MenuPart.m_RecyleCB.isChecked = true;
			OnRecycleActivate(active: true);
		}
	}

	private void Awake()
	{
		m_SubEngneering.onEnhancedItemChanged = OnEnhanceItemChanged;
		m_SubEngneering.onRepairedItemChanged = OnRepairItemChanged;
		m_SubEngneering.onRecycleItemChanged = OnRecycleItemChanged;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return;
		}
		if (playerPackageCmpt == null || player == null || player != PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			player = PeSingleton<PeCreature>.Instance.mainPlayer;
			playerPackageCmpt = player.GetCmpt<PlayerPackageCmpt>();
		}
		bool flag = true;
		if (m_CurType != 6)
		{
			foreach (CSUI_MaterialGrid mat in m_MatList)
			{
				mat.ItemNum = playerPackageCmpt.package.GetCount(mat.ItemID);
				if (mat.ItemNum < mat.NeedCnt)
				{
					flag = false;
				}
			}
		}
		else
		{
			foreach (CSUI_MaterialGrid mat2 in m_MatList)
			{
				mat2.ItemNum = 0;
			}
		}
		if (m_CurType == 4)
		{
			if (m_Enhance.m_Item == null || !m_Enhance.IsRunning)
			{
				m_Handle.m_OKBtn.isEnabled = false;
				m_Handle.m_ResetBtn.isEnabled = false;
				return;
			}
			if (!m_Enhance.isDeleting && !m_Enhance.IsEnhancing && flag)
			{
				m_Handle.m_OKBtn.isEnabled = true;
			}
			else
			{
				m_Handle.m_OKBtn.isEnabled = false;
			}
			if (flag)
			{
				m_Handle.m_ResetBtn.isEnabled = !m_Handle.m_OKBtn.isEnabled;
			}
			else
			{
				m_Handle.m_ResetBtn.isEnabled = false;
			}
		}
		else if (m_CurType == 5)
		{
			if (m_Repair.m_Item == null || !m_Repair.IsRunning || m_Repair.m_Item.GetValue().ExpendValue == 0f)
			{
				m_Handle.m_OKBtn.isEnabled = false;
				m_Handle.m_ResetBtn.isEnabled = false;
				return;
			}
			if (m_Repair.m_Item != null && m_Repair.IsFull())
			{
				m_Handle.m_OKBtn.isEnabled = false;
			}
			else if (!m_Repair.isDeleting && !m_Repair.IsRepairingM && flag)
			{
				m_Handle.m_OKBtn.isEnabled = true;
			}
			else
			{
				m_Handle.m_OKBtn.isEnabled = false;
			}
			if (flag)
			{
				m_Handle.m_ResetBtn.isEnabled = !m_Handle.m_OKBtn.isEnabled;
			}
			else
			{
				m_Handle.m_ResetBtn.isEnabled = false;
			}
		}
		else
		{
			if (m_CurType != 6)
			{
				return;
			}
			if (m_Recycle.m_Item != null && !m_Recycle.isDeleting && !m_Recycle.IsRecycling && m_Recycle.IsRunning)
			{
				m_Handle.m_OKBtn.isEnabled = true;
			}
			else
			{
				m_Handle.m_OKBtn.isEnabled = false;
			}
			if (m_Recycle.m_Item == null || !m_Recycle.IsRunning)
			{
				m_Handle.m_OKBtn.isEnabled = false;
				m_Handle.m_ResetBtn.isEnabled = false;
				return;
			}
			if (!m_Recycle.isDeleting && !m_Recycle.IsRecycling)
			{
				m_Handle.m_OKBtn.isEnabled = true;
			}
			else
			{
				m_Handle.m_OKBtn.isEnabled = false;
			}
			m_Handle.m_ResetBtn.isEnabled = !m_Handle.m_OKBtn.isEnabled;
		}
	}

	public void StartWorkerResult(int type, CSEntity m_entity, string rolename)
	{
		if (type != m_CurType)
		{
			return;
		}
		if (m_CurType == 4)
		{
			if (m_Enhance != m_entity)
			{
				return;
			}
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToEnhance.GetString(), m_Enhance.m_Item.protoData.GetName()));
			if (!(PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<EntityInfoCmpt>().characterName.givenName == rolename))
			{
				return;
			}
			{
				foreach (CSUI_MaterialGrid mat in m_MatList)
				{
					Vector3 position = mat.transform.position;
					CSUI_MainWndCtrl.CreatePopupHint(position, base.transform, new Vector3(10f, -2f, -8f), " - " + mat.NeedCnt, bGreen: false);
				}
				return;
			}
		}
		if (m_CurType == 5)
		{
			if (m_Repair == m_entity)
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToRepair.GetString(), m_Repair.m_Item.protoData.GetName()));
			}
		}
		else if (m_CurType == 6 && m_Recycle == m_entity)
		{
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mStartToRecycle.GetString(), m_Recycle.m_Item.protoData.GetName()));
		}
	}
}
