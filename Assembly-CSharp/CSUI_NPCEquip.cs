using System;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using Pathea.PeEntityExt;
using UnityEngine;
using WhiteCat;

public class CSUI_NPCEquip : MonoBehaviour
{
	public enum EEventType
	{
		CantWork,
		PutItemInto,
		TakeAwayItem,
		ResortItem,
		SplitItem,
		DeleteItem
	}

	public delegate void OpStatusDel(EEventType type, object obj1, object obj2);

	[SerializeField]
	private UIGrid m_EquipRoot;

	[SerializeField]
	private UITexture m_NpcTex;

	[SerializeField]
	private UILabel m_BuffStrsLb;

	[SerializeField]
	private CSUI_Grid m_GridPrefab;

	private PeEntity m_RefNpc;

	private EquipmentCmpt equipmentCmpt;

	private CommonCmpt m_NpcCommonInfo;

	private NpcCmpt m_NpcCmpt;

	private BiologyViewCmpt m_ViewCmpt;

	[SerializeField]
	private PeViewController _viewController;

	private GameObject _viewModel;

	private bool mInit;

	private List<CSUI_Grid> m_EquipGrids = new List<CSUI_Grid>();

	private string[] mSpName = new string[10];

	[SerializeField]
	private GameObject waitingImage;

	private CreationController[] _meshControllers;

	private GameObject _newViewModel;

	public EquipmentCmpt NpcEquipment
	{
		get
		{
			return equipmentCmpt;
		}
		set
		{
			if (equipmentCmpt != null)
			{
				EquipmentCmpt obj = equipmentCmpt;
				obj.onSuitSetChange = (Action<List<SuitSetData.MatchData>>)Delegate.Remove(obj.onSuitSetChange, new Action<List<SuitSetData.MatchData>>(UpdateSuitBuffTips));
			}
			equipmentCmpt = value;
			if (equipmentCmpt != null)
			{
				EquipmentCmpt obj2 = equipmentCmpt;
				obj2.onSuitSetChange = (Action<List<SuitSetData.MatchData>>)Delegate.Combine(obj2.onSuitSetChange, new Action<List<SuitSetData.MatchData>>(UpdateSuitBuffTips));
				UpdateSuitBuffTips(equipmentCmpt.matchDatas);
			}
			else
			{
				UpdateSuitBuffTips(null);
			}
		}
	}

	public PeEntity RefNpc
	{
		get
		{
			return m_RefNpc;
		}
		set
		{
			m_RefNpc = value;
			if (m_RefNpc != null)
			{
				NpcEquipment = m_RefNpc.equipmentCmpt;
				NpcEquipment.changeEventor.Unsubscribe(EquipmentChangeEvent);
				NpcEquipment.changeEventor.Subscribe(EquipmentChangeEvent);
				m_NpcCommonInfo = m_RefNpc.commonCmpt;
				m_NpcCmpt = m_RefNpc.NpcCmpt;
				m_ViewCmpt = m_RefNpc.biologyViewCmpt;
			}
			else
			{
				NpcEquipment = null;
				m_NpcCommonInfo = null;
				m_NpcCmpt = null;
				m_ViewCmpt = null;
				m_NpcTex.enabled = false;
			}
			UpdateEquipAndTex();
			UpdateMustNotTag();
		}
	}

	public bool IsShow => base.gameObject.activeInHierarchy;

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

	private void UpdateMustNotTag()
	{
		if (m_RefNpc == null || m_RefNpc.IsRandomNpc())
		{
			for (int i = 0; i < m_EquipGrids.Count; i++)
			{
				m_EquipGrids[i].m_Grid.MustNot = false;
			}
		}
		else
		{
			for (int j = 0; j < m_EquipGrids.Count; j++)
			{
				m_EquipGrids[j].m_Grid.MustNot = true;
			}
		}
	}

	private void InitBodyCamera()
	{
		_viewController = PeViewStudio.CreateViewController(ViewControllerParam.DefaultCharacter);
		_viewController.SetLocalPos(PeViewStudio.s_ViewPos);
		_viewController.name = "ViewController_NPCEquip";
		UpdateBodyCamera();
	}

