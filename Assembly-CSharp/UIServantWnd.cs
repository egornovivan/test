using System;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using ItemAsset.SlotListHelper;
using Pathea;
using UnityEngine;
using WhiteCat;

public class UIServantWnd : UIBaseWnd
{
	public enum ServantIndex
	{
		Index_0,
		Index_1,
		Max
	}

	[SerializeField]
	private UIPlayerInfoScall mScall;

	[SerializeField]
	private UITexture mEqTex;

	[SerializeField]
	private Grid_N mGridPrefab;

	[SerializeField]
	private Transform mTsSkillGrids;

	[SerializeField]
	private Transform mTsInteractionGrids;

	[SerializeField]
	private Transform mTsInteraction2Grids;

	[SerializeField]
	private Transform mTsPrivateItemGrids;

	[SerializeField]
	private Transform mTsEquipmentGrids;

	[SerializeField]
	private GameObject mPageInfo;

	[SerializeField]
	private GameObject mPageInventory;

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
	private UILabel mLbMoney;

	[SerializeField]
	private UILabel mLbPrivatePageText;

	[SerializeField]
	private UILabel mLbNextServant;

	[SerializeField]
	private UICheckbox mCkInventory;

	[SerializeField]
	private GameObject mAttackBtn;

	[SerializeField]
	private GameObject mDefenceBtn;

	[SerializeField]
	private GameObject mRestBtn;

	[SerializeField]
	private GameObject mStayBtn;

	[SerializeField]
	private N_ImageButton mWorkBtn;

	[SerializeField]
	private N_ImageButton mCallBtn;

	[SerializeField]
	private N_ImageButton mFreeBtn;

	private int mTabelIndex;

	private int mSkillGridCount = 5;

	private int mInteractionGridCount = 15;

	private int mInteraction2GridCount = 10;

	private int mPrivateItemGridCount = 10;

	private AttributeInfo AttrHpInfo = new AttributeInfo(AttribType.Hp, AttribType.HpMax);

	private AttributeInfo AttrHungerInfo = new AttributeInfo(AttribType.Hunger, AttribType.HungerMax);

	private AttributeInfo AttrComfortInfo = new AttributeInfo(AttribType.Comfort, AttribType.ComfortMax);

	private AttributeInfo AttrShieldInfo = new AttributeInfo(AttribType.Shield, AttribType.ShieldMax);

	private AttributeInfo AttrEnergyInfo = new AttributeInfo(AttribType.Energy, AttribType.EnergyMax);

	private AttributeInfo AttrAtkInfo = new AttributeInfo(AttribType.Atk, AttribType.Atk);

	private AttributeInfo AttrDefInfo = new AttributeInfo(AttribType.Def, AttribType.Def);

	[SerializeField]
	private UILabel m_BuffStrsLb;

	private string m_BuffStrFormat = "+{0}";

	private List<Grid_N> mSkillList;

	private List<Grid_N> mInteractionList;

	private List<Grid_N> mInteraction2List;

	private List<Grid_N> mPrivateList;

	private List<Grid_N> mEquipmentList;

	private PeViewController _viewController;

	private GameObject _viewModel;

	private PlayerPackageCmpt playerPackageCmpt;

	private PeEntity m_Servant;

	private ServantLeaderCmpt leaderCmpt;

	private NpcCmpt m_NpcCmpt;

	private BiologyViewCmpt viewCmpt;

	private CommonCmpt commonCmpt;

	private EquipmentCmpt m_EquipmentCmpt;

	private NpcPackageCmpt packageCmpt;

	private EntityInfoCmpt entityInfoCmpt;

	private SlotList mInteractionPackage;

	private SlotList mInteraction2Package;

	private SlotList mPrivatePakge;

	[SerializeField]
	private UIGrid mAbnormalGrid;

	[SerializeField]
	private CSUI_BuffItem mAbnormalPrefab;

	private bool mReposition;

	private List<CSUI_BuffItem> mAbnormalList = new List<CSUI_BuffItem>(1);

	[SerializeField]
	private GameObject waitingImage;

	private CreationController[] _meshControllers;

	private GameObject _newViewModel;

	private int mPageIndex = 1;

	public ServantIndex mCurrentIndex { get; set; }

	private PeEntity servant
	{
		get
		{
			return m_Servant;
		}
		set
		{
			m_Servant = value;
			AttrHpInfo.SetEntity(value);
			AttrHungerInfo.SetEntity(value);
			AttrComfortInfo.SetEntity(value);
			AttrShieldInfo.SetEntity(value);
			AttrEnergyInfo.SetEntity(value);
			AttrAtkInfo.SetEntity(value);
			AttrDefInfo.SetEntity(value);
		}
	}

	private NpcCmpt npcCmpt
	{
		get
		{
			return m_NpcCmpt;
		}
		set
		{
			if (null != m_NpcCmpt && null != m_NpcCmpt.Entity)
			{
				AbnormalConditionCmpt cmpt = m_NpcCmpt.Entity.GetCmpt<AbnormalConditionCmpt>();
				if (cmpt != null)
				{
					cmpt.evtStart -= AddNpcAbnormal;
					cmpt.evtEnd -= RemoveNpcAbnormal;
				}
			}
			m_NpcCmpt = value;
			if (null != m_NpcCmpt && null != m_NpcCmpt.Entity)
			{
				AbnormalConditionCmpt cmpt2 = m_NpcCmpt.Entity.GetCmpt<AbnormalConditionCmpt>();
				if (cmpt2 != null)
				{
					cmpt2.evtStart += AddNpcAbnormal;
					cmpt2.evtEnd += RemoveNpcAbnormal;
				}
			}
		}
	}

