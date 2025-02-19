using System;
using System.IO;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class SceneDoodadLodCmpt : LodCmpt, IPeMsg
{
	private const byte c_VerToAddIsDamagable = 2;

	private const byte c_VerToAddIsShown = 3;

	private const byte c_CurVer = 3;

	protected SceneObjAdditionalSaveData _additionalData = new SceneObjAdditionalSaveData();

	private bool _bShown;

	private bool _bDamagable;

	private IBoundInScene _bound;

	public bool IsConstructed => base.Entity.viewCmpt == null || base.Entity.viewCmpt.hasView;

	public int Index { get; set; }

	public bool IsShown
	{
		get
		{
			return _bShown;
		}
		set
		{
			if (_bShown == value)
			{
				return;
			}
			_bShown = value;
			if (_bShown)
			{
				SceneMan.AddSceneObj(this);
				return;
			}
			SceneMan.RemoveSceneObj(this);
			if (IsConstructed)
			{
				OnDestruct();
			}
		}
	}

	public bool IsDamagable
	{
		get
		{
			return _bDamagable;
		}
		set
		{
			_bDamagable = value;
			SkAliveEntity aliveEntity = base.Entity.aliveEntity;
			if (aliveEntity != null)
			{
				if (_bDamagable)
				{
					aliveEntity.SetAttribute(AttribType.CampID, 0f);
					aliveEntity.SetAttribute(AttribType.DamageID, 25f);
				}
				else
				{
					aliveEntity.SetAttribute(AttribType.CampID, 28f);
					aliveEntity.SetAttribute(AttribType.DamageID, 28f);
				}
			}
		}
	}

	public override GameObject Go => (!Equals(null) && base.Entity.hasView) ? base.Entity.gameObject : null;

	public override IBoundInScene Bound => _bound;

	public override bool NeedToActivate => false;

	public override bool TstYOnActivate => false;

	public static event Action<SkEntity, SkEntity> commonDeathEvent;

	public void SetShowVar(bool bShown)
	{
		_bShown = bShown;
	}

	public void SetDamagable(int campId, int damageId, int playerId)
	{
		SkAliveEntity aliveEntity = base.Entity.aliveEntity;
		if (aliveEntity != null)
		{
			aliveEntity.SetAttribute(AttribType.CampID, campId);
			aliveEntity.SetAttribute(AttribType.DamageID, damageId);
			_bDamagable = campId != 28 || damageId != 28;
			if (playerId >= 0)
			{
				aliveEntity.SetAttribute(AttribType.DefaultPlayerID, playerId);
			}
		}
	}

	public override void Start()
	{
		BaseStart();
		DoodadProtoDb.Item item = DoodadProtoDb.Get(base.Entity.entityProto.protoId);
		_bound = new RadiusBound(item.range, base.Entity.peTrans.trans);
		SkAliveEntity aliveEntity = base.Entity.aliveEntity;
		aliveEntity.deathEvent += DoodadEntityCreator.OnDoodadDeath;
	}

	public override void OnConstruct()
	{
		if (Equals(null))
		{
			SceneMan.RemoveSceneObj(this);
			return;
		}
		BuildView();
		if (onConstruct != null)
		{
			onConstruct(base.Entity);
		}
	}

	public override void OnDestruct()
	{
		if (Equals(null))
		{
			SceneMan.RemoveSceneObj(this);
			return;
		}
		if (onDestruct != null)
		{
			onDestruct(base.Entity);
		}
		DestroyView();
	}

	public override void OnActivate()
	{
		if (Equals(null))
		{
			SceneMan.RemoveSceneObj(this);
		}
		else if (onActivate != null)
		{
			onActivate(base.Entity);
		}
	}

	public override void OnDeactivate()
	{
		if (Equals(null))
		{
			SceneMan.RemoveSceneObj(this);
		}
		else if (onDeactivate != null)
		{
			onDeactivate(base.Entity);
		}
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.View_Prefab_Build:
			DispatchViewData();
			break;
		case EMsg.View_Prefab_Destroy:
			CollectViewData();
			break;
		}
	}

	public override void Deserialize(BinaryReader r)
	{
		base.Deserialize(r);
		byte b = r.ReadByte();
		if (b < 2)
		{
			IsDamagable = b != 0;
		}
		else
		{
			switch (b)
			{
			case 2:
				r.ReadBoolean();
				Index = r.ReadInt32();
				break;
			case 3:
				IsShown = r.ReadBoolean();
				r.ReadBoolean();
				Index = r.ReadInt32();
				break;
			default:
				Debug.LogError("Unrecognized doodad save data:" + b);
				return;
			}
		}
		_additionalData.Deserialize(r);
		DispatchViewData();
	}

	public override void Serialize(BinaryWriter w)
	{
		base.Serialize(w);
		w.Write((byte)3);
		w.Write(IsShown);
		w.Write(IsDamagable);
		w.Write(Index);
		CollectViewData();
		_additionalData.Serialize(w);
	}

	private void CollectViewData()
	{
		if (base.Entity.viewCmpt != null && base.Entity.biologyViewCmpt.tView != null)
		{
			_additionalData.CollectData(base.Entity.biologyViewCmpt.tView.gameObject);
		}
	}

	private void DispatchViewData()
	{
		if (base.Entity.viewCmpt != null && base.Entity.biologyViewCmpt.tView != null)
		{
			_additionalData.DispatchData(base.Entity.biologyViewCmpt.tView.gameObject);
		}
	}
}
