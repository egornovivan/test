using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;
using WhiteCat;

public class CSUI_SubEngneering : MonoBehaviour
{
	[Serializable]
	public class ItemPart
	{
		public CSUI_Grid m_GridPrefab;

		public Transform m_Root;

		public UILabel m_Name;

		public UILabel m_CostsTime;
	}

	[Serializable]
	public class EnhancePropertyPart
	{
		public Transform m_Root;

		public UILabel m_Durability;

		public UILabel m_Atk;

		public UILabel m_Defense;

		public UILabel m_TimesEnhcance;
	}

	[Serializable]
	public class RepairPropertyPart
	{
		public Transform m_Root;

		public UILabel m_Durability;
	}

	[Serializable]
	public class RecyclePropertyPart
	{
		public Transform m_Root;

		public UILabel m_Durability;
	}

	[Serializable]
	public class NPCPart
	{
		public UIGrid m_Root;

		public CSUI_NPCGrid m_Prefab;

		public UIButton m_AuttoSettleBtn;

		public UIButton m_DisbandAllBtn;
	}

	[Serializable]
	public class PopupHintsPart
	{
		public GameObject m_EnhanceHintGo;

		public GameObject m_RepairHintGo;

		public GameObject m_RecyleHintGo;
	}

	public delegate void ItemDelegate(ItemObject item);

	public int m_Type;

	public CSEntity m_Entity;

	public ItemDelegate onEnhancedItemChanged;

	public ItemDelegate onRepairedItemChanged;

	public ItemDelegate onRecycleItemChanged;

	public Strengthen m_enhanceItem;

	public Repair m_repairItem;

	public Recycle m_recycleItem;

	[SerializeField]
	private ItemPart m_ItemPart;

	private CSUI_Grid m_ItemGrid;

	[SerializeField]
	private EnhancePropertyPart m_EnhanceProperty;

	[SerializeField]
	private RepairPropertyPart m_RepairProperty;

	[SerializeField]
	private RecyclePropertyPart m_RecycleProperty;

	[SerializeField]
	private NPCPart m_NpcPart;

	private List<CSUI_NPCGrid> m_NpcGirds = new List<CSUI_NPCGrid>();

	public Dictionary<int, List<CSUI_PopupHint>> m_PopupHints = new Dictionary<int, List<CSUI_PopupHint>>();

	[SerializeField]
	private PopupHintsPart m_PopupHintsPart;

	public CSUI_Grid ItemGrid => m_ItemGrid;

	public void SetEntity(CSCommon entity)
	{
		if (entity != null && entity.m_Type != 4 && entity.m_Type != 5 && entity.m_Type != 6)
		{
			Debug.Log("The giving Entity is not allowed!");
			return;
		}
		m_Type = entity.m_Type;
		m_Entity = entity;
		CSUI_MainWndCtrl.Instance.mSelectedEnntity = m_Entity;
		if (m_Type == 4)
		{
			CSEnhance cSEnhance = m_Entity as CSEnhance;
			if (cSEnhance.m_Item == null)
			{
				SetItem(null);
			}
			else
			{
				SetItem(cSEnhance.m_Item.itemObj);
			}
		}
		else if (m_Type == 5)
		{
			CSRepair cSRepair = m_Entity as CSRepair;
			if (cSRepair.m_Item == null)
			{
				SetItem(null);
			}
			else
			{
				SetItem(cSRepair.m_Item.itemObj);
			}
		}
		else if (m_Type == 6)
		{
			CSRecycle cSRecycle = m_Entity as CSRecycle;
			if (cSRecycle.m_Item == null)
			{
				SetItem(null);
			}
			else
			{
				SetItem(cSRecycle.m_Item.itemObj);
			}
		}
	}

