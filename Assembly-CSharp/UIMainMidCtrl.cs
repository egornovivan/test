using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;
using WhiteCat;

public class UIMainMidCtrl : UIStaticWnd
{
	public delegate void OnDropItemTask();

	public delegate void DTweenFinished(bool forward);

	[SerializeField]
	private GameObject mNewGridPrefab;

	[SerializeField]
	private GameObject mEngun;

	[SerializeField]
	private GameObject mgun;

	[SerializeField]
	private GameObject mBow;

	[SerializeField]
	private UIGridBoxBars mBoxBar;

	[SerializeField]
	private int mItemCount;

	[SerializeField]
	private UIHealthBar mHp_0;

	[SerializeField]
	private UIHealthBar mStaminaBar_1;

	[SerializeField]
	private UISlider mScEnerger_2;

	[SerializeField]
	private UISlider mScJect_3;

	[SerializeField]
	private UISlider mComfort_4;

	[SerializeField]
	private UISlider mScHunger_5;

	[SerializeField]
	private GameObject mGun_6;

	[SerializeField]
	private UISlider mScShild_8;

	[SerializeField]
	private UISlider mScOxygen_9;

	[SerializeField]
	private UILabel mGunLabel;

	[SerializeField]
	private UISprite mComfortSprite;

	[SerializeField]
	private AnimationCurve mComfortAlphaCurve;

	[SerializeField]
	private UISprite mComfortForeground;

	[SerializeField]
	private float m_ComfortFallValue = 10f;

	[SerializeField]
	private float m_lastComfort;

	private float m_CurComfort;

	private bool m_CurvePlay;

	private float m_CurveTotalTime;

	private static UIMainMidCtrl _instance;

	private static readonly float AttrLerpF = 5f;

	private List<QuickBarItem_N> mItems = new List<QuickBarItem_N>();

	private bool mUpdateLink;

	private ShortCutSlotList mCutSlotList;

	private PlayerPackageCmpt mPackageCmpt;

	private Motion_Equip mEquip;

	private PeEntity mMainPlayer;

	private PEGun mPeGun;

	private PEBow mPEBow;

	public bool playTween;

	private bool forward = true;

	public TweenInterpolator tweener;

	public UIGrid m_BuffGrid;

	public CSUI_BuffItem m_BuffPrefab;

	private List<CSUI_BuffItem> m_BuffList = new List<CSUI_BuffItem>();

	private List<string> m_IconList = new List<string>();

	private bool m_Reposition;

	[SerializeField]
	private Transform m_QuickBarTutorialParent;

	[SerializeField]
	private UIWndTutorialTip_N m_QuickBarTutorialPrefab;

	[SerializeField]
	private Transform m_FullQuickBarTutorialParent;

	[SerializeField]
	private UIWndTutorialTip_N m_FullQuickBarTutorialPrefab;

	public static UIMainMidCtrl Instance => _instance;

	private Motion_Equip equip
	{
		get
		{
			if (null == mEquip && null != GameUI.Instance.mMainPlayer)
			{
				mEquip = GameUI.Instance.mMainPlayer.GetCmpt<Motion_Equip>();
			}
			return mEquip;
		}
	}

	private PeEntity mainPlayer
	{
		get
		{
			if (null == mMainPlayer)
			{
				mMainPlayer = PeSingleton<MainPlayer>.Instance.entity;
				if (null != mMainPlayer)
				{
					mMainPlayer.equipmentCmpt.changeEventor.Subscribe(SetCurUseItemByEvent);
				}
			}
			return mMainPlayer;
		}
	}

	public float EnergyValue
	{
		get
		{
			return mScEnerger_2.sliderValue;
		}
		set
		{
			mScEnerger_2.sliderValue = value;
		}
	}

	public float Ject_Value
	{
		get
		{
			return mScJect_3.sliderValue;
		}
		set
		{
			mScJect_3.sliderValue = value;
		}
	}

	private PEGun peGun
	{
		get
		{
			if (null == mPeGun)
			{
				PeEntity peEntity = PeSingleton<PeCreature>.Instance.mainPlayer;
				if (null != peEntity && null != peEntity.motionEquipment)
				{
					mPeGun = peEntity.motionEquipment.gun;
				}
			}
			return mPeGun;
		}
	}

