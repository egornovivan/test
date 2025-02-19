using System;
using System.Collections.Generic;
using System.IO;
using Pathea;
using PETools;
using UnityEngine;

namespace SkillSystem;

public class SkEntity : MonoBehaviour, ISkEntity
{
	private Dictionary<int, int> specialHatredData;

	internal SkAttribs _attribs;

	internal List<SkInst> _instsActFromExt = new List<SkInst>();

	public float _lastestTimeOfHurtingSb;

	public float _lastestTimeOfGettingHurt;

	public float _lastestTimeOfConsumingStamina;

	internal NetworkInterface _net;

	private SKCanLoop _skCanLoop = new SKCanLoop();

	internal SkBeModified _beModified = new SkBeModified();

	public ISkAttribs attribs => _attribs;

	public Dictionary<int, int> SpecialHatredData
	{
		get
		{
			if (specialHatredData == null)
			{
				specialHatredData = new Dictionary<int, int>();
			}
			return specialHatredData;
		}
		set
		{
			specialHatredData = value;
		}
	}

	public PackBase BuffAttribs
	{
		get
		{
			return _attribs.pack;
		}
		set
		{
			_attribs.pack = value;
		}
	}

	public PackBase Pack
	{
		get
		{
			return _attribs.pack;
		}
		set
		{
			_attribs.pack = value;
		}
	}

	internal void UpdateAttribs()
	{
		_beModified.Clear();
		_attribs.Update(_beModified);
	}

	public void Init(Action<int, float, float> onAlterAttribs, Action<List<ItemToPack>> onPutinPackage, SkEntity parent, bool[] useParentMasks)
	{
		_attribs = new SkAttribs(this, parent._attribs, useParentMasks);
		PeSingleton<SkEntityAttribsUpdater>.Instance.Register(this);
		if (onAlterAttribs != null)
		{
			_attribs._OnAlterNumAttribs += onAlterAttribs;
		}
		if (onPutinPackage != null)
		{
			_attribs._OnPutInPakAttribs += onPutinPackage;
		}
		_attribs._OnAlterNumAttribs += OnNetAlterAttribs;
	}

	public void Init(Action<int, float, float> onAlterAttribs, Action<List<ItemToPack>> onPutinPackage, int nAttribs = 1)
	{
		_attribs = new SkAttribs(this, nAttribs);
		PeSingleton<SkEntityAttribsUpdater>.Instance.Register(this);
		if (onAlterAttribs != null)
		{
			_attribs._OnAlterNumAttribs += onAlterAttribs;
		}
		if (onPutinPackage != null)
		{
			_attribs._OnPutInPakAttribs += onPutinPackage;
		}
		_attribs._OnAlterNumAttribs += OnNetAlterAttribs;
	}

	public void Init(SkAttribs attribs, Action<int, float, float> onAlterAttribs, Action<List<ItemToPack>> onPutinPackage)
	{
		_attribs = attribs;
		PeSingleton<SkEntityAttribsUpdater>.Instance.Register(this);
		if (onAlterAttribs != null)
		{
			_attribs._OnAlterNumAttribs += onAlterAttribs;
		}
		if (onPutinPackage != null)
		{
			_attribs._OnPutInPakAttribs += onPutinPackage;
		}
		_attribs._OnAlterNumAttribs += OnNetAlterAttribs;
	}

	public void CancelBuffById(int id)
	{
		_attribs.pack -= id;
	}

	public void CancelSkillById(int id)
	{
		SkInst.StopSkill((SkInst inst) => inst.Caster == this && inst.SkillID == id);
		_skCanLoop.Reset();
		if (IsController())
		{
			_net.RPCServer(EPacketType.PT_InGame_SKStopSkill, id, PlayerNetwork.mainPlayerId);
		}
	}

