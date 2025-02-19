using ItemAsset;
using Pathea;
using SkillSystem;
using UnityEngine;

public class DragItemMousePick : MousePickableChildCollider
{
	private ItemObject m_ItemObj;

	private PlayerPackage mPkg;

	private PeTrans mView;

	[HideInInspector]
	public bool cancmd = true;

	protected GameObject rootGameObject => base.gameObject;

	protected int id
	{
		get
		{
			ItemScript script = GetScript();
			if (null == script)
			{
				return -1;
			}
			return script.id;
		}
	}

	protected int itemObjectId
	{
		get
		{
			ItemScript script = GetScript();
			if (null == script)
			{
				return -1;
			}
			return script.itemObjectId;
		}
	}

	protected ItemObject itemObj
	{
		get
		{
			if (m_ItemObj == null)
			{
				m_ItemObj = PeSingleton<ItemMgr>.Instance.Get(itemObjectId);
			}
			return m_ItemObj;
		}
	}

	protected PlayerPackage pkg
	{
		get
		{
			if (mPkg == null)
			{
				PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
				mPkg = cmpt.package;
			}
			return mPkg;
		}
	}

	private Vector3 playerPos
	{
		get
		{
			if (null == mView && PeSingleton<PeCreature>.Instance.mainPlayer != null)
			{
				mView = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PeTrans>();
			}
			if (null == mView)
			{
				return Vector3.zero;
			}
			return mView.position;
		}
	}

	protected override string tipsText
	{
		get
		{
			if (itemObj != null && itemObj.protoData != null)
			{
				return "[5CB0FF]" + itemObj.protoData.dragName + "[-]\n" + PELocalization.GetString(8000129);
			}
			return string.Empty;
		}
	}

	protected Vector3 GetPos()
	{
		return DragItemAgent.GetById(id)?.position ?? Vector3.zero;
	}

	protected ItemScript GetScript()
	{
		return GetComponent<ItemScript>();
	}

	private void Awake()
	{
		base.priority = MousePicker.EPriority.Level2;
		if (MissionManager.Instance != null && MissionManager.Instance.m_PlayerMission.isRecordCreation && base.name.StartsWith("Creation"))
		{
			Invoke("CheckCanCmd", 0.5f);
		}
	}

	private void CheckCanCmd()
	{
		if (itemObj == null || itemObj.protoData == null || itemObj.protoData.itemClassId == 66)
		{
			cancmd = false;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		SkAliveEntity component = GetComponent<SkAliveEntity>();
		if (component != null)
		{
			component.deathEvent += OnDeath;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (GameUI.Instance != null && null != GameUI.Instance.mItemOp)
		{
			GameUI.Instance.mItemOp.GetItem(null, this);
		}
	}

	protected void HideItemOpGui()
	{
		GameUI.Instance.mItemOp.Hide();
	}

	protected virtual void InitCmd(CmdList cmdList)
	{
		cmdList.Add("Turn", Turn90Degree);
		cmdList.Add("Get", OnGetBtn);
	}

	public virtual bool CanCmd()
	{
		if (null != SelectItem_N.Instance && SelectItem_N.Instance.HaveOpItem())
		{
			return false;
		}
		if (DistanceInRange(playerPos, operateDistance))
		{
			SkAliveEntity component = GetComponent<SkAliveEntity>();
			if (component != null && component.isDead)
			{
				return false;
			}
			if (!cancmd)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public virtual void DoGetItem()
	{
		if (itemObj == null)
		{
			return;
		}
		if (!GameConfig.IsMultiMode)
		{
			if (PlayerPackageCmpt.LockStackCount && !ItemMgr.IsCreationItem(itemObj.protoId))
			{
				PeSingleton<ItemMgr>.Instance.DestroyItem(itemObj.instanceId);
			}
			else if (pkg != null)
			{
				if (ItemPackage.InvalidIndex == pkg.AddItem(itemObj))
				{
					PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
					return;
				}
				if (MissionManager.Instance != null && PeSingleton<PeCreature>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
				{
					MissionManager.Instance.ProcessUseItemMissionByID(itemObj.protoId, PeSingleton<PeCreature>.Instance.mainPlayer.position, -1);
				}
			}
			DragItemAgent byId = DragItemAgent.GetById(id);
			if (byId != null)
			{
				DragItemAgent.Destory(byId);
			}
			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
		else if (null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.RequestGetItemBack(itemObjectId);
		}
		HideItemOpGui();
	}

	public virtual void OnGetBtn()
	{
		GameUI.Instance.mItemOp.GetItem(DoGetItem, this);
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.PreRequestGetItemBack(itemObjectId);
		}
	}

	public virtual void Turn90Degree()
	{
		if (GameConfig.IsMultiMode)
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Turn, itemObjectId);
			}
		}
		else
		{
			DragItemAgent.GetById(id)?.Rotate(new Vector3(0f, 90f, 0f));
		}
	}

	protected override bool CheckPick(Ray camMouseRay, out float dis)
	{
		if (CanCmd())
		{
			return base.CheckPick(camMouseRay, out dis);
		}
		dis = float.MaxValue;
		return false;
	}

	protected override void CheckOperate()
	{
		if (PeInput.Get(PeInput.LogicFunction.OpenItemMenu) && CanCmd())
		{
			CmdList cmdList = new CmdList();
			InitCmd(cmdList);
			GameUI.Instance.mItemOp.SetCmdList(this, cmdList);
		}
	}

	private void OnDeath(SkEntity a, SkEntity b)
	{
		GameUI.Instance.mItemOp.Hide();
	}
}
