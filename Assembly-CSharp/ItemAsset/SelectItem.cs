using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtNpcPackage;

namespace ItemAsset;

public class SelectItem
{
	private static EquipInfo EInfo = new EquipInfo();

	public static bool IsEquipItemObject(ItemObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		return SwichEquipInfo(obj) != null;
	}

	public static EquipInfo SwichEquipInfo(ItemObject obj)
	{
		if (obj == null)
		{
			return null;
		}
		switch (obj.protoData.id)
		{
		case 60:
			EInfo.SetEquipInfo(EequipEditorType.gun, EeqSelect.combat);
			return EInfo;
		case 167:
		case 168:
		case 169:
		case 170:
			EInfo.SetEquipInfo(EequipEditorType.energy_sheild, EeqSelect.energy_sheild);
			return EInfo;
		case 228:
			EInfo.SetEquipInfo(EequipEditorType.battery, EeqSelect.energy);
			return EInfo;
		default:
		{
			EequipEditorType editorTypeId = (EequipEditorType)obj.protoData.editorTypeId;
			switch (editorTypeId)
			{
			case EequipEditorType.sword:
				EInfo.SetEquipInfo(editorTypeId, EeqSelect.combat);
				break;
			case EequipEditorType.axe:
				EInfo.SetEquipInfo(editorTypeId, EeqSelect.tool);
				break;
			case EequipEditorType.bow:
				EInfo.SetEquipInfo(editorTypeId, EeqSelect.combat);
				break;
			case EequipEditorType.gun:
				EInfo.SetEquipInfo(editorTypeId, EeqSelect.combat);
				break;
			case EequipEditorType.shield:
				EInfo.SetEquipInfo(editorTypeId, EeqSelect.protect);
				break;
			default:
				return null;
			}
			return EInfo;
		}
		}
	}

	public static EeqSelect GetEquipSelect(ItemObject obj)
	{
		return SwichEquipInfo(obj)?._selectType ?? EeqSelect.None;
	}

	public static bool EquipByObj(PeEntity entity, ItemObject obj)
	{
		if (obj == null || entity.equipmentCmpt == null)
		{
			return false;
		}
		EquipmentCmpt.Receiver packageCmpt = entity.packageCmpt;
		if (GameConfig.IsMultiMode)
		{
			if (entity.equipmentCmpt.NetTryPutOnEquipment(obj, addToReceiver: true, packageCmpt))
			{
				entity.netCmpt.RequestUseItem(obj.instanceId);
				return true;
			}
		}
		else if (entity.equipmentCmpt.PutOnEquipment(obj, addToReceiver: true, packageCmpt))
		{
			if (SelectItem_N.Instance.ItemObj != null && SelectItem_N.Instance.ItemObj.Equals(obj))
			{
				SelectItem_N.Instance.SetItem(null);
			}
			entity.RemoveFromBag(obj);
			return true;
		}
		return false;
	}

	public static bool TakeOffEquip(PeEntity entity)
	{
		if (entity == null || entity.motionEquipment == null || entity.motionEquipment.PEHoldAbleEqObj == null)
		{
			return false;
		}
		if (entity.equipmentCmpt == null)
		{
			return false;
		}
		EquipmentCmpt.Receiver packageCmpt = entity.packageCmpt;
		if (GameConfig.IsMultiMode)
		{
			if (entity.equipmentCmpt.TryTakeOffEquipment(entity.motionEquipment.PEHoldAbleEqObj, addToReceiver: true, packageCmpt))
			{
				PlayerNetwork.mainPlayer.RequestNpcTakeOffEquip(entity.Id, entity.motionEquipment.PEHoldAbleEqObj.instanceId, -1);
				return true;
			}
			return false;
		}
		return entity.equipmentCmpt.TakeOffEquipment(entity.motionEquipment.PEHoldAbleEqObj, addToReceiver: true, packageCmpt);
	}

