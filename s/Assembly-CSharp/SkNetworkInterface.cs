using System;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;
using PETools;
using SkillSystem;
using uLink;
using UnityEngine;

public class SkNetworkInterface : GroupNetInterface, ISceneObject
{
	protected bool _bDeath;

	public SkEntity _skEntity;

	private OperatorEnum _operator;

	private int _eEState;

	public int authId { get; protected set; }

	public Vector3 Pos => base.transform.position;

	public bool _OnCar { get; set; }

	public bool _OnTrain { get; set; }

	public bool _OnRide { get; set; }

	public int _SeatIndex { get; set; }

	public CreationNetwork _Creation { get; set; }

	public OperatorEnum Operator => _operator;

	public event Action<SkNetworkInterface> DeathEventHandler;

	public event Action<SkNetworkInterface, int, float> DamageEventHandler;

	public static event Action<SkNetworkInterface, int> OnPEACCheckEventHandler;

	public virtual void SkCreater()
	{
		if (LogFilter.logDebug)
		{
			Debug.LogWarning("virtual funtion is running!!! SkNetworkInterface Creater");
		}
	}

	private void SetEEState(PEActionType proType)
	{
		switch (proType)
		{
		case PEActionType.Death:
			_eEState |= 1;
			break;
		case PEActionType.EquipmentHold:
			_eEState |= 2;
			break;
		case PEActionType.HoldShield:
			_eEState |= 4;
			break;
		case PEActionType.GunHold:
			_eEState |= 8;
			break;
		case PEActionType.BowHold:
			_eEState |= 16;
			break;
		case PEActionType.AimEquipHold:
			_eEState |= 32;
			break;
		case PEActionType.HoldFlashLight:
			_eEState |= 64;
			break;
		case PEActionType.TwoHandSwordHold:
			_eEState |= 128;
			break;
		case PEActionType.Sleep:
			_eEState |= 512;
			break;
		case PEActionType.Sit:
			_eEState |= 256;
			break;
		}
	}

	public void CancelEEState(PEActionType proType)
	{
		switch (proType)
		{
		case PEActionType.Death:
			if ((_eEState & 1) != 0)
			{
				_eEState ^= 1;
			}
			break;
		case PEActionType.EquipmentHold:
			if ((_eEState & 2) != 0)
			{
				_eEState ^= 2;
			}
			break;
		case PEActionType.HoldShield:
			if ((_eEState & 4) != 0)
			{
				_eEState ^= 4;
			}
			break;
		case PEActionType.GunHold:
			if ((_eEState & 8) != 0)
			{
				_eEState ^= 8;
			}
			break;
		case PEActionType.BowHold:
			if ((_eEState & 0x10) != 0)
			{
				_eEState ^= 16;
			}
			break;
		case PEActionType.AimEquipHold:
			if ((_eEState & 0x20) != 0)
			{
				_eEState ^= 32;
			}
			break;
		case PEActionType.HoldFlashLight:
			if ((_eEState & 0x40) != 0)
			{
				_eEState ^= 64;
			}
			break;
		case PEActionType.TwoHandSwordHold:
			if ((_eEState & 0x80) != 0)
			{
				_eEState ^= 128;
			}
			break;
		case PEActionType.Sit:
			if ((_eEState & 0x100) != 0)
			{
				_eEState ^= 256;
			}
			break;
		case PEActionType.Sleep:
			if ((_eEState & 0x200) != 0)
			{
				_eEState ^= 512;
			}
			break;
		case PEActionType.Cure:
			if ((_eEState & 0x400) != 0)
			{
				_eEState ^= 1024;
			}
			break;
		case PEActionType.Operation:
			if ((_eEState & 0x800) != 0)
			{
				_eEState ^= 2048;
			}
			break;
		}
	}

