using System;
using System.Collections.Generic;
using System.IO;
using Pathea;

namespace SkillSystem;

public class SkAttribs : ISkAttribs
{
	public const int MinAttribsCnt = 1;

	private ISkEntity _entity;

	private IExpFuncSet _expFunc;

	private IList<float> _raws;

	private IList<float> _sums;

	private ModFlags _dirties;

	private SkPackage _pack;

	private int _nAttribs;

	private bool _numAttribsEventEnable = true;

	private bool _pakAttribsEventEnable = true;

	private static List<AttribType> _sendDValue = new List<AttribType>();

	private SkNetworkInterface _net;

	private bool _fromNet;

	private bool _lockModifyBySingle = true;

	private int _netCaster;

	public ISkEntity entity => _entity;

	public IExpFuncSet expFunc => _expFunc;

	public IList<float> raws => _raws;

	public IList<float> sums => _sums;

	public IList<bool> modflags => _dirties;

	public PackBase pack
	{
		get
		{
			return _pack;
		}
		set
		{
			_pack = (SkPackage)value;
		}
	}

	public float buffMul { get; set; }

	public float buffPreAdd { get; set; }

	public float buffPostAdd { get; set; }

	public bool FromNet
	{
		get
		{
			return _fromNet;
		}
		set
		{
			_fromNet = value;
		}
	}

	public bool LockModifyBySingle
	{
		get
		{
			return _lockModifyBySingle;
		}
		set
		{
			_lockModifyBySingle = value;
		}
	}

	public event Action<int, float, float> _OnAlterNumAttribs;

	public event Action<List<ItemToPack>> _OnPutInPakAttribs;

	public SkAttribs(ISkEntity ent, int nAttribs)
	{
		_entity = ent;
		_expFunc = new ExpFuncSet(this);
		_nAttribs = nAttribs;
		_dirties = new ModFlags(nAttribs);
		_raws = new NumList(nAttribs, delegate(NumList n, int i, float v)
		{
			RawSetter(i, v);
		});
		_sums = new NumList(nAttribs, delegate(NumList n, int i, float v)
		{
			SumSetter(i, v);
		});
		_pack = new SkPackage(this);
		AddToSendDValue(AttribType.Hp);
	}

	public SkAttribs(ISkEntity ent, SkAttribs baseAttribs, bool[] masks)
	{
		_entity = ent;
		_expFunc = new ExpFuncSet(this);
		int num = (_nAttribs = baseAttribs._raws.Count);
		_dirties = new ModFlags(num);
		_raws = new NumListWithParent(baseAttribs._raws as NumList, masks, num, delegate(NumList n, int i, float v)
		{
			RawSetter(i, v);
		});
		_sums = new NumListWithParent(baseAttribs._sums as NumList, masks, num, delegate(NumList n, int i, float v)
		{
			SumSetter(i, v);
		});
		_pack = new SkPackage(this);
		AddToSendDValue(AttribType.Hp);
	}

	public SkAttribs(ISkEntity ent, List<int> idxList, List<float> valList)
	{
		_entity = ent;
		_expFunc = new ExpFuncSet(this);
		int num = 0;
		int count = idxList.Count;
		for (int j = 0; j < count; j++)
		{
			if (idxList[j] > num)
			{
				num = idxList[j];
			}
		}
		_nAttribs = num + 1;
		_dirties = new ModFlags(_nAttribs);
		_raws = new NumList(_nAttribs, delegate(NumList n, int i, float v)
		{
			RawSetter(i, v);
		});
		_sums = new NumList(_nAttribs, delegate(NumList n, int i, float v)
		{
			SumSetter(i, v);
		});
		_pack = new SkPackage(this);
		for (int k = 0; k < count; k++)
		{
			int index = idxList[k];
			IList<float> list = _sums;
			float value = valList[k];
			_raws[index] = value;
			list[index] = value;
		}
		AddToSendDValue(AttribType.Hp);
	}

	public void OnAlterNumAttribs(int idx, float oldValue, float newValue)
	{
		if (this._OnAlterNumAttribs != null && _numAttribsEventEnable)
		{
			this._OnAlterNumAttribs(idx, oldValue, newValue);
		}
	}

	public void OnPutInPakAttribs(List<ItemToPack> ids)
	{
		if (this._OnPutInPakAttribs != null && _pakAttribsEventEnable)
		{
			this._OnPutInPakAttribs(ids);
		}
	}

	private void RawSetter(int idx, float v)
	{
		NumList numList = (NumList)_raws;
		numList.Set(idx, v);
		_dirties[idx] = true;
	}