	public bool SetItem(ItemObject item)
	{
		if (m_Entity == null)
		{
			return false;
		}
		if (m_ItemGrid == null)
		{
			InitItemGrid();
		}
		if (item == null)
		{
			m_ItemGrid.m_Grid.SetItem(null);
			m_enhanceItem = null;
			m_repairItem = null;
			m_recycleItem = null;
			OnItemChanged(null);
			return true;
		}
		if (m_Type == 4)
		{
			Strengthen cmpt = item.GetCmpt<Strengthen>();
			if (cmpt == null)
			{
				return false;
			}
			m_ItemGrid.m_Grid.SetItem(item);
			m_enhanceItem = cmpt;
			m_repairItem = null;
			m_recycleItem = null;
			OnItemChanged(item);
		}
		else if (m_Type == 5)
		{
			Repair cmpt2 = item.GetCmpt<Repair>();
			if (cmpt2 == null)
			{
				return false;
			}
			m_ItemGrid.m_Grid.SetItem(item);
			m_enhanceItem = null;
			m_repairItem = cmpt2;
			m_recycleItem = null;
			OnItemChanged(item);
		}
		else if (m_Type == 6)
		{
			Recycle cmpt3 = item.GetCmpt<Recycle>();
			if (cmpt3 == null)
			{
				return false;
			}
			m_ItemGrid.m_Grid.SetItem(item);
			m_enhanceItem = null;
			m_repairItem = null;
			m_recycleItem = cmpt3;
			OnItemChanged(item);
		}
		return true;
	}

	public void UpdatePopupHintInfo(CSEntity entiy)
	{
		if (entiy == null)
		{
			return;
		}
		int type = entiy.m_Type;
		switch (type)
		{
		case 4:
		{
			if (m_enhanceItem == null)
			{
				break;
			}
			float num = 0f;
			float num2 = 0f;
			if (!m_PopupHints.ContainsKey(type))
			{
				m_PopupHints.Add(type, new List<CSUI_PopupHint>());
			}
			foreach (CSUI_PopupHint item3 in m_PopupHints[type])
			{
				if (item3 != null && item3.gameObject != null)
				{
					UnityEngine.Object.Destroy(item3.gameObject);
				}
			}
			m_PopupHints[type].Clear();
			Vector3 position2 = m_EnhanceProperty.m_Durability.transform.position;
			num = m_enhanceItem.GetCurMaxDurability();
			num2 = m_enhanceItem.GetNextMaxDurability();
			string text2 = " + " + Mathf.FloorToInt(num2 - num);
			CSUI_PopupHint item2 = CSUI_MainWndCtrl.CreatePopupHint(position2, m_PopupHintsPart.m_EnhanceHintGo.transform, new Vector3(10f, -2f, -6f), text2);
			m_PopupHints[type].Add(item2);
			position2 = m_EnhanceProperty.m_Atk.transform.position;
			num = m_enhanceItem.GetCurLevelProperty(AttribType.Atk);
			num2 = m_enhanceItem.GetNextLevelProperty(AttribType.Atk);
			text2 = " + " + Mathf.FloorToInt(num2 - num);
			item2 = CSUI_MainWndCtrl.CreatePopupHint(position2, m_PopupHintsPart.m_EnhanceHintGo.transform, new Vector3(10f, -2f, -6f), text2);
			m_PopupHints[type].Add(item2);
			position2 = m_EnhanceProperty.m_Defense.transform.position;
			num = m_enhanceItem.GetCurLevelProperty(AttribType.Def);
			num2 = m_enhanceItem.GetNextLevelProperty(AttribType.Def);
			text2 = " + " + Mathf.FloorToInt(num2 - num);
			item2 = CSUI_MainWndCtrl.CreatePopupHint(position2, m_PopupHintsPart.m_EnhanceHintGo.transform, new Vector3(10f, -2f, -6f), text2);
			m_PopupHints[type].Add(item2);
			break;
		}
		case 5:
		{
			CSRepair cSRepair = entiy as CSRepair;
			if (!m_PopupHints.ContainsKey(type))
			{
				m_PopupHints.Add(type, new List<CSUI_PopupHint>());
			}
			foreach (CSUI_PopupHint item4 in m_PopupHints[type])
			{
				if (item4 != null && item4.gameObject != null)
				{
					UnityEngine.Object.Destroy(item4.gameObject);
				}
			}
			m_PopupHints[type].Clear();
			Vector3 position = m_EnhanceProperty.m_Durability.transform.position;
			float increasingDura = cSRepair.GetIncreasingDura();
			string text = " + " + Mathf.FloorToInt(increasingDura);
			CSUI_PopupHint item = CSUI_MainWndCtrl.CreatePopupHint(position, m_PopupHintsPart.m_RepairHintGo.transform, new Vector3(10f, -2f, 0f), text);
			m_PopupHints[type].Add(item);
			break;
		}
		}
	}