	private EquipmentCmpt equipmentCmpt
	{
		get
		{
			return m_EquipmentCmpt;
		}
		set
		{
			if (m_EquipmentCmpt != null)
			{
				EquipmentCmpt obj = m_EquipmentCmpt;
				obj.onSuitSetChange = (Action<List<SuitSetData.MatchData>>)Delegate.Remove(obj.onSuitSetChange, new Action<List<SuitSetData.MatchData>>(UpdateSuitBuffTips));
			}
			m_EquipmentCmpt = value;
			if (m_EquipmentCmpt != null)
			{
				EquipmentCmpt obj2 = m_EquipmentCmpt;
				obj2.onSuitSetChange = (Action<List<SuitSetData.MatchData>>)Delegate.Combine(obj2.onSuitSetChange, new Action<List<SuitSetData.MatchData>>(UpdateSuitBuffTips));
				UpdateSuitBuffTips(m_EquipmentCmpt.matchDatas);
			}
			else
			{
				UpdateSuitBuffTips(null);
			}
		}
	}

	public bool ServantIsNotNull => npcCmpt != null;

	public int GetCurServantId => (!(npcCmpt == null)) ? npcCmpt.Entity.Id : (-1);

	private bool waitingToCloneModel
	{
		get
		{
			return waitingImage.activeSelf;
		}
		set
		{
			waitingImage.SetActive(value);
		}
	}

	private int mMaxPageIndex => (mPrivatePakge == null) ? 1 : (mPrivatePakge.Count / mPrivateItemGridCount);

	private void GetServentCmpt()
	{
		if (leaderCmpt != null)
		{
			NpcCmpt npcCmpt = leaderCmpt.GetServant((int)mCurrentIndex);
			if (npcCmpt != null && npcCmpt != this.npcCmpt)
			{
				viewCmpt = npcCmpt.Entity.biologyViewCmpt;
				commonCmpt = npcCmpt.Entity.commonCmpt;
				packageCmpt = npcCmpt.GetComponent<NpcPackageCmpt>();
				GetNpcPakgeSlotList();
				entityInfoCmpt = npcCmpt.Entity.enityInfoCmpt;
				if (equipmentCmpt != null)
				{
					equipmentCmpt.changeEventor.Unsubscribe(EquipmentChangeEvent);
				}
				equipmentCmpt = npcCmpt.Entity.equipmentCmpt;
				equipmentCmpt.changeEventor.Subscribe(EquipmentChangeEvent);
			}
			this.npcCmpt = npcCmpt;
			CheckWhtherCanGet();
			servant = ((!(this.npcCmpt != null)) ? null : this.npcCmpt.Entity);
		}
		else
		{
			this.npcCmpt = null;
			CheckWhtherCanGet();
		}
		if (this.npcCmpt == null)
		{
			viewCmpt = null;
			commonCmpt = null;
			equipmentCmpt = null;
			packageCmpt = null;
			entityInfoCmpt = null;
			mInteractionPackage = null;
			mPrivatePakge = null;
			mSprSex.spriteName = "null";
			ClearEqList();
			ClearNpcPackage();
		}
		mEqTex.enabled = this.npcCmpt != null;
	}

	private void CheckWhtherCanGet()
	{
	}

	public void Refresh()
	{
		ServantLeaderCmpt cmpt = GameUI.Instance.mMainPlayer.GetCmpt<ServantLeaderCmpt>();
		if (cmpt != leaderCmpt)
		{
			if (leaderCmpt != null)
			{
				leaderCmpt.changeEventor.Unsubscribe(ServentChange);
			}
			leaderCmpt = cmpt;
			leaderCmpt.changeEventor.Subscribe(ServentChange);
			playerPackageCmpt = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
		}
		GetServentCmpt();
		ReflashSkill();
		RefreshNpcAbnormal();
		if (!(npcCmpt == null))
		{
			if (equipmentCmpt != null)
			{
				UpdateEquipAndTex();
			}
			if (packageCmpt != null)
			{
				Reflashpackage();
			}
		}
	}

	public NpcCmpt GetNeedReviveServant()
	{
		ServantLeaderCmpt cmpt = GameUI.Instance.mMainPlayer.GetCmpt<ServantLeaderCmpt>();
		NpcCmpt npcCmpt = cmpt.GetServant((int)mCurrentIndex);
		if (npcCmpt == null)
		{
			return null;
		}
		if (npcCmpt.Alive.isDead)
		{
			return npcCmpt;
		}
		return null;
	}

	private void ServentChange(object sender, ServantLeaderCmpt.ServantChanged arg)
	{
		Refresh();
	}

	public override void Show()
	{
		base.Show();
		Refresh();
		if (_viewModel != null)
		{
			_viewModel.gameObject.SetActive(value: true);
		}
		_viewController.gameObject.SetActive(value: true);
	}

	public void Show(ServantIndex index)
	{
		mCurrentIndex = index;
		Show();
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (_viewModel != null)
		{
			_viewModel.gameObject.SetActive(value: false);
		}
		_viewController.gameObject.SetActive(value: false);
	}

	protected override void InitWindow()
	{
		mCurrentIndex = ServantIndex.Index_0;
		base.InitWindow();
		base.SelfWndType = UIEnum.WndType.Servant;
	}

	public override void OnCreate()
	{
		base.OnCreate();
		_viewController = PeViewStudio.CreateViewController(ViewControllerParam.DefaultCharacter);
		_viewController.SetLocalPos(PeViewStudio.s_ViewPos);
		_viewController.name = "ViewController_Servant";
		_viewController.gameObject.SetActive(value: false);
		InitGrid();
	}

