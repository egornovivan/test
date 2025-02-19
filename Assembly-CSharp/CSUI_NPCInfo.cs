using System;
using System.Collections.Generic;
using ItemAsset;
using ItemAsset.SlotListHelper;
using Pathea;
using UnityEngine;

public class CSUI_NPCInfo : MonoBehaviour
{
	[SerializeField]
	private UILabel mLbName;

	[SerializeField]
	private UISprite mSprSex;

	[SerializeField]
	private UILabel mLbHealth;

	[SerializeField]
	private UISlider mSdHealth;

	[SerializeField]
	private UILabel mLbHealthBuff;

	[SerializeField]
	private UILabel mLbHunger;

	[SerializeField]
	private UISlider mSdHunger;

	[SerializeField]
	private UILabel mLbHungerBuff;

	[SerializeField]
	private UILabel mLbComfort;

	[SerializeField]
	private UISlider mSdComfort;

	[SerializeField]
	private UILabel mLbComfortBuff;

	[SerializeField]
	private UILabel mLbShield;

	[SerializeField]
	private UISlider mSdShield;

	[SerializeField]
	private UILabel mLbShieldBuff;

	[SerializeField]
	private UILabel mLbEnergy;

	[SerializeField]
	private UISlider mSdEnergy;

	[SerializeField]
	private UILabel mLbEnergyBuff;

	[SerializeField]
	private UILabel mLbAttack;

	[SerializeField]
	private UILabel mLbAttackBuff;

	[SerializeField]
	private UILabel mLbDefense;

	[SerializeField]
	private UILabel mLbDefenseBuff;

	[SerializeField]
	private Transform m_Interaction1Root;

	[SerializeField]
	private Transform m_Interaction2Root;

	[SerializeField]
	private Transform m_PrivateRoot;

	[SerializeField]
	private UIGrid m_SkillRoot;

	[SerializeField]
	private int m_Interaction1Count = 15;

	[SerializeField]
	private int m_Interaction2Count = 10;

	[SerializeField]
	private GameObject m_InfoPage;

	[SerializeField]
	private GameObject m_InventoryPage;

	[SerializeField]
	private GameObject m_WorkPage;

	[SerializeField]
	private N_ImageButton m_CallBtn;

	public UICheckbox m_InfoCk;

	public UICheckbox m_InventoryCk;

	public UICheckbox m_WorkCk;

	private AttributeInfo AttrHpInfo = new AttributeInfo(AttribType.Hp, AttribType.HpMax);

	private AttributeInfo AttrHungerInfo = new AttributeInfo(AttribType.Hunger, AttribType.HungerMax);

	private AttributeInfo AttrComfortInfo = new AttributeInfo(AttribType.Comfort, AttribType.ComfortMax);

	private AttributeInfo AttrShieldInfo = new AttributeInfo(AttribType.Shield, AttribType.ShieldMax);

	private AttributeInfo AttrEnergyInfo = new AttributeInfo(AttribType.Energy, AttribType.EnergyMax);

	private AttributeInfo AttrAtkInfo = new AttributeInfo(AttribType.Atk, AttribType.Atk);

	private AttributeInfo AttrDefInfo = new AttributeInfo(AttribType.Def, AttribType.Def);

	public Grid_N m_GridPrefab;

	private CSPersonnel m_RefNpc;

	private List<Grid_N> m_InteractionGrids1 = new List<Grid_N>();

	private List<Grid_N> m_InteractionGrids2 = new List<Grid_N>();

	private List<Grid_N> m_PrivateGrids = new List<Grid_N>();

	private List<Grid_N> m_SkillGrids = new List<Grid_N>();

	private SlotList mInteractionPackage1;

	private SlotList mInteractionPackage2;

	private SlotList mPrivatePakge;

	private NpcPackageCmpt packageCmpt;

	private NpcCmpt cmpt;

	[SerializeField]
	private UIGrid mAbnormalGrid;

	[SerializeField]
	private CSUI_BuffItem mAbnormalPrefab;

	private bool mReposition;

	private List<CSUI_BuffItem> mAbnormalList = new List<CSUI_BuffItem>(1);