	public void CancelAllSkills()
	{
		SkInst.StopSkill((SkInst inst) => inst.Caster == this);
		_skCanLoop.Reset();
		if (IsController())
		{
			_net.RPCServer(EPacketType.PT_InGame_SKStopSkill, -1, PlayerNetwork.mainPlayerId);
		}
	}

	public void CancelSkill(SkInst inst)
	{
		inst.Stop();
		_skCanLoop.Reset();
		if (IsController())
		{
			_net.RPCServer(EPacketType.PT_InGame_SKStopSkill, inst.SkillID, PlayerNetwork.mainPlayerId);
		}
	}

	public virtual SkInst StartSkill(SkEntity target, int id, ISkPara para = null, bool bStartImm = true)
	{
		SkInst skInst = SkInst.StartSkill(this, target, id, para, bStartImm);
		if (skInst != null && IsController())
		{
			if (para is ISkParaNet)
			{
				SendStartSkill(target, id, ((ISkParaNet)para).ToFloatArray());
			}
			else if (para == null)
			{
				SendStartSkill(target, id);
			}
			else
			{
				Debug.LogError("error skill para");
			}
		}
		return skInst;
	}

	public SkInst GetSkInst(int id)
	{
		return SkInst.GetSkill((SkInst inst) => inst.Caster == this && inst.SkillID == id);
	}

	public SkBuffInst GetSkBuffInst(int id)
	{
		if (_attribs.pack is SkPackage skPackage)
		{
			for (int i = 0; i < skPackage._buffs.Count; i++)
			{
				if (skPackage._buffs[i]._buff._id == id)
				{
					return skPackage._buffs[i];
				}
			}
		}
		return null;
	}

	public bool IsSkillRunning(int id, bool cdInclude = true)
	{
		return null != SkInst.GetSkill((SkInst inst) => inst.Caster == this && inst.SkillID == id && (cdInclude || inst.IsActive));
	}

	public bool IsSkillRunning(bool cdInclude = true)
	{
		return null != SkInst.GetSkill((SkInst inst) => inst.Caster == this && (cdInclude || inst.IsActive));
	}

	public bool IsSkillRunnable(int id)
	{
		return SkInst.IsSkillRunnable(this, id);
	}

	public float GetAttribute(int type, bool bSum = true)
	{
		return (!bSum) ? _attribs.raws[type] : _attribs.sums[type];
	}

	public void SetAttribute(int type, float value, bool eventOff = true, bool bBoth = true)
	{
		if (eventOff)
		{
			_attribs.DisableNumAttribsEvent();
		}
		if (bBoth)
		{
			_attribs.raws[type] = value;
		}
		_attribs.sums[type] = value;
		if (eventOff)
		{
			_attribs.EnableNumAttribsEvent();
		}
	}

	public void SetAttribute(int type, float value, bool eventOff, bool bRaw, int caster)
	{
		if (eventOff)
		{
			_attribs.DisableNumAttribsEvent();
		}
		_attribs.SetNetCaster(caster);
		if (bRaw)
		{
			_attribs.raws[type] = value;
		}
		else
		{
			_attribs.sums[type] = value;
		}
		_attribs.SetNetCaster(0);
		if (eventOff)
		{
			_attribs.EnableNumAttribsEvent();
		}
	}

	public SkEntity GetCasterToModAttrib(int idx)
	{
		return _attribs.GetModCaster(idx);
	}

	public SkEntity GetNetCasterToModAttrib(int idx)
	{
		return _attribs.GetNetModCaster(idx);
	}

	public void RegisterInstFromExt(SkInst inst)
	{
		_instsActFromExt.Add(inst);
	}

	public void UnRegisterInstFromExt(SkInst inst)
	{
		_instsActFromExt.Remove(inst);
	}

	public void CollisionCheck(Collider selfCol, Collider otherCol)
	{
		PECapsuleHitResult pECapsuleHitResult = new PECapsuleHitResult();
		pECapsuleHitResult.selfTrans = selfCol.transform;
		pECapsuleHitResult.hitTrans = otherCol.transform;
		int count = _instsActFromExt.Count;
		for (int i = 0; i < count; i++)
		{
			_instsActFromExt[i].ExecEventsFromCol(pECapsuleHitResult);
		}
	}