	private void OnItemChanged(ItemObject item)
	{
		if (m_Type == 4)
		{
			CSEnhance cSEnhance = m_Entity as CSEnhance;
			cSEnhance.m_Item = m_enhanceItem;
			m_EnhanceProperty.m_Root.gameObject.SetActive(value: true);
			m_RepairProperty.m_Root.gameObject.SetActive(value: false);
			m_RecycleProperty.m_Root.gameObject.SetActive(value: false);
			if (onEnhancedItemChanged != null)
			{
				onEnhancedItemChanged(item);
			}
		}
		else if (m_Type == 5)
		{
			CSRepair cSRepair = m_Entity as CSRepair;
			cSRepair.m_Item = m_repairItem;
			m_EnhanceProperty.m_Root.gameObject.SetActive(value: false);
			m_RepairProperty.m_Root.gameObject.SetActive(value: true);
			m_RecycleProperty.m_Root.gameObject.SetActive(value: false);
			if (onRepairedItemChanged != null)
			{
				onRepairedItemChanged(item);
			}
		}
		else if (m_Type == 6)
		{
			CSRecycle cSRecycle = m_Entity as CSRecycle;
			cSRecycle.m_Item = m_recycleItem;
			m_EnhanceProperty.m_Root.gameObject.SetActive(value: false);
			m_RepairProperty.m_Root.gameObject.SetActive(value: false);
			m_RecycleProperty.m_Root.gameObject.SetActive(value: true);
			if (onRecycleItemChanged != null)
			{
				onRecycleItemChanged(item);
			}
		}
	}

	private void InitItemGrid()
	{
		m_ItemGrid = UnityEngine.Object.Instantiate(m_ItemPart.m_GridPrefab);
		m_ItemGrid.transform.parent = m_ItemPart.m_Root;
		m_ItemGrid.transform.localPosition = Vector3.zero;
		m_ItemGrid.transform.localRotation = Quaternion.identity;
		m_ItemGrid.transform.localScale = Vector3.one;
		m_ItemGrid.m_Active = true;
		m_ItemGrid.onCheckItem = OnGridCheckItem;
		m_ItemGrid.OnItemChanged = OnGirdItemChanged;
		if (GameConfig.IsMultiMode)
		{
			m_ItemGrid.OnDropItemMulti = OnDropItemMulti;
			m_ItemGrid.OnLeftMouseClickedMulti = OnLeftMouseClickedMulti;
			m_ItemGrid.OnRightMouseClickedMulti = OnRightMouseClickedMulti;
		}
	}