	public CSPersonnel RefNpc
	{
		get
		{
			return m_RefNpc;
		}
		set
		{
			if (m_RefNpc != null && null != m_RefNpc.m_Npc)
			{
				AbnormalConditionCmpt abnormalConditionCmpt = m_RefNpc.m_Npc.GetCmpt<AbnormalConditionCmpt>();
				if (abnormalConditionCmpt != null)
				{
					abnormalConditionCmpt.evtStart -= AddNpcAbnormal;
					abnormalConditionCmpt.evtEnd -= RemoveNpcAbnormal;
				}
			}
			m_RefNpc = value;
			PeEntity entity = null;
			if (m_RefNpc != null && null != m_RefNpc.m_Npc)
			{
				entity = m_RefNpc.m_Npc;
				AbnormalConditionCmpt abnormalConditionCmpt2 = m_RefNpc.m_Npc.GetCmpt<AbnormalConditionCmpt>();
				if (abnormalConditionCmpt2 != null)
				{
					abnormalConditionCmpt2.evtStart += AddNpcAbnormal;
					abnormalConditionCmpt2.evtEnd += RemoveNpcAbnormal;
				}
			}
			AttrHpInfo.SetEntity(entity);
			AttrHungerInfo.SetEntity(entity);
			AttrComfortInfo.SetEntity(entity);
			AttrShieldInfo.SetEntity(entity);
			AttrEnergyInfo.SetEntity(entity);
			AttrAtkInfo.SetEntity(entity);
			AttrDefInfo.SetEntity(entity);
			UpdateItemGrid();
			RefreshNpcAbnormal();
			UpdateSkills();
		}
	}

	private void Start()
	{
		for (int i = 0; i < m_Interaction1Count; i++)
		{
			Grid_N grid_N = UnityEngine.Object.Instantiate(m_GridPrefab);
			grid_N.transform.parent = m_Interaction1Root.transform;
			grid_N.transform.localPosition = Vector3.zero;
			grid_N.transform.localRotation = Quaternion.identity;
			grid_N.transform.localScale = Vector3.one;
			m_InteractionGrids1.Add(grid_N);
			m_InteractionGrids1[i].transform.localPosition = new Vector3(i % 5 * 60, -(i / 5) * 54, 0f);
			grid_N.SetItemPlace(ItemPlaceType.IPT_ColonyServantInteractionPersonel, i);
			Grid_N grid_N2 = m_InteractionGrids1[i];
			grid_N2.onDropItem = (Grid_N.GridDelegate)Delegate.Combine(grid_N2.onDropItem, new Grid_N.GridDelegate(OnDropItem_InterPackage));
			Grid_N grid_N3 = m_InteractionGrids1[i];
			grid_N3.onLeftMouseClicked = (Grid_N.GridDelegate)Delegate.Combine(grid_N3.onLeftMouseClicked, new Grid_N.GridDelegate(OnLeftMouseCliked_InterPackage));
			Grid_N grid_N4 = m_InteractionGrids1[i];
			grid_N4.onRightMouseClicked = (Grid_N.GridDelegate)Delegate.Combine(grid_N4.onRightMouseClicked, new Grid_N.GridDelegate(OnRightMouseCliked_InterPackage));
		}
		for (int j = 0; j < m_Interaction2Count; j++)
		{
			Grid_N grid_N5 = UnityEngine.Object.Instantiate(m_GridPrefab);
			grid_N5.transform.parent = m_Interaction2Root.transform;
			grid_N5.transform.localPosition = Vector3.zero;
			grid_N5.transform.localRotation = Quaternion.identity;
			grid_N5.transform.localScale = Vector3.one;
			m_InteractionGrids2.Add(grid_N5);
			m_InteractionGrids2[j].transform.localPosition = new Vector3(j % 5 * 60, -(j / 5) * 54, 0f);
			grid_N5.SetItemPlace(ItemPlaceType.IPT_ColonyServantInteraction2Personel, j);
			Grid_N grid_N6 = m_InteractionGrids2[j];
			grid_N6.onDropItem = (Grid_N.GridDelegate)Delegate.Combine(grid_N6.onDropItem, new Grid_N.GridDelegate(OnDropItem_InterPackage2));
			Grid_N grid_N7 = m_InteractionGrids2[j];
			grid_N7.onLeftMouseClicked = (Grid_N.GridDelegate)Delegate.Combine(grid_N7.onLeftMouseClicked, new Grid_N.GridDelegate(OnLeftMouseCliked_InterPackage2));
			Grid_N grid_N8 = m_InteractionGrids2[j];
			grid_N8.onRightMouseClicked = (Grid_N.GridDelegate)Delegate.Combine(grid_N8.onRightMouseClicked, new Grid_N.GridDelegate(OnRightMouseCliked_InterPackage2));
		}
		for (int k = 0; k < 10; k++)
		{
			Grid_N grid_N9 = UnityEngine.Object.Instantiate(m_GridPrefab);
			grid_N9.transform.parent = m_PrivateRoot.transform;
			grid_N9.transform.localPosition = Vector3.zero;
			grid_N9.transform.localRotation = Quaternion.identity;
			grid_N9.transform.localScale = Vector3.one;
			m_PrivateGrids.Add(grid_N9);
			if (k < 5)
			{
				m_PrivateGrids[k].transform.localPosition = new Vector3(k * 60, 0f, 0f);
			}
			else
			{
				m_PrivateGrids[k].transform.localPosition = new Vector3((k - 5) * 60, -55f, 0f);
			}
			grid_N9.SetItemPlace(ItemPlaceType.IPT_ColonyServantInteractionPersonel, k);
		}
		for (int l = 0; l < 5; l++)
		{
			Grid_N grid_N10 = UnityEngine.Object.Instantiate(m_GridPrefab);
			grid_N10.transform.parent = m_SkillRoot.transform;
			grid_N10.transform.localPosition = Vector3.zero;
			grid_N10.transform.localRotation = Quaternion.identity;
			grid_N10.transform.localScale = Vector3.one;
			grid_N10.ItemIndex = l;
			m_SkillGrids.Add(grid_N10);
		}
		m_SkillRoot.repositionNow = true;
		UIEventListener.Get(m_CallBtn.gameObject).onClick = OnCallBtn;
	}