	protected override void OnPEStart()
	{
		if ((bool)(this as Player))
		{
			_operator = OperatorEnum.Oper_Player;
		}
		else if ((bool)(this as AiAdNpcNetwork))
		{
			_operator = OperatorEnum.Oper_Npc;
		}
		else if ((bool)(this as AiMonsterNetwork))
		{
			_operator = OperatorEnum.Oper_Monster;
		}
		else
		{
			_operator = OperatorEnum.Oper_None;
		}
		base.OnPEStart();
		BindAction(EPacketType.PT_InGame_SKSyncAttr, RPC_SKSyncAttr);
		BindAction(EPacketType.PT_InGame_SKStartSkill, RPC_SKStartSkill);
		BindAction(EPacketType.PT_InGame_SKStopSkill, RPC_SKStopSkill);
		BindAction(EPacketType.PT_InGame_SKBLoop, RPC_SKBLoop);
		BindAction(EPacketType.PT_InGame_SKSyncInitAttrs, RPC_SKSyncAttrs);
		BindAction(EPacketType.PT_InGame_SKSendBaseAttrs, RPC_SKSendBaseAttrs);
		BindAction(EPacketType.PT_InGame_SKFellTree, RPC_SKFellTree);
		BindAction(EPacketType.PT_InGame_SKDigTerrain, RPC_SKDigTerrain);
		BindAction(EPacketType.PT_InGame_SKChangeTerrain, RPC_SKChangeTerrain);
		BindAction(EPacketType.PT_InGame_SKIKPos, RPC_SKIKPos);
		BindAction(EPacketType.PT_InGame_ClearGrass, RPC_C2S_ClearGrass);
		BindAction(EPacketType.PT_InGame_ClearTree, RPC_C2S_ClearTree);
		BindAction(EPacketType.PT_InGame_SKDAVVF, RPC_SKDAVVF);
		BindAction(EPacketType.PT_InGame_SKDAVFNS, RPC_SKDAVFNS);
		BindAction(EPacketType.PT_InGame_SKDANO, RPC_SKDANO);
		BindAction(EPacketType.PT_InGame_SKDAV, RPC_SKDAV);
		BindAction(EPacketType.PT_InGame_SKDAVVN, RPC_SKDAVVN);
		BindAction(EPacketType.PT_InGame_SKDAVQNS, RPC_SKDAVQNS);
		BindAction(EPacketType.PT_InGame_SKDAVQ, RPC_SKDAVQ);
		BindAction(EPacketType.PT_InGame_SKDAS, RPC_SKDAS);
		BindAction(EPacketType.PT_InGame_SKDAVQS, RPC_SKDAVQS);
		BindAction(EPacketType.PT_InGame_SKDAVQSN, RPC_SKDAVQSN);
		BindAction(EPacketType.PT_InGame_SKDAVVNN, RPC_SKDAVVNN);
		BindAction(EPacketType.PT_InGame_SKDAN, RPC_SKDAN);
		BindAction(EPacketType.PT_InGame_SKDAB, RPC_SKDAB);
		BindAction(EPacketType.PT_InGame_SKDAVQN, RPC_SKDAVQN);
		BindAction(EPacketType.PT_InGame_SKDAVFVFS, RPC_SKDAVFVFS);
		BindAction(EPacketType.PT_InGame_SKDAEndAction, RPC_SKDAEndAction);
		BindAction(EPacketType.PT_InGame_SKDAEndImmediately, RPC_SKDAEndImmediately);
		BindAction(EPacketType.PT_InGame_SKDAQueryEntityState, RPC_SKDAQueryEntityState);
		BindAction(EPacketType.PT_InGame_SKDAVQSNS, RPC_SKDAVQSNS);
		BindAction(EPacketType.PT_InGame_AbnormalConditionStart, RPC_C2S_AbnormalConditionStart);
		BindAction(EPacketType.PT_InGame_AbnormalConditionEnd, RPC_C2S_AbnormalConditionEnd);
		BindAction(EPacketType.PT_InGame_AbnormalCondition, RPC_C2S_AbnormalCondition);
		BindAction(EPacketType.PT_InGame_Jump, RPC_C2S_Jump);
		BindAction(EPacketType.PT_InGame_SkillBlockRange, RPC_C2S_BlockDestroyInRange);
		BindAction(EPacketType.PT_InGame_SkillVoxelRange, RPC_C2S_TerrainDestroyInRange);
		if (ServerConfig.IsCustom)
		{
			this.DeathEventHandler = (Action<SkNetworkInterface>)Delegate.Combine(this.DeathEventHandler, new Action<SkNetworkInterface>(OnSyncDeath));
			this.DamageEventHandler = (Action<SkNetworkInterface, int, float>)Delegate.Combine(this.DamageEventHandler, new Action<SkNetworkInterface, int, float>(OnSyncDamage));
		}
	}