	private bool OnGridCheckItem(ItemObject item, CSUI_Grid.ECheckItemType check_type)
	{
		if (!CSUI_MainWndCtrl.IsWorking())
		{
			return false;
		}
		if (!m_Entity.IsRunning)
		{
			if (!(m_Entity is CSCommon cSCommon))
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantWorkWithoutElectricity.GetString(), CSUtils.GetEntityName(m_Type)), Color.red);
			}
			else if (cSCommon.Assembly == null)
			{
				CSUI_MainWndCtrl.ShowStatusBar("The machine is invalid.", Color.red);
			}
			else
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantWorkWithoutElectricity.GetString(), CSUtils.GetEntityName(m_Type)), Color.red);
			}
			return false;
		}
		if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar)
		{
			return false;
		}
		if (m_Type == 4)
		{
			if ((m_Entity as CSEnhance).IsEnhancing)
			{
				if (m_enhanceItem != null)
				{
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mHasBeenEnhancingTheItem.GetString(), m_enhanceItem.protoData.GetName()), Color.red);
				}
				return false;
			}
			if (item != null)
			{
				Strengthen cmpt = item.GetCmpt<Strengthen>();
				if (cmpt == null)
				{
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(8000180), item.protoData.GetName()), Color.red);
					return false;
				}
				if (cmpt.strengthenTime >= 100)
				{
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(8000181), item.protoData.GetName()), Color.red);
					return false;
				}
			}
		}
		else if (m_Type == 5)
		{
			if ((m_Entity as CSRepair).IsRepairingM)
			{
				if (m_repairItem != null)
				{
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mHasBeenRepairingTheItem.GetString(), m_repairItem.protoData.GetName()), Color.red);
				}
				return false;
			}
			if (item != null)
			{
				Repair cmpt2 = item.GetCmpt<Repair>();
				if (cmpt2 == null)
				{
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mNotRequireRepair.GetString(), item.protoData.GetName()), Color.red);
					return false;
				}
			}
		}
		else if (m_Type == 6)
		{
			if ((m_Entity as CSRecycle).IsRecycling)
			{
				if (m_recycleItem != null)
				{
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mHasBeenRecyclingTheItem.GetString(), m_recycleItem.protoData.GetName()), Color.red);
				}
				return false;
			}
			if (item != null)
			{
				Recycle cmpt3 = item.GetCmpt<Recycle>();
				if (cmpt3 == null || cmpt3.GetRecycleItems() == null)
				{
					if (cmpt3 != null)
					{
						Debug.LogError(item.nameText + " " + item.protoId + " should not have Recycle!");
					}
					CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mCantRecycle.GetString(), item.protoData.GetName()), Color.red);
					return false;
				}
			}
		}
		return true;
	}

	private void OnGirdItemChanged(ItemObject item, ItemObject oldItem, int index)
	{
		if (oldItem != null)
		{
			if (item == null)
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayFromMachine.GetString(), oldItem.protoData.GetName(), CSUtils.GetEntityName(m_Type)));
			}
			else if (item != oldItem)
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutIntoMachine.GetString(), item.protoData.GetName(), CSUtils.GetEntityName(m_Type)));
			}
		}
		else if (item != null)
		{
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutIntoMachine.GetString(), item.protoData.GetName(), CSUtils.GetEntityName(m_Type)));
		}
		if (item != null)
		{
			m_enhanceItem = item.GetCmpt<Strengthen>();
			m_repairItem = item.GetCmpt<Repair>();
			m_recycleItem = item.GetCmpt<Recycle>();
		}
		else
		{
			m_enhanceItem = null;
			m_repairItem = null;
			m_recycleItem = null;
		}
		OnItemChanged(item);
	}

	private void OnAutoSettleBtn()
	{
		if (m_Entity is CSCommon cSCommon)
		{
			cSCommon.AutoSettleWorkers();
		}
	}

	private void OnDisbandAllBtn()
	{
		if (m_Entity is CSCommon cSCommon)
		{
			cSCommon.ClearWorkers();
		}
	}

	private void Start()
	{
		for (int i = 0; i < 4; i++)
		{
			CSUI_NPCGrid cSUI_NPCGrid = UnityEngine.Object.Instantiate(m_NpcPart.m_Prefab);
			cSUI_NPCGrid.transform.parent = m_NpcPart.m_Root.transform;
			cSUI_NPCGrid.transform.localPosition = Vector3.zero;
			cSUI_NPCGrid.transform.localRotation = Quaternion.identity;
			cSUI_NPCGrid.transform.localScale = Vector3.one;
			cSUI_NPCGrid.NpcIconRadio = m_NpcPart.m_Root.transform;
			m_NpcGirds.Add(cSUI_NPCGrid);
		}
		m_NpcPart.m_Root.repositionNow = true;
	}

	private void Update()
	{
		if (m_ItemGrid != null)
		{
			ItemObject itemObj = m_ItemGrid.m_Grid.ItemObj;
			if (itemObj == null)
			{
				m_ItemPart.m_Name.text = string.Empty;
				if (m_Type == 4)
				{
					m_EnhanceProperty.m_Durability.text = "0 ([00BBFF] +0 [ffffff])";
					m_EnhanceProperty.m_Atk.text = "0 ([00BBFF] +0 [ffffff])";
					m_EnhanceProperty.m_Defense.text = "0 ([00BBFF] +0 [ffffff])";
					m_EnhanceProperty.m_TimesEnhcance.text = "0";
				}
				else if (m_Type == 5)
				{
					m_RepairProperty.m_Durability.text = "0 ([00BBFF] +0 [ffffff])";
				}
				else if (m_Type == 6)
				{
					m_RecycleProperty.m_Durability.text = "0 ([00BBFF] +0 [ffffff])";
				}
				m_ItemPart.m_CostsTime.text = "00:00";
				m_PopupHintsPart.m_EnhanceHintGo.SetActive(value: false);
				m_PopupHintsPart.m_RepairHintGo.SetActive(value: false);
				m_PopupHintsPart.m_RecyleHintGo.SetActive(value: false);
			}
			else
			{
				m_ItemPart.m_Name.text = itemObj.protoData.name;
				float num = 0f;
				float num2 = 0f;
				if (m_Type == 4)
				{
					if (m_enhanceItem != null)
					{
						num = m_enhanceItem.GetCurLevelProperty(AttribType.Atk);
						num2 = m_enhanceItem.GetNextLevelProperty(AttribType.Atk);
						m_EnhanceProperty.m_Atk.text = Mathf.FloorToInt(num) + " ([00BBFF] + " + Mathf.FloorToInt(num2 - num) + "[ffffff])";
						num = m_enhanceItem.GetCurLevelProperty(AttribType.Def);
						num2 = m_enhanceItem.GetNextLevelProperty(AttribType.Def);
						m_EnhanceProperty.m_Defense.text = Mathf.FloorToInt(num) + " ([00BBFF] + " + Mathf.FloorToInt(num2 - num) + "[ffffff])";
						num = m_enhanceItem.GetCurMaxDurability();
						num2 = m_enhanceItem.GetNextMaxDurability();
						CreationItemClass creationItemClass = CreationItemClass.None;
						if (m_enhanceItem.itemObj != null)
						{
							creationItemClass = CreationHelper.GetCreationItemClass(m_enhanceItem.itemObj);
						}
						if (creationItemClass != 0)
						{
							switch (creationItemClass)
							{
							case CreationItemClass.Sword:
							case CreationItemClass.Shield:
							case CreationItemClass.HandGun:
							case CreationItemClass.Rifle:
							case CreationItemClass.Axe:
							case CreationItemClass.Bow:
								num *= PEVCConfig.equipDurabilityShowScale;
								num2 *= PEVCConfig.equipDurabilityShowScale;
								break;
							}
						}
						m_EnhanceProperty.m_Durability.text = Mathf.CeilToInt(num) + " ([00BBFF] + " + Mathf.CeilToInt(num2 - num) + "[ffffff])";
						m_EnhanceProperty.m_TimesEnhcance.text = m_enhanceItem.strengthenTime.ToString();
					}
					CSEnhance cSEnhance = m_Entity as CSEnhance;
					m_ItemPart.m_CostsTime.text = CSUtils.GetRealTimeMS((int)cSEnhance.CostsTime);
					m_PopupHintsPart.m_EnhanceHintGo.SetActive(value: true);
					m_PopupHintsPart.m_RepairHintGo.SetActive(value: false);
					m_PopupHintsPart.m_RecyleHintGo.SetActive(value: false);
				}
				else if (m_Type == 5)
				{
					if (m_repairItem != null)
					{
						num = m_repairItem.GetValue().current;
						float num3 = m_repairItem.GetValue().ExpendValue;
						CreationItemClass creationItemClass2 = CreationItemClass.None;
						if (m_repairItem.itemObj != null)
						{
							creationItemClass2 = CreationHelper.GetCreationItemClass(m_repairItem.itemObj);
						}
						if (creationItemClass2 != 0)
						{
							switch (creationItemClass2)
							{
							case CreationItemClass.Sword:
							case CreationItemClass.Shield:
							case CreationItemClass.HandGun:
							case CreationItemClass.Rifle:
							case CreationItemClass.Axe:
							case CreationItemClass.Bow:
								num *= PEVCConfig.equipDurabilityShowScale;
								num3 *= PEVCConfig.equipDurabilityShowScale;
								break;
							}
						}
						else
						{
							num *= 0.01f;
							num3 *= 0.01f;
						}
						m_RepairProperty.m_Durability.text = Mathf.CeilToInt(num) + " ([00BBFF] + " + Mathf.CeilToInt(num3) + "[ffffff])";
					}
					CSRepair cSRepair = m_Entity as CSRepair;
					m_ItemPart.m_CostsTime.text = CSUtils.GetRealTimeMS((int)cSRepair.CostsTime);
					m_PopupHintsPart.m_EnhanceHintGo.SetActive(value: false);
					m_PopupHintsPart.m_RepairHintGo.SetActive(value: true);
					m_PopupHintsPart.m_RecyleHintGo.SetActive(value: false);
				}
				else if (m_Type == 6)
				{
					CSRecycle cSRecycle = m_Entity as CSRecycle;
					m_ItemPart.m_CostsTime.text = CSUtils.GetRealTimeMS((int)cSRecycle.CostsTime);
					if (m_recycleItem != null)
					{
						num = ((m_recycleItem.GetCurrent() != null) ? m_recycleItem.GetCurrent().current : 0f);
					}
					CreationItemClass creationItemClass3 = CreationItemClass.None;
					if (m_recycleItem.itemObj != null)
					{
						creationItemClass3 = CreationHelper.GetCreationItemClass(m_recycleItem.itemObj);
					}
					if (creationItemClass3 != 0)
					{
						switch (creationItemClass3)
						{
						case CreationItemClass.Sword:
						case CreationItemClass.Shield:
						case CreationItemClass.HandGun:
						case CreationItemClass.Rifle:
						case CreationItemClass.Axe:
						case CreationItemClass.Bow:
							num *= PEVCConfig.equipDurabilityShowScale;
							break;
						}
					}
					m_RecycleProperty.m_Durability.text = Mathf.CeilToInt(num).ToString();
					m_PopupHintsPart.m_EnhanceHintGo.SetActive(value: false);
					m_PopupHintsPart.m_RepairHintGo.SetActive(value: false);
					m_PopupHintsPart.m_RecyleHintGo.SetActive(value: true);
				}
			}
		}
		if (!(m_Entity is CSCommon { WorkerCount: var workerCount } cSCommon))
		{
			return;
		}
		for (int i = 0; i < m_NpcGirds.Count; i++)
		{
			if (i < workerCount)
			{
				m_NpcGirds[i].m_Npc = cSCommon.Worker(i);
			}
			else
			{
				m_NpcGirds[i].m_Npc = null;
			}
		}
		m_NpcPart.m_AuttoSettleBtn.isEnabled = workerCount != cSCommon.WorkerMaxCount;
		m_NpcPart.m_DisbandAllBtn.isEnabled = workerCount != 0;
	}

	public void OnDropItemMulti(Grid_N grid, int m_Index)
	{
		ItemObject itemObj = SelectItem_N.Instance.ItemObj;
		if (m_Type == 4)
		{
			m_Entity._ColonyObj._Network.EHN_SetItem(itemObj);
		}
		else if (m_Type == 5)
		{
			m_Entity._ColonyObj._Network.RPA_SetItem(itemObj.instanceId);
		}
		else if (m_Type == 6)
		{
			m_Entity._ColonyObj._Network.RCY_SetItem(itemObj);
		}
		SelectItem_N.Instance.SetItem(null);
	}

	public void OnLeftMouseClickedMulti(Grid_N grid, int m_Index)
	{
		if (m_Type != 4 && m_Type != 5 && m_Type != 6)
		{
		}
	}

	public void OnRightMouseClickedMulti(Grid_N grid, int m_Index)
	{
		if (m_Type == 4)
		{
			m_Entity._ColonyObj._Network.EHN_Fetch();
		}
		else if (m_Type == 5)
		{
			m_Entity._ColonyObj._Network.RPA_FetchItem();
		}
		else if (m_Type == 6)
		{
			m_Entity._ColonyObj._Network.RCY_FetchItem();
		}
	}

	public void SetResult(bool success, int objId, CSEntity entity)
	{
		if (success && m_Entity == entity)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(objId);
			ItemObject itemObj = m_ItemGrid.m_Grid.ItemObj;
			CSUI_Grid itemGrid = m_ItemGrid;
			itemGrid.m_Grid.SetItem(itemObject);
			if (itemGrid.OnItemChanged != null)
			{
				itemGrid.OnItemChanged(itemObject, itemObj, itemGrid.m_Index);
			}
		}
	}

	public void FetchResult(bool success, CSEntity entity)
	{
		if (success && m_Entity == entity)
		{
			ItemObject itemObject = null;
			ItemObject itemObj = m_ItemGrid.m_Grid.ItemObj;
			CSUI_Grid itemGrid = m_ItemGrid;
			GameUI.Instance.mItemPackageCtrl.ResetItem();
			itemGrid.m_Grid.SetItem(itemObject);
			if (itemGrid.OnItemChanged != null)
			{
				itemGrid.OnItemChanged(itemObject, itemObj, itemGrid.m_Index);
			}
		}
	}
}