	private void UpdateBodyCamera()
	{
		if (m_RefNpc == null || NpcEquipment == null || m_NpcCmpt == null)
		{
			m_NpcTex.enabled = false;
			return;
		}
		if (_viewModel != null)
		{
			UnityEngine.Object.Destroy(_viewModel);
		}
		_viewModel = PeViewStudio.CloneCharacterViewModel(m_ViewCmpt);
		if (_viewModel != null)
		{
			_viewController.SetTarget(_viewModel.transform);
		}
		m_NpcTex.mainTexture = _viewController.RenderTex;
		m_NpcTex.GetComponent<UIViewController>().Init(_viewController);
		m_NpcTex.enabled = true;
	}

	private void Start()
	{
		InitBodyCamera();
		InitEquipGrids();
		mInit = true;
	}

	private void OnEnable()
	{
		if (null != _viewController)
		{
			_viewController.gameObject.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
		if (null != _viewController)
		{
			_viewController.gameObject.SetActive(value: false);
		}
	}

	private void InitEquipGrids()
	{
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
			CSUI_Grid cSUI_Grid = UnityEngine.Object.Instantiate(m_GridPrefab);
			cSUI_Grid.transform.parent = m_EquipRoot.transform;
			if (i < 5)
			{
				cSUI_Grid.transform.localPosition = new Vector3(-112f, 176 - i % 5 * 58, -2f);
			}
			else
			{
				cSUI_Grid.transform.localPosition = new Vector3(112f, 176 - i % 5 * 58, -2f);
			}
			cSUI_Grid.transform.localRotation = Quaternion.identity;
			cSUI_Grid.transform.localScale = Vector3.one;
			cSUI_Grid.m_Grid.SetItemPlace(ItemPlaceType.IPT_ConolyServantEquPersonel, i);
			cSUI_Grid.m_Grid.SetGridMask((GridMask)(1 << i));
			cSUI_Grid.m_Grid.mScriptIco.spriteName = mSpName[i];
			cSUI_Grid.m_Grid.mScriptIco.MakePixelPerfect();
			cSUI_Grid.m_Active = false;
			m_EquipGrids.Add(cSUI_Grid);
			cSUI_Grid.m_Grid.onLeftMouseClicked = OnEquipLeftMouseClicked;
			cSUI_Grid.m_Grid.onDropItem = OnEquipDropItem;
			cSUI_Grid.m_Grid.onRightMouseClicked = OnEquipRightMouseClicked;
		}
		m_EquipRoot.repositionNow = true;
	}