	private void InitGrid()
	{
		mSkillList = new List<Grid_N>();
		for (int i = 0; i < mSkillGridCount; i++)
		{
			mSkillList.Add(UnityEngine.Object.Instantiate(mGridPrefab));
			mSkillList[i].gameObject.name = "SkillGrid" + i;
			mSkillList[i].transform.parent = mTsSkillGrids;
			mSkillList[i].transform.localPosition = new Vector3(35 + i * 60, -38f, 0f);
			mSkillList[i].transform.localRotation = Quaternion.identity;
			mSkillList[i].transform.localScale = Vector3.one;
		}
		mInteractionList = new List<Grid_N>();
		for (int j = 0; j < mInteractionGridCount; j++)
		{
			mInteractionList.Add(UnityEngine.Object.Instantiate(mGridPrefab));
			mInteractionList[j].gameObject.name = "Interaction" + j;
			mInteractionList[j].transform.parent = mTsInteractionGrids;
			mInteractionList[j].transform.localPosition = new Vector3(j % 5 * 60, -(j / 5) * 54, 0f);
			mInteractionList[j].transform.localRotation = Quaternion.identity;
			mInteractionList[j].transform.localScale = Vector3.one;
			mInteractionList[j].SetItemPlace(ItemPlaceType.IPT_ServantInteraction, j);
			Grid_N grid_N = mInteractionList[j];
			grid_N.onDropItem = (Grid_N.GridDelegate)Delegate.Combine(grid_N.onDropItem, new Grid_N.GridDelegate(OnDropItem_InterPackage));
			Grid_N grid_N2 = mInteractionList[j];
			grid_N2.onLeftMouseClicked = (Grid_N.GridDelegate)Delegate.Combine(grid_N2.onLeftMouseClicked, new Grid_N.GridDelegate(OnLeftMouseCliked_InterPackage));
			Grid_N grid_N3 = mInteractionList[j];
			grid_N3.onRightMouseClicked = (Grid_N.GridDelegate)Delegate.Combine(grid_N3.onRightMouseClicked, new Grid_N.GridDelegate(OnRightMouseCliked_InterPackage));
		}
		mInteraction2List = new List<Grid_N>();
		for (int k = 0; k < mInteraction2GridCount; k++)
		{
			mInteraction2List.Add(UnityEngine.Object.Instantiate(mGridPrefab));
			mInteraction2List[k].gameObject.name = "Interaction" + k;
			mInteraction2List[k].transform.parent = mTsInteraction2Grids;
			mInteraction2List[k].transform.localPosition = new Vector3(k % 5 * 60, -(k / 5) * 54, 0f);
			mInteraction2List[k].transform.localRotation = Quaternion.identity;
			mInteraction2List[k].transform.localScale = Vector3.one;
			mInteraction2List[k].SetItemPlace(ItemPlaceType.IPT_ServantInteraction2, k);
			Grid_N grid_N4 = mInteraction2List[k];
			grid_N4.onDropItem = (Grid_N.GridDelegate)Delegate.Combine(grid_N4.onDropItem, new Grid_N.GridDelegate(OnDropItem_InterPackage2));
			Grid_N grid_N5 = mInteraction2List[k];
			grid_N5.onLeftMouseClicked = (Grid_N.GridDelegate)Delegate.Combine(grid_N5.onLeftMouseClicked, new Grid_N.GridDelegate(OnLeftMouseCliked_InterPackage2));
			Grid_N grid_N6 = mInteraction2List[k];
			grid_N6.onRightMouseClicked = (Grid_N.GridDelegate)Delegate.Combine(grid_N6.onRightMouseClicked, new Grid_N.GridDelegate(OnRightMouseCliked_InterPackage2));
		}
		mPrivateList = new List<Grid_N>();
		for (int l = 0; l < mPrivateItemGridCount; l++)
		{
			mPrivateList.Add(UnityEngine.Object.Instantiate(mGridPrefab));
			mPrivateList[l].gameObject.name = "PrivateItem" + l;
			mPrivateList[l].transform.parent = mTsPrivateItemGrids;
			if (l < 5)
			{
				mPrivateList[l].transform.localPosition = new Vector3(l * 60, 0f, 0f);
			}
			else if (l < 10)
			{
				mPrivateList[l].transform.localPosition = new Vector3((l - 5) * 60, -55f, 0f);
			}
			else
			{
				mPrivateList[l].transform.localPosition = new Vector3((l - 10) * 60, -110f, 0f);
			}
			mPrivateList[l].transform.localRotation = Quaternion.identity;
			mPrivateList[l].transform.localScale = Vector3.one;
		}
		string[] array = new string[10] { "Icons_helmet", "Icons_coat", "Icons_gloves", "Icons_battery", "Icons_arms", "Icons_back", "Icons_pants", "Icons_shoes", "Icons_glass", "Icons_dun" };
		mEquipmentList = new List<Grid_N>();
		for (int m = 0; m < 10; m++)
		{
			mEquipmentList.Add(UnityEngine.Object.Instantiate(mGridPrefab));
			mEquipmentList[m].gameObject.name = "HotKey" + m;
			mEquipmentList[m].transform.parent = mTsEquipmentGrids;
			if (m < 5)
			{
				mEquipmentList[m].transform.localPosition = new Vector3(-112f, 176 - m % 5 * 58, -2f);
			}
			else
			{
				mEquipmentList[m].transform.localPosition = new Vector3(112f, 176 - m % 5 * 58, -2f);
			}
			mEquipmentList[m].transform.localRotation = Quaternion.identity;
			mEquipmentList[m].transform.localScale = Vector3.one;
			mEquipmentList[m].SetItemPlace(ItemPlaceType.IPT_ServantEqu, m);
			mEquipmentList[m].SetGridMask((GridMask)(1 << m));
			mEquipmentList[m].mScriptIco.spriteName = array[m];
			mEquipmentList[m].mScriptIco.MakePixelPerfect();
			Grid_N grid_N7 = mEquipmentList[m];
			grid_N7.onDropItem = (Grid_N.GridDelegate)Delegate.Combine(grid_N7.onDropItem, new Grid_N.GridDelegate(OnDropItem_Equip));
			Grid_N grid_N8 = mEquipmentList[m];
			grid_N8.onLeftMouseClicked = (Grid_N.GridDelegate)Delegate.Combine(grid_N8.onLeftMouseClicked, new Grid_N.GridDelegate(OnLeftMouseCliked_Equip));
			Grid_N grid_N9 = mEquipmentList[m];
			grid_N9.onRightMouseClicked = (Grid_N.GridDelegate)Delegate.Combine(grid_N9.onRightMouseClicked, new Grid_N.GridDelegate(OnRightMouseCliked_Equip));
		}
	}