	protected override void OnPEDestroy()
	{
		base.OnPEDestroy();
		SkEntityMgr.Remove(base.Id);
		if (ServerConfig.IsCustom)
		{
			this.DeathEventHandler = (Action<SkNetworkInterface>)Delegate.Remove(this.DeathEventHandler, new Action<SkNetworkInterface>(OnSyncDeath));
			this.DamageEventHandler = (Action<SkNetworkInterface, int, float>)Delegate.Remove(this.DamageEventHandler, new Action<SkNetworkInterface, int, float>(OnSyncDamage));
		}
	}

	public void AddSkEntity()
	{
		_skEntity = base.gameObject.GetComponent<SkEntity>();
		if (_skEntity == null)
		{
			SkEntity skEntity = (_skEntity = base.gameObject.AddComponent<SkEntity>());
			SkCreater();
			skEntity.Init(this);
			SkEntityMgr.Add(base.Id, skEntity);
		}
	}

	public void CheckAuth()
	{
		if (authId == -1)
		{
			return;
		}
		ObjNetInterface objNetInterface = ObjNetInterface.Get(authId);
		if (null != objNetInterface)
		{
			Vector3 position = objNetInterface.transform.position;
			if (Mathf.Abs(position.x - base.transform.position.x) <= 128f && Mathf.Abs(position.y - base.transform.position.y) <= 128f && Mathf.Abs(position.z - base.transform.position.y) <= 128f)
			{
				return;
			}
		}
		authId = -1;
	}

	private void OnDeathEvent()
	{
		if (this.DeathEventHandler != null)
		{
			this.DeathEventHandler(this);
		}
	}

	private void OnSyncDeath(SkNetworkInterface net)
	{
		int customId = SPTerrainEvent.GetCustomId(net.Id);
		ChannelNetwork.SyncChannel(net.WorldId, EPacketType.PT_CustomEvent_Death, net.Id, customId);
	}

	private void OnDamageEvent(int casterId, float damage)
	{
		if (this.DamageEventHandler != null)
		{
			this.DamageEventHandler(this, casterId, damage);
		}
	}

	private void OnSyncDamage(SkNetworkInterface net, int casterId, float damage)
	{
		int customId = SPTerrainEvent.GetCustomId(net.Id);
		int customId2 = SPTerrainEvent.GetCustomId(casterId);
		ChannelNetwork.SyncChannel(net.WorldId, EPacketType.PT_CustomEvent_Damage, net.Id, customId, casterId, customId2, damage);
	}

