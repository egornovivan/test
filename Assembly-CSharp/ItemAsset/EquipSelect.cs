using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExtNpcPackage;
using UnityEngine;

namespace ItemAsset;

public class EquipSelect
{
	private List<ItemObject> _hasSelectsAtkEq;

	private List<ItemObject> _hasSelectsDefEq;

	private List<ItemObject> _hasSelectsToolEq;

	private List<ItemObject> _hasSelectsEnergyEq;

	private List<ItemObject> _hasSelectsEnergy_SheildEq;

	private List<EvaluateInfo> _hasSelectsAtkInfo;

	private ItemObject _BetterAtkObj;

	private List<ItemObject> _tempList = new List<ItemObject>();

	public ItemObject BetterAtkObj => _BetterAtkObj;

	public EquipSelect()
	{
		_hasSelectsAtkEq = new List<ItemObject>();
		_hasSelectsDefEq = new List<ItemObject>();
		_hasSelectsToolEq = new List<ItemObject>();
		_hasSelectsEnergyEq = new List<ItemObject>();
		_hasSelectsEnergy_SheildEq = new List<ItemObject>();
	}

	public ItemObject GetBetterObj(PeEntity npc, Enemy enmey, EeqSelect select)
	{
		return select switch
		{
			EeqSelect.combat => GetBetterAtkObj(npc, enmey), 
			EeqSelect.protect => GetBetterDefObj(), 
			EeqSelect.tool => GetBetterToolObj(), 
			_ => null, 
		};
	}

	public bool SetSelect(PeEntity npc, EeqSelect select)
	{
		return select switch
		{
			EeqSelect.combat => SetSelectObjsAtk(npc, select), 
			EeqSelect.protect => SetSelectObjsDef(npc, select), 
			EeqSelect.tool => SetSelectObjsTool(npc, select), 
			_ => false, 
		};
	}

	public void ClearSelect()
	{
		_BetterAtkObj = null;
	}

	public bool AddSeclectObjAtk(ItemObject obj)
	{
		if (obj == null || obj.protoData.weaponInfo == null)
		{
			return false;
		}
		_hasSelectsAtkEq.Add(obj);
		return true;
	}

	public void ClearAtkSelects()
	{
		_hasSelectsAtkEq.Clear();
	}

	public bool AddSelectObjsAtk(PeEntity npc, List<ItemObject> objs)
	{
		if (objs == null)
		{
			return false;
		}
		for (int i = 0; i < objs.Count; i++)
		{
			if (SelectItem.EquipCanAttack(npc, objs[i]))
			{
				_hasSelectsAtkEq.Add(objs[i]);
			}
		}
		return true;
	}

	public bool SetSelectObjsAtk(PeEntity npc, EeqSelect selcet)
	{
		AddSelectObjsAtk(npc, npc.GetEquipObjs(selcet));
		ItemObject pEHoldAbleEqObj = npc.motionEquipment.PEHoldAbleEqObj;
		EeqSelect equipSelect = SelectItem.GetEquipSelect(pEHoldAbleEqObj);
		bool flag = ((pEHoldAbleEqObj != null && pEHoldAbleEqObj.protoData.weaponInfo != null && pEHoldAbleEqObj.protoData.weaponInfo.attackModes[0].type == AttackType.Ranged) ? true : false);
		bool flag2 = SelectItem.EquipCanAttack(npc, pEHoldAbleEqObj);
		if (flag && !flag2)
		{
			SelectItem.TakeOffEquip(npc);
		}
		if (equipSelect == selcet && flag2)
		{
			AddSeclectObjAtk(pEHoldAbleEqObj);
		}
		return _hasSelectsAtkEq.Count > 0;
	}

	private List<ItemObject> FilterAtkObjs(AttackType type, List<ItemObject> objs)
	{
		if (objs == null)
		{
			return null;
		}
		_tempList.AddRange(objs);
		for (int i = 0; i < _tempList.Count; i++)
		{
			if (_tempList[i].protoData.weaponInfo != null && _tempList[i].protoData.weaponInfo.attackModes[0].type != type)
			{
				objs.Remove(_tempList[i]);
			}
		}
		_tempList.Clear();
		return objs;
	}

