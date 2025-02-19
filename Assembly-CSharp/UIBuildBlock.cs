using System;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;
using UnityEngine;
using WhiteCat;

public class UIBuildBlock : MonoBehaviour
{
	public enum BrushType
	{
		bt_point,
		bt_box,
		bt_inclined,
		bt_selectBlock,
		bt_selectVoxel,
		bt_selectInclined,
		bt_iso,
		bt_selectAll,
		bt_null
	}

	public delegate void DTweenFinished(bool forward);

	private const int NumPerPage = 22;

	private const string SAVESHOWTIPKEY = "UIBuildTips";

	public Action onSaveIsoClick;

	public Action<BSIsoHeadData> onIsoExport;

	private static UIBuildBlock mInstance;

	public UIBuildMenuControl mMenuCtrl;

	public UIBuildWndContol mWndCtrl;

	public UIBuildSaveWndCtrl mSaveWnd;

	public UIBuildTutorialCtrl mBuildTutorialCtrl;

	public GameObject mTipsRoot;

	public UISprite mTipsNormSprite;

	public UISprite mTipsShowSprite;

	public Color menuItemAddColor = Color.green;

	public Color menuItemRemoveColor = Color.red;

	private EBSBrushMode pointMode;

	private EBSBrushMode boxMode;

	private int diagonalRot;

	public bool playTween;

	private bool forward = true;

	private BrushType m_CurBrushType;

	private BSBrush m_CurBrush;

	private bool _initVoxelPage;

	private List<int> m_VoxelProtoItems;

	private List<int> m_VoxelTypeList;

	private List<int> m_BlockProtoItems;

	private bool _initBlockPage;

	private bool _initIosBlockPage;

	private List<BSIsoHeadData> m_IsoHeaders;

	public TweenInterpolator tipTweener;

	public TweenInterpolator menuTweener;

	private int _deleteIndex = -1;

	private bool IsoRePos;

	private bool _showTips = true;

	public static UIBuildBlock Instance => mInstance;

	public event DTweenFinished onTweenFinished;

	private void Awake()
	{
		mInstance = this;
		mMenuCtrl.BtnB += BtnBOnClick;
		mMenuCtrl.BtnDle += DeleteOnClick;
		mMenuCtrl.BtnSelectShape += SelectShapeOnClick;
		mMenuCtrl.BtnSelectType += SelectTypeOnClick;
		mMenuCtrl.onCanClickSaveBtn += OnCanClickSaveBtn;
		mMenuCtrl.onQuickBarFunc += OnMenuQuickBarClick;
		mMenuCtrl.onBrushItemClick += OnBrushMenuItemClick;
		mMenuCtrl.MenuItemClick += OnMenuItemClick;
		mMenuCtrl.BtnBlockSelect += OnMenuBlockSelectClick;
		mMenuCtrl.BtnVoxelSelect += OnMenuVoxelSelectClick;
		mMenuCtrl.ToolTip += OnItemToolTip;
		mMenuCtrl.onIntiMenuList += OnInitMenuList;
		mMenuCtrl.onDropItem += OnMenuDropItem;
		mWndCtrl.onPageClick += OnPageClick;
		mWndCtrl.BtnDelete += OnIsoDeleteClick;
		mWndCtrl.BtnExport += OnIsoExportClick;
		mSaveWnd.btnSave += OnSaveIsoClick;
		mSaveWnd.OnWndClosed += OnSaveIsoClose;
		InitVoxelPage();
		InitBlockPage();
		InitBuildTips();
		if (GameUI.Instance.mSkillWndCtrl != null)
		{
			GameUI.Instance.mSkillWndCtrl.onRefreshTypeData += OnRefreshSkill;
			OnRefreshSkill(GameUI.Instance.mSkillWndCtrl);
		}
	}

	private void OnDestroy()
	{
		if (GameUI.Instance.mSkillWndCtrl != null)
		{
			GameUI.Instance.mSkillWndCtrl.onRefreshTypeData -= OnRefreshSkill;
		}
	}