	private void Update()
	{
		mLbName.text = ((m_RefNpc != null) ? m_RefNpc.FullName : "--");
		mSprSex.spriteName = ((m_RefNpc == null) ? "man" : ((m_RefNpc.Sex != PeSex.Male) ? "woman" : "man"));
		mSprSex.MakePixelPerfect();
		float value = AttrHpInfo.CurValue;
		float num = AttrHpInfo.MaxValue;
		mLbHealth.text = AttrHpInfo.GetCur_MaxStr();
		mSdHealth.sliderValue = ((!(num <= 0f)) ? (Convert.ToSingle(value) / num) : 0f);
		mLbHealthBuff.text = AttrHpInfo.GetBuffStr();
		value = AttrHungerInfo.CurValue;
		num = AttrHungerInfo.MaxValue;
		mLbHunger.text = AttrHungerInfo.GetCur_MaxStr();
		mSdHunger.sliderValue = ((!(num <= 0f)) ? (Convert.ToSingle(value) / num) : 0f);
		mLbHungerBuff.text = AttrHungerInfo.GetBuffStr();
		value = AttrComfortInfo.CurValue;
		num = AttrComfortInfo.MaxValue;
		mLbComfort.text = AttrComfortInfo.GetCur_MaxStr();
		mSdComfort.sliderValue = ((!(num <= 0f)) ? (Convert.ToSingle(value) / num) : 0f);
		mLbComfortBuff.text = AttrComfortInfo.GetBuffStr();
		value = AttrShieldInfo.CurValue;
		num = AttrShieldInfo.MaxValue;
		mLbShield.text = AttrShieldInfo.GetCur_MaxStr();
		mSdShield.sliderValue = ((!(num <= 0f)) ? (Convert.ToSingle(value) / num) : 0f);
		mLbShieldBuff.text = AttrShieldInfo.GetBuffStr();
		value = AttrEnergyInfo.CurValue;
		num = AttrEnergyInfo.MaxValue;
		mLbEnergy.text = AttrEnergyInfo.GetCur_MaxStr();
		mSdEnergy.sliderValue = ((!(num <= 0f)) ? (Convert.ToSingle(value) / num) : 0f);
		mLbEnergyBuff.text = AttrEnergyInfo.GetBuffStr();
		value = AttrAtkInfo.CurValue;
		mLbAttack.text = string.Format((AttrAtkInfo.BuffValue <= 0) ? "[FFE000]{0}[-]" : "[B7EF54]{0}[-]", value);
		value = AttrDefInfo.CurValue;
		mLbDefense.text = string.Format((AttrDefInfo.BuffValue <= 0) ? "[FFE000]{0}[-]" : "[B7EF54]{0}[-]", value);
	}