	public static bool EquipCanAttack(PeEntity npc, ItemObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.protoData.weaponInfo == null)
		{
			return false;
		}
		if (obj.protoData.weaponInfo.attackModes == null)
		{
			return false;
		}
		if (obj.protoData.weaponInfo.attackModes[0].damage <= float.Epsilon)
		{
			return false;
		}
		if (npc.NpcCmpt == null)
		{
			return false;
		}
		if (!npc.NpcCmpt.HasConsume)
		{
			return true;
		}
		if (npc.biologyViewCmpt != null && npc.biologyViewCmpt.monoPhyCtrl != null && npc.biologyViewCmpt.monoPhyCtrl.feetInWater && obj.protoData.weaponInfo.costItem == 49)
		{
			return false;
		}
		Durability cmpt = obj.GetCmpt<Durability>();
		if (cmpt != null && cmpt.floatValue.current < float.Epsilon)
		{
			return false;
		}
		if (obj.protoData.weaponInfo.useEnergry)
		{
			if (npc.GetAttribute(AttribType.Energy) > 10f)
			{
				return true;
			}
			Energy cmpt2 = obj.GetCmpt<Energy>();
			if (cmpt2 != null && cmpt2.floatValue.current > 10f)
			{
				return true;
			}
			GunAmmo cmpt3 = obj.GetCmpt<GunAmmo>();
			if (cmpt3 != null && cmpt3.count > 5)
			{
				return true;
			}
			List<ItemObject> equipObjs = npc.GetEquipObjs(EeqSelect.energy);
			for (int i = 0; i < equipObjs.Count; i++)
			{
				Energy cmpt4 = equipObjs[i].GetCmpt<Energy>();
				if (cmpt4 != null && cmpt4.floatValue.current > 10f)
				{
					return true;
				}
			}
			return false;
		}
		if (obj.protoData.weaponInfo.costItem > 0)
		{
			GunAmmo cmpt5 = obj.GetCmpt<GunAmmo>();
			return (cmpt5 != null && cmpt5.count > 0) || npc.GetItemCount(obj.protoData.weaponInfo.costItem) > 0;
		}
		return true;
	}

	public static bool EqToolCanUse(PeEntity npc, ItemObject obj)
	{
		if (obj.protoId == 1469 && npc.GetAttribute(AttribType.Energy) < float.Epsilon)
		{
			return false;
		}
		return true;
	}

	public static bool EqEnergySheildCanUse(PeEntity npc, ItemObject obj)
	{
		Equip cmpt = obj.GetCmpt<Equip>();
		if (cmpt == null)
		{
			return false;
		}
		if (!PeGender.IsMatch(cmpt.sex, npc.ExtGetSex()))
		{
			return false;
		}
		return true;
	}

	public static bool HasCanAttackEquip(PeEntity entity, AttackType type)
	{
		List<ItemObject> equipObjs = entity.GetEquipObjs(EeqSelect.combat);
		if (equipObjs == null)
		{
			return false;
		}
		for (int i = 0; i < equipObjs.Count; i++)
		{
			if (EquipCanAttack(entity, equipObjs[i]))
			{
				AttackMode attackMode = equipObjs[i].protoData.weaponInfo.attackModes[0];
				if (attackMode.type == type)
				{
					return true;
				}
			}
		}
		ItemObject pEHoldAbleEqObj = entity.motionEquipment.PEHoldAbleEqObj;
		EeqSelect equipSelect = GetEquipSelect(pEHoldAbleEqObj);
		bool flag = ((pEHoldAbleEqObj != null && pEHoldAbleEqObj.protoData.weaponInfo != null && pEHoldAbleEqObj.protoData.weaponInfo.attackModes[0].type == AttackType.Ranged) ? true : false);
		bool flag2 = EquipCanAttack(entity, pEHoldAbleEqObj);
		if (flag && !flag2)
		{
			TakeOffEquip(entity);
		}
		if (equipSelect == EeqSelect.combat && flag2)
		{
			AttackMode attackMode2 = pEHoldAbleEqObj.protoData.weaponInfo.attackModes[0];
			if (attackMode2.type == type)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasCanUseEquip(PeEntity entity, EeqSelect selcet)
	{
		List<ItemObject> equipObjs = entity.GetEquipObjs(selcet);
		return (equipObjs != null && equipObjs.Count > 0) ? true : false;
	}

	public static bool HasCanEquip(PeEntity entity, EeqSelect selcet, AttackType type)
	{
		return selcet switch
		{
			EeqSelect.combat => HasCanAttackEquip(entity, type), 
			EeqSelect.protect => HasCanUseEquip(entity, selcet), 
			EeqSelect.energy => HasCanUseEquip(entity, selcet), 
			EeqSelect.energy_sheild => HasCanUseEquip(entity, selcet), 
			EeqSelect.tool => HasCanUseEquip(entity, selcet), 
			_ => false, 
		};
	}

	public static bool MatchEnemyEquip(PeEntity npc, PeEntity target)
	{
		if (npc == null || target == null)
		{
			return false;
		}
		if (npc.IsMotorNpc)
		{
			return true;
		}
		if (target.IsBoss)
		{
			return HasCanEquip(npc, EeqSelect.combat, AttackType.Ranged);
		}
		return target.Field switch
		{
			MovementField.Land => true, 
			MovementField.Sky => HasCanEquip(npc, EeqSelect.combat, AttackType.Ranged), 
			MovementField.water => true, 
			MovementField.Amphibian => true, 
			_ => false, 
		};
	}

	public static bool MatchEnemyAttack(PeEntity npc, PeEntity target)
	{
		if (npc == null || target == null)
		{
			return false;
		}
		if (target.isRagdoll)
		{
			return false;
		}
		if (npc.IsMotorNpc)
		{
			return true;
		}
		bool flag = HasCanEquip(npc, EeqSelect.combat, AttackType.Ranged);
		bool flag2 = HasCanEquip(npc, EeqSelect.combat, AttackType.Melee);
		bool isBoss = target.IsBoss;
		float attribute = npc.GetAttribute(AttribType.Hp);
		float attribute2 = npc.GetAttribute(AttribType.HpMax);
		float attribute3 = npc.GetAttribute(AttribType.Atk);
		float attribute4 = npc.GetAttribute(AttribType.Def);
		float attribute5 = target.GetAttribute(AttribType.Atk);
		bool result = attribute > attribute2 * 0.2f;
		switch (target.Field)
		{
		case MovementField.Land:
		case MovementField.water:
			if (flag)
			{
				return true;
			}
			if (isBoss)
			{
				return false;
			}
			return result;
		case MovementField.Sky:
			return flag;
		default:
			return true;
		}
	}
}
