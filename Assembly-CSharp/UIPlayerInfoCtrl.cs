using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using PeUIEffect;
using UnityEngine;
using WhiteCat;

public class UIPlayerInfoCtrl : UIBaseWnd
{
	public GameObject GridContent;

	public Grid_N mGridPrefab;

	public UITexture mEqTex;

	public GameObject mPlayerStagePrefab;

	public Vector3 mPlayerStagePos;

	public Vector3 mPlayerStageScale;

	public UILabel mLbName;

	public UISprite mSprSexFemale;

	public UISprite mSprSexMale;

	public UILabel mLbHealth;

	public UISlider mSdHealth;

	public UILabel mLbHealthBuff;

	public UILabel mLbStamina;

	public UISlider mSdStamina;

	public UILabel mLbStaminaBuff;

	public UILabel mLbHunger;

	public UISlider mSdHunger;

	public UILabel mLbHungerBuff;

	public UILabel mLbComfort;

	public UISlider mSdComfort;

	public UILabel mLbComfortBuff;

	public UILabel mLbOxygen;

	public UISlider mSdOxygen;

	public UILabel mLbOxygenBuff;

	public UILabel mLbShield;

	public UISlider mSdShield;

	public UILabel mLbShieldBuff;

	public UILabel mLbEnergy;

	public UISlider mSdEnergy;

	public UILabel mLbEnergyBuff;

	public UILabel mLbAttack;

	public UILabel mLbAttackBuff;

	public UILabel mLbDefense;

	public UILabel mLbDefenseBuff;

	public List<BoxCollider> mSuiteBtns;

	public GameObject mEquipmentPage;

	public GameObject armorPage;

	public GameObject waitingImage;

	public UIArmorSuitEvent[] armorSuitButtons;

	public Bone2DObjects bone2DObjects;

	public UIDragObject dragObject;

	[SerializeField]
	private UISpriteScaleEffect effect;

	private AttributeInfo AttrHpInfo = new AttributeInfo(AttribType.Hp, AttribType.HpMax);

	private AttributeInfo AttrStaminaInfo = new AttributeInfo(AttribType.Stamina, AttribType.StaminaMax);

	private AttributeInfo AttrHungerInfo = new AttributeInfo(AttribType.Hunger, AttribType.HungerMax);

	private AttributeInfo AttrComfortInfo = new AttributeInfo(AttribType.Comfort, AttribType.ComfortMax);

	private AttributeInfo AttrOxygenInfo = new AttributeInfo(AttribType.Oxygen, AttribType.OxygenMax);

	private AttributeInfo AttrShieldInfo = new AttributeInfo(AttribType.Shield, AttribType.ShieldMax);

	private AttributeInfo AttrEnergyInfo = new AttributeInfo(AttribType.Energy, AttribType.EnergyMax);

	private AttributeInfo AttrAtkInfo = new AttributeInfo(AttribType.Atk, AttribType.Atk);

	private AttributeInfo AttrDefInfo = new AttributeInfo(AttribType.Def, AttribType.Def);

	[SerializeField]
	private UILabel m_BuffStrsLb;

	private string m_BuffStrFormat = "+{0}";

	private PeEntity m_Player;

	private BiologyViewCmpt viewCmpt;

	private CommonCmpt commonCmpt;

	private EquipmentCmpt m_EquipmentCmpt;

	private PlayerPackageCmpt packageCmpt;

	private EntityInfoCmpt entityInfoCmpt;

	private PlayerArmorCmpt playerArmorCmpt;

	private PeViewController _viewController;

	private GameObject _viewModel;

	private List<Grid_N> mEquipment = new List<Grid_N>();

	private string[] mSpName;

	private CreationController[] _meshControllers;

	private GameObject _newViewModel;

	private int _delay;

	private ItemObject _selectedArmorItem;

	private ArmorType _selectedArmorType;