	private void Update()
	{
		InitVoxelPage();
		InitBlockPage();
		if (playTween)
		{
			PlayTween(forward);
			forward = !forward;
			playTween = false;
		}
		if (mWndCtrl.BlockMatSelectIndex != -1)
		{
			mMenuCtrl.mTypeSelectedItem.SetIcon(mWndCtrl.mBlockMatList[mWndCtrl.BlockMatSelectIndex].mContentSprite.spriteName);
			if (mWndCtrl.BlockPatternSelectIndex != -1)
			{
				mMenuCtrl.mShapeSelectedItem.SetIcon(mWndCtrl.mBlockPatternList[mWndCtrl.BlockPatternSelectIndex].mContentSprite.spriteName);
			}
			mMenuCtrl.mSelectBtn.boxCollier.enabled = true;
			mMenuCtrl.mSelectBlockBtn.boxCollier.enabled = true;
			mMenuCtrl.mDiagonalBtn.boxCollier.enabled = true;
			mMenuCtrl.mSaveBtn.boxCollier.enabled = true;
			mMenuCtrl.mDeleteBtn.boxCollier.enabled = true;
			mMenuCtrl.mSelectBtn.bgSprite.color = Color.white;
			mMenuCtrl.mSelectBlockBtn.bgSprite.color = Color.white;
			mMenuCtrl.mDiagonalBtn.bgSprite.color = Color.white;
			mMenuCtrl.mSaveBtn.bgSprite.color = Color.white;
			mMenuCtrl.mDeleteBtn.bgSprite.color = Color.white;
		}
		else if (mWndCtrl.TextureListSelectIndex != -1)
		{
			mMenuCtrl.mTypeSelectedItem.SetIcon(mWndCtrl.mTypeList[mWndCtrl.TypeSelectIndex].mContentSprite.spriteName);
			mMenuCtrl.mShapeSelectedItem.SetIcon("voxel");
			mMenuCtrl.mSelectBtn.boxCollier.enabled = false;
			mMenuCtrl.mSelectBlockBtn.boxCollier.enabled = false;
			mMenuCtrl.mDiagonalBtn.boxCollier.enabled = false;
			mMenuCtrl.mSaveBtn.boxCollier.enabled = false;
			mMenuCtrl.mDeleteBtn.boxCollier.enabled = false;
			mMenuCtrl.mSelectBtn.bgSprite.color = Color.white * 0.6f;
			mMenuCtrl.mSelectBlockBtn.bgSprite.color = Color.white * 0.6f;
			mMenuCtrl.mDiagonalBtn.bgSprite.color = Color.white * 0.6f;
			mMenuCtrl.mSaveBtn.bgSprite.color = Color.white * 0.6f;
			mMenuCtrl.mDeleteBtn.bgSprite.color = Color.white * 0.6f;
		}
		else
		{
			mMenuCtrl.mShapeSelectedItem.SetIcon("Null");
			mMenuCtrl.mTypeSelectedItem.SetIcon("Null");
		}
		if (mMenuCtrl.mPointBtn.checkBox.isChecked)
		{
			if (m_CurBrush is BSPointBrush)
			{
				if (m_CurBrush.mode == EBSBrushMode.Add)
				{
					mMenuCtrl.SetBrushItemSprite(UIBrushMenuItem.BrushType.pointAdd, menuItemAddColor);
				}
				else if (m_CurBrush.mode == EBSBrushMode.Subtract)
				{
					mMenuCtrl.SetBrushItemSprite(UIBrushMenuItem.BrushType.pointRemove, menuItemRemoveColor);
				}
			}
		}
		else if (mMenuCtrl.mBoxBtn.checkBox.isChecked && m_CurBrush is BSBoxBrush)
		{
			if (m_CurBrush.mode == EBSBrushMode.Add)
			{
				mMenuCtrl.SetBrushItemSprite(UIBrushMenuItem.BrushType.boxAdd, menuItemAddColor);
			}
			else if (m_CurBrush.mode == EBSBrushMode.Subtract)
			{
				mMenuCtrl.SetBrushItemSprite(UIBrushMenuItem.BrushType.boxRemove, menuItemRemoveColor);
			}
		}
		if (mWndCtrl.BlockMatSelectIndex != -1)
		{
			if (!mMenuCtrl.mBlockSelectCB.isChecked)
			{
				mMenuCtrl.mBlockSelectCB.isChecked = true;
			}
		}
		else if (!mMenuCtrl.mVoxelSelectCB.isChecked)
		{
			mMenuCtrl.mVoxelSelectCB.isChecked = true;
		}
	}

	private void LateUpdate()
	{
		if (IsoRePos)
		{
			IsoRePos = false;
			mWndCtrl.ResetCostPostion();
		}
	}

	public void EnterBuildMode()
	{
		if (PeSingleton<PeCreature>.Instance != null && null != PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			MotionMgrCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<MotionMgrCmpt>();
			if (null != cmpt)
			{
				cmpt.DoAction(PEActionType.Build);
			}
		}
	}

	public void QuitBuildMode()
	{
		if (PeSingleton<PeCreature>.Instance != null && null != PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			MotionMgrCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<MotionMgrCmpt>();
			if (null != cmpt)
			{
				cmpt.EndAction(PEActionType.Build);
			}
		}
		else
		{
			QuitBlock();
		}
	}

	public void EnterBlock()
	{
		base.gameObject.SetActive(value: true);
		PeInput.ArrowAxisEnable = false;
		if (Key_wordMgr.Self != null)
		{
			Key_wordMgr.Self.enableQuickKey = false;
		}
	}

	public void QuitBlock()
	{
		PeInput.ArrowAxisEnable = true;
		base.gameObject.SetActive(value: false);
		CreateBrush(BrushType.bt_null);
		if (Key_wordMgr.Self != null)
		{
			Key_wordMgr.Self.enableQuickKey = true;
		}
	}

