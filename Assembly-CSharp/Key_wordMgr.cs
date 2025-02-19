using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class Key_wordMgr : MonoBehaviour
{
	private static Key_wordMgr _self;

	public bool enableQuickKey = true;

	private CursorHandler m_CursorHandler = new CursorHandler();

	public static Key_wordMgr Self => _self;

	private void Awake()
	{
		_self = this;
		m_CursorHandler.Type = CursorState.EType.Hand;
	}

	private void Start()
	{
	}

	private bool CloseFrontWnd()
	{
		if (UIStateMgr.Instance == null)
		{
			return false;
		}
		if (GameUI.Instance == null)
		{
			return false;
		}
		if (GameUI.Instance.mUIWorldMap.isShow)
		{
			GameUI.Instance.mUIWorldMap.Hide();
			return true;
		}
		if (GameUI.Instance.mOption.isShow)
		{
			GameUI.Instance.mOption.Hide();
			return true;
		}
		if (GameUI.Instance.mSaveLoad.isShow)
		{
			GameUI.Instance.mSaveLoad.Hide();
			return true;
		}
		if (MessageBox_N.IsShowing && (bool)MessageBox_N.Instance)
		{
			MsgInfoType currentInfoTypeP = MessageBox_N.Instance.GetCurrentInfoTypeP();
			if (currentInfoTypeP != MsgInfoType.LobbyLoginMask && currentInfoTypeP != MsgInfoType.ServerDeleteMask && currentInfoTypeP != MsgInfoType.ServerLoginMask)
			{
				MessageBox_N.CancelMask(currentInfoTypeP);
			}
			return true;
		}
		List<UIBaseWnd> mBaseWndList = UIStateMgr.Instance.mBaseWndList;
		UIBaseWnd uIBaseWnd = null;
		foreach (UIBaseWnd item in mBaseWndList)
		{
			if (item == GameUI.Instance.mMissionTrackWnd || item == GameUI.Instance.mCustomMissionTrack.missionInterpreter.missionTrackWnd || item == GameUI.Instance.mRevive || !item.isShow || !item.Active)
			{
				continue;
			}
			uIBaseWnd = item;
			uIBaseWnd.Hide();
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (GameUI.Instance == null)
		{
			return;
		}
		if (null != GameUI.Instance.mMainPlayer && !GameConfig.IsInVCE && !UICamera.inputHasFocus)
		{
			if (PeInput.Get(PeInput.LogicFunction.OptionsUI))
			{
				KeyFunc_OptionUI();
			}
			else if (PeInput.Get(PeInput.LogicFunction.SaveMenuUI))
			{
				if (!GameConfig.IsMultiMode)
				{
					KeyFunc_SaveUI();
				}
			}
			else if (PeInput.Get(PeInput.LogicFunction.LoadMenuUI) && !GameConfig.IsMultiMode)
			{
				KeyFunc_LoadUI();
			}
		}
		if (GameConfig.IsInVCE && PeInput.Get(PeInput.LogicFunction.CreationSystem) && !UICamera.inputHasFocus && VCEditor.Instance != null && VCEditor.Instance.m_UI != null)
		{
			VCEditor.Instance.m_UI.OnQuitClick();
		}
		if (!(null != GameUI.Instance.mMainPlayer) || GameConfig.IsInVCE || UISystemMenu.IsSystemOping() || UICamera.inputHasFocus)
		{
			return;
		}
		if (PeInput.Get(PeInput.LogicFunction.PackageUI))
		{
			KeyFunc_ItemPackge();
		}
		if (PeInput.Get(PeInput.LogicFunction.WorldMapUI))
		{
			GlobalEvent.NoticeMouseUnlock();
			KeyFunc_WorldMap();
		}
		if (PeInput.Get(PeInput.LogicFunction.CharacterUI))
		{
			KeyFunc_Character();
		}
		if (PeInput.Get(PeInput.LogicFunction.MissionUI))
		{
			KeyFunc_MissionUI();
		}
		if (PeInput.Get(PeInput.LogicFunction.ColonyUI))
		{
			KeyFunc_ColonyUI();
		}
		if (PeInput.Get(PeInput.LogicFunction.ReplicationUI))
		{
			KeyFunc_ReplicationUI();
		}
		if (PeInput.Get(PeInput.LogicFunction.FollowersUI))
		{
			KeyFunc_FollowerUI();
		}
		if (PeInput.Get(PeInput.LogicFunction.SkillUI))
		{
			KeyFunc_SkillUI();
		}
		if (PeInput.Get(PeInput.LogicFunction.HandheldPcUI))
		{
			KeyFunc_PhoneUI();
		}
		if (PeInput.Get(PeInput.LogicFunction.CreationSystem) && !VCEditor.s_Active && UISightingTelescope.Instance.CurType == UISightingTelescope.SightingType.Null)
		{
			VCEditor.Open();
		}
		if (PeInput.Get(PeInput.LogicFunction.TalkMenuUI))
		{
			KeyFunc_TalkMenuUI();
		}
		if (PeInput.Get(PeInput.LogicFunction.GameMenuUI))
		{
			KeyFunc_GameMenuUI();
		}
		if (enableQuickKey)
		{
			if (PeInput.Get(PeInput.LogicFunction.QuickBar1))
			{
				KeyFunc_QuickBar(0);
			}
			if (PeInput.Get(PeInput.LogicFunction.QuickBar2))
			{
				KeyFunc_QuickBar(1);
			}
			if (PeInput.Get(PeInput.LogicFunction.QuickBar3))
			{
				KeyFunc_QuickBar(2);
			}
			if (PeInput.Get(PeInput.LogicFunction.QuickBar4))
			{
				KeyFunc_QuickBar(3);
			}
			if (PeInput.Get(PeInput.LogicFunction.QuickBar5))
			{
				KeyFunc_QuickBar(4);
			}
			if (PeInput.Get(PeInput.LogicFunction.QuickBar6))
			{
				KeyFunc_QuickBar(5);
			}
			if (PeInput.Get(PeInput.LogicFunction.QuickBar7))
			{
				KeyFunc_QuickBar(6);
			}
			if (PeInput.Get(PeInput.LogicFunction.QuickBar8))
			{
				KeyFunc_QuickBar(7);
			}
			if (PeInput.Get(PeInput.LogicFunction.QuickBar9))
			{
				KeyFunc_QuickBar(8);
			}
			if (PeInput.Get(PeInput.LogicFunction.QuickBar10))
			{
				KeyFunc_QuickBar(9);
			}
			if (PeInput.Get(PeInput.LogicFunction.PrevQuickBar))
			{
				KeyFunc_PrevQuickBar();
			}
			if (PeInput.Get(PeInput.LogicFunction.NextQuickBar))
			{
				KeyFunc_NextQuickBar();
			}
		}
	}

	private void KeyFunc_WorldMap()
	{
		if (PeGameMgr.sceneMode != PeGameMgr.ESceneMode.Custom && PeGameMgr.playerType != PeGameMgr.EPlayerType.Tutorial)
		{
			GameUI.Instance.mUIWorldMap.ChangeWindowShowState();
			GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(active: false);
		}
	}

	private void KeyFunc_ItemPackge()
	{
		GameUI.Instance.mItemPackageCtrl.ChangeWindowShowState();
		GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(active: false);
	}

	private void KeyFunc_Character()
	{
		GameUI.Instance.mUIPlayerInfoCtrl.ChangeWindowShowState();
		GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(active: false);
	}

	private void KeyFunc_QuickBar(int index)
	{
		GameUI.Instance.mUIMainMidCtrl.OnKeyDown_QuickBar(index);
	}

	private void KeyFunc_OptionUI()
	{
		if (GameUI.Instance.mSystemMenu.isShow)
		{
			GameUI.Instance.mSystemMenu.ChangeWindowShowState();
		}
		else if (!CloseFrontWnd())
		{
			GameUI.Instance.mSystemMenu.ChangeWindowShowState();
		}
	}

	private void KeyFunc_SaveUI()
	{
		if (UISaveLoad.Instance.isShow)
		{
			UISaveLoad.Instance.Hide();
		}
		else if (!UISystemMenu.IsSystemOping())
		{
			GameUI.Instance.mSystemMenu.OnSaveBtn();
		}
	}

	private void KeyFunc_LoadUI()
	{
		if (UISaveLoad.Instance.isShow)
		{
			UISaveLoad.Instance.Hide();
		}
		else if (!UISystemMenu.IsSystemOping())
		{
			GameUI.Instance.mSystemMenu.OnLoadBtn();
		}
	}

	private void KeyFunc_MissionUI()
	{
		if (PeGameMgr.IsCustom)
		{
			GameUI.Instance.mMissionGoal.ChangeWindowShowState();
			if (GameUI.Instance.mMissionGoal.isShow)
			{
				GameUI.Instance.mCustomMissionTrack.Show();
			}
		}
		else
		{
			GameUI.Instance.mUIMissionWndCtrl.ChangeWindowShowState();
		}
		GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(active: false);
	}

	private void KeyFunc_ColonyUI()
	{
		if (!PeGameMgr.IsTutorial && !PeGameMgr.IsCustom && !PeGameMgr.IsMultiCustom)
		{
			GameUI.Instance.mCSUI_MainWndCtrl.ChangeWindowShowState();
			GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(active: false);
		}
	}

	private void KeyFunc_ReplicationUI()
	{
		GameUI.Instance.mCompoundWndCtrl.ChangeWindowShowState();
		GameUI.Instance.mMainPlayer.SendMsg(EMsg.UI_ShowChange, true);
	}

	private void KeyFunc_FollowerUI()
	{
		GameUI.Instance.mServantWndCtrl.ChangeWindowShowState();
		GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(active: false);
	}

	private void KeyFunc_SkillUI()
	{
		if (PeGameMgr.IsAdventure && RandomMapConfig.useSkillTree)
		{
			GameUI.Instance.mSkillWndCtrl.ChangeWindowShowState();
			GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(active: false);
		}
	}

	private void KeyFunc_PhoneUI()
	{
		GameUI.Instance.mPhoneWnd.ChangeWindowShowState();
		GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(active: false);
	}

	private void KeyFunc_TalkMenuUI()
	{
		UITalkwithctr.Instance.ShowMenu();
	}

	private void KeyFunc_GameMenuUI()
	{
		UIGameMenuCtrl.Instance.Show();
	}

	private void KeyFunc_PrevQuickBar()
	{
		if (null != UIMainMidCtrl.Instance)
		{
			UIMainMidCtrl.Instance.OnKeyFunc_PrevQuickBar();
		}
	}

	private void KeyFunc_NextQuickBar()
	{
		if (null != UIMainMidCtrl.Instance)
		{
			UIMainMidCtrl.Instance.OnKeyFunc_NextQuickBar();
		}
	}
}
