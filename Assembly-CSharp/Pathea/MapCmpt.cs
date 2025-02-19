using PeMap;
using UnityEngine;

namespace Pathea;

public class MapCmpt : PeCmpt, ILabel, IPeMsg
{
	private PeTrans mTrans;

	private EntityInfoCmpt mEntityInfo;

	private CommonCmpt mCommon;

	public CommonCmpt Common => mCommon;

	void IPeMsg.OnMsg(EMsg msg, params object[] args)
	{
		if (PeGameMgr.IsMulti && null != mCommon && mCommon.Identity == EIdentity.Player)
		{
			switch (msg)
			{
			case EMsg.Net_Instantiate:
				PeSingleton<LabelMgr>.Instance.Add(this);
				break;
			case EMsg.Net_Destroy:
				PeSingleton<LabelMgr>.Instance.Remove(this);
				break;
			case EMsg.Net_Controller:
			case EMsg.Net_Proxy:
				break;
			}
		}
		else
		{
			switch (msg)
			{
			case EMsg.Lod_Collider_Created:
				PeSingleton<LabelMgr>.Instance.Add(this);
				break;
			case EMsg.View_Prefab_Destroy:
				PeSingleton<LabelMgr>.Instance.Remove(this);
				break;
			}
		}
	}

	int ILabel.GetIcon()
	{
		if (mCommon != null)
		{
			switch (mCommon.Race)
			{
			case ERace.Mankind:
				if (GameConfig.IsMultiMode)
				{
					if (mCommon.Identity == EIdentity.Player)
					{
						EntityInfoCmpt cmpt = base.Entity.GetCmpt<EntityInfoCmpt>();
						if (null != cmpt)
						{
							return cmpt.mapIcon;
						}
						break;
					}
					if (mCommon.Identity == EIdentity.Npc)
					{
						return 14;
					}
					return 18;
				}
				if (mCommon.Identity == EIdentity.Npc)
				{
					return 14;
				}
				return 18;
			case ERace.Monster:
				if (!mCommon.IsBoss)
				{
					return 18;
				}
				return 17;
			case ERace.Puja:
				if (!mCommon.IsBoss)
				{
					return 20;
				}
				return 21;
			case ERace.Paja:
				if (!mCommon.IsBoss)
				{
					return 23;
				}
				return 24;
			case ERace.Alien:
				return 18;
			case ERace.Tower:
				return 8;
			}
		}
		return 18;
	}

	Vector3 ILabel.GetPos()
	{
		return mTrans.position;
	}

	string ILabel.GetText()
	{
		if (mEntityInfo == null)
		{
			return string.Empty;
		}
		if (mEntityInfo.characterName == null)
		{
			return string.Empty;
		}
		if (base.Entity.entityProto.proto == EEntityProto.RandomNpc || base.Entity.entityProto.proto == EEntityProto.Npc || base.Entity.entityProto.proto == EEntityProto.Player)
		{
			return mEntityInfo.characterName.fullName;
		}
		return string.Empty;
	}

	bool ILabel.FastTravel()
	{
		if (PeGameMgr.IsSingle)
		{
			return false;
		}
		if (mEntityInfo != null && mEntityInfo.mapIcon == 11 && PeGameMgr.IsMulti)
		{
			bool flag = PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId == 0;
			bool flag2 = mEntityInfo.Entity != null && mEntityInfo.Entity.netCmpt != null && mEntityInfo.Entity.netCmpt.network != null && mEntityInfo.Entity.netCmpt.network is PlayerNetwork && (mEntityInfo.Entity.netCmpt.network as PlayerNetwork)._curSceneId == 0;
			return flag && flag2;
		}
		return false;
	}

	ELabelType ILabel.GetType()
	{
		return ELabelType.Npc;
	}

	bool ILabel.NeedArrow()
	{
		return false;
	}

	float ILabel.GetRadius()
	{
		return -1f;
	}

	EShow ILabel.GetShow()
	{
		if (mCommon != null)
		{
			switch (mCommon.Race)
			{
			case ERace.Mankind:
				if (GameConfig.IsMultiMode)
				{
					if (mCommon.Identity == EIdentity.Npc || mCommon.Identity == EIdentity.Player)
					{
						return EShow.All;
					}
					return EShow.MinMap;
				}
				if (mCommon.Identity == EIdentity.Npc)
				{
					return EShow.All;
				}
				return EShow.MinMap;
			case ERace.Monster:
				if (!mCommon.IsBoss)
				{
					return EShow.MinMap;
				}
				return EShow.All;
			case ERace.Puja:
				return EShow.MinMap;
			case ERace.Paja:
				return EShow.MinMap;
			case ERace.Alien:
				return EShow.MinMap;
			}
		}
		return EShow.Max;
	}

	public override void Start()
	{
		base.Start();
		mTrans = base.Entity.peTrans;
		mEntityInfo = base.Entity.GetCmpt<EntityInfoCmpt>();
		mCommon = base.Entity.GetCmpt<CommonCmpt>();
	}

	public bool CompareTo(ILabel label)
	{
		if (label is MapCmpt)
		{
			MapCmpt mapCmpt = (MapCmpt)label;
			if (mapCmpt == this)
			{
				return true;
			}
			return false;
		}
		return false;
	}
}