	private PEBow peBow
	{
		get
		{
			if (null == mPEBow)
			{
				PeEntity peEntity = PeSingleton<PeCreature>.Instance.mainPlayer;
				if (null != peEntity && null != peEntity.motionEquipment)
				{
					mPEBow = peEntity.motionEquipment.bow;
				}
			}
			return mPEBow;
		}
	}

	public bool quickBarForbiden
	{
		set
		{
			if (mItems.Count == 0)
			{
				return;
			}
			foreach (QuickBarItem_N mItem in mItems)
			{
				mItem.SetGridForbiden(value);
			}
		}
	}

	public event OnDropItemTask e_OnDropItemTask;

	public event DTweenFinished onTweenFinished;

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
		InitGrid();
	}

	private void UpdatePlayerAtrbute()
	{
		mHp_0.mMaxValue = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.HpMax);
		mHp_0.mCurValue = Mathf.Lerp(mHp_0.mCurValue, GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Hp), AttrLerpF * Time.deltaTime);
		mStaminaBar_1.mMaxValue = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.StaminaMax);
		mStaminaBar_1.mCurValue = Mathf.Lerp(mStaminaBar_1.mCurValue, GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Stamina), AttrLerpF * Time.deltaTime);
		float num = 0f;
		float attribute = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Energy);
		num = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.EnergyMax);
		mScEnerger_2.sliderValue = ((!(num <= 0f)) ? Convert.ToSingle(attribute / num) : 0f);
		attribute = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Comfort);
		num = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.ComfortMax);
		mComfort_4.sliderValue = ((!(num <= 0f)) ? Convert.ToSingle(attribute / num) : 0f);
		attribute = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Hunger);
		num = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.HungerMax);
		mScHunger_5.sliderValue = ((!(num <= 0f)) ? Convert.ToSingle(attribute / num) : 0f);
		attribute = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Shield);
		num = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.ShieldMax);
		mScShild_8.sliderValue = ((!(num <= 0f)) ? Convert.ToSingle(attribute / num) : 0f);
		attribute = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Oxygen);
		num = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.OxygenMax);
		mScOxygen_9.sliderValue = ((!(num <= 0f)) ? Convert.ToSingle(attribute / num) : 0f);
		m_CurComfort = GameUI.Instance.mMainPlayer.GetAttribute(AttribType.Comfort);
		if (m_CurComfort > m_lastComfort)
		{
			m_lastComfort = m_CurComfort;
		}
		else if (m_lastComfort - m_CurComfort > m_ComfortFallValue)
		{
			m_CurvePlay = true;
			m_lastComfort = m_CurComfort;
			m_CurveTotalTime = 0f;
		}
		else
		{
			m_lastComfort = m_CurComfort;
		}
		if (m_CurvePlay)
		{
			m_CurveTotalTime += Time.deltaTime;
			mComfortForeground.alpha = mComfortAlphaCurve.Evaluate(m_CurveTotalTime);
			if (mComfortForeground.alpha >= 1f)
			{
				m_CurvePlay = false;
			}
		}
		string empty = string.Empty;
		empty = ((mComfort_4.sliderValue < 0.2f) ? "face3" : ((!(mComfort_4.sliderValue < 0.5f)) ? "face1" : "face2"));
		if (empty != string.Empty && empty != mComfortSprite.spriteName)
		{
			mComfortSprite.spriteName = empty;
			mComfortSprite.MakePixelPerfect();
		}
	}

	private void GetCutSlotList()
	{
		if (GameUI.Instance == null || GameUI.Instance.mMainPlayer == null)
		{
			return;
		}
		mPackageCmpt = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (mPackageCmpt != null)
		{
			mCutSlotList = mPackageCmpt.shortCutSlotList;
			if (mCutSlotList != null)
			{
				mCutSlotList.onListUpdate += OnShortCutUpdate;
				OnShortCutUpdate();
			}
		}
	}

	private void Update()
	{
		if (playTween)
		{
			PlayTween(forward);
			forward = !forward;
			playTween = false;
		}
		if (!(GameUI.Instance == null))
		{
			UpdateShortCut();
			if (GameUI.Instance.mMainPlayer != null)
			{
				UpdatePlayerAtrbute();
			}
			UpdataGunNum();
			UpdataJetPack();
		}
	}

	private void LateUpdate()
	{
		UpdateReposition();
	}

	private void InitGrid()
	{
		mBoxBar.Init(mNewGridPrefab.gameObject, mItemCount);
		if (mBoxBar.Items.Count > 0)
		{
			mItems.Add(mBoxBar.Items[0].GetComponent<QuickBarItem_N>());
			mItems[0].UpdateKeyInfo(-1);
			if (null != mainPlayer)
			{
				SetCurUseItem(mainPlayer.equipmentCmpt.mainHandEquipment);
			}
			else
			{
				PeSingleton<MainPlayer>.Instance.mainPlayerCreatedEventor.Subscribe(delegate
				{
					SetCurUseItem(mainPlayer.equipmentCmpt.mainHandEquipment);
				});
			}
			for (int i = 1; i < mItemCount; i++)
			{
				mItems.Add(mBoxBar.Items[i].GetComponent<QuickBarItem_N>());
				mItems[i].SetItemPlace(ItemPlaceType.IPT_HotKeyBar, i);
				mItems[i].SetGridMask(GridMask.GM_Any);
				mItems[i].UpdateKeyInfo(i);
				mItems[i].ItemIndex = i - 1;
				mItems[i].onLeftMouseClicked = OnLeftMouseCliked;
				mItems[i].onRightMouseClicked = OnRightMouseCliked;
				mItems[i].onDropItem = OnDropItem;
			}
		}
		mBoxBar.e_PageIndexChange += delegate
		{
			OnShortCutUpdate();
		};
	}

	private void OnLeftMouseCliked(Grid_N grid)
	{
		if (grid.Item != null && !GameUI.Instance.mItemPackageCtrl.EqualUsingItem(grid.Item))
		{
			SelectItem_N.Instance.SetItemGrid(grid);
		}
	}

	private void OnRightMouseCliked(Grid_N grid)
	{
		if (grid.Item != null && !GameUI.Instance.mItemPackageCtrl.EqualUsingItem(grid.Item))
		{
			int index = grid.ItemIndex + GetCurPageIndex();
			UseItem(mCutSlotList.GetItemObj(index));
		}
	}

	public void OnKeyDown_QuickBar(int inputIndex)
	{
		if (inputIndex < 0 || inputIndex >= mItems.Count - 1 || mItems[inputIndex + 1].IsForbiden())
		{
			return;
		}
		int index = inputIndex + GetCurPageIndex();
		ShortCutItem item = mCutSlotList.GetItem(index);
		if (item != null)
		{
			ItemObject itemObj = mCutSlotList.GetItemObj(index);
			if (!GameUI.Instance.mItemPackageCtrl.EqualUsingItem(itemObj))
			{
				UseItem(itemObj);
			}
		}
	}

	public void OnKeyFunc_PrevQuickBar()
	{
		mBoxBar.BtnPageUpOnClick();
	}

	public void OnKeyFunc_NextQuickBar()
	{
		mBoxBar.BtnPageDnOnClick();
	}

	private void UseItem(ItemObject itemObj)
	{
		if (itemObj != null)
		{
			UseItemCmpt useItemCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<UseItemCmpt>();
			if (null == useItemCmpt)
			{
				useItemCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.Add<UseItemCmpt>();
			}
			if (!useItemCmpt.Request(itemObj))
			{
			}
		}
	}

	public void SetCurUseItemByEvent(object sender, EquipmentCmpt.EventArg arg)
	{
		if ((arg.itemObj.protoData.equipPos & 0x10) != 0)
		{
			SetCurUseItem((!arg.isAdd) ? null : arg.itemObj);
		}
	}

	private void SetCurUseItem(ItemObject itemObj)
	{
		if (mItems != null && mItems.Count > 0)
		{
			mItems[0].SetItem(itemObj);
			mItems[0].mScriptIco.spriteName = ((itemObj != null) ? "itemhand_get" : "itemhand");
			mItems[0].mScriptIco.MakePixelPerfect();
		}
	}

	public void OnDropItem(Grid_N grid)
	{
		if (null == SelectItem_N.Instance || SelectItem_N.Instance.ItemObj == null || null == GameUI.Instance || mCutSlotList == null || null == grid || (null != GameUI.Instance.mItemPackageCtrl && GameUI.Instance.mItemPackageCtrl.EqualUsingItem(SelectItem_N.Instance.ItemSample)))
		{
			return;
		}
		if (this.e_OnDropItemTask != null && SelectItem_N.Instance.ItemObj.protoId == 916)
		{
			this.e_OnDropItemTask();
		}
		if (GameConfig.IsMultiMode)
		{
			int num = SelectItem_N.Instance.Index + GetCurPageIndex();
			int destIndex = grid.ItemIndex + GetCurPageIndex();
			int srcIndex = -1;
			int itemId = -1;
			if (SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar)
			{
				ShortCutItem item = mCutSlotList.GetItem(num);
				if (item != null)
				{
					srcIndex = num;
					itemId = item.itemInstanceId;
				}
			}
			else
			{
				srcIndex = -1;
				itemId = SelectItem_N.Instance.ItemObj.instanceId;
			}
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RequestSetShortcuts(itemId, srcIndex, destIndex, SelectItem_N.Instance.Place);
			}
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		switch (SelectItem_N.Instance.Place)
		{
		case ItemPlaceType.IPT_HotKeyBar:
			if (grid.Item == null)
			{
				int index = SelectItem_N.Instance.Index + GetCurPageIndex();
				int index2 = grid.ItemIndex + GetCurPageIndex();
				mCutSlotList.PutItem(mCutSlotList.GetItem(index), index2);
				mCutSlotList.PutItem(null, index);
			}
			else
			{
				int index3 = SelectItem_N.Instance.Index + GetCurPageIndex();
				ShortCutItem item2 = mCutSlotList.GetItem(index3);
				int index4 = grid.ItemIndex + GetCurPageIndex();
				ShortCutItem item3 = mCutSlotList.GetItem(index4);
				if (item2 != null)
				{
					mCutSlotList.PutItem(item2, index4);
				}
				mCutSlotList.PutItem(item3, index3);
			}
			SelectItem_N.Instance.SetItem(null);
			break;
		case ItemPlaceType.IPT_Equipment:
			break;
		case ItemPlaceType.IPT_Bag:
		{
			int num2 = 0;
			bool isMission = false;
			if (null != GameUI.Instance.mItemPackageCtrl)
			{
				if (!GameUI.Instance.mItemPackageCtrl.isMission)
				{
					num2 = ItemPackage.CodeIndex((ItemPackage.ESlotType)GameUI.Instance.mItemPackageCtrl.CurrentPickTab, SelectItem_N.Instance.Index);
				}
				else
				{
					num2 = ItemPackage.CodeIndex(ItemPackage.ESlotType.Item, SelectItem_N.Instance.Index);
					isMission = true;
				}
				if (null != mPackageCmpt)
				{
					mPackageCmpt.PutItemToShortCutList(num2, grid.ItemIndex + GetCurPageIndex(), isMission);
				}
			}
			SelectItem_N.Instance.SetItem(null);
			break;
		}
		}
	}

	private int GetCurPageIndex()
	{
		return (mBoxBar.PageIndex - 1) * (mBoxBar.ItemCount - 1);
	}

	public void SetItemWithIndex(ItemObject grid, int index, bool fromHotBar = false)
	{
		mItems[index].SetItem(grid);
	}

	public void RemoveItemWithIndex(int index)
	{
		if (mCutSlotList == null)
		{
			GetCutSlotList();
		}
		if (mCutSlotList != null)
		{
			index += GetCurPageIndex();
			if (GameConfig.IsMultiMode)
			{
				PlayerNetwork.mainPlayer.RequestSetShortcuts(-1, index, -1, ItemPlaceType.IPT_Null);
			}
			else
			{
				mCutSlotList.PutItem(null, index);
			}
		}
	}

	public void BtnBuildOnClick()
	{
		if (SingleGameStory.curType == SingleGameStory.StoryScene.MainLand || SingleGameStory.curType == SingleGameStory.StoryScene.TrainingShip)
		{
			GameUI.Instance.mBuildBlock.EnterBuildMode();
		}
		else
		{
			new PeTipMsg("[C8C800]" + PELocalization.GetString(82209004), PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
		}
	}

	public void OnHealthBtnClick()
	{
		GameUI.Instance.mUIPlayerInfoCtrl.Show();
	}

	private void OnShortCutUpdate()
	{
		if (mCutSlotList != null)
		{
			int curPageIndex = GetCurPageIndex();
			for (int i = 1; i < mBoxBar.ItemCount; i++)
			{
				ShortCutItem item = mCutSlotList.GetItem(curPageIndex + (i - 1));
				mItems[i].SetItem(item);
				ItemObject itemObj = mCutSlotList.GetItemObj(curPageIndex + (i - 1));
				mItems[i].SetDurabilityBg(itemObj);
			}
		}
	}

	private void UpdateShortCut()
	{
		if (mCutSlotList == null)
		{
			GetCutSlotList();
		}
	}

	private void UpdataGunNum()
	{
		if (UIDrivingCtrl.Instance != null && UIDrivingCtrl.Instance.IsShow)
		{
			mGun_6.SetActive(value: false);
			return;
		}
		int num = 0;
		int num2 = 0;
		if (peGun != null)
		{
			switch (peGun.m_AmmoType)
			{
			case AmmoType.Bullet:
				mEngun.SetActive(value: false);
				mgun.SetActive(value: true);
				mBow.SetActive(value: false);
				break;
			case AmmoType.Energy:
				mEngun.SetActive(value: true);
				mgun.SetActive(value: false);
				mBow.SetActive(value: false);
				break;
			}
			num = (int)peGun.magazineValue;
			num2 = (int)peGun.magazineSize;
			if (num2 == 99999)
			{
				mGunLabel.text = num.ToString();
			}
			else
			{
				mGunLabel.text = num + "/" + num2;
			}
		}
		else
		{
			if (!(peBow != null) || !(null != PeSingleton<PeCreature>.Instance.mainPlayer))
			{
				mGun_6.SetActive(value: false);
				return;
			}
			mEngun.SetActive(value: false);
			mgun.SetActive(value: false);
			mBow.SetActive(value: true);
			num2 = PeSingleton<PeCreature>.Instance.mainPlayer.packageCmpt.GetItemCount(peBow.curItemID);
			mGunLabel.text = num2.ToString();
		}
		mGunLabel.gameObject.SetActive(value: true);
		mGun_6.SetActive(value: true);
	}

	private void UpdataJetPack()
	{
		if (null != equip)
		{
			mScJect_3.sliderValue = ((!(equip.jetPackEnMax > 0f)) ? 0f : (equip.jetPackEnCurrent / equip.jetPackEnMax));
		}
		else
		{
			mScJect_3.sliderValue = 0f;
		}
	}

	public void PlayTween(bool forward)
	{
		tweener.isPlaying = true;
		if (forward)
		{
			if (tweener.speed < 0f)
			{
				tweener.ReverseSpeed();
			}
		}
		else if (tweener.speed > 0f)
		{
			tweener.ReverseSpeed();
		}
	}

	public void OnTweenFinish(bool forward)
	{
		if (this.onTweenFinished != null)
		{
			this.onTweenFinished(forward: true);
		}
	}

	public void AddBuffShow(string _icon, string _describe)
	{
		if (m_IconList.Contains(_icon))
		{
			m_IconList.Add(_icon);
			return;
		}
		m_IconList.Add(_icon);
		CSUI_BuffItem cSUI_BuffItem = UnityEngine.Object.Instantiate(m_BuffPrefab);
		if (!cSUI_BuffItem.gameObject.activeSelf)
		{
			cSUI_BuffItem.gameObject.SetActive(value: true);
		}
		cSUI_BuffItem.transform.parent = m_BuffGrid.transform;
		CSUtils.ResetLoacalTransform(cSUI_BuffItem.transform);
		cSUI_BuffItem.SetInfo(_icon, _describe);
		m_BuffList.Add(cSUI_BuffItem);
		m_Reposition = true;
	}

	public void DeleteBuffShow(string _icon)
	{
		List<CSUI_BuffItem> list = m_BuffList.FindAll((CSUI_BuffItem i) => i._icon == _icon);
		if (list != null)
		{
			if (list.Count == 1)
			{
				UnityEngine.Object.Destroy(list[0].gameObject);
				m_BuffList.Remove(list[0]);
				m_IconList.Remove(_icon);
			}
			else if (list.Count > 1)
			{
				m_IconList.Remove(_icon);
			}
			m_Reposition = true;
		}
	}

	private void UpdateReposition()
	{
		if (m_Reposition)
		{
			m_Reposition = false;
			m_BuffGrid.repositionNow = true;
		}
	}

	public void ShowQuickBarTutorial()
	{
		if (PeGameMgr.IsTutorial)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_QuickBarTutorialPrefab.gameObject);
			gameObject.transform.parent = m_QuickBarTutorialParent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
		}
	}

	public void ShowFullQuickBarTutorial()
	{
		if (PeGameMgr.IsTutorial)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_FullQuickBarTutorialPrefab.gameObject);
			gameObject.transform.parent = m_FullQuickBarTutorialParent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
		}
	}
}