	private PeEntity player
	{
		get
		{
			return m_Player;
		}
		set
		{
			m_Player = value;
			AttrHpInfo.SetEntity(value);
			AttrStaminaInfo.SetEntity(value);
			AttrHungerInfo.SetEntity(value);
			AttrComfortInfo.SetEntity(value);
			AttrOxygenInfo.SetEntity(value);
			AttrShieldInfo.SetEntity(value);
			AttrEnergyInfo.SetEntity(value);
			AttrAtkInfo.SetEntity(value);
			AttrDefInfo.SetEntity(value);
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

	public override void OnCreate()
	{
		base.OnCreate();
		_viewController = PeViewStudio.CreateViewController(ViewControllerParam.DefaultCharacter);
		_viewController.SetLocalPos(PeViewStudio.s_ViewPos);
		_viewController.name = "ViewController_Player";
		_viewController.gameObject.SetActive(value: false);
		InitGrid();
		if (MainPlayerCmpt.gMainPlayer != null)
		{
			MainPlayerCmpt.gMainPlayer.onDurabilityDeficiency += DurabilityDeficiencyTip;
		}
	}

	private void Start()
	{
		if (mSuiteBtns == null || mSuiteBtns.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < mSuiteBtns.Count; i++)
		{
			UIEventListener uIEventListener = UIEventListener.Get(mSuiteBtns[i].gameObject);
			uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, (UIEventListener.BoolDelegate)delegate(GameObject go, bool hover)
			{
				if (hover)
				{
					UITooltip.ShowText(PELocalization.GetString(8000542) + (mSuiteBtns.FindIndex((BoxCollider a) => a.gameObject == go) + 1));
				}
				else
				{
					UITooltip.ShowText(string.Empty);
				}
			});
		}
	}

	private void DurabilityDeficiencyTip()
	{
		new PeTipMsg(PELocalization.GetString(8000851), PeTipMsg.EMsgLevel.Warning);
	}

	private void Update()
	{
		UpdatePalyerInfo();
		UpdateModel();
		UpdateArmor();
	}

	private void InitGrid()
	{
		mSpName = new string[10];
		mSpName[0] = "Icons_helmet";
		mSpName[1] = "Icons_coat";
		mSpName[2] = "Icons_gloves";
		mSpName[3] = "Icons_battery";
		mSpName[4] = "Icons_arms";
		mSpName[5] = "Icons_back";
		mSpName[6] = "Icons_pants";
		mSpName[7] = "Icons_shoes";
		mSpName[8] = "Icons_glass";
		mSpName[9] = "Icons_dun";
		for (int i = 0; i < 10; i++)
		{
			mEquipment.Add(UnityEngine.Object.Instantiate(mGridPrefab));
			mEquipment[i].gameObject.name = "HotKey" + i;
			mEquipment[i].transform.parent = GridContent.transform;
			mEquipment[i].transform.localPosition = new Vector3(i / 5 * 235, -i % 5 * 58, -2f);
			mEquipment[i].transform.localRotation = Quaternion.identity;
			mEquipment[i].transform.localScale = Vector3.one;
			mEquipment[i].SetItemPlace(ItemPlaceType.IPT_Equipment, i);
			mEquipment[i].SetGridMask((GridMask)(1 << i));
			mEquipment[i].mScriptIco.spriteName = mSpName[i];
			Grid_N grid_N = mEquipment[i];
			grid_N.onDropItem = (Grid_N.GridDelegate)Delegate.Combine(grid_N.onDropItem, new Grid_N.GridDelegate(OnDropItemToEquipment));
			Grid_N grid_N2 = mEquipment[i];
			grid_N2.onLeftMouseClicked = (Grid_N.GridDelegate)Delegate.Combine(grid_N2.onLeftMouseClicked, new Grid_N.GridDelegate(OnLeftMouseCliked));
			Grid_N grid_N3 = mEquipment[i];
			grid_N3.onRightMouseClicked = (Grid_N.GridDelegate)Delegate.Combine(grid_N3.onRightMouseClicked, new Grid_N.GridDelegate(OnRightMouseCliked));
		}
	}

	private void UpdatePalyerInfo()
	{
		if (null == entityInfoCmpt)
		{
			mLbName.text = "--";
			mLbName.MakePixelPerfect();
		}
		else
		{
			mLbName.text = entityInfoCmpt.characterName.ToString();
			mLbName.MakePixelPerfect();
		}
		if (null == commonCmpt)
		{
			mSprSexFemale.spriteName = "male_icon";
			mSprSexFemale.MakePixelPerfect();
		}
		else if (commonCmpt.sex == PeSex.Female)
		{
			mSprSexFemale.spriteName = "female_icon";
			mSprSexFemale.enabled = true;
			mSprSexMale.enabled = false;
		}
		else
		{
			mSprSexMale.spriteName = "male_icon";
			mSprSexFemale.enabled = false;
			mSprSexMale.enabled = true;
		}
		float value = AttrHpInfo.CurValue;
		float num = AttrHpInfo.MaxValue;
		mLbHealth.text = AttrHpInfo.GetCur_MaxStr();
		mSdHealth.sliderValue = ((!(num <= 0f)) ? (Convert.ToSingle(value) / num) : 0f);
		mLbHealthBuff.text = AttrHpInfo.GetBuffStr();
		value = AttrStaminaInfo.CurValue;
		num = AttrStaminaInfo.MaxValue;
		mLbStamina.text = AttrStaminaInfo.GetCur_MaxStr();
		mSdStamina.sliderValue = ((!(num <= 0f)) ? (Convert.ToSingle(value) / num) : 0f);
		mLbStaminaBuff.text = AttrStaminaInfo.GetBuffStr();
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
		value = AttrOxygenInfo.CurValue;
		num = AttrOxygenInfo.MaxValue;
		mLbOxygen.text = AttrOxygenInfo.GetCur_MaxStr();
		mSdOxygen.sliderValue = ((!(num <= 0f)) ? (Convert.ToSingle(value) / num) : 0f);
		mLbOxygenBuff.text = AttrOxygenInfo.GetBuffStr();
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

	private void GetEntityCmpt()
	{
		if (player != GameUI.Instance.mMainPlayer)
		{
			player = GameUI.Instance.mMainPlayer;
			viewCmpt = player.biologyViewCmpt;
			commonCmpt = player.commonCmpt;
			equipmentCmpt = player.equipmentCmpt;
			packageCmpt = player.GetCmpt<PlayerPackageCmpt>();
			entityInfoCmpt = player.enityInfoCmpt;
			playerArmorCmpt = player.GetCmpt<PlayerArmorCmpt>();
		}
	}

	private void EquipmentChange(object sender, EquipmentCmpt.EventArg arg)
	{
		if (mInit && isShow)
		{
			RefreshEquipmentList();
			RefreshEquipment();
		}
	}

	private void RefreshEquipmentList()
	{
		for (int i = 0; i < 10; i++)
		{
			mEquipment[i].SetItem(null);
		}
		foreach (ItemObject item in equipmentCmpt._ItemList)
		{
			Equip cmpt = item.GetCmpt<Equip>();
			for (int j = 0; j < 10; j++)
			{
				if (Convert.ToBoolean(cmpt.equipPos & (int)mEquipment[j].ItemMask))
				{
					mEquipment[j].SetItem(item);
				}
			}
		}
	}

	private void RefreshEquipment()
	{
		if (mInit && isShow && (bool)viewCmpt && viewCmpt != null)
		{
			StopRefreshEquipment();
			waitingToCloneModel = true;
			_meshControllers = viewCmpt.GetComponentsInChildren<CreationController>();
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
		_delay = 1;
		_meshControllers = null;
	}

	private void OnItemSelected(ItemObject item)
	{
		if (item != null && armorPage.activeInHierarchy)
		{
			_selectedArmorType = CreationHelper.GetArmorType(item.instanceId);
			bone2DObjects.SetActiveGroup(_selectedArmorType);
			if (_selectedArmorType != ArmorType.None)
			{
				_selectedArmorItem = item;
			}
			else
			{
				_selectedArmorItem = null;
			}
		}
	}

	private void UpdateArmor()
	{
		if (!Input.GetMouseButtonUp(0) || _selectedArmorItem == null || _selectedArmorType == ArmorType.None)
		{
			return;
		}
		if (bone2DObjects.GetHoverBone(out var boneGroup, out var boneIndex))
		{
			if (PeGameMgr.IsMulti)
			{
				if (!playerArmorCmpt.hasRequest)
				{
					playerArmorCmpt.C2S_EquipArmorPartFromPackage(_selectedArmorItem.instanceId, (int)_selectedArmorType, boneGroup, boneIndex, OnArmorPartEquiped);
				}
			}
			else
			{
				OnArmorPartEquiped(playerArmorCmpt.EquipArmorPartFromPackage(_selectedArmorItem, _selectedArmorType, boneGroup, boneIndex));
			}
		}
		_selectedArmorItem = null;
		_selectedArmorType = ArmorType.None;
		bone2DObjects.HideAll();
	}

	private void UpdateModel()
	{
		if (!waitingToCloneModel)
		{
			return;
		}
		if (!_newViewModel)
		{
			if (_meshControllers != null)
			{
				for (int i = 0; i < _meshControllers.Length; i++)
				{
					if (_meshControllers[i] != null && !_meshControllers[i].isBuildFinished)
					{
						return;
					}
				}
				_meshControllers = null;
			}
			if (_delay != 0)
			{
				_delay--;
				return;
			}
			_newViewModel = PeViewStudio.CloneCharacterViewModel(viewCmpt);
			if (null != _newViewModel)
			{
				_newViewModel.transform.position = new Vector3(0f, -1000f, 0f);
				SkinnedMeshRenderer component = _newViewModel.GetComponent<SkinnedMeshRenderer>();
				component.updateWhenOffscreen = true;
				_delay = 0;
			}
		}
		if (!_newViewModel)
		{
			return;
		}
		if (_delay == 0)
		{
			if (_viewModel != null)
			{
				UnityEngine.Object.Destroy(_viewModel);
			}
			_viewModel = _newViewModel;
			_viewController.SetTarget(_viewModel.transform);
			mEqTex.GetComponent<UIViewController>().Init(_viewController);
			bone2DObjects.Init(_viewController, _viewModel, playerArmorCmpt);
			mEqTex.mainTexture = _viewController.RenderTex;
			_newViewModel = null;
			StopRefreshEquipment();
		}
		else
		{
			_delay--;
		}
	}

	public override void Show()
	{
		if (!(null == GameUI.Instance.mMainPlayer))
		{
			base.Show();
			effect.Play();
			GetEntityCmpt();
			if ((bool)equipmentCmpt)
			{
				equipmentCmpt.changeEventor.Subscribe(EquipmentChange);
			}
			if ((bool)playerArmorCmpt)
			{
				playerArmorCmpt.onAddOrRemove += RefreshEquipment;
			}
			GameUI.Instance.mItemPackageCtrl.onItemSelected += OnItemSelected;
			RefreshEquipmentList();
			RefreshEquipment();
			_viewController.gameObject.SetActive(value: true);
		}
	}

	protected override void OnHide()
	{
		_viewController.gameObject.SetActive(value: false);
		if (_viewModel != null)
		{
			UnityEngine.Object.Destroy(_viewModel);
		}
		if ((bool)equipmentCmpt)
		{
			equipmentCmpt.changeEventor.Unsubscribe(EquipmentChange);
		}
		if ((bool)playerArmorCmpt)
		{
			playerArmorCmpt.onAddOrRemove -= RefreshEquipment;
		}
		GameUI.Instance.mItemPackageCtrl.onItemSelected -= OnItemSelected;
		StopRefreshEquipment();
		base.OnHide();
	}

	public bool RemoveEquipmentByIndex(int index)
	{
		if (mEquipment != null && index >= 0 && index < mEquipment.Count && mEquipment[index].ItemObj != null)
		{
			if (GameConfig.IsMultiMode)
			{
				if (equipmentCmpt.TryTakeOffEquipment(mEquipment[index].ItemObj))
				{
					return true;
				}
			}
			else if (equipmentCmpt.TakeOffEquipment(mEquipment[index].ItemObj, addToReceiver: false))
			{
				GameUI.Instance.PlayTakeOffEquipAudio();
				return true;
			}
		}
		PeTipMsg.Register(PELocalization.GetString(8000594), PeTipMsg.EMsgLevel.Error);
		return false;
	}

	public void OnLeftMouseCliked(Grid_N grid)
	{
		if (!(m_Player == null) && !(null == equipmentCmpt) && !(null == grid) && grid.ItemObj != null)
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

	public void OnRightMouseCliked(Grid_N grid)
	{
		if (null == player || GameUI.Instance.bMainPlayerIsDead)
		{
			return;
		}
		if (GameConfig.IsMultiMode)
		{
			if (grid.ItemObj != null && equipmentCmpt.TryTakeOffEquipment(grid.ItemObj))
			{
				PlayerNetwork.mainPlayer.RequestTakeOffEquipment(grid.ItemObj);
			}
		}
		else if (grid.ItemObj != null && equipmentCmpt.TakeOffEquipment(grid.ItemObj))
		{
			if (GameUI.Instance.mItemPackageCtrl != null)
			{
				GameUI.Instance.mItemPackageCtrl.ResetItem();
			}
			GameUI.Instance.PlayTakeOffEquipAudio();
		}
	}

	public void OnDropItemToEquipment(Grid_N grid)
	{
		if (SelectItem_N.Instance.Place != ItemPlaceType.IPT_Bag || !UIToolFuncs.CanEquip(SelectItem_N.Instance.ItemObj, commonCmpt.sex) || ((uint)grid.ItemMask & (uint)SelectItem_N.Instance.ItemObj.protoData.equipPos) == 0)
		{
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		if (GameConfig.IsMultiMode)
		{
			if (equipmentCmpt.NetTryPutOnEquipment(SelectItem_N.Instance.ItemObj))
			{
				PlayerNetwork.mainPlayer.RequestPutOnEquipment(SelectItem_N.Instance.ItemObj, SelectItem_N.Instance.Index);
				GameUI.Instance.PlayPutOnEquipAudio();
			}
			SelectItem_N.Instance.SetItem(null);
			return;
		}
		ItemPlaceType place = SelectItem_N.Instance.Place;
		if (place == ItemPlaceType.IPT_Bag)
		{
			if (equipmentCmpt.PutOnEquipment(SelectItem_N.Instance.ItemObj))
			{
				SelectItem_N.Instance.RemoveOriginItem();
				grid.SetItem(SelectItem_N.Instance.ItemObj);
				SelectItem_N.Instance.SetItem(null);
				GameUI.Instance.PlayPutOnEquipAudio();
			}
		}
		else
		{
			SelectItem_N.Instance.SetItem(null);
		}
	}

	private void OnEquipmentBtn(bool active)
	{
		if (active)
		{
			mEquipmentPage.SetActive(value: true);
		}
		else
		{
			mEquipmentPage.SetActive(value: false);
		}
	}

	private void OnArmorBtn(bool active)
	{
		armorPage.SetActive(active);
		if (active)
		{
			SetArmorSuit(playerArmorCmpt.currentSuitIndex);
		}
	}

	public void SetArmorSuit(int selectedIndex)
	{
		if (PeGameMgr.IsMulti)
		{
			if (playerArmorCmpt.hasRequest)
			{
				return;
			}
			playerArmorCmpt.C2S_SwitchArmorSuit(selectedIndex, OnArmorSuitChanged);
		}
		else
		{
			OnArmorSuitChanged(playerArmorCmpt.SwitchArmorSuit(selectedIndex));
		}
		bone2DObjects.HideAll();
	}

	public void OnArmorSuitChanged(bool success)
	{
		if (success)
		{
			for (int i = 0; i < armorSuitButtons.Length; i++)
			{
				armorSuitButtons[i].SetSelected(playerArmorCmpt.currentSuitIndex == i);
			}
		}
		else
		{
			MessageBox_N.ShowOkBox("/\\__/\\");
		}
	}

	public void OnArmorPartEquiped(bool success)
	{
		if (!success)
		{
			MessageBox_N.ShowOkBox("/\\__/\\");
		}
	}
}