	private void SumSetter(int idx, float v)
	{
		NumList numList = (NumList)_sums;
		float oldValue = numList.Get(idx);
		numList.Set(idx, v);
		OnAlterNumAttribs(idx, oldValue, v);
	}

	public void EnableNumAttribsEvent()
	{
		_numAttribsEventEnable = true;
	}

	public void DisableNumAttribsEvent()
	{
		_numAttribsEventEnable = false;
	}

	public SkEntity GetModCaster(int idx)
	{
		return _dirties.GetCaster(idx);
	}

	public SkEntity GetNetModCaster(int idx)
	{
		if (PeSingleton<EntityMgr>.Instance.Get(_netCaster) != null)
		{
			return PeSingleton<EntityMgr>.Instance.Get(_netCaster).skEntity;
		}
		return null;
	}

	public void Update(SkBeModified beModified)
	{
		_pack.ExecBuffs();
		int count = _raws.Count;
		for (int i = 0; i < count; i++)
		{
			if (_dirties[i])
			{
				buffMul = 1f;
				buffPreAdd = 0f;
				buffPostAdd = 0f;
				_pack.ExecTmpBuffs(i);
				_sums[i] = buffMul * (_raws[i] + buffPreAdd) + buffPostAdd;
				SkEntity modCaster = GetModCaster(i);
				int item = 0;
				if (modCaster != null)
				{
					item = modCaster.GetId();
				}
				_dirties[i] = false;
				beModified.indexList.Add(i);
				beModified.valueList.Add(_sums[i]);
				beModified.casterIdList.Add(item);
			}
		}
	}

	public void Serialize(BinaryWriter w)
	{
		int count = _raws.Count;
		for (int i = 0; i < count; i++)
		{
			w.Write(_raws[i]);
		}
	}

	public void Deserialize(BinaryReader r)
	{
		DisableNumAttribsEvent();
		int count = _raws.Count;
		for (int i = 0; i < count; i++)
		{
			IList<float> list = _sums;
			int index = i;
			float value = r.ReadSingle();
			_raws[i] = value;
			list[index] = value;
		}
		_dirties.Clear();
		EnableNumAttribsEvent();
	}

	public void SetNetCaster(int caster)
	{
		_netCaster = caster;
	}

	private void AddToSendDValue(AttribType type)
	{
		if (!_sendDValue.Contains(type))
		{
			_sendDValue.Add(type);
		}
	}

	private static bool IsSendDValue(AttribType type)
	{
		return _sendDValue.Contains(type);
	}

	private void RawSetterNet(int idx, float v)
	{
		NumList numList = (NumList)_raws;
		float num = numList.Get(idx);
		float num2 = v - num;
		bool flag = true;
		if (FromNet)
		{
			numList.Set(idx, v);
			_dirties[idx] = true;
			return;
		}
		bool bSendDValue = false;
		if (!CheckAttrNet((AttribType)idx, num, v, numList, out bSendDValue))
		{
			return;
		}
		if (!bSendDValue)
		{
			v = CheckAttrMax((AttribType)idx, num, v, (NumList)_sums);
		}
		SkEntity modCaster = GetModCaster(idx);
		if (_net != null && LockModifyBySingle)
		{
			return;
		}
		if (!bSendDValue)
		{
			num2 = v;
			numList.Set(idx, v);
			_dirties[idx] = true;
		}
		if (modCaster != null && _net != null)
		{
			if (modCaster.IsController() && !_net.IsStaticNet())
			{
				_net.RPCServer(EPacketType.PT_InGame_SKSyncAttr, (byte)idx, num2, modCaster.GetId(), flag, bSendDValue);
			}
		}
		else if (_net != null && _net.hasOwnerAuth)
		{
			_net.RPCServer(EPacketType.PT_InGame_SKSyncAttr, (byte)idx, num2, -1, flag, bSendDValue);
		}
		else if (idx == 95 && _net is MapObjNetwork)
		{
			_net.RPCServer(EPacketType.PT_InGame_SKSyncAttr, (byte)idx, num2, -1, flag, bSendDValue);
		}
	}

