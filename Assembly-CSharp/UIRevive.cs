using System;
using ItemAsset;
using Pathea;
using Pathea.PeEntityExt;
using PeMap;
using SkillSystem;
using UnityEngine;

public class UIRevive : UIBaseWnd
{
	private enum ReivieState
	{
		None,
		Reivie_Player,
		Reivie_Servant
	}

	private const int mRevivePrtroId = 937;

	public UILabel mName;

	public UILabel mLeftTimeLabal;

	public Grid_N mGrid;

	public Grid_N mGrid_2;

	public UITexture mHeadTex;

	[SerializeField]
	private N_ImageButton mPlayerOkBtn;

	[SerializeField]
	private N_ImageButton mPlayerCancelBtn;

	[SerializeField]
	private N_ImageButton mServantOkBtn;

	[SerializeField]
	private N_ImageButton mServantCancelBtn;

	public Transform mTsPlayer;

	public Transform mTsServant;

	private ReivieState currentState;

	private PlayerPackageCmpt playerPackage;

	private PeEntity currentEntity;

	private ItemSample item;

	private float m_DelayTime = 100000f;

	private bool mStartReciprocal;

	private NpcCmpt mServantRevided;

	public override void OnCreate()
	{
		base.OnCreate();
		PeSingleton<MainPlayer>.Instance.mainPlayerCreatedEventor.Subscribe(delegate
		{
			AttachEvent();
		});
		if (null != PeSingleton<MainPlayer>.Instance.entity)
		{
			AttachEvent();
		}
		UIEventListener uIEventListener = UIEventListener.Get(mPlayerOkBtn.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			BtnClick_OnRevive();
		});
		UIEventListener uIEventListener2 = UIEventListener.Get(mPlayerCancelBtn.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
		{
			BtnClick_OnCancel();
		});
		UIEventListener uIEventListener3 = UIEventListener.Get(mServantOkBtn.gameObject);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, (UIEventListener.VoidDelegate)delegate
		{
			BtnClick_OnServentRevive();
		});
		UIEventListener uIEventListener4 = UIEventListener.Get(mServantCancelBtn.gameObject);
		uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, (UIEventListener.VoidDelegate)delegate
		{
			BtnClick_OnCancel();
		});
	}

	private void Start()
	{
		item = new ItemSample(937);
		mGrid.SetItem(item);
		mGrid_2.SetItem(item);
	}

	public override void Show()
	{
		if (currentState == ReivieState.None)
		{
			Hide();
			return;
		}
		base.Show();
		playerPackage = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
		mTsPlayer.gameObject.SetActive(currentState == ReivieState.Reivie_Player);
		mTsServant.gameObject.SetActive(currentState == ReivieState.Reivie_Servant);
		mStartReciprocal = true;
	}

	public void mUpdate()
	{
		bool isEnabled = CanRevive();
		mPlayerOkBtn.isEnabled = isEnabled;
		mServantOkBtn.isEnabled = isEnabled;
		int count = UINPCfootManMgr.Instance.mItemList.Count;
		for (int i = 0; i < count; i++)
		{
			UIfootManItem uIfootManItem = UINPCfootManMgr.Instance.mItemList[i];
			if (null != uIfootManItem.npcCmpt)
			{
				if (uIfootManItem.npcCmpt == mServantRevided)
				{
					SetReviveWaitTime(uIfootManItem.ReviveTimer);
				}
				if (uIfootManItem.ReviveTimer <= 0f && ServantLeaderCmpt.Instance != null)
				{
					ServantLeaderCmpt.Instance.RemoveServant(uIfootManItem.npcCmpt.Entity.GetCmpt<NpcCmpt>());
					HideServantRevive();
				}
			}
		}
	}

	private void AttachEvent()
	{
		SkAliveEntity cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<SkAliveEntity>();
		if (cmpt != null)
		{
			cmpt.deathEvent += OnPlayerDead;
			if (cmpt.isDead)
			{
				OnPlayerDead(cmpt, null);
			}
		}
	}

	private void OnPlayerDead(SkEntity skSelf, SkEntity skCaster)
	{
		currentState = ReivieState.Reivie_Player;
		currentEntity = (skSelf as SkAliveEntity).Entity;
		Show();
	}

	public void ShowServantRevive(PeEntity servant)
	{
	}

	public void ShowServantRevive(NpcCmpt servant)
	{
		SkAliveEntity cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<SkAliveEntity>();
		if (!cmpt.isDead)
		{
			currentState = ReivieState.Reivie_Servant;
			currentEntity = servant.Entity;
			SetServantReviveInfo(servant.Follwerentity.ExtGetFaceTex(), currentEntity.ExtGetName());
			mServantRevided = servant;
			Show();
		}
	}

	public void HideServantRevive()
	{
		mStartReciprocal = false;
		if (currentState == ReivieState.Reivie_Servant || currentEntity == null)
		{
			Hide();
		}
	}

	public void ShowServantRevive()
	{
		if (!(UINPCfootManMgr.Instance == null))
		{
			NpcCmpt needReviveServant = GameUI.Instance.mServantWndCtrl.GetNeedReviveServant();
			if (needReviveServant != null)
			{
				ShowServantRevive(needReviveServant);
			}
		}
	}

	public void SetReviveWaitTime(float delayTime)
	{
		int num = (int)delayTime;
		int num2 = num / 60 / 60 % 24;
		int num3 = num / 60 % 60;
		int num4 = num % 60;
		mLeftTimeLabal.text = num2 + ":" + num3 + ":" + num4;
	}

	private void SetServantReviveInfo(Texture head, string name)
	{
		mName.text = name;
		mHeadTex.mainTexture = head;
	}

	public bool CanRevive()
	{
		if (playerPackage == null)
		{
			if (GameUI.Instance != null && GameUI.Instance.mMainPlayer != null)
			{
				playerPackage = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
			}
			if (playerPackage == null)
			{
				return false;
			}
		}
		return playerPackage.ContainsItem(937);
	}

	private bool DoRevive(bool immediately = false)
	{
		MotionMgrCmpt cmpt = currentEntity.GetCmpt<MotionMgrCmpt>();
		PEActionParamB param = PEActionParamB.param;
		if (immediately)
		{
			param.b = true;
			cmpt.DoActionImmediately(PEActionType.Revive, param);
		}
		else
		{
			param.b = false;
			cmpt.DoAction(PEActionType.Revive, param);
		}
		return cmpt.IsActionRunning(PEActionType.Revive);
	}

	private Vector3 GetNearFastTrvalPos(Vector3 playerPos)
	{
		Vector3 nearPos = playerPos;
		float distance = float.MaxValue;
		if (PeGameMgr.IsCustom)
		{
			nearPos = PeSingleton<CustomGameData.Mgr>.Instance.curGameData.curPlayer.StartLocation;
		}
		PeSingleton<LabelMgr>.Instance.ForEach(delegate(ILabel label)
		{
			if (label.FastTravel())
			{
				float num = Vector3.Distance(playerPos, label.GetPos());
				if (num < distance)
				{
					distance = num;
					nearPos = label.GetPos();
				}
			}
		});
		return nearPos;
	}

	private void BtnClick_OnRevive()
	{
		if (currentEntity == null)
		{
			return;
		}
		if (!PeGameMgr.IsMulti)
		{
			if (DoRevive())
			{
				UseItemCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<UseItemCmpt>();
				cmpt.Revive();
				Hide();
			}
			return;
		}
		if (null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_PlayerRevive, currentEntity.position);
		}
		Hide();
	}

	private void BtnClick_OnServentRevive()
	{
		if ((mServantRevided != null && !mServantRevided.CanRecive) || currentEntity == null)
		{
			return;
		}
		foreach (UIfootManItem mItem in UINPCfootManMgr.Instance.mItemList)
		{
			if (null != mItem.npcCmpt && mItem.npcCmpt == mServantRevided)
			{
				mItem.InitReviveTime();
			}
		}
		if (!PeGameMgr.IsMulti)
		{
			if (DoRevive())
			{
				UseItemCmpt useItemCmpt = currentEntity.GetCmpt<UseItemCmpt>();
				if (null == useItemCmpt)
				{
					useItemCmpt = currentEntity.Add<UseItemCmpt>();
				}
				useItemCmpt.ReviveServent();
				Hide();
			}
		}
		else
		{
			PlayerNetwork.RequestServantRevive(currentEntity.Id, currentEntity.position);
			Hide();
		}
	}

	private void BtnClick_OnCancel()
	{
		if (currentState == ReivieState.Reivie_Servant || currentEntity == null)
		{
			Hide();
			return;
		}
		if (!PeGameMgr.IsMulti)
		{
			ReviveLabel reviveLabel = new ReviveLabel();
			reviveLabel.pos = currentEntity.position;
			PeSingleton<ReviveLabel.Mgr>.Instance.Add(reviveLabel);
			if (RandomDungenMgrData.InDungeon)
			{
				if (DoRevive(immediately: true))
				{
					currentEntity.position = RandomDungenMgrData.revivePos;
					Hide();
				}
				return;
			}
			if (null != MissionManager.Instance)
			{
				MissionManager.Instance.RemoveFollowTowerMission();
			}
			DoRevive(immediately: true);
			Vector3 pos = ((SingleGameStory.curType == SingleGameStory.StoryScene.DienShip0) ? new Vector3(14798.09f, 20.98818f, 8246.396f) : ((SingleGameStory.curType == SingleGameStory.StoryScene.L1Ship) ? new Vector3(9649.354f, 90.488f, 12744.77f) : ((SingleGameStory.curType != SingleGameStory.StoryScene.PajaShip) ? GetNearFastTrvalPos(currentEntity.position) : new Vector3(1593.53f, 148.635f, 8022.03f))));
			PeSingleton<FastTravelMgr>.Instance.TravelTo(pos);
		}
		else
		{
			Vector3 zero = Vector3.zero;
			if (RandomDungenMgrData.InDungeon)
			{
				zero = RandomDungenMgrData.revivePos;
			}
			else if (PeGameMgr.IsMultiCoop)
			{
				zero = GetNearFastTrvalPos(currentEntity.position);
			}
			else if (PeGameMgr.IsCustom)
			{
				zero = PlayerNetwork.mainPlayer.GetCustomModePos();
			}
			else
			{
				IntVector2 spawnPos = VArtifactUtil.GetSpawnPos();
				zero = new Vector3(spawnPos.x, VFDataRTGen.GetPosTop(spawnPos), spawnPos.y);
			}
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_PlayerReset, zero);
			PlayerNetwork.mainPlayer.RequestChangeScene(0);
		}
		Hide();
	}
}