	public void CreateBrush(BrushType type)
	{
		if (PEBuildingMan.Self == null)
		{
			return;
		}
		m_CurBrushType = type;
		switch (type)
		{
		case BrushType.bt_null:
			m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.None);
			break;
		case BrushType.bt_box:
			m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.Box);
			m_CurBrush.mode = boxMode;
			break;
		case BrushType.bt_inclined:
		{
			m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.B45Diagonal);
			BSB45DiagonalBrush bSB45DiagonalBrush = m_CurBrush as BSB45DiagonalBrush;
			if (bSB45DiagonalBrush != null)
			{
				bSB45DiagonalBrush.m_Rot = diagonalRot;
			}
			break;
		}
		case BrushType.bt_point:
			m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.Point);
			m_CurBrush.mode = pointMode;
			break;
		case BrushType.bt_selectBlock:
			PEBuildingMan.Self.selectVoxel = false;
			m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.Select);
			break;
		case BrushType.bt_selectVoxel:
			PEBuildingMan.Self.selectVoxel = true;
			m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.Select);
			break;
		case BrushType.bt_selectInclined:
			m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.None);
			break;
		case BrushType.bt_selectAll:
			PEBuildingMan.Self.selectVoxel = false;
			m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.IsoSelectBrush);
			break;
		case BrushType.bt_iso:
			m_CurBrush = BuildingMan.CreateBrush(BuildingMan.EBrushType.Iso);
			break;
		}
	}

	private void InitVoxelPage()
	{
		if (_initVoxelPage)
		{
			return;
		}
		UISkillWndCtrl mSkillWndCtrl = GameUI.Instance.mSkillWndCtrl;
		if (mSkillWndCtrl._SkillMgr != null && !mSkillWndCtrl._SkillMgr.CheckUnlockBuildBlockVoxel())
		{
			return;
		}
		m_VoxelProtoItems = BSVoxelMatMap.GetAllProtoItems();
		foreach (int voxelProtoItem in m_VoxelProtoItems)
		{
			ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(voxelProtoItem);
			int index = mWndCtrl.AddVoxelMatListItem(itemProto.icon[0], "Icon");
			mWndCtrl.mVoxelMatList[index].SetItemID(itemProto.id);
		}
		mWndCtrl.ToolTip += OnItemToolTip;
		mWndCtrl.TextureItemOnClick += OnVoxelMatItemClick;
		mWndCtrl.VoxelTypeOnClick += OnVoxelTypeClick;
		mWndCtrl.ResetVoxelPostion();
		_initVoxelPage = true;
	}

	private void InitBlockPage()
	{
		if (_initBlockPage)
		{
			return;
		}
		m_BlockProtoItems = BSBlockMatMap.GetAllProtoItems();
		foreach (int blockProtoItem in m_BlockProtoItems)
		{
			ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(blockProtoItem);
			int index = mWndCtrl.AddBlockListItem(itemProto.icon[0], "Icon");
			mWndCtrl.mBlockMatList[index].SetItemID(blockProtoItem);
		}
		foreach (KeyValuePair<int, BSPattern> s_tblPattern in BSPattern.s_tblPatterns)
		{
			int num = mWndCtrl.AddBlockPatternItem(s_tblPattern.Value.IconName, "Icon");
			mWndCtrl.mBlockPatternList[num].SetNumber(s_tblPattern.Value.size.ToString());
			mWndCtrl.mBlockPatternList[num].SetItemID(s_tblPattern.Key);
			if (s_tblPattern.Value == PEBuildingMan.Self.Pattern)
			{
				mWndCtrl.BlockPatternSelectIndex = num;
			}
		}
		mWndCtrl.ResetBlockPostion();
		mWndCtrl.ToolTip += OnItemToolTip;
		mWndCtrl.BlockItemOnClick += OnBlockMatClick;
		mWndCtrl.BlockPatternOnClick += OnBlockPatternClick;
		_initBlockPage = true;
	}

	private void InitIsoBlockPage()
	{
		UISkillWndCtrl mSkillWndCtrl = GameUI.Instance.mSkillWndCtrl;
		if ((mSkillWndCtrl._SkillMgr != null && !mSkillWndCtrl._SkillMgr.CheckUnlockBuildBlockIso()) || _initIosBlockPage)
		{
			return;
		}
		mWndCtrl.ClearIsoList();
		mWndCtrl.ClearCostList();
		m_IsoHeaders = new List<BSIsoHeadData>(PEBuildingMan.Self.ExtractTheHeaders());
		foreach (BSIsoHeadData isoHeader in m_IsoHeaders)
		{
			Texture2D texture2D = new Texture2D(32, 32, TextureFormat.ARGB32, mipmap: false);
			texture2D.LoadImage(isoHeader.IconTex);
			mWndCtrl.AddIsoListItem(isoHeader.Name, texture2D);
		}
		mWndCtrl.IsoItemOnClick += OnIsoClick;
		mWndCtrl.ResetIsoPostion();
		_initIosBlockPage = true;
	}

	public void PlayTween(bool forward)
	{
		if (forward)
		{
			if (tipTweener.speed < 0f)
			{
				tipTweener.ReverseSpeed();
			}
			if (menuTweener.speed < 0f)
			{
				menuTweener.ReverseSpeed();
			}
		}
		else
		{
			if (tipTweener.speed > 0f)
			{
				tipTweener.ReverseSpeed();
			}
			if (menuTweener.speed > 0f)
			{
				menuTweener.ReverseSpeed();
			}
		}
		tipTweener.isPlaying = true;
		menuTweener.isPlaying = true;
	}

	public void OnTweenFinish(bool forward)
	{
		if (this.onTweenFinished != null)
		{
			this.onTweenFinished(forward: true);
		}
	}

	private void BtnBOnClick()
	{
		QuitBuildMode();
	}

	private void DeleteOnClick()
	{
		BSMiscBrush bSMiscBrush = m_CurBrush as BSMiscBrush;
		if (bSMiscBrush != null)
		{
			if (!bSMiscBrush.IsEmpty())
			{
				bSMiscBrush.DeleteVoxel();
			}
			return;
		}
		BSIsoSelectBrush bSIsoSelectBrush = m_CurBrush as BSIsoSelectBrush;
		if (bSIsoSelectBrush != null)
		{
			bSIsoSelectBrush.DeleteVoxels();
		}
	}

	private void SelectTypeOnClick()
	{
		if (mWndCtrl.BlockMatSelectIndex != -1)
		{
			mWndCtrl.Show();
			mWndCtrl.ChangeCkMenu(1);
		}
		else if (mWndCtrl.TypeSelectIndex != -1)
		{
			mWndCtrl.Show();
			mWndCtrl.ChangeCkMenu(0);
		}
	}

	private void SelectShapeOnClick()
	{
		if (mWndCtrl.BlockMatSelectIndex != -1 && mWndCtrl.BlockPatternSelectIndex != -1)
		{
			mWndCtrl.Show();
			mWndCtrl.ChangeCkMenu(1);
		}
		else if (mWndCtrl.TypeSelectIndex != -1)
		{
			mWndCtrl.Show();
			mWndCtrl.ChangeCkMenu(0);
		}
	}

	private bool OnCanClickSaveBtn()
	{
		if (PEBuildingMan.Self == null)
		{
			return true;
		}
		if (!PEBuildingMan.Self.selectVoxel)
		{
			BSSelectBrush bSSelectBrush = m_CurBrush as BSSelectBrush;
			if (bSSelectBrush != null && !bSSelectBrush.IsEmpty())
			{
				PEBuildingMan.Self.IsoCaputure.Computer.ClearDataDS();
				foreach (KeyValuePair<IntVector3, byte> selection in bSSelectBrush.Selections)
				{
					BSVoxel bSVoxel = BuildingMan.Blocks.Read(selection.Key.x, selection.Key.y, selection.Key.z);
					PEBuildingMan.Self.IsoCaputure.Computer.AlterBlockInBuild(selection.Key.x, selection.Key.y, selection.Key.z, bSVoxel.ToBlock());
				}
				PEBuildingMan.Self.IsoCaputure.Computer.RebuildMesh();
				PEBuildingMan.Self.IsoCaputure.EnableCapture();
				mSaveWnd.SetIsoItemContent(PEBuildingMan.Self.IsoCaputure.photoRT);
				bSSelectBrush.canDo = false;
				return true;
			}
			BSIsoSelectBrush bSIsoSelectBrush = m_CurBrush as BSIsoSelectBrush;
			if (bSIsoSelectBrush != null)
			{
				List<IntVector3> selectionPos = bSIsoSelectBrush.GetSelectionPos();
				if (selectionPos.Count != 0)
				{
					PEBuildingMan.Self.IsoCaputure.Computer.ClearDataDS();
					for (int i = 0; i < selectionPos.Count; i++)
					{
						BSVoxel bSVoxel2 = BuildingMan.Blocks.Read(selectionPos[i].x, selectionPos[i].y, selectionPos[i].z);
						PEBuildingMan.Self.IsoCaputure.Computer.AlterBlockInBuild(selectionPos[i].x, selectionPos[i].y, selectionPos[i].z, bSVoxel2.ToBlock());
					}
					PEBuildingMan.Self.IsoCaputure.Computer.RebuildMesh();
					PEBuildingMan.Self.IsoCaputure.EnableCapture();
					mSaveWnd.SetIsoItemContent(PEBuildingMan.Self.IsoCaputure.photoRT);
					return true;
				}
			}
		}
		return false;
	}

	private void OnRefreshSkill(UISkillWndCtrl uiSkill)
	{
		if (!RandomMapConfig.useSkillTree)
		{
			return;
		}
		foreach (UIBuildWndItem mBlockMat in mWndCtrl.mBlockMatList)
		{
			ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(mBlockMat.ItemId);
			if (uiSkill._SkillMgr != null)
			{
				if (!uiSkill._SkillMgr.CheckBuildBlockLevel(itemProto.level))
				{
					mBlockMat.IsActive = false;
				}
				else
				{
					mBlockMat.IsActive = true;
				}
			}
		}
		foreach (UIBuildWndItem mBlockPattern in mWndCtrl.mBlockPatternList)
		{
			int mIndex = mBlockPattern.mIndex;
			if (uiSkill._SkillMgr != null)
			{
				if (!uiSkill._SkillMgr.CheckBuildShape(mIndex))
				{
					mBlockPattern.IsActive = false;
				}
				else
				{
					mBlockPattern.IsActive = true;
				}
			}
		}
		foreach (UIBuildWndItem mMenu in mMenuCtrl.mMenuList)
		{
			if (mMenu.mTargetItemType == UIBuildWndItem.ItemType.mBlockMat)
			{
				ItemProto itemData = ItemProto.GetItemData(mMenu.ItemId);
				if (uiSkill._SkillMgr != null)
				{
					if (!uiSkill._SkillMgr.CheckBuildBlockLevel(itemData.level))
					{
						mMenu.IsActive = false;
					}
					else
					{
						mMenu.IsActive = true;
					}
				}
			}
			else
			{
				if (mMenu.mTargetItemType != UIBuildWndItem.ItemType.mBlockPattern)
				{
					continue;
				}
				int mTargetIndex = mMenu.mTargetIndex;
				if (uiSkill._SkillMgr != null)
				{
					if (!uiSkill._SkillMgr.CheckBuildShape(mTargetIndex))
					{
						mMenu.IsActive = false;
					}
					else
					{
						mMenu.IsActive = true;
					}
				}
			}
		}
	}

	private void OnInitMenuList()
	{
		UISkillWndCtrl mSkillWndCtrl = GameUI.Instance.mSkillWndCtrl;
		if (mSkillWndCtrl == null)
		{
			return;
		}
		foreach (UIBuildWndItem mMenu in mMenuCtrl.mMenuList)
		{
			if (mMenu.mTargetItemType == UIBuildWndItem.ItemType.mBlockMat)
			{
				ItemProto itemData = ItemProto.GetItemData(mMenu.ItemId);
				if (mSkillWndCtrl._SkillMgr != null)
				{
					if (!mSkillWndCtrl._SkillMgr.CheckBuildBlockLevel(itemData.level))
					{
						mMenu.IsActive = false;
					}
					else
					{
						mMenu.IsActive = true;
					}
				}
			}
			else
			{
				if (mMenu.mTargetItemType != UIBuildWndItem.ItemType.mBlockPattern)
				{
					continue;
				}
				int mTargetIndex = mMenu.mTargetIndex;
				if (mSkillWndCtrl._SkillMgr != null)
				{
					if (!mSkillWndCtrl._SkillMgr.CheckBuildShape(mTargetIndex))
					{
						mMenu.IsActive = false;
					}
					else
					{
						mMenu.IsActive = true;
					}
				}
			}
		}
	}

	private void OnMenuDropItem(UIBuildWndItem item)
	{
		UISkillWndCtrl mSkillWndCtrl = GameUI.Instance.mSkillWndCtrl;
		if (mSkillWndCtrl == null)
		{
			return;
		}
		if (item.mTargetItemType == UIBuildWndItem.ItemType.mBlockMat)
		{
			ItemProto itemData = ItemProto.GetItemData(item.ItemId);
			if (!mSkillWndCtrl._SkillMgr.CheckBuildBlockLevel(itemData.level))
			{
				item.IsActive = false;
			}
			else
			{
				item.IsActive = true;
			}
		}
		else
		{
			if (item.mTargetItemType != UIBuildWndItem.ItemType.mBlockPattern)
			{
				return;
			}
			int mTargetIndex = item.mTargetIndex;
			if (mSkillWndCtrl._SkillMgr != null)
			{
				if (!mSkillWndCtrl._SkillMgr.CheckBuildShape(mTargetIndex))
				{
					item.IsActive = false;
				}
				else
				{
					item.IsActive = true;
				}
			}
		}
	}

	private void OnMenuQuickBarClick(UIBuildWndItem item)
	{
		if (item.mTargetItemType != 0 && item.mTargetItemType != UIBuildWndItem.ItemType.mMenu && !item.IsActive)
		{
			new PeTipMsg(PELocalization.GetString(8000854), PeTipMsg.EMsgLevel.Warning);
			return;
		}
		switch (item.mTargetItemType)
		{
		case UIBuildWndItem.ItemType.mVoxelType:
		{
			if (!(PEBuildingMan.Self != null) || !GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckUnlockBuildBlockVoxel())
			{
				break;
			}
			PEBuildingMan.Self.Manipulator.MaterialType = (byte)item.ItemId;
			PEBuildingMan.Self.Pattern = BSPattern.DefaultV1;
			mWndCtrl.DisselectBlock();
			int itemID = BSVoxelMatMap.GetItemID(item.ItemId);
			for (int i = 0; i < mWndCtrl.mVoxelMatList.Count; i++)
			{
				if (mWndCtrl.mVoxelMatList[i].ItemId == itemID)
				{
					mWndCtrl.TextureListSelectIndex = i;
					OnVoxelMatItemClick(i);
					mWndCtrl.TypeSelectIndex = item.mTargetIndex;
					break;
				}
			}
			ChangeBrushToBox();
			break;
		}
		case UIBuildWndItem.ItemType.mBlockMat:
		{
			if (!(PEBuildingMan.Self != null))
			{
				break;
			}
			PEBuildingMan.Self.Manipulator.MaterialType = (byte)PEBuildingMan.GetBlockMaterialType(item.ItemId);
			if (PEBuildingMan.Self.Pattern == null || PEBuildingMan.Self.Pattern.type == EBSVoxelType.Voxel)
			{
				if (mWndCtrl.BlockPatternSelectIndex == -1)
				{
					PEBuildingMan.Self.Pattern = BSPattern.DefaultB1;
				}
				else if (mWndCtrl.mBlockPatternList.Count > mWndCtrl.BlockPatternSelectIndex && mWndCtrl.TextureListSelectIndex > -1)
				{
					PEBuildingMan.Self.Pattern = BSPattern.s_tblPatterns[mWndCtrl.mBlockPatternList[mWndCtrl.BlockPatternSelectIndex].ItemId];
				}
			}
			mWndCtrl.DisselectVoxel();
			for (int j = 0; j < mWndCtrl.mBlockMatList.Count; j++)
			{
				if (mWndCtrl.mBlockMatList[j].ItemId == item.ItemId)
				{
					mWndCtrl.BlockMatSelectIndex = j;
					OnBlockMatClick(j);
					break;
				}
			}
			ChangeBrushToBox();
			break;
		}
		case UIBuildWndItem.ItemType.mBlockPattern:
		{
			if (!(PEBuildingMan.Self != null))
			{
				break;
			}
			if (PEBuildingMan.Self.Pattern == null)
			{
				OnBlockMatClick(0);
			}
			else if (PEBuildingMan.Self.Pattern.type == EBSVoxelType.Voxel && mWndCtrl.mVoxelMatList.Count > mWndCtrl.TextureListSelectIndex && mWndCtrl.TextureListSelectIndex > -1)
			{
				PEBuildingMan.Self.Manipulator.MaterialType = (byte)PEBuildingMan.GetBlockMaterialType(mWndCtrl.mVoxelMatList[mWndCtrl.TextureListSelectIndex].ItemId);
			}
			mWndCtrl.DisselectVoxel();
			int num = 0;
			foreach (KeyValuePair<int, BSPattern> s_tblPattern in BSPattern.s_tblPatterns)
			{
				if (num == item.mTargetIndex)
				{
					if (mWndCtrl.TextureListSelectIndex != -1)
					{
						int itemId = mWndCtrl.mVoxelMatList[mWndCtrl.TextureListSelectIndex].ItemId;
						PEBuildingMan.Self.Manipulator.MaterialType = (byte)PEBuildingMan.GetBlockMaterialType(itemId);
						int num2 = mWndCtrl.mBlockMatList.FindIndex((UIBuildWndItem item0) => item0.ItemId == itemId);
						if (num2 != -1)
						{
							mWndCtrl.BlockMatSelectIndex = num2;
						}
						else
						{
							mWndCtrl.BlockMatSelectIndex = 0;
						}
					}
					else if (mWndCtrl.BlockMatSelectIndex == -1)
					{
						PEBuildingMan.Self.Manipulator.MaterialType = (byte)PEBuildingMan.GetBlockMaterialType(mWndCtrl.mBlockMatList[0].ItemId);
						mWndCtrl.BlockMatSelectIndex = 0;
					}
					PEBuildingMan.Self.Pattern = s_tblPattern.Value;
					break;
				}
				num++;
			}
			if (item.mTargetIndex != -1)
			{
				mWndCtrl.BlockPatternSelectIndex = item.mTargetIndex;
			}
			ChangeBrushToBox();
			break;
		}
		case UIBuildWndItem.ItemType.mVoxelMat:
			break;
		}
	}

	private bool OnSaveIsoClick(string iso_name)
	{
		BSMiscBrush bSMiscBrush = m_CurBrush as BSMiscBrush;
		BSIsoSelectBrush bSIsoSelectBrush = m_CurBrush as BSIsoSelectBrush;
		if (bSMiscBrush != null || bSIsoSelectBrush != null)
		{
			if (iso_name != string.Empty)
			{
				int width = PEBuildingMan.Self.IsoCaputure.photoRT.width;
				int height = PEBuildingMan.Self.IsoCaputure.photoRT.height;
				Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, mipmap: false);
				RenderTexture.active = PEBuildingMan.Self.IsoCaputure.photoRT;
				texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
				texture2D.Apply();
				RenderTexture.active = null;
				BSIsoData outData = null;
				if (bSMiscBrush != null && bSMiscBrush.SaveToIso(iso_name, texture2D.EncodeToPNG(), out outData))
				{
					_initIosBlockPage = false;
					InitIsoBlockPage();
					if (onSaveIsoClick != null)
					{
						onSaveIsoClick();
					}
					return true;
				}
				if (bSIsoSelectBrush != null && bSIsoSelectBrush.SaveToIso(iso_name, texture2D.EncodeToPNG(), out outData))
				{
					_initIosBlockPage = false;
					InitIsoBlockPage();
					if (onSaveIsoClick != null)
					{
						onSaveIsoClick();
					}
					return true;
				}
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000494));
			}
			else
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000494));
			}
		}
		return false;
	}

	private void OnSaveIsoClose()
	{
		PEBuildingMan.Self.IsoCaputure.DisableCapture();
		BSSelectBrush bSSelectBrush = m_CurBrush as BSSelectBrush;
		if (bSSelectBrush != null)
		{
			bSSelectBrush.canDo = true;
		}
	}

	private void OnPageClick(int type)
	{
		switch (type)
		{
		case 0:
			InitVoxelPage();
			break;
		case 1:
			InitBlockPage();
			break;
		case 2:
			InitIsoBlockPage();
			break;
		}
	}

	private void OnIsoDeleteClick(int index)
	{
		if (index != -1)
		{
			_deleteIndex = index;
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000501), OnSureToDeleteIso);
		}
	}

	private void OnSureToDeleteIso()
	{
		string text = m_IsoHeaders[_deleteIndex].Name;
		string path = GameConfig.GetUserDataPath() + BuildingMan.s_IsoPath + text + ".biso";
		if (File.Exists(path))
		{
			File.Delete(path);
			mWndCtrl.ClearCostList();
			mWndCtrl.IsoListSelectIndex = -1;
			m_IsoHeaders.RemoveAt(_deleteIndex);
			mWndCtrl.RemoveIsoItem(_deleteIndex);
			mWndCtrl.ResetIsoPostion();
			Debug.Log("Delete the " + text + " successfully");
		}
	}

	private void OnIsoExportClick(int index)
	{
		BSIsoHeadData obj = m_IsoHeaders[index];
		CreateBrush(BrushType.bt_iso);
		BSIsoBrush bSIsoBrush = m_CurBrush as BSIsoBrush;
		bSIsoBrush.File_Name = obj.Name;
		bSIsoBrush.Gen = true;
		bSIsoBrush.onCancelClick += OnIsoCancelClick;
		mWndCtrl.Hide();
		if (onIsoExport != null)
		{
			onIsoExport(obj);
		}
	}

	private void OnIsoCancelClick()
	{
		mMenuCtrl.ResetMenuButtonClickEvent(started: false);
		mWndCtrl.Show();
		mWndCtrl.ChangeCkMenu(2);
	}

	private void OnItemToolTip(bool show, UIBuildWndItem.ItemType item_type, int item_index)
	{
		switch (item_type)
		{
		case UIBuildWndItem.ItemType.mVoxelMat:
		{
			ItemProto itemProto6 = PeSingleton<ItemProto.Mgr>.Instance.Get(m_VoxelProtoItems[item_index]);
			UITooltip.ShowText(itemProto6.GetName());
			break;
		}
		case UIBuildWndItem.ItemType.mBlockMat:
		{
			ItemProto itemProto5 = PeSingleton<ItemProto.Mgr>.Instance.Get(m_BlockProtoItems[item_index]);
			UITooltip.ShowText(itemProto5.GetName());
			break;
		}
		case UIBuildWndItem.ItemType.mMenu:
		{
			UIBuildWndItem component = mMenuCtrl.mBoxBars.Items[item_index].GetComponent<UIBuildWndItem>();
			if (component != null)
			{
				if (component.mTargetItemType == UIBuildWndItem.ItemType.mVoxelMat)
				{
					ItemProto itemProto2 = PeSingleton<ItemProto.Mgr>.Instance.Get(m_VoxelProtoItems[component.mTargetIndex]);
					UITooltip.ShowText(itemProto2.GetName());
				}
				else if (component.mTargetItemType == UIBuildWndItem.ItemType.mBlockMat)
				{
					ItemProto itemProto3 = PeSingleton<ItemProto.Mgr>.Instance.Get(m_BlockProtoItems[component.mTargetIndex]);
					UITooltip.ShowText(itemProto3.GetName());
				}
				else if (component.mTargetItemType == UIBuildWndItem.ItemType.mVoxelType)
				{
					int itemID2 = BSVoxelMatMap.GetItemID(component.ItemId);
					ItemProto itemProto4 = PeSingleton<ItemProto.Mgr>.Instance.Get(itemID2);
					UITooltip.ShowText(itemProto4.GetName());
				}
			}
			break;
		}
		case UIBuildWndItem.ItemType.mVoxelType:
		{
			int itemID = BSVoxelMatMap.GetItemID(m_VoxelTypeList[item_index]);
			ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(itemID);
			UITooltip.ShowText(itemProto.GetName());
			break;
		}
		}
	}

	private void OnVoxelMatItemClick(int index)
	{
		if (mWndCtrl.TextureListSelectIndex == -1 || mWndCtrl.TextureListSelectIndex >= m_VoxelProtoItems.Count)
		{
			return;
		}
		if (m_VoxelProtoItems == null)
		{
			InitVoxelPage();
		}
		if (m_VoxelProtoItems == null)
		{
			return;
		}
		m_VoxelTypeList = BSVoxelMatMap.GetMaterialIDs(m_VoxelProtoItems[mWndCtrl.TextureListSelectIndex]);
		List<string> list = new List<string>();
		foreach (int voxelType in m_VoxelTypeList)
		{
			BSVoxelMatMap.MapData mapData = BSVoxelMatMap.GetMapData(voxelType);
			if (mapData != null)
			{
				list.Add(mapData.icon);
			}
		}
		PEBuildingMan.Self.Pattern = BSPattern.DefaultV1;
		mWndCtrl.RefreshTypeLisItem(list.ToArray(), "Icon");
		for (int i = 0; i < mWndCtrl.mTypeList.Count; i++)
		{
			mWndCtrl.mTypeList[i].SetItemID(m_VoxelTypeList[i]);
		}
		if (mWndCtrl.TypeSelectIndex > -1)
		{
			if (mWndCtrl.TypeSelectIndex >= mWndCtrl.mTypeList.Count)
			{
				mWndCtrl.TypeSelectIndex = 0;
			}
			PEBuildingMan.Self.Manipulator.MaterialType = (byte)mWndCtrl.mTypeList[mWndCtrl.TypeSelectIndex].ItemId;
		}
		else
		{
			mWndCtrl.TypeSelectIndex = 0;
			OnVoxelTypeClick(mWndCtrl.TypeSelectIndex);
		}
		mWndCtrl.DisselectBlock();
		ChangeBrushToBox();
	}

	private void OnVoxelTypeClick(int index)
	{
		if (!(PEBuildingMan.Self == null))
		{
			UIBuildWndItem uIBuildWndItem = null;
			if (mWndCtrl.TextureListSelectIndex > -1)
			{
				uIBuildWndItem = mWndCtrl.mVoxelMatList[mWndCtrl.TextureListSelectIndex];
				uIBuildWndItem.mSubsetIndex = index;
				PEBuildingMan.Self.Manipulator.MaterialType = (byte)m_VoxelTypeList[mWndCtrl.TypeSelectIndex];
				PEBuildingMan.Self.Pattern = BSPattern.DefaultV1;
			}
		}
	}

	private void OnBlockMatClick(int index)
	{
		if (PEBuildingMan.Self == null)
		{
			return;
		}
		if (!mWndCtrl.mBlockMatList[index].IsActive)
		{
			new PeTipMsg(PELocalization.GetString(8000854), PeTipMsg.EMsgLevel.Warning);
			return;
		}
		mWndCtrl.DisselectVoxel();
		PEBuildingMan.Self.Manipulator.MaterialType = (byte)PEBuildingMan.GetBlockMaterialType(mWndCtrl.mBlockMatList[index].ItemId);
		if (GameUI.Instance.mSkillWndCtrl != null && GameUI.Instance.mSkillWndCtrl._SkillMgr != null && !GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckBuildShape(mWndCtrl.BlockPatternSelectIndex))
		{
			PEBuildingMan.Self.Pattern = BSPattern.DefaultB1;
		}
		else if (mWndCtrl.BlockPatternSelectIndex == -1)
		{
			PEBuildingMan.Self.Pattern = BSPattern.DefaultB1;
		}
		else
		{
			PEBuildingMan.Self.Pattern = BSPattern.s_tblPatterns[mWndCtrl.mBlockPatternList[mWndCtrl.BlockPatternSelectIndex].ItemId];
		}
		ChangeBrushToBox();
	}

	private void OnBlockPatternClick(int index)
	{
		if (PEBuildingMan.Self == null)
		{
			return;
		}
		if (!mWndCtrl.mBlockPatternList[index].IsActive)
		{
			new PeTipMsg(PELocalization.GetString(8000854), PeTipMsg.EMsgLevel.Warning);
			return;
		}
		int num = 0;
		foreach (KeyValuePair<int, BSPattern> s_tblPattern in BSPattern.s_tblPatterns)
		{
			if (num == index)
			{
				if (mWndCtrl.BlockMatSelectIndex == -1)
				{
					if (mWndCtrl.TextureListSelectIndex != -1)
					{
						int itemId = mWndCtrl.mVoxelMatList[mWndCtrl.TextureListSelectIndex].ItemId;
						PEBuildingMan.Self.Manipulator.MaterialType = (byte)PEBuildingMan.GetBlockMaterialType(itemId);
						int num2 = mWndCtrl.mBlockMatList.FindIndex((UIBuildWndItem item0) => item0.ItemId == itemId);
						if (num2 != -1)
						{
							mWndCtrl.BlockMatSelectIndex = num2;
						}
						else
						{
							mWndCtrl.BlockMatSelectIndex = 0;
						}
						mWndCtrl.DisselectVoxel();
					}
					else
					{
						PEBuildingMan.Self.Manipulator.MaterialType = (byte)PEBuildingMan.GetBlockMaterialType(mWndCtrl.mBlockMatList[0].ItemId);
						mWndCtrl.BlockMatSelectIndex = 0;
					}
				}
				ItemProto itemData = ItemProto.GetItemData(mWndCtrl.mBlockMatList[mWndCtrl.BlockMatSelectIndex].ItemId);
				if (!GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckBuildBlockLevel(itemData.level))
				{
					PEBuildingMan.Self.Manipulator.MaterialType = 2;
				}
				PEBuildingMan.Self.Pattern = s_tblPattern.Value;
				ChangeBrushToBox();
				break;
			}
			num++;
		}
	}

	private void OnIsoClick(int index)
	{
		if (mWndCtrl.IsoListSelectIndex <= -1)
		{
			return;
		}
		mWndCtrl.ClearCostList();
		foreach (KeyValuePair<byte, uint> cost in m_IsoHeaders[mWndCtrl.IsoListSelectIndex].costs)
		{
			int blockItemProtoID = PEBuildingMan.GetBlockItemProtoID(cost.Key);
			if (blockItemProtoID == -1)
			{
				return;
			}
			ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(blockItemProtoID);
			PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			int itemCount = cmpt.GetItemCount(blockItemProtoID);
			int num = Mathf.Clamp(itemCount, 0, 9999);
			mWndCtrl.AddCostListItem(itemProto.GetName(), Mathf.CeilToInt((float)cost.Value / 4f).ToString() + '/' + num.ToString(), itemProto.icon[0], "Icon");
		}
		IsoRePos = true;
	}

	private void OnBrushMenuItemClick(UIBrushMenuItem.BrushType type)
	{
		switch (type)
		{
		case UIBrushMenuItem.BrushType.pointAdd:
			mMenuCtrl.SetBrushItemSprite(type, menuItemAddColor);
			pointMode = EBSBrushMode.Add;
			if (m_CurBrush as BSPointBrush != null)
			{
				m_CurBrush.mode = pointMode;
			}
			mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mPointBtn);
			break;
		case UIBrushMenuItem.BrushType.pointRemove:
			mMenuCtrl.SetBrushItemSprite(type, menuItemRemoveColor);
			pointMode = EBSBrushMode.Subtract;
			if (m_CurBrush as BSPointBrush != null)
			{
				m_CurBrush.mode = pointMode;
			}
			mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mPointBtn);
			break;
		case UIBrushMenuItem.BrushType.boxAdd:
			mMenuCtrl.SetBrushItemSprite(type, menuItemAddColor);
			boxMode = EBSBrushMode.Add;
			if (m_CurBrush as BSBoxBrush != null)
			{
				m_CurBrush.mode = boxMode;
			}
			mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mBoxBtn);
			break;
		case UIBrushMenuItem.BrushType.boxRemove:
			mMenuCtrl.SetBrushItemSprite(type, menuItemRemoveColor);
			boxMode = EBSBrushMode.Subtract;
			if (m_CurBrush as BSBoxBrush != null)
			{
				m_CurBrush.mode = boxMode;
			}
			mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mBoxBtn);
			break;
		case UIBrushMenuItem.BrushType.diagonalXPos:
		{
			diagonalRot = 0;
			BSB45DiagonalBrush bSB45DiagonalBrush3 = m_CurBrush as BSB45DiagonalBrush;
			if (bSB45DiagonalBrush3 != null)
			{
				bSB45DiagonalBrush3.m_Rot = diagonalRot;
			}
			mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mDiagonalBtn);
			break;
		}
		case UIBrushMenuItem.BrushType.diagonalXNeg:
		{
			mMenuCtrl.SetBrushItemSprite(type, menuItemAddColor);
			diagonalRot = 2;
			BSB45DiagonalBrush bSB45DiagonalBrush = m_CurBrush as BSB45DiagonalBrush;
			if (bSB45DiagonalBrush != null)
			{
				bSB45DiagonalBrush.m_Rot = diagonalRot;
			}
			mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mDiagonalBtn);
			break;
		}
		case UIBrushMenuItem.BrushType.diagonalZPos:
		{
			mMenuCtrl.SetBrushItemSprite(type, menuItemAddColor);
			diagonalRot = 1;
			BSB45DiagonalBrush bSB45DiagonalBrush4 = m_CurBrush as BSB45DiagonalBrush;
			if (bSB45DiagonalBrush4 != null)
			{
				bSB45DiagonalBrush4.m_Rot = diagonalRot;
			}
			mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mDiagonalBtn);
			break;
		}
		case UIBrushMenuItem.BrushType.diagonalZNeg:
		{
			mMenuCtrl.SetBrushItemSprite(type, menuItemAddColor);
			diagonalRot = 3;
			BSB45DiagonalBrush bSB45DiagonalBrush2 = m_CurBrush as BSB45DiagonalBrush;
			if (bSB45DiagonalBrush2 != null)
			{
				bSB45DiagonalBrush2.m_Rot = diagonalRot;
			}
			mMenuCtrl.ManualEnbleBtn(mMenuCtrl.mDiagonalBtn);
			break;
		}
		case UIBrushMenuItem.BrushType.SelectAll:
			mMenuCtrl.SetBrushItemSprite(type, menuItemAddColor);
			if (m_CurBrush as BSIsoSelectBrush != null)
			{
				CreateBrush(BrushType.bt_selectAll);
			}
			break;
		case UIBrushMenuItem.BrushType.SelectDetail:
			mMenuCtrl.SetBrushItemSprite(type, menuItemAddColor);
			if (m_CurBrush as BSMiscBrush != null)
			{
				CreateBrush(BrushType.bt_selectBlock);
			}
			break;
		}
	}

	private void OnMenuItemClick(int index)
	{
		if (Input.GetMouseButtonUp(1))
		{
			OnMenuQuickBarClick(mMenuCtrl.mMenuList[index]);
			mMenuCtrl.mMenuList[index].PlayGridEffect();
		}
	}

	private void OnMenuBlockSelectClick()
	{
		if (mWndCtrl.BlockMatSelectIndex == -1)
		{
			mWndCtrl.BlockMatSelectIndex = 0;
			OnBlockMatClick(0);
			mWndCtrl.BlockPatternSelectIndex = 0;
			OnBlockPatternClick(0);
		}
	}

	private void OnMenuVoxelSelectClick()
	{
		if (mWndCtrl.TextureListSelectIndex == -1)
		{
			mWndCtrl.TextureListSelectIndex = 0;
			OnVoxelMatItemClick(0);
			mWndCtrl.TypeSelectIndex = 0;
			OnVoxelTypeClick(0);
		}
	}

	private void InitBuildTips()
	{
		if (null != UIRecentDataMgr.Instance)
		{
			_showTips = UIRecentDataMgr.Instance.GetIntValue("UIBuildTips", _showTips ? 1 : 0) > 0;
			SetTipsState(_showTips);
		}
	}

	private void OnTipsBtnClick()
	{
		SetTipsState(!_showTips);
	}

	private void SetTipsState(bool isShowTips)
	{
		mTipsRoot.SetActive(isShowTips);
		if (isShowTips)
		{
			mTipsNormSprite.gameObject.SetActive(value: true);
			mTipsShowSprite.gameObject.SetActive(value: false);
		}
		else
		{
			mTipsShowSprite.gameObject.SetActive(value: true);
			mTipsNormSprite.gameObject.SetActive(value: false);
		}
		if (_showTips != isShowTips)
		{
			_showTips = isShowTips;
			if (null != UIRecentDataMgr.Instance)
			{
				UIRecentDataMgr.Instance.SetIntValue("UIBuildTips", isShowTips ? 1 : 0);
			}
		}
	}

	private void ChangeBrushToBox()
	{
		if (m_CurBrushType == BrushType.bt_selectAll || m_CurBrushType == BrushType.bt_selectBlock || m_CurBrushType == BrushType.bt_inclined)
		{
			mMenuCtrl.mBoxBtn.checkBox.isChecked = true;
			Instance.CreateBrush(BrushType.bt_box);
		}
	}

	public void ShowAllBuildTutorial()
	{
		if (PeGameMgr.IsTutorial)
		{
			if (!base.gameObject.activeSelf)
			{
				EnterBuildMode();
			}
			mBuildTutorialCtrl.ShowAllBuildTutorial();
		}
	}
}