	private void SumSetterNet(int idx, float v)
	{
		NumList numList = (NumList)_sums;
		float num = numList.Get(idx);
		float num2 = v - num;
		bool flag = false;
		SkEntity modCaster = GetModCaster(idx);
		if (FromNet)
		{
			numList.Set(idx, v);
			OnAlterNumAttribs(idx, num, v);
			return;
		}
		bool bSendDValue = false;
		if (!CheckAttrNet((AttribType)idx, num, v, numList, out bSendDValue) || (bSendDValue && num2 == 0f))
		{
			return;
		}
		if (!bSendDValue)
		{
			v = CheckAttrMax((AttribType)idx, num, v, (NumList)_sums);
		}
		if (_net != null && LockModifyBySingle)
		{
			return;
		}
		if (!bSendDValue)
		{
			num2 = v;
			numList.Set(idx, v);
			OnAlterNumAttribs(idx, num, v);
		}
		if (modCaster != null && _net != null)
		{
			if (modCaster.IsController() && !_net.IsStaticNet())
			{
				_net.RPCServer(EPacketType.PT_InGame_SKSyncAttr, (byte)idx, num2, modCaster.GetId(), flag, bSendDValue);
			}
		}
		else if (_net != null && _net.hasOwnerAuth)
		{
			_net.RPCServer(EPacketType.PT_InGame_SKSyncAttr, (byte)idx, num2, -1, flag, bSendDValue);
		}
		else if (idx == 95 && _net is MapObjNetwork)
		{
			_net.RPCServer(EPacketType.PT_InGame_SKSyncAttr, (byte)idx, num2, -1, flag, bSendDValue);
		}
	}

	public void SwitchSeterToNet(SkNetworkInterface net)
	{
		_net = net;
		((NumList)_raws).Setter = delegate(NumList n, int i, float v)
		{
			RawSetterNet(i, v);
		};
		((NumList)_sums).Setter = delegate(NumList n, int i, float v)
		{
			SumSetterNet(i, v);
		};
	}

	private bool CheckAttrNet(AttribType attType, float oldVal, float newVal, NumList r, out bool bSendDValue)
	{
		bSendDValue = IsSendDValue(attType);
		switch (attType)
		{
		case AttribType.Stamina:
			if (newVal < 0f)
			{
				newVal = 0f;
			}
			else if (newVal > r[2])
			{
				newVal = r[2];
			}
			if (newVal == oldVal)
			{
				return false;
			}
			break;
		case AttribType.Comfort:
			if (newVal < 0f)
			{
				newVal = 0f;
			}
			else if (newVal > r[7])
			{
				newVal = r[7];
			}
			if (newVal == oldVal)
			{
				return false;
			}
			break;
		case AttribType.Oxygen:
			if (newVal < 0f)
			{
				newVal = 0f;
			}
			else if (newVal > r[11])
			{
				newVal = r[11];
			}
			if (newVal == oldVal)
			{
				return false;
			}
			break;
		case AttribType.Hunger:
			if (newVal < 0f)
			{
				newVal = 0f;
			}
			else if (newVal > r[15])
			{
				newVal = r[15];
			}
			if (newVal == oldVal)
			{
				return false;
			}
			break;
		case AttribType.Rigid:
			if (newVal < 0f)
			{
				newVal = 0f;
			}
			else if (newVal > r[28])
			{
				newVal = r[28];
			}
			if (newVal == oldVal)
			{
				return false;
			}
			break;
		case AttribType.Hitfly:
			if (newVal < 0f)
			{
				newVal = 0f;
			}
			else if (newVal > r[40])
			{
				newVal = r[40];
			}
			if (newVal == oldVal)
			{
				return false;
			}
			break;
		}
		return true;
	}

	private float CheckAttrMax(AttribType attType, float oldVal, float newVal, NumList r)
	{
		switch (attType)
		{
		case AttribType.Stamina:
			if (newVal < 0f)
			{
				newVal = 0f;
			}
			else if (newVal > r[2])
			{
				newVal = r[2];
			}
			break;
		case AttribType.Comfort:
			if (newVal < 0f)
			{
				newVal = 0f;
			}
			else if (newVal > r[7])
			{
				newVal = r[7];
			}
			break;
		case AttribType.Oxygen:
			if (newVal < 0f)
			{
				newVal = 0f;
			}
			else if (newVal > r[11])
			{
				newVal = r[11];
			}
			break;
		case AttribType.Hunger:
			if (newVal < 0f)
			{
				newVal = 0f;
			}
			else if (newVal > r[15])
			{
				newVal = r[15];
			}
			break;
		case AttribType.Rigid:
			if (newVal < 0f)
			{
				newVal = 0f;
			}
			else if (newVal > r[28])
			{
				newVal = r[28];
			}
			break;
		case AttribType.Hitfly:
			if (newVal < 0f)
			{
				newVal = 0f;
			}
			else if (newVal > r[40])
			{
				newVal = r[40];
			}
			break;
		}
		return newVal;
	}
}