	private void UpdateItemGrid()
	{
		if (m_InteractionGrids1.Count == 0 || m_InteractionGrids2.Count == 0 || m_PrivateGrids.Count == 0)
		{
			return;
		}
		for (int i = 0; i < m_Interaction1Count; i++)
		{
			m_InteractionGrids1[i].SetItem(null);
		}
		for (int j = 0; j < m_Interaction2Count; j++)
		{
			m_InteractionGrids2[j].SetItem(null);
		}
		for (int k = 0; k < 10; k++)
		{
			m_PrivateGrids[k].SetItem(null);
		}
		if (m_RefNpc == null)
		{
			return;
		}
		if (m_RefNpc != null && null != m_RefNpc.m_Npc)
		{
			cmpt = m_RefNpc.m_Npc.GetCmpt<NpcCmpt>();
			if (null != cmpt)
			{
				packageCmpt = cmpt.GetComponent<NpcPackageCmpt>();
				if (null != packageCmpt)
				{
					if (mInteractionPackage1 != null)
					{
						mInteractionPackage1.eventor.Unsubscribe(InteractionpackageChange);
					}
					mInteractionPackage1 = packageCmpt.GetSlotList();
					mInteractionPackage1.eventor.Subscribe(InteractionpackageChange);
					if (mInteractionPackage2 != null)
					{
						mInteractionPackage2.eventor.Unsubscribe(Interactionpackage2Change);
					}
					mInteractionPackage2 = packageCmpt.GetHandinList();
					mInteractionPackage2.eventor.Subscribe(Interactionpackage2Change);
					if (mPrivatePakge != null)
					{
						mPrivatePakge.eventor.Unsubscribe(PrivatepackageChange);
					}
					mPrivatePakge = packageCmpt.GetPrivateSlotList();
					mPrivatePakge.eventor.Subscribe(PrivatepackageChange);
				}
			}
		}
		Reflashpackage();
	}

	private void Reflashpackage()
	{
		ReflashInteractionpackage();
		ReflashInteractionpackage2();
		ReflashPrivatePackage();
	}

	private void InteractionpackageChange(object sender, SlotList.ChangeEvent arg)
	{
		ReflashInteractionpackage();
	}

	private void ReflashInteractionpackage()
	{
		ClearInteractionpackage1();
		for (int i = 0; i < m_InteractionGrids1.Count; i++)
		{
			if (mInteractionPackage1 == null)
			{
				m_InteractionGrids1[i].SetItem(null);
			}
			else
			{
				m_InteractionGrids1[i].SetItem(mInteractionPackage1[i]);
			}
		}
	}

	private void Interactionpackage2Change(object sender, SlotList.ChangeEvent arg)
	{
		ReflashInteractionpackage2();
	}

	private void ReflashInteractionpackage2()
	{
		ClearInteractionpackage2();
		for (int i = 0; i < m_InteractionGrids2.Count; i++)
		{
			if (mInteractionPackage2 == null)
			{
				m_InteractionGrids2[i].SetItem(null);
			}
			else
			{
				m_InteractionGrids2[i].SetItem(mInteractionPackage2[i]);
			}
		}
	}

	private void ClearInteractionpackage1()
	{
		foreach (Grid_N item in m_InteractionGrids1)
		{
			item.SetItem(null);
		}
	}

	private void ClearInteractionpackage2()
	{
		foreach (Grid_N item in m_InteractionGrids2)
		{
			item.SetItem(null);
		}
	}