	public EquipmentCmpt GetCurServantEquipCmpt()
	{
		return (!(npcCmpt == null)) ? npcCmpt.Entity.equipmentCmpt : null;
	}

	private void Start()
	{
		if (!mInit)
		{
			InitGrid();
		}
	}

	private void ClearSkilllist()
	{
		for (int i = 0; i < mSkillList.Count; i++)
		{
			mSkillList[i].SetSkill(0, i, null, string.Empty);
		}
	}

	private void ReflashSkill()
	{
		if (npcCmpt == null || npcCmpt.Npcskillcmpt == null)
		{
			ClearSkilllist();
		}
		else
		{
			if (npcCmpt.Alive == null)
			{
				return;
			}
			int num = 0;
			ClearSkilllist();
			if (npcCmpt.Npcskillcmpt.CurNpcAblitys != null)
			{
				foreach (NpcAbility curNpcAblity in npcCmpt.Npcskillcmpt.CurNpcAblitys)
				{
					if (curNpcAblity != null)
					{
						mSkillList[num].SetSkill(curNpcAblity.skillId, num, npcCmpt.Alive, curNpcAblity.icon, curNpcAblity.desc);
						num++;
					}
				}
			}
			mWorkBtn.gameObject.SetActive(npcCmpt.Npcskillcmpt.HasCollectSkill());
		}
	}

	private void UpdateOpBtnState()
	{
		bool isEnabled = null != npcCmpt && !npcCmpt.Entity.IsDeath();
		mFreeBtn.isEnabled = isEnabled;
		mCallBtn.isEnabled = isEnabled;
		if (mWorkBtn.gameObject.activeSelf)
		{
			mWorkBtn.isEnabled = isEnabled;
		}
	}

	private void Update()
	{
		if (null == entityInfoCmpt)
		{
			mLbName.text = "--";
			mLbName.MakePixelPerfect();
		}
		else
		{
			mLbName.text = entityInfoCmpt.characterName.fullName;
			mLbName.MakePixelPerfect();
		}
		if (null == commonCmpt)
		{
			mSprSex.spriteName = "man";
			mSprSex.MakePixelPerfect();
		}
		else
		{
			mSprSex.spriteName = ((commonCmpt.sex != PeSex.Male) ? "woman" : "man");
			mSprSex.MakePixelPerfect();
		}
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
		UpdateOpBtnState();
		mSprSex.enabled = !(servant == null);
		if (packageCmpt != null)
		{
			mLbPrivatePageText.text = mPageIndex + " / " + mMaxPageIndex;
			mLbMoney.text = packageCmpt.money.current.ToString();
		}
		else
		{
			mLbPrivatePageText.text = "0 / 0";
			mLbMoney.text = "--";
		}
		mLbNextServant.text = (int)(mCurrentIndex + 1) + "/" + 2;
		ShowBattle();
	}

	private void UpdateSuitBuffTips(List<SuitSetData.MatchData> datas)
	{
		string text = string.Empty;
		if (datas != null && datas.Count > 0)
		{
			SuitSetData.MatchData matchData = default(SuitSetData.MatchData);
			for (int i = 0; i < datas.Count; i++)
			{
				matchData = datas[i];
				if (matchData.tips == null || matchData.tips.Length <= 0)
				{
					continue;
				}
				for (int j = 0; j < matchData.tips.Length; j++)
				{
					if (matchData.tips[j] != 0 && matchData.activeTipsIndex >= j)
					{
						text = text + PELocalization.GetString(matchData.tips[j]) + "\n";
					}
				}
			}
		}
		if (text.Length > 0)
		{
			text = text.Substring(0, text.Length - 1);
		}
		m_BuffStrsLb.text = text;
		m_BuffStrsLb.MakePixelPerfect();
	}

	private void BtnNextOnClick()
	{
		mCurrentIndex = ((mCurrentIndex == ServantIndex.Index_0) ? ServantIndex.Index_1 : ServantIndex.Index_0);
		Refresh();
	}

	private void BtnInfoOnClick(bool isActive)
	{
		if (isActive)
		{
			mTabelIndex = 0;
			mPageInfo.SetActive(value: true);
			mPageInventory.SetActive(value: false);
			Refresh();
		}
	}

	private void BtnInvetoryOnClick(bool isActive)
	{
		if (isActive)
		{
			mTabelIndex = 1;
			mPageInfo.SetActive(value: false);
			mPageInventory.SetActive(value: true);
			Refresh();
		}
	}