	public bool SetSelectObjsAtk(PeEntity npc, AttackType type)
	{
		AddSelectObjsAtk(npc, FilterAtkObjs(type, npc.GetEquipObjs(EeqSelect.combat)));
		ItemObject pEHoldAbleEqObj = npc.motionEquipment.PEHoldAbleEqObj;
		EeqSelect equipSelect = SelectItem.GetEquipSelect(pEHoldAbleEqObj);
		bool flag = ((pEHoldAbleEqObj != null && pEHoldAbleEqObj.protoData.weaponInfo != null && pEHoldAbleEqObj.protoData.weaponInfo.attackModes[0].type == AttackType.Ranged) ? true : false);
		bool flag2 = SelectItem.EquipCanAttack(npc, pEHoldAbleEqObj);
		if (flag && !flag2)
		{
			SelectItem.TakeOffEquip(npc);
		}
		if (equipSelect == EeqSelect.combat && pEHoldAbleEqObj.protoData.weaponInfo.attackModes[0].type == type && flag2)
		{
			AddSeclectObjAtk(pEHoldAbleEqObj);
		}
		return _hasSelectsAtkEq.Count > 0;
	}

	public ItemObject GetBetterAtkObj(PeEntity npc, Enemy enemy)
	{
		List<EvaluateInfo> list = EvaluateMgr.Evaluates(npc, enemy, _hasSelectsAtkEq);
		if (list == null || list.Count <= 0)
		{
			if (npc.biologyViewCmpt != null && !npc.biologyViewCmpt.monoPhyCtrl.Equals(null) && npc.biologyViewCmpt.monoPhyCtrl.feetInWater)
			{
				SelectItem.TakeOffEquip(npc);
			}
			return null;
		}
		if (list.Count == 1)
		{
			_BetterAtkObj = list[0].ItemObj;
			return _BetterAtkObj;
		}
		EvaluateInfo evaluateInfo = list[0];
		for (int i = 1; i < list.Count; i++)
		{
			if (evaluateInfo.EvaluateValue < list[i].EvaluateValue)
			{
				evaluateInfo = list[i];
			}
			EvaluateMgr.Recyle(list[i]);
		}
		_BetterAtkObj = evaluateInfo.ItemObj;
		return _BetterAtkObj;
	}

	public void ClearDefSelects()
	{
		_hasSelectsDefEq.Clear();
	}

	public bool AddSeclectObjDef(ItemObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		_hasSelectsDefEq.Add(obj);
		return true;
	}

	public bool SetSelectObjsDef(List<ItemObject> objs)
	{
		if (objs == null)
		{
			return false;
		}
		ClearDefSelects();
		_hasSelectsDefEq.AddRange(objs);
		return true;
	}

	public bool SetSelectObjsDef(PeEntity npc, EeqSelect selcet)
	{
		SetSelectObjsDef(npc.GetEquipObjs(selcet));
		if (npc.motionEquipment.sheild != null && npc.motionEquipment.sheild.m_ItemObj != null)
		{
			AddSeclectObjDef(npc.motionEquipment.sheild.m_ItemObj);
		}
		return _hasSelectsDefEq.Count > 0;
	}

	public ItemObject GetBetterDefObj()
	{
		if (_hasSelectsDefEq == null || _hasSelectsDefEq.Count <= 0)
		{
			return null;
		}
		if (_hasSelectsDefEq.Count == 1)
		{
			return _hasSelectsDefEq[0];
		}
		ItemObject itemObject = _hasSelectsDefEq[0];
		for (int i = 1; i < _hasSelectsDefEq.Count; i++)
		{
			if (_hasSelectsDefEq[i].protoData.propertyList.GetProperty(AttribType.Def) > itemObject.protoData.propertyList.GetProperty(AttribType.Def))
			{
				itemObject = _hasSelectsDefEq[i];
			}
		}
		return itemObject;
	}

	public void ClearToolSelects()
	{
		_hasSelectsToolEq.Clear();
	}

	private bool SetSelectObjsTool(PeEntity npc, List<ItemObject> objs)
	{
		if (objs == null)
		{
			return false;
		}
		ClearToolSelects();
		for (int i = 0; i < objs.Count; i++)
		{
			if (SelectItem.EqToolCanUse(npc, objs[i]))
			{
				AddSelectToolObj(objs[i]);
			}
		}
		return true;
	}

	public void AddSelectToolObj(ItemObject obj)
	{
		if (obj != null)
		{
			_hasSelectsToolEq.Add(obj);
		}
	}