	public bool EquipItem(ItemObject itemObj)
	{
		if (NpcEquipment != null)
		{
			EquipmentCmpt.Receiver receiver = ((!(PeSingleton<PeCreature>.Instance.mainPlayer == null)) ? PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PackageCmpt>() : null);
			if (NpcEquipment.PutOnEquipment(itemObj, addToReceiver: true, receiver))
			{
				GameUI.Instance.PlayPutOnEquipAudio();
				return true;
			}
		}
		return false;
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

	public void UpdateEquipAndTex()
	{
		if (mInit && IsShow)
		{
			UpdateEquipGrid();
			RefreshEquipment();
		}
	}

	private void EquipmentChangeEvent(object sender, EquipmentCmpt.EventArg arg)
	{
		UpdateEquipAndTex();
	}

	public void UpdateEquipGrid()
	{
		for (int i = 0; i < m_EquipGrids.Count; i++)
		{
			m_EquipGrids[i].m_Grid.SetItem(null);
		}
		if (m_RefNpc == null || !(NpcEquipment != null))
		{
			return;
		}
		foreach (ItemObject item in NpcEquipment._ItemList)
		{
			for (int j = 0; j < m_EquipGrids.Count; j++)
			{
				if (((uint)item.protoData.equipPos & (uint)m_EquipGrids[j].m_Grid.ItemMask) != 0)
				{
					m_EquipGrids[j].m_Grid.SetItem(item);
				}
			}
		}
	}

	private void RefreshEquipment()
	{
		if (mInit && IsShow && null != m_ViewCmpt)
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
		if (null != m_ViewCmpt)
		{
			_meshControllers = m_ViewCmpt.GetComponentsInChildren<CreationController>();
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
			_newViewModel = PeViewStudio.CloneCharacterViewModel(m_ViewCmpt);
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
			m_NpcTex.GetComponent<UIViewController>().Init(_viewController);
			m_NpcTex.mainTexture = _viewController.RenderTex;
			m_NpcTex.enabled = true;
			_newViewModel = null;
		}
		waitingToCloneModel = false;
	}

	private void OnEquipDropItem(Grid_N grid)
	{
		if (m_RefNpc == null || NpcEquipment == null || SelectItem_N.Instance.Place == ItemPlaceType.IPT_HotKeyBar || !UIToolFuncs.CanEquip(SelectItem_N.Instance.ItemObj, m_NpcCommonInfo.sex) || ((uint)grid.ItemMask & (uint)SelectItem_N.Instance.ItemObj.protoData.equipPos) == 0)
		{
			return;
		}
		if (GameConfig.IsMultiMode)
		{
			if (NpcEquipment.NetTryPutOnEquipment(SelectItem_N.Instance.ItemObj))
			{
				PlayerNetwork.mainPlayer.RequestNpcPutOnEquip(m_RefNpc.Id, SelectItem_N.Instance.ItemObj.instanceId, SelectItem_N.Instance.Place);
				SelectItem_N.Instance.SetItem(null);
				GameUI.Instance.PlayPutOnEquipAudio();
			}
		}
		else if (NpcEquipment.PutOnEquipment(SelectItem_N.Instance.ItemObj))
		{
			SelectItem_N.Instance.RemoveOriginItem();
			SelectItem_N.Instance.SetItem(null);
			UpdateEquipAndTex();
			GameUI.Instance.PlayPutOnEquipAudio();
		}
	}

	public bool EquipRemoveOriginItem(int index)
	{
		if (m_EquipGrids != null && index >= 0 && index < m_EquipGrids.Count && m_EquipGrids[index].m_Grid.ItemObj != null)
		{
			if (GameConfig.IsMultiMode)
			{
				if (equipmentCmpt.TryTakeOffEquipment(m_EquipGrids[index].m_Grid.ItemObj))
				{
					return true;
				}
			}
			else if (equipmentCmpt.TakeOffEquipment(m_EquipGrids[index].m_Grid.ItemObj, addToReceiver: false))
			{
				GameUI.Instance.PlayTakeOffEquipAudio();
				return true;
			}
		}
		PeTipMsg.Register(PELocalization.GetString(8000594), PeTipMsg.EMsgLevel.Error);
		return false;
	}

	private void OnEquipLeftMouseClicked(Grid_N grid)
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

	private void OnEquipRightMouseClicked(Grid_N grid)
	{
		if (m_RefNpc == null || grid.ItemObj == null)
		{
			return;
		}
		if (GameConfig.IsMultiMode)
		{
			if (NpcEquipment.TryTakeOffEquipment(grid.ItemObj))
			{
				PlayerNetwork.mainPlayer.RequestNpcTakeOffEquip(m_RefNpc.Id, grid.ItemObj.instanceId, -1);
				GameUI.Instance.PlayTakeOffEquipAudio();
			}
			return;
		}
		PlayerPackageCmpt playerPackageCmpt = ((!(PeSingleton<PeCreature>.Instance.mainPlayer == null)) ? PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>() : null);
		if (NpcEquipment.TakeOffEquipment(grid.ItemObj, addToReceiver: true, playerPackageCmpt))
		{
			GameUI.Instance.mItemPackageCtrl.Show();
			if (GameUI.Instance.mItemPackageCtrl != null)
			{
				GameUI.Instance.mItemPackageCtrl.ResetItem();
			}
			GameUI.Instance.PlayTakeOffEquipAudio();
		}
		else if (null == playerPackageCmpt || playerPackageCmpt.package.CanAdd(grid.ItemObj))
		{
			CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(8000594));
		}
		else
		{
			CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(8000050).Replace("\\n", " "));
		}
	}
}