	private void BtnLeftOnClick()
	{
		if (mPageIndex > 1)
		{
			mPageIndex--;
		}
		ReflashPrivatePackage();
	}

	private void BtnRightOnClick()
	{
		if (mPageIndex < mMaxPageIndex)
		{
			mPageIndex++;
		}
		ReflashPrivatePackage();
	}

	private void BtnTakeAllOnClick()
	{
		if (servant == null)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.RequestGetAllItemFromNpc(servant.Id, 0);
			return;
		}
		PlayerPackageCmpt cmpt = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
		List<ItemObject> items = mInteractionPackage.ToList();
		if (cmpt.CanAddItemList(items))
		{
			cmpt.AddItemList(items);
			mInteractionPackage.Clear();
		}
	}

	private void BtnTakeAllInventory2OnClick()
	{
		if (servant == null)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.RequestGetAllItemFromNpc(servant.Id, 1);
			return;
		}
		PlayerPackageCmpt cmpt = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
		List<ItemObject> items = mInteraction2Package.ToList();
		if (cmpt.CanAddItemList(items))
		{
			cmpt.AddItemList(items);
			mInteraction2Package.Clear();
		}
	}

	private void BtnResortInteractive1BoxOnClick()
	{
		if (servant == null)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.RequestNpcPackageSort(servant.Id, 0);
			return;
		}
		ClearInteractionpackage();
		mInteractionPackage.Reduce();
		mInteractionPackage.Sort();
		for (int i = 0; i < mInteractionGridCount; i++)
		{
			if (mInteractionPackage == null || mInteractionPackage[i] == null)
			{
				mInteractionList[i].SetItem(null);
			}
			else
			{
				mInteractionList[i].SetItem(mInteractionPackage[i]);
			}
		}
	}

	private void BtnResortInteractive2BoxOnClick()
	{
		if (servant == null)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.RequestNpcPackageSort(servant.Id, 1);
			return;
		}
		ClearInteractionpackage2();
		mInteraction2Package.Reduce();
		mInteraction2Package.Sort();
		for (int i = 0; i < mInteraction2GridCount; i++)
		{
			if (mInteraction2Package == null || mInteraction2Package[i] == null)
			{
				mInteraction2List[i].SetItem(null);
			}
			else
			{
				mInteraction2List[i].SetItem(mInteraction2Package[i]);
			}
		}
	}

	private void BtnStayOnClick()
	{
		if (!(npcCmpt == null))
		{
			npcCmpt.FollowerSentry = !npcCmpt.FollowerSentry;
		}
	}

	private void BtnWorkOnClick()
	{
		if (!(npcCmpt == null))
		{
			npcCmpt.FollowerWork = true;
		}
	}

	private void BtnCallOnClick()
	{
		if (!(npcCmpt == null))
		{
			npcCmpt.FollowerWork = false;
			npcCmpt.ServantCallBack();
		}
	}

	private void BtnFreeOnClick()
	{
		MessageBox_N.ShowYNBox(PELocalization.GetString(8000031), FreeServant);
	}

	private void FreeServant()
	{
		if (npcCmpt == null || leaderCmpt == null)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			if (null != npcCmpt)
			{
				leaderCmpt.OnFreeNpc(npcCmpt.Entity.Id);
			}
		}
		else
		{
			leaderCmpt.RemoveServant(npcCmpt);
		}
	}

	private void OnAttackChosebtn()
	{
		if (npcCmpt != null)
		{
			npcCmpt.Battle = ENpcBattle.Attack;
		}
	}

	private void OnDefenceChoseBtn()
	{
		if (npcCmpt != null)
		{
			npcCmpt.Battle = ENpcBattle.Defence;
		}
	}

	private void OnRestChoseBtn()
	{
		if (npcCmpt != null)
		{
			npcCmpt.Battle = ENpcBattle.Passive;
		}
	}

	private void OnStayChoosebtn()
	{
		if (npcCmpt != null)
		{
			npcCmpt.Battle = ENpcBattle.Stay;
		}
	}

	private void ShowBattle()
	{
		if (npcCmpt == null)
		{
			if (mAttackBtn.activeSelf)
			{
				mAttackBtn.SetActive(value: false);
			}
			if (mDefenceBtn.activeSelf)
			{
				mDefenceBtn.SetActive(value: false);
			}
			if (mRestBtn.activeSelf)
			{
				mRestBtn.SetActive(value: false);
			}
			if (mStayBtn.activeSelf)
			{
				mStayBtn.SetActive(value: false);
			}
			return;
		}
		switch (npcCmpt.Battle)
		{
		case ENpcBattle.Attack:
			mAttackBtn.SetActive(value: true);
			mDefenceBtn.SetActive(value: false);
			mRestBtn.SetActive(value: false);
			mStayBtn.SetActive(value: false);
			break;
		case ENpcBattle.Defence:
			mAttackBtn.SetActive(value: false);
			mDefenceBtn.SetActive(value: true);
			mRestBtn.SetActive(value: false);
			mStayBtn.SetActive(value: false);
			break;
		case ENpcBattle.Passive:
			mAttackBtn.SetActive(value: false);
			mDefenceBtn.SetActive(value: false);
			mRestBtn.SetActive(value: true);
			mStayBtn.SetActive(value: false);
			break;
		case ENpcBattle.Stay:
			mAttackBtn.SetActive(value: false);
			mDefenceBtn.SetActive(value: false);
			mRestBtn.SetActive(value: false);
			mStayBtn.SetActive(value: true);
			break;
		case ENpcBattle.Evasion:
			break;
		}
	}

	private void OnGUI()
	{
		if (PeSingleton<EntityMgr>.Instance == null || !Application.isEditor || !GUI.Button(new Rect(300f, 100f, 90f, 20f), "AddServant"))
		{
			return;
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(9008);
		if (peEntity == null)
		{
			Debug.Log("Get entity failed!");
			return;
		}
		NpcCmpt npcCmpt = peEntity.NpcCmpt;
		if (npcCmpt != null)
		{
			CSMain.SetNpcFollower(peEntity);
		}
	}

	private void RefreshNpcAbnormal()
	{
		RemoveAllAbnormal();
		if (npcCmpt == null)
		{
			return;
		}
		List<PEAbnormalType> activeAbnormalList = npcCmpt.Entity.Alnormal.GetActiveAbnormalList();
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

	private new void LateUpdate()
	{
		UpdateReposition();
	}

	public bool RemoveEqByIndex(int index)
	{
		if (mEquipmentList != null && index >= 0 && index < mEquipmentList.Count && mEquipmentList[index].ItemObj != null)
		{
			if (GameConfig.IsMultiMode)
			{
				if (equipmentCmpt.TryTakeOffEquipment(mEquipmentList[index].ItemObj))
				{
					return true;
				}
			}
			else if (equipmentCmpt.TakeOffEquipment(mEquipmentList[index].ItemObj, addToReceiver: false))
			{
				GameUI.Instance.PlayTakeOffEquipAudio();
				return true;
			}
		}
		PeTipMsg.Register(PELocalization.GetString(8000594), PeTipMsg.EMsgLevel.Error);
		return false;
	}

	public bool RemoveEqByObj(ItemObject itemObj, bool addToReceiver, EquipmentCmpt.Receiver receiver)
	{
		if (equipmentCmpt.TakeOffEquipment(itemObj, addToReceiver, receiver))
		{
			GameUI.Instance.PlayTakeOffEquipAudio();
			return true;
		}
		return false;
	}

	public bool EquipItem(ItemObject itemObj)
	{
		if (equipmentCmpt != null)
		{
			EquipmentCmpt.Receiver receiver = ((!(PeSingleton<PeCreature>.Instance.mainPlayer == null)) ? PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PackageCmpt>() : null);
			if (equipmentCmpt.PutOnEquipment(itemObj, addToReceiver: true, receiver))
			{
				GameUI.Instance.PlayPutOnEquipAudio();
				return true;
			}
		}
		return false;
	}

	public bool EquipItem(ItemObject itemObj, EquipmentCmpt.Receiver receiver)
	{
		if (equipmentCmpt != null && equipmentCmpt.PutOnEquipment(itemObj, addToReceiver: true, receiver))
		{
			GameUI.Instance.PlayPutOnEquipAudio();
			return true;
		}
		return false;
	}

	private void RefreshEqList()
	{
		ClearEqList();
		if (!(null != equipmentCmpt) || equipmentCmpt._ItemList == null || equipmentCmpt._ItemList.Count <= 0)
		{
			return;
		}
		foreach (ItemObject item in equipmentCmpt._ItemList)
		{
			Equip cmpt = item.GetCmpt<Equip>();
			if (cmpt == null || mEquipmentList == null || mEquipmentList.Count < 10)
			{
				continue;
			}
			for (int i = 0; i < 10; i++)
			{
				if (Convert.ToBoolean(cmpt.equipPos & (int)mEquipmentList[i].ItemMask))
				{
					mEquipmentList[i].SetItem(item);
				}
			}
		}
	}

	private void ClearEqList()
	{
		if (mEquipmentList != null && mEquipmentList.Count >= 10)
		{
			for (int i = 0; i < 10; i++)
			{
				mEquipmentList[i].SetItem(null);
			}
		}
	}

	public void UpdateEquipAndTex()
	{
		if (mInit && isShow)
		{
			RefreshEqList();
			RefreshEquipment();
		}
	}

	private void EquipmentChangeEvent(object sender, EquipmentCmpt.EventArg arg)
	{
		UpdateEquipAndTex();
	}

	private void RefreshEquipment()
	{
		if (mInit && isShow && null != viewCmpt)
		{
			StopCoroutine("UpdateModelIterator");
			StopRefreshEquipment();
			StartCoroutine("UpdateModelIterator");
		}
	}

	private void StopRefreshEquipment()
	{
		waitingToCloneModel = false;
		if ((bool)_newViewModel)
		{
			UnityEngine.Object.Destroy(_newViewModel);
			_newViewModel = null;
		}
		_meshControllers = null;
	}

	private IEnumerator UpdateModelIterator()
	{
		waitingToCloneModel = true;
		if (null != viewCmpt)
		{
			_meshControllers = viewCmpt.GetComponentsInChildren<CreationController>();
			if (_meshControllers != null)
			{
				for (int i = 0; i < _meshControllers.Length; i++)
				{
					if (_meshControllers[i] != null && !_meshControllers[i].isBuildFinished)
					{
						yield return null;
						i--;
					}
				}
			}
			_meshControllers = null;
			yield return null;
			_newViewModel = PeViewStudio.CloneCharacterViewModel(viewCmpt);
			if ((bool)_newViewModel)
			{
				_newViewModel.transform.position = new Vector3(0f, -1000f, 0f);
				SkinnedMeshRenderer renderer = _newViewModel.GetComponent<SkinnedMeshRenderer>();
				renderer.updateWhenOffscreen = true;
			}
		}
		if ((bool)_newViewModel)
		{
			if (_viewModel != null)
			{
				UnityEngine.Object.Destroy(_viewModel);
			}
			_viewModel = _newViewModel;
			_viewController.SetTarget(_viewModel.transform);
			mEqTex.GetComponent<UIViewController>().Init(_viewController);
			mEqTex.mainTexture = _viewController.RenderTex;
			mEqTex.enabled = true;
			_newViewModel = null;
		}
		waitingToCloneModel = false;
	}

	public void OnLeftMouseCliked_Equip(Grid_N grid)
	{
		if (!(m_NpcCmpt == null) && !(null == equipmentCmpt) && !(null == grid) && grid.ItemObj != null)
		{
			if (equipmentCmpt.TryTakeOffEquipment(grid.ItemObj))
			{
				SelectItem_N.Instance.SetItemGrid(grid);
			}
			else
			{
				PeTipMsg.Register(PELocalization.GetString(8000594), PeTipMsg.EMsgLevel.Error);
			}
		}
	}

	public void OnRightMouseCliked_Equip(Grid_N grid)
	{
		if (null == equipmentCmpt || grid.ItemObj == null)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			if (equipmentCmpt.TryTakeOffEquipment(grid.ItemObj))
			{
				PlayerNetwork.mainPlayer.RequestNpcTakeOffEquip(servant.Id, grid.ItemObj.instanceId, -1);
				GameUI.Instance.PlayTakeOffEquipAudio();
			}
			return;
		}
		PlayerPackageCmpt playerPackageCmpt = ((!(PeSingleton<PeCreature>.Instance.mainPlayer == null)) ? PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>() : null);
		if (equipmentCmpt.TakeOffEquipment(grid.ItemObj, addToReceiver: true, playerPackageCmpt))
		{
			GameUI.Instance.mItemPackageCtrl.ResetItem();
			GameUI.Instance.PlayTakeOffEquipAudio();
		}
		else if (null == playerPackageCmpt || playerPackageCmpt.package.CanAdd(grid.ItemObj))
		{
			MessageBox_N.ShowOkBox(PELocalization.GetString(8000594));
		}
		else
		{
			CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(8000050));
		}
	}

	public void OnDropItem_Equip(Grid_N grid)
	{
		if (null == equipmentCmpt)
		{
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar || !UIToolFuncs.CanEquip(SelectItem_N.Instance.ItemObj, commonCmpt.sex) || ((uint)grid.ItemMask & (uint)SelectItem_N.Instance.ItemObj.protoData.equipPos) == 0)
		{
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			if (equipmentCmpt.NetTryPutOnEquipment(SelectItem_N.Instance.ItemObj))
			{
				PlayerNetwork.mainPlayer.RequestNpcPutOnEquip(servant.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
				SelectItem_N.Instance.SetItem(null);
				GameUI.Instance.PlayPutOnEquipAudio();
			}
			return;
		}
		switch (SelectItem_N.Instance.Place)
		{
		case ItemPlaceType.IPT_Bag:
		case ItemPlaceType.IPT_ServantInteraction:
		case ItemPlaceType.IPT_ServantInteraction2:
			if (equipmentCmpt.PutOnEquipment(SelectItem_N.Instance.ItemObj))
			{
				SelectItem_N.Instance.RemoveOriginItem();
				grid.SetItem(SelectItem_N.Instance.ItemObj);
				SelectItem_N.Instance.SetItem(null);
				GameUI.Instance.PlayPutOnEquipAudio();
			}
			break;
		default:
			SelectItem_N.Instance.SetItem(null);
			break;
		}
	}

	public bool SetItemWithIndex(ItemObject itemObj, int index = -1)
	{
		if (!mCkInventory.isChecked)
		{
			mCkInventory.isChecked = true;
		}
		if (index == -1)
		{
			return mInteractionPackage.Add(itemObj);
		}
		if (index < 0 || index > mInteractionPackage.Count)
		{
			return false;
		}
		if (mInteractionPackage != null)
		{
			mInteractionPackage[index] = itemObj;
		}
		return true;
	}

	public bool SetItemWithIndexWithPackage2(ItemObject itemObj, int index = -1)
	{
		if (index == -1)
		{
			return mInteraction2Package.Add(itemObj);
		}
		if (index < 0 || index > mInteraction2Package.Count)
		{
			return false;
		}
		if (mInteraction2Package != null)
		{
			mInteraction2Package[index] = itemObj;
		}
		return true;
	}

	private void GetNpcPakgeSlotList()
	{
		if (mInteractionPackage != null)
		{
			mInteractionPackage.eventor.Unsubscribe(InteractionpackageChange);
		}
		mInteractionPackage = packageCmpt.GetSlotList();
		mInteractionPackage.eventor.Subscribe(InteractionpackageChange);
		if (mInteraction2Package != null)
		{
			mInteraction2Package.eventor.Unsubscribe(InteractionPackage2Change);
		}
		mInteraction2Package = packageCmpt.GetHandinList();
		mInteraction2Package.eventor.Subscribe(InteractionPackage2Change);
		if (mPrivatePakge != null)
		{
			mPrivatePakge.eventor.Unsubscribe(PrivatepackageChange);
		}
		mPrivatePakge = packageCmpt.GetPrivateSlotList();
		mPrivatePakge.eventor.Subscribe(PrivatepackageChange);
	}

	private void Reflashpackage()
	{
		RefreshInteractionpackage();
		ReflashPrivatePackage();
		RefreshInteractionPackage2();
	}

	private void ClearNpcPackage()
	{
		ClearInteractionpackage();
		ClearPrivatePackage();
		ClearInteractionpackage2();
	}

	private void InteractionpackageChange(object sender, SlotList.ChangeEvent arg)
	{
		RefreshInteractionpackage();
	}

	private void RefreshInteractionpackage()
	{
		ClearInteractionpackage();
		for (int i = 0; i < mInteractionGridCount; i++)
		{
			if (mInteractionPackage == null)
			{
				mInteractionList[i].SetItem(null);
			}
			else
			{
				mInteractionList[i].SetItem(mInteractionPackage[i]);
			}
		}
	}

	private void ClearInteractionpackage()
	{
		foreach (Grid_N mInteraction in mInteractionList)
		{
			mInteraction.SetItem(null);
		}
	}

	private void InteractionPackage2Change(object sender, SlotList.ChangeEvent arg)
	{
		RefreshInteractionPackage2();
	}

	private void RefreshInteractionPackage2()
	{
		ClearInteractionpackage2();
		for (int i = 0; i < mInteraction2GridCount; i++)
		{
			if (mInteraction2Package == null)
			{
				mInteraction2List[i].SetItem(null);
			}
			else
			{
				mInteraction2List[i].SetItem(mInteraction2Package[i]);
			}
		}
	}

	private void ClearInteractionpackage2()
	{
		foreach (Grid_N mInteraction in mInteraction2List)
		{
			mInteraction.SetItem(null);
		}
	}

	private void PrivatepackageChange(object sender, SlotList.ChangeEvent arg)
	{
		ReflashPrivatePackage();
	}

	private void ReflashPrivatePackage()
	{
		ClearPrivatePackage();
		int num = (mPageIndex - 1) * mPrivateItemGridCount;
		for (int i = 0; i < mPrivateList.Count; i++)
		{
			mPrivateList[i].SetItem(mPrivatePakge[num + i]);
		}
	}

	private void ClearPrivatePackage()
	{
		foreach (Grid_N mPrivate in mPrivateList)
		{
			mPrivate.SetItem(null);
		}
	}

	public void OnLeftMouseCliked_InterPackage(Grid_N grid)
	{
		if (!(null == servant))
		{
			SelectItem_N.Instance.SetItem(grid.ItemObj, grid.ItemPlace, grid.ItemIndex);
		}
	}

	public void OnRightMouseCliked_InterPackage(Grid_N grid)
	{
		if (!(null == servant))
		{
			UseItemCmpt useItemCmpt = servant.GetCmpt<UseItemCmpt>();
			if (null == useItemCmpt)
			{
				useItemCmpt = servant.Add<UseItemCmpt>();
			}
			if (!useItemCmpt.Request(grid.ItemObj))
			{
			}
		}
	}

	public void OnDropItem_InterPackage(Grid_N grid)
	{
		if (null == servant || grid.ItemObj != null)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			switch (SelectItem_N.Instance.Place)
			{
			case ItemPlaceType.IPT_ServantEqu:
				if (SelectItem_N.Instance.RemoveOriginItem())
				{
					PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, servant.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
				}
				break;
			case ItemPlaceType.IPT_Bag:
			case ItemPlaceType.IPT_ServantInteraction2:
				PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, servant.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
				break;
			}
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		switch (SelectItem_N.Instance.Place)
		{
		case ItemPlaceType.IPT_Bag:
		case ItemPlaceType.IPT_ServantEqu:
		case ItemPlaceType.IPT_ServantInteraction:
		case ItemPlaceType.IPT_ServantInteraction2:
			if (SelectItem_N.Instance.RemoveOriginItem())
			{
				SetItemWithIndex(SelectItem_N.Instance.ItemObj, grid.ItemIndex);
				grid.SetItem(SelectItem_N.Instance.ItemObj);
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
		if (!(null == servant))
		{
			SelectItem_N.Instance.SetItem(grid.ItemObj, grid.ItemPlace, grid.ItemIndex);
		}
	}

	public void OnRightMouseCliked_InterPackage2(Grid_N grid)
	{
		if (!(null == servant))
		{
			UseItemCmpt useItemCmpt = servant.GetCmpt<UseItemCmpt>();
			if (null == useItemCmpt)
			{
				useItemCmpt = servant.Add<UseItemCmpt>();
			}
			if (!useItemCmpt.Request(grid.ItemObj))
			{
			}
		}
	}

	public void OnDropItem_InterPackage2(Grid_N grid)
	{
		if (null == servant || grid.ItemObj != null)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			switch (SelectItem_N.Instance.Place)
			{
			case ItemPlaceType.IPT_ServantEqu:
				if (SelectItem_N.Instance.RemoveOriginItem())
				{
					PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, servant.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
				}
				break;
			case ItemPlaceType.IPT_Bag:
			case ItemPlaceType.IPT_ServantInteraction:
				PlayerNetwork.mainPlayer.RequestGiveItem2Npc((int)grid.ItemPlace, servant.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
				break;
			}
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		switch (SelectItem_N.Instance.Place)
		{
		case ItemPlaceType.IPT_Bag:
		case ItemPlaceType.IPT_ServantEqu:
		case ItemPlaceType.IPT_ServantInteraction:
		case ItemPlaceType.IPT_ServantInteraction2:
			if (SelectItem_N.Instance.RemoveOriginItem())
			{
				SetItemWithIndexWithPackage2(SelectItem_N.Instance.ItemObj, grid.ItemIndex);
				grid.SetItem(SelectItem_N.Instance.ItemObj);
				SelectItem_N.Instance.SetItem(null);
			}
			break;
		default:
			SelectItem_N.Instance.SetItem(null);
			break;
		}
	}
}