	public bool SetSelectObjsTool(PeEntity npc, EeqSelect selcet)
	{
		SetSelectObjsTool(npc, npc.GetEquipObjs(selcet));
		if (npc.motionEquipment.axe != null && npc.motionEquipment.axe.m_ItemObj != null)
		{
			AddSelectToolObj(npc.motionEquipment.axe.m_ItemObj);
		}
		return _hasSelectsToolEq.Count > 0;
	}

	public ItemObject GetBetterToolObj()
	{
		if (_hasSelectsToolEq == null || _hasSelectsToolEq.Count <= 0)
		{
			return null;
		}
		if (_hasSelectsToolEq.Count == 1)
		{
			return _hasSelectsToolEq[0];
		}
		ItemObject itemObject = _hasSelectsToolEq[0];
		for (int i = 1; i < _hasSelectsToolEq.Count; i++)
		{
			if (_hasSelectsToolEq[i].protoData.propertyList.GetProperty(AttribType.Atk) > itemObject.protoData.propertyList.GetProperty(AttribType.Atk))
			{
				itemObject = _hasSelectsToolEq[i];
			}
		}
		return itemObject;
	}

	public bool SetSelectObjsEnergy(PeEntity npc, EeqSelect selcet)
	{
		if (npc.GetAttribute(AttribType.Energy) > float.Epsilon)
		{
			return false;
		}
		SetSelectObjsEnergy(npc.GetEquipObjs(selcet));
		return _hasSelectsEnergyEq.Count > 0;
	}

	public ItemObject GetBetterEnergyObj()
	{
		if (_hasSelectsEnergyEq == null || _hasSelectsEnergyEq.Count <= 0)
		{
			return null;
		}
		if (_hasSelectsEnergyEq.Count == 1)
		{
			return _hasSelectsEnergyEq[0];
		}
		ItemObject itemObject = _hasSelectsEnergyEq[0];
		for (int i = 1; i < _hasSelectsEnergyEq.Count; i++)
		{
			itemObject = CompearEnery(_hasSelectsEnergyEq[i], itemObject);
		}
		return itemObject;
	}

	private void SetSelectObjsEnergy(List<ItemObject> objs)
	{
		if (objs == null)
		{
			return;
		}
		_hasSelectsEnergyEq.Clear();
		for (int i = 0; i < objs.Count; i++)
		{
			if (EnergyObjCanUse(objs[i]))
			{
				_hasSelectsEnergyEq.Add(objs[i]);
			}
		}
	}

	public bool EnergyObjCanUse(ItemObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		Energy cmpt = obj.GetCmpt<Energy>();
		return cmpt != null && cmpt.floatValue.current > float.Epsilon;
	}

	private ItemObject CompearEnery(ItemObject obj0, ItemObject obj1)
	{
		Energy cmpt = obj0.GetCmpt<Energy>();
		Energy cmpt2 = obj1.GetCmpt<Energy>();
		if (cmpt.floatValue.current > cmpt2.floatValue.current)
		{
			return obj0;
		}
		return obj1;
	}

	public bool SetSelectObjsEnergySheild(PeEntity npc, EeqSelect selcet)
	{
		if (npc.motionEquipment == null || npc.motionEquipment.energySheild != null)
		{
			return false;
		}
		setSelectObjEnergySheild(npc, npc.GetEquipObjs(selcet));
		return _hasSelectsEnergy_SheildEq.Count > 0;
	}

	public void AddEnergySheildObj(PeEntity npc, ItemObject obj)
	{
		_hasSelectsEnergy_SheildEq.Add(obj);
	}

	private void setSelectObjEnergySheild(PeEntity npc, List<ItemObject> objs)
	{
		if (objs == null)
		{
			return;
		}
		_hasSelectsEnergy_SheildEq.Clear();
		for (int i = 0; i < objs.Count; i++)
		{
			if (SelectItem.EqEnergySheildCanUse(npc, objs[i]))
			{
				_hasSelectsEnergy_SheildEq.Add(objs[i]);
			}
		}
	}

	public ItemObject GetBetterEnergySheild()
	{
		if (_hasSelectsEnergy_SheildEq == null)
		{
			return null;
		}
		if (_hasSelectsEnergy_SheildEq.Count == 1)
		{
			return _hasSelectsEnergy_SheildEq[0];
		}
		return _hasSelectsEnergy_SheildEq[Random.Range(0, _hasSelectsEnergy_SheildEq.Count)];
	}
}