	private void PrivatepackageChange(object sender, SlotList.ChangeEvent arg)
	{
		ReflashPrivatePackage();
	}

	private void ReflashPrivatePackage()
	{
		ClearPrivatePackage();
		for (int i = 0; i < m_PrivateGrids.Count; i++)
		{
			if (mPrivatePakge == null)
			{
				m_PrivateGrids[i].SetItem(null);
			}
			else
			{
				m_PrivateGrids[i].SetItem(mPrivatePakge[i]);
			}
		}
	}

	private void ClearPrivatePackage()
	{
		foreach (Grid_N privateGrid in m_PrivateGrids)
		{
			privateGrid.SetItem(null);
		}
	}

	public bool SetInteractionItemWithIndex(ItemObject itemObj, int index = -1)
	{
		if (index == -1)
		{
			return mInteractionPackage1.Add(itemObj);
		}
		if (index < 0 || index > mInteractionPackage1.Count)
		{
			return false;
		}
		if (mInteractionPackage1 != null)
		{
			mInteractionPackage1[index] = itemObj;
		}
		return true;
	}

	public bool SetInteraction2ItemWithIndex(ItemObject itemObj, int index = -1)
	{
		if (index == -1)
		{
			return mInteractionPackage2.Add(itemObj);
		}
		if (index < 0 || index > mInteractionPackage2.Count)
		{
			return false;
		}
		if (mInteractionPackage2 != null)
		{
			mInteractionPackage2[index] = itemObj;
		}
		return true;
	}