	public void CollisionCheck(PECapsuleHitResult colInfo)
	{
		int count = _instsActFromExt.Count;
		for (int i = 0; i < count; i++)
		{
			_instsActFromExt[i].ExecEventsFromCol(colInfo);
		}
	}

	public static SkBuffInst MountBuff(SkEntity target, int buffId, List<int> idxList, List<float> valList)
	{
		int num = 0;
		int num2 = idxList?.Count ?? 0;
		for (int i = 0; i < num2; i++)
		{
			if (idxList[i] > num)
			{
				num = idxList[i];
			}
		}
		SkAttribs skAttribs = new SkAttribs(null, num + 1);
		for (int j = 0; j < num2; j++)
		{
			int index = idxList[j];
			IList<float> sums = skAttribs.sums;
			float value = valList[j];
			skAttribs.raws[index] = value;
			sums[index] = value;
		}
		PeEntity component = target.GetComponent<PeEntity>();
		if (null != component && component.IsMainPlayer)
		{
			InGameAidData.CheckInBuff(buffId);
		}
		return SkBuffInst.Mount(target._attribs.pack as SkPackage, SkBuffInst.Create(buffId, skAttribs, target._attribs));
	}

	public static void UnmountBuff(SkEntity target, int buffId)
	{
		SkBuffInst.Unmount(target._attribs.pack as SkPackage, (SkBuffInst it) => it._buff._id == buffId);
	}

	public static void UnmountBuff(SkEntity target, SkBuffInst inst)
	{
		SkBuffInst.Unmount(target._attribs.pack as SkPackage, inst);
	}

	public virtual void ApplySe(int seId, SkRuntimeInfo info)
	{
	}

	public virtual void ApplyAnim(string anim, SkRuntimeInfo info)
	{
	}

	public virtual void ApplyEmission(int emitId, SkRuntimeInfo info)
	{
	}

	public virtual void ApplyEff(int effId, SkRuntimeInfo info)
	{
	}

	public virtual void ApplyCamEff(int effId, SkRuntimeInfo info)
	{
	}

	public virtual void ApplyRepelEff(SkRuntimeInfo info)
	{
	}

	public virtual void CondTstFunc(SkFuncInOutPara funcInOut)
	{
	}

	public virtual Transform GetTransform()
	{
		return base.transform;
	}

	public virtual Collider GetCollider(string name)
	{
		return null;
	}

	public virtual void OnHurtSb(SkInst inst, float dmg)
	{
	}

	public virtual void OnGetHurt(SkInst inst, float dmg)
	{
	}

	public virtual void GetCollisionInfo(out List<KeyValuePair<Collider, Collider>> colPairs)
	{
		colPairs = null;
	}

	public virtual void OnBuffAdd(int buffId)
	{
	}

	public virtual void OnBuffRemove(int buffId)
	{
	}

	public void SetNet(NetworkInterface rpcnet, bool isSwitch = true)
	{
		_net = rpcnet;
		if (isSwitch && _attribs != null && _net is SkNetworkInterface && !((SkNetworkInterface)_net).IsStaticNet())
		{
			_attribs.SwitchSeterToNet((SkNetworkInterface)_net);
		}
	}

	public int GetId()
	{
		if (_net != null)
		{
			return _net.Id;
		}
		return 0;
	}

	public bool IsController()
	{
		if (_net != null)
		{
			if (_net is SubTerrainNetwork || _net is VoxelTerrainNetwork)
			{
				return true;
			}
			return _net.hasOwnerAuth;
		}
		return false;
	}

	public bool IsStaticNet()
	{
		if (_net != null && ((SkNetworkInterface)_net).IsStaticNet())
		{
			return true;
		}
		return false;
	}