	protected void RPC_C2S_AbnormalConditionStart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int num3 = stream.Read<int>(new object[0]);
		ObjNetInterface objNetInterface = ObjNetInterface.Get<ObjNetInterface>(num);
		if (!(null == objNetInterface) && objNetInterface.AbnormalModule != null)
		{
			if (num3 == 1)
			{
				byte[] array = stream.Read<byte[]>(new object[0]);
				objNetInterface.AbnormalModule.ApplyAbnormalCondition(num2, array);
				RPCOthers(EPacketType.PT_InGame_AbnormalConditionStart, num, num2, num3, array);
			}
			else
			{
				objNetInterface.AbnormalModule.ApplyAbnormalCondition(num2, null);
				RPCOthers(EPacketType.PT_InGame_AbnormalConditionStart, num, num2, num3);
			}
		}
	}

	protected void RPC_C2S_AbnormalConditionEnd(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		ObjNetInterface objNetInterface = ObjNetInterface.Get<ObjNetInterface>(num);
		if (!(null == objNetInterface) && objNetInterface.AbnormalModule != null)
		{
			objNetInterface.AbnormalModule.EndAbnormalCondition(num2);
			RPCOthers(EPacketType.PT_InGame_AbnormalConditionEnd, num, num2);
		}
	}

	protected void RPC_C2S_AbnormalCondition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (base.AbnormalModule != null)
		{
			RPCPeer(info.sender, EPacketType.PT_InGame_AbnormalCondition, base.AbnormalModule.ExportData());
		}
	}

	protected void RPC_C2S_Jump(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RPCOthers(EPacketType.PT_InGame_Jump, stream.Read<double>(new object[0]));
	}

	public void SetAllAttribute(AttribType type, float value)
	{
		_skEntity.SetAllAttribute(type, value);
	}

	public void SetAttribute(AttribType type, float value)
	{
		SetAttribute(type, value, isBase: false);
	}

	public void SetAttribute(AttribType type, float value, bool isBase)
	{
		SetAttribute(type, value, isBase, -1);
	}

	public void SetAttribute(AttribType type, float value, bool isBase, int casterId)
	{
		_skEntity.SetAttribute(type, value, isBase: true);
		RPCOthers(EPacketType.PT_InGame_SKSyncAttr, (byte)type, value, true, casterId);
	}

	public void ChangeAttribute(AttribType attr, float plusValue)
	{
		float num = _skEntity.GetAttribute(attr, isBase: true) + plusValue;
		_skEntity.SetAttribute(attr, num, isBase: true);
		RPCOthers(EPacketType.PT_InGame_SKSyncAttr, (byte)attr, num, true, -1);
	}

	private bool CheckIgnoreDamage(AttribType indexData, bool bRaw)
	{
		if (indexData == AttribType.Hp && _skEntity.GetAttribute(AttribType.DebuffReduce10, bRaw) > Mathf.Epsilon && this is Player)
		{
			return true;
		}
		return false;
	}

	private void RPC_SKSyncAttr(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<byte>(new object[0]);
		float num2 = stream.Read<float>(new object[0]);
		int num3 = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		bool flag2 = stream.Read<bool>(new object[0]);
		if (!(_skEntity == null) && !CheckIgnoreDamage((AttribType)num, flag))
		{
			_skEntity.CheckAttrEvent((AttribType)num, _skEntity.GetAttribute((AttribType)num, flag), num2, flag, flag2);
			if (flag2)
			{
				RPCOthers(EPacketType.PT_InGame_SKSyncAttr, (byte)num, _skEntity.GetAttribute((AttribType)num, flag), flag, num3);
			}
			else
			{
				RPCProxy(info.sender, EPacketType.PT_InGame_SKSyncAttr, (byte)num, _skEntity.GetAttribute((AttribType)num, flag), flag, num3);
			}
			if (!flag)
			{
				OnAttrChange((AttribType)num, _skEntity.GetAttribute((AttribType)num), num3, num2);
			}
		}
	}

	private void RPC_SKStartSkill(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		OnStartSkill(num, num2);
		if (flag)
		{
			float[] array = stream.Read<float[]>(new object[0]);
			RPCProxy(info.sender, EPacketType.PT_InGame_SKStartSkill, num, num2, flag, array);
		}
		else
		{
			RPCProxy(info.sender, EPacketType.PT_InGame_SKStartSkill, num, num2, flag);
		}
	}

	private void RPC_SKStopSkill(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKStopSkill, num);
	}

	private void RPC_SKBLoop(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKBLoop, num, num2, flag);
	}

	private void RPC_SKSyncAttrs(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] array = _skEntity.Export();
		if (_skEntity != null && array != null)
		{
			RPCPeer(info.sender, EPacketType.PT_InGame_SKSyncInitAttrs, array);
		}
	}

	private void RPC_SKSendBaseAttrs(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] data = stream.Read<byte[]>(new object[0]);
		if (_skEntity != null && _skEntity._baseAttribs == null)
		{
			_skEntity.CreateBaseAttr(data);
			_skEntity.Import(data);
		}
	}

	private void RPC_SKFellTree(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int typeIndex = stream.Read<int>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		float height = stream.Read<float>(new object[0]);
		float width = stream.Read<float>(new object[0]);
		GameWorld.GetGameWorld(base.WorldId)?.FellTree(_skEntity, pos, typeIndex, height, width);
	}

	private void RPC_C2S_ClearGrass(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] array = stream.Read<byte[]>(new object[0]);
		GameWorld gameWorld = GameWorld.GetGameWorld(base.WorldId);
		if (gameWorld != null)
		{
			gameWorld.DeleteGrass(array);
			RPCOthers(EPacketType.PT_InGame_ClearGrass, array);
		}
	}

	private void RPC_C2S_ClearTree(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] array = stream.Read<byte[]>(new object[0]);
		GameWorld gameWorld = GameWorld.GetGameWorld(base.WorldId);
		if (gameWorld != null)
		{
			gameWorld.DeleteTree(array);
			RPCOthers(EPacketType.PT_InGame_ClearTree, array);
		}
	}

	private void RPC_SKDigTerrain(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		IntVector3 intPos = stream.Read<IntVector3>(new object[0]);
		float durDec = stream.Read<float>(new object[0]);
		float radius = stream.Read<float>(new object[0]);
		float resourceBonus = stream.Read<float>(new object[0]);
		byte[] data = stream.Read<byte[]>(new object[0]);
		bool bReturnItem = stream.Read<bool>(new object[0]);
		bool bGetSpItems = stream.Read<bool>(new object[0]);
		float height = stream.Read<float>(new object[0]);
		GameWorld.GetGameWorld(base.WorldId)?.DigTerrain(this, intPos, durDec, radius, resourceBonus, data, bReturnItem, bGetSpItems, height);
	}

	private void RPC_SKChangeTerrain(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		IntVector3 intPos = stream.Read<IntVector3>(new object[0]);
		float radius = stream.Read<float>(new object[0]);
		byte targetType = stream.Read<byte>(new object[0]);
		byte[] array = stream.Read<byte[]>(new object[0]);
		GameWorld world = GameWorld.GetGameWorld(base.WorldId);
		if (world == null)
		{
			return;
		}
		Serialize.Import(array, delegate(BinaryReader _out)
		{
			_out.ReadInt32();
			for (float num = 0f - radius; num <= radius; num += 1f)
			{
				for (float num2 = 0f - radius; num2 <= radius; num2 += 1f)
				{
					for (float num3 = 0f - radius; num3 <= radius; num3 += 1f)
					{
						IntVector3 pos = new IntVector3((float)intPos.x + num, (float)intPos.y + num3, (float)intPos.z + num2);
						BufferHelper.ReadVFVoxel(_out, out var _value);
						world.ChangeTerrain(pos, targetType, _value);
					}
				}
			}
		});
		ChannelNetwork.SyncChannel(base.WorldId, EPacketType.PT_InGame_SKChangeTerrain, intPos, radius, targetType, array);
	}

	public void RPC_C2S_BlockDestroyInRange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		float power = stream.Read<float>(new object[0]);
		float range = stream.Read<float>(new object[0]);
		float minBrush = stream.Read<float>(new object[0]);
		GameWorld gameWorld = GameWorld.GetGameWorld(base.WorldId);
		if (gameWorld != null)
		{
			Dictionary<IntVector3, BSVoxel> blocks = gameWorld.ChangeBlockDataInRange(pos, power, range, minBrush);
			byte[] array = GameWorld.GenBlockData(blocks);
			ChannelNetwork.SyncChannel(base.WorldId, EPacketType.PT_InGame_SkillBlockRange, array);
		}
	}

	public void RPC_C2S_TerrainDestroyInRange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		float power = stream.Read<float>(new object[0]);
		float radius = stream.Read<float>(new object[0]);
		byte[] buff = stream.Read<byte[]>(new object[0]);
		GameWorld world = GameWorld.GetGameWorld(base.WorldId);
		if (world == null)
		{
			return;
		}
		IntVector3 basePos = new IntVector3(pos - radius * Vector3.one);
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			for (int i = 0; (float)i < 2f * radius; i++)
			{
				for (int j = 0; (float)j < 2f * radius; j++)
				{
					for (int k = 0; (float)k < 2f * radius; k++)
					{
						IntVector3 intVector = new IntVector3(basePos.x + i, basePos.y + j, basePos.z + k);
						if (Vector3.Distance(intVector, pos) <= radius)
						{
							byte type = BufferHelper.ReadByte(r);
							byte volume = BufferHelper.ReadByte(r);
							world.ChangeVoxelDataInRange(intVector, pos, type, volume, power, radius);
						}
					}
				}
			}
		});
		ChannelNetwork.SyncChannel(base.WorldId, EPacketType.PT_InGame_SkillVoxelRange, pos, power, radius);
	}

	protected virtual void RPC_C2S_LostController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	protected virtual void RPC_C2S_SetController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
	}

	protected override void OnPlayerDisconnect(Player player)
	{
		if (authId != -1 && !(null == player) && authId == player.Id && !_bDeath)
		{
			authId = -1;
			RPCProxy(EPacketType.PT_InGame_LostController);
		}
	}

	private void RPC_SKDAVVF(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		Vector3 vector2 = stream.Read<Vector3>(new object[0]);
		float num = stream.Read<float>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAVVF, pEActionType, vector, vector2, num);
	}

	private void RPC_SKDAVFNS(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		float num = stream.Read<float>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAVFNS, pEActionType, vector, num, num2, text);
	}

	private void RPC_SKDANO(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDANO, pEActionType);
	}

	private void RPC_SKDAV(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAV, pEActionType, vector);
	}

	private void RPC_SKDAVVN(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		Vector3 vector2 = stream.Read<Vector3>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAVVN, pEActionType, vector, vector2, num);
	}

	private void RPC_SKDAVQNS(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAVQNS, pEActionType, vector, quaternion, num, text);
	}

	private void RPC_SKDAVQ(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAVQ, pEActionType, vector, quaternion);
	}

	private void RPC_SKDAS(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAS, pEActionType, text);
	}

	private void RPC_SKDAVQS(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAVQS, pEActionType, vector, quaternion, text);
	}

	private void RPC_SKDAVQSN(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAVQSN, pEActionType, vector, quaternion, text, num);
	}

	private void RPC_SKDAVVNN(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		Vector3 vector2 = stream.Read<Vector3>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAVVNN, pEActionType, vector, vector2, num, num2);
	}

	private void RPC_SKDAN(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAN, pEActionType, num);
	}

	private void RPC_SKDAB(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		bool flag = stream.Read<bool>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAB, pEActionType, flag);
	}

	private void RPC_SKDAVQN(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAVQN, pEActionType, vector, quaternion, num);
	}

	private void RPC_SKDAVFVFS(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		float num = stream.Read<float>(new object[0]);
		Vector3 vector2 = stream.Read<Vector3>(new object[0]);
		float num2 = stream.Read<float>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAVFVFS, pEActionType, vector, num, vector2, num2, text);
	}

	private void RPC_SKDAEndAction(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		CancelEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAEndAction, pEActionType);
	}

	private void RPC_SKDAEndImmediately(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		CancelEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAEndImmediately, pEActionType);
	}

	private void RPC_SKDAQueryEntityState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		RPCPeer(info.sender, EPacketType.PT_InGame_SKDAQueryEntityState, _eEState);
	}

	private void RPC_SKDAVQSNS(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		PEActionType pEActionType = stream.Read<PEActionType>(new object[0]);
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		Quaternion quaternion = stream.Read<Quaternion>(new object[0]);
		string text = stream.Read<string>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		string text2 = stream.Read<string>(new object[0]);
		SetEEState(pEActionType);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKDAVQSNS, pEActionType, vector, quaternion, text, num, text2);
	}

	private void RPC_SKIKPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 vector = stream.Read<Vector3>(new object[0]);
		RPCProxy(info.sender, EPacketType.PT_InGame_SKIKPos, vector);
	}

	internal virtual void OnStartSkill(int skId, int targetId)
	{
	}

	internal virtual void OnDeath(int casterId)
	{
		OnDeathEvent();
		SkEntity skEntity = SkEntityMgr.GetSkEntity(casterId);
		if (skEntity != null && this is Player)
		{
			if (((SkNetworkInterface)skEntity._net).Operator == OperatorEnum.Oper_Player)
			{
				ActionEventsMgr._self.ProcessAction(((SkNetworkInterface)skEntity._net).Operator, ActionOpportunity.Opp_OnDeath, this, casterId);
				((SkNetworkInterface)skEntity._net).OnKill(base.Id);
			}
			else
			{
				ActionEventsMgr._self.ProcessAction(((SkNetworkInterface)skEntity._net).Operator, ActionOpportunity.Opp_OnDeath, this, casterId);
			}
		}
		else if (skEntity != null && this is AiMonsterNetwork)
		{
			if (((SkNetworkInterface)skEntity._net).Operator == OperatorEnum.Oper_Player)
			{
				ActionEventsMgr._self.ProcessAction(((SkNetworkInterface)skEntity._net).Operator, ActionOpportunity.Opp_OnDeath, this, casterId);
				((SkNetworkInterface)skEntity._net).OnKill(base.Id);
			}
			else
			{
				ActionEventsMgr._self.ProcessAction(((SkNetworkInterface)skEntity._net).Operator, ActionOpportunity.Opp_OnDeath, this, casterId);
			}
		}
	}

	internal virtual void OnDamage(int casterId, float damage = 0f)
	{
		OnDamageEvent(casterId, damage);
		SkEntity skEntity = SkEntityMgr.GetSkEntity(casterId);
		if (skEntity != null && this is Player)
		{
			if (((SkNetworkInterface)skEntity._net).Operator == OperatorEnum.Oper_Player)
			{
				ActionEventsMgr._self.ProcessAction(((SkNetworkInterface)skEntity._net).Operator, ActionOpportunity.Opp_OnChangeHP, this, casterId);
			}
			else
			{
				ActionEventsMgr._self.ProcessAction(((SkNetworkInterface)skEntity._net).Operator, ActionOpportunity.Opp_OnChangeHP, this, casterId);
			}
		}
		else if (skEntity != null && this is AiMonsterNetwork)
		{
			if (((SkNetworkInterface)skEntity._net).Operator == OperatorEnum.Oper_Player)
			{
				ActionEventsMgr._self.ProcessAction(((SkNetworkInterface)skEntity._net).Operator, ActionOpportunity.Opp_OnChangeHP, this, casterId);
			}
			else
			{
				ActionEventsMgr._self.ProcessAction(((SkNetworkInterface)skEntity._net).Operator, ActionOpportunity.Opp_OnChangeHP, this, casterId);
			}
		}
	}

	internal virtual void OnPEACCheckEvent(SkNetworkInterface skNet, int casterId)
	{
		if (SkNetworkInterface.OnPEACCheckEventHandler != null)
		{
			SkNetworkInterface.OnPEACCheckEventHandler(skNet, casterId);
		}
	}

	internal virtual void OnKill(int targetId)
	{
		SkEntity skEntity = SkEntityMgr.GetSkEntity(targetId);
		if (skEntity != null && this is Player)
		{
			if (((SkNetworkInterface)skEntity._net).Operator == OperatorEnum.Oper_Player)
			{
				ActionEventsMgr._self.ProcessAction(((SkNetworkInterface)skEntity._net).Operator, ActionOpportunity.Opp_OnKillSb, this, targetId);
			}
			else
			{
				ActionEventsMgr._self.ProcessAction(((SkNetworkInterface)skEntity._net).Operator, ActionOpportunity.Opp_OnKillSb, this, targetId);
			}
		}
		else if (skEntity != null && this is AiMonsterNetwork)
		{
			if (((SkNetworkInterface)skEntity._net).Operator == OperatorEnum.Oper_Player)
			{
				ActionEventsMgr._self.ProcessAction(((SkNetworkInterface)skEntity._net).Operator, ActionOpportunity.Opp_OnKillSb, this, targetId);
			}
			else
			{
				ActionEventsMgr._self.ProcessAction(((SkNetworkInterface)skEntity._net).Operator, ActionOpportunity.Opp_OnKillSb, this, targetId);
			}
		}
	}

	private void OnAttrChange(AttribType type, float value, int casterId, float dValue)
	{
		switch (type)
		{
		case AttribType.Hp:
			OnDamage(casterId, dValue);
			if (value < Mathf.Epsilon && !_bDeath)
			{
				_bDeath = true;
				OnDeath(casterId);
			}
			OnPEACCheckEvent(this, casterId);
			break;
		case AttribType.Atk:
			OnPEACCheckEvent(this, casterId);
			break;
		case AttribType.Def:
			OnPEACCheckEvent(this, casterId);
			break;
		}
	}

	public virtual void WeaponReload(int objId, int oldProtoId, int newProtoId, float magazineSize)
	{
	}

	public virtual void WeaponReload(Player player, int objId, int oldProtoId, int newProtoId, float magazineSize)
	{
	}

	public virtual bool GunEnergyReload(ItemObject item, float num)
	{
		if (item == null)
		{
			return false;
		}
		return false;
	}

	public virtual bool BatteryEnergyReload(ItemObject item, float num)
	{
		if (item == null)
		{
			return false;
		}
		return false;
	}

	public virtual void JetPackEnergyReload(ItemObject item, float num)
	{
		if (item != null)
		{
		}
	}

	public virtual void ItemAttrChange(int itemObjId, float num)
	{
	}

	public virtual void EquipItemCost(int itemObjId, float num)
	{
	}

	public virtual void PackageItemCost(int itemObjId, float num)
	{
	}

	public virtual void WeaponDurabilityChange(ItemObject item)
	{
	}

	public virtual float ArmorDurabilityChange(int itemObjId, float damage)
	{
		return 0f;
	}

	virtual int ISceneObject.get_Id()
	{
		return base.Id;
	}

	virtual int ISceneObject.get_WorldId()
	{
		return base.WorldId;
	}
}