	private void BtnTakeAllOnClick()
	{
		if (m_RefNpc == null || mInteractionPackage1 == null || !m_RefNpc.IsRandomNpc)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.RequestGetAllItemFromNpc(m_RefNpc.NPC.Id, 0);
			return;
		}
		PlayerPackageCmpt playerPackageCmpt = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
		List<ItemObject> items = mInteractionPackage1.ToList();
		if (playerPackageCmpt.CanAddItemList(items))
		{
			playerPackageCmpt.AddItemList(items);
			mInteractionPackage1.Clear();
		}
	}

	private void BtnTakeAllInventory2OnClick()
	{
		if (m_RefNpc == null || mInteractionPackage2 == null)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.RequestGetAllItemFromNpc(m_RefNpc.NPC.Id, 1);
			return;
		}
		PlayerPackageCmpt playerPackageCmpt = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
		List<ItemObject> items = mInteractionPackage2.ToList();
		if (playerPackageCmpt.CanAddItemList(items))
		{
			playerPackageCmpt.AddItemList(items);
			mInteractionPackage2.Clear();
		}
	}

	private void BtnResortInteractive1BoxOnClick()
	{
		if (m_RefNpc == null)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.RequestNpcPackageSort(m_RefNpc.NPC.Id, 0);
			return;
		}
		ClearInteractionpackage1();
		mInteractionPackage1.Reduce();
		mInteractionPackage1.Sort();
		for (int i = 0; i < m_Interaction1Count; i++)
		{
			if (mInteractionPackage1 == null || mInteractionPackage1[i] == null)
			{
				m_InteractionGrids1[i].SetItem(null);
			}
			else
			{
				m_InteractionGrids1[i].SetItem(mInteractionPackage1[i]);
			}
		}
	}

	private void BtnResortInteractive2BoxOnClick()
	{
		if (m_RefNpc == null)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.RequestNpcPackageSort(m_RefNpc.NPC.Id, 1);
			return;
		}
		ClearInteractionpackage2();
		mInteractionPackage2.Reduce();
		mInteractionPackage2.Sort();
		for (int i = 0; i < m_Interaction2Count; i++)
		{
			if (mInteractionPackage2 == null || mInteractionPackage2[i] == null)
			{
				m_InteractionGrids2[i].SetItem(null);
			}
			else
			{
				m_InteractionGrids2[i].SetItem(mInteractionPackage2[i]);
			}
		}
	}

	public void OnDropItem_InterPackage(Grid_N grid)
	{
		if (m_RefNpc == null || !m_RefNpc.IsRandomNpc || grid.ItemObj != null)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			switch (SelectItem_N.Instance.Place)
			{
			case ItemPlaceType.IPT_ConolyServantEquPersonel:
				if (SelectItem_N.Instance.RemoveOriginItem())
				{
					PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, m_RefNpc.m_Npc.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
				}
				break;
			case ItemPlaceType.IPT_Bag:
			case ItemPlaceType.IPT_ColonyServantInteraction2Personel:
				PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, m_RefNpc.m_Npc.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
				break;
			}
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		switch (SelectItem_N.Instance.Place)
		{
		case ItemPlaceType.IPT_Bag:
		case ItemPlaceType.IPT_ConolyServantEquPersonel:
		case ItemPlaceType.IPT_ColonyServantInteractionPersonel:
		case ItemPlaceType.IPT_ColonyServantInteraction2Personel:
			if (SelectItem_N.Instance.RemoveOriginItem())
			{
				SetInteractionItemWithIndex(SelectItem_N.Instance.ItemObj, grid.ItemIndex);
				SelectItem_N.Instance.RemoveOriginItem();
				SelectItem_N.Instance.SetItem(null);
			}
			break;
		default:
			SelectItem_N.Instance.SetItem(null);
			break;
		}
	}

	public void OnLeftMouseCliked_InterPackage(Grid_N grid)
	{
		if (m_RefNpc != null && m_RefNpc.IsRandomNpc)
		{
			SelectItem_N.Instance.SetItem(grid.ItemObj, grid.ItemPlace, grid.ItemIndex);
		}
	}

	public void OnRightMouseCliked_InterPackage(Grid_N grid)
	{
		if (m_RefNpc != null && m_RefNpc.IsRandomNpc)
		{
			UseItemCmpt useItemCmpt = m_RefNpc.m_Npc.GetCmpt<UseItemCmpt>();
			if (null == useItemCmpt)
			{
				useItemCmpt = m_RefNpc.m_Npc.Add<UseItemCmpt>();
			}
			if (useItemCmpt.Request(grid.ItemObj))
			{
				GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.UpdateEquipAndTex();
			}
		}
	}

	public void OnDropItem_InterPackage2(Grid_N grid)
	{
		if (m_RefNpc == null || !m_RefNpc.IsRandomNpc || grid.ItemObj != null)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			switch (SelectItem_N.Instance.Place)
			{
			case ItemPlaceType.IPT_ConolyServantEquPersonel:
				if (SelectItem_N.Instance.RemoveOriginItem())
				{
					PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, m_RefNpc.m_Npc.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
				}
				break;
			case ItemPlaceType.IPT_Bag:
			case ItemPlaceType.IPT_ColonyServantInteractionPersonel:
				PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, m_RefNpc.m_Npc.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
				break;
			}
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		switch (SelectItem_N.Instance.Place)
		{
		case ItemPlaceType.IPT_Bag:
		case ItemPlaceType.IPT_ConolyServantEquPersonel:
		case ItemPlaceType.IPT_ColonyServantInteractionPersonel:
		case ItemPlaceType.IPT_ColonyServantInteraction2Personel:
			if (SelectItem_N.Instance.RemoveOriginItem())
			{
				SetInteraction2ItemWithIndex(SelectItem_N.Instance.ItemObj, grid.ItemIndex);
				SelectItem_N.Instance.SetItem(null);
			}
			break;
		default:
			SelectItem_N.Instance.SetItem(null);
			break;
		}
	}

	public void OnLeftMouseCliked_InterPackage2(Grid_N grid)
	{
		if (m_RefNpc != null && m_RefNpc.IsRandomNpc)
		{
			SelectItem_N.Instance.SetItem(grid.ItemObj, grid.ItemPlace, grid.ItemIndex);
		}
	}

	public void OnRightMouseCliked_InterPackage2(Grid_N grid)
	{
		if (m_RefNpc != null && m_RefNpc.IsRandomNpc)
		{
			UseItemCmpt useItemCmpt = m_RefNpc.m_Npc.GetCmpt<UseItemCmpt>();
			if (null == useItemCmpt)
			{
				useItemCmpt = m_RefNpc.m_Npc.Add<UseItemCmpt>();
			}
			if (useItemCmpt.Request(grid.ItemObj))
			{
				GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.UpdateEquipAndTex();
			}
		}
	}

	private void ClearSkill()
	{
		for (int i = 0; i < m_SkillGrids.Count; i++)
		{
			m_SkillGrids[i].SetSkill(0, i, null, string.Empty);
		}
	}

	private void UpdateSkills()
	{
		if (m_RefNpc == null || m_RefNpc.NPC == null)
		{
			ClearSkill();
		}
		else
		{
			if (m_RefNpc.SkAlive == null)
			{
				return;
			}
			int num = 0;
			ClearSkill();
			if (m_RefNpc.Npcabliys == null)
			{
				return;
			}
			foreach (NpcAbility npcabliy in m_RefNpc.Npcabliys)
			{
				if (npcabliy != null)
				{
					m_SkillGrids[num].SetSkill(npcabliy.skillId, num, m_RefNpc.SkAlive, npcabliy.icon, npcabliy.desc);
					num++;
				}
			}
		}
	}

	private bool OnCheckItemGrid(ItemObject item, CSUI_Grid.ECheckItemType check_type)
	{
		if (m_RefNpc == null)
		{
			return false;
		}
		return true;
	}

	private void OnItemGridChanged(ItemObject item, ItemObject oldItem, int index)
	{
		if (oldItem != null)
		{
			if (item == null)
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayItemFromNpc.GetString(), oldItem.protoData.GetName(), m_RefNpc.FullName));
			}
			else if (item == oldItem)
			{
				CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mNotEnoughGrid.GetString(), Color.red);
			}
			else
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutItemToNpc.GetString(), item.protoData.GetName(), m_RefNpc.FullName));
			}
		}
		else if (item != null)
		{
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutItemToNpc.GetString(), item.protoData.GetName(), m_RefNpc.FullName));
		}
	}

	private void OnGridsExchangeItem(Grid_N grid, ItemObject item)
	{
		grid.SetItem(item);
	}

	private void PageInfoOnActive(bool active)
	{
		m_InfoPage.SetActive(active);
		UpdateSkills();
	}

	private void PageInvetoryOnActive(bool active)
	{
		m_InventoryPage.SetActive(active);
		UpdateSkills();
	}

	private void PageWorkOnActive(bool active)
	{
		m_WorkPage.SetActive(active);
		UpdateSkills();
	}

	private void OnCallBtn(GameObject go)
	{
		if (m_RefNpc == null || !(null != m_RefNpc.NPC))
		{
			return;
		}
		if (!NpcMgr.CallBackColonyNpcToPlayer(m_RefNpc.NPC, out var state))
		{
			switch (state)
			{
			case ECsNpcState.None:
				break;
			case ECsNpcState.Working:
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(82201077), m_RefNpc.FullName));
				break;
			case ECsNpcState.InMission:
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(PELocalization.GetString(82201078), m_RefNpc.FullName));
				break;
			case ECsNpcState.OutOfRadiu:
				CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(82201079));
				break;
			}
		}
		else
		{
			CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(82201080));
		}
	}

	private void RefreshNpcAbnormal()
	{
		RemoveAllAbnormal();
		if (m_RefNpc == null)
		{
			return;
		}
		List<PEAbnormalType> list = new List<PEAbnormalType>();
		if (null != cmpt && null != cmpt.Entity && null != cmpt.Entity.Alnormal)
		{
			list.AddRange(cmpt.Entity.Alnormal.GetActiveAbnormalList());
		}
		if (list.Count != 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				AddNpcAbnormal(list[i]);
			}
		}
	}

	private void AddNpcAbnormal(PEAbnormalType type)
	{
		AbnormalData data = AbnormalData.GetData(type);
		if (data != null && !(data.iconName == "0"))
		{
			CSUI_BuffItem cSUI_BuffItem = UnityEngine.Object.Instantiate(mAbnormalPrefab);
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
				UnityEngine.Object.Destroy(cSUI_BuffItem.gameObject);
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
				UnityEngine.Object.Destroy(mAbnormalList[i].gameObject);
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