	public void Kill(bool eventOff, bool bBoth = true)
	{
		SetAttribute(1, 0f, eventOff, bBoth);
	}

	public void SendBLoop(int skId, int targetId, bool bLoop)
	{
		if (IsController())
		{
			_net.RPCServer(EPacketType.PT_InGame_SKBLoop, skId, targetId, bLoop);
		}
	}

	public void SetBLoop(bool bLoop, int skId)
	{
		_skCanLoop._bLoop = bLoop;
		_skCanLoop._skillId = skId;
		_skCanLoop._casterId = GetId();
		_skCanLoop._bFailedRecv = true;
	}

	public void SetCondRet(SkFuncInOutPara funcInOut)
	{
		if (_skCanLoop._bFailedRecv && _skCanLoop._casterId == funcInOut._inst._caster.GetId() && _skCanLoop._skillId == funcInOut._inst.SkillID)
		{
			funcInOut._ret = _skCanLoop._bLoop;
			_skCanLoop.Reset();
		}
	}

	private void OnNetAlterAttribs(int idx, float oldValue, float newValue)
	{
	}

	public void SendFellTree(int proType, Vector3 pos, float height, float width)
	{
		if (IsController())
		{
			_net.RPCServer(EPacketType.PT_InGame_SKFellTree, proType, pos, height, width);
		}
	}

	public void SendStartSkill(SkEntity target, int id, float[] para = null)
	{
		_skCanLoop.Reset();
		if (!IsController())
		{
			return;
		}
		if (para != null && para.Length > 0)
		{
			if (target != null && target.GetId() != 0)
			{
				_net.RPCServer(EPacketType.PT_InGame_SKStartSkill, id, target.GetId(), true, para);
			}
			else
			{
				_net.RPCServer(EPacketType.PT_InGame_SKStartSkill, id, 0, true, para);
			}
		}
		else if (target != null && target.GetId() != 0)
		{
			_net.RPCServer(EPacketType.PT_InGame_SKStartSkill, id, target.GetId(), false);
		}
		else
		{
			_net.RPCServer(EPacketType.PT_InGame_SKStartSkill, id, 0, false);
		}
	}

	public void Import(byte[] data)
	{
		if (data == null || data.Length <= 0)
		{
			return;
		}
		using MemoryStream memoryStream = new MemoryStream(data);
		using (BinaryReader binaryReader = new BinaryReader(memoryStream))
		{
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int type = binaryReader.ReadInt32();
				float value = binaryReader.ReadSingle();
				SetAttribute(type, value, eventOff: true, bRaw: true, 0);
				type = binaryReader.ReadInt32();
				value = binaryReader.ReadSingle();
				SetAttribute(type, value, eventOff: true, bRaw: false, 0);
			}
			binaryReader.Close();
		}
		memoryStream.Close();
	}

	public byte[] Export()
	{
		byte[] array = null;
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		int num = 0;
		for (int i = 0; i < attribs.raws.Count; i++)
		{
			if (!(GetAttribute(i, bSum: false) <= float.Epsilon))
			{
				num++;
			}
		}
		binaryWriter.Write(num);
		for (int j = 0; j < attribs.raws.Count; j++)
		{
			if (!(GetAttribute(j, bSum: false) <= float.Epsilon) && !(GetAttribute(j) <= float.Epsilon))
			{
				binaryWriter.Write(j);
				binaryWriter.Write(GetAttribute(j, bSum: false));
				binaryWriter.Write(j);
				binaryWriter.Write(GetAttribute(j));
			}
		}
		binaryWriter.Close();
		array = memoryStream.ToArray();
		memoryStream.Close();
		return array;
	}

	public static explicit operator SkEntity(Collider col)
	{
		return PEUtil.GetComponent<SkEntity>(col.gameObject);
	}

	public static explicit operator SkEntity(Transform t)
	{
		return PEUtil.GetComponent<SkEntity>(t.gameObject);
	}
}
