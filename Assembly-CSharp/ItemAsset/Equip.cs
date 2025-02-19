using Pathea;
using SkillSystem;
using UnityEngine;

namespace ItemAsset;

public class Equip : Cmpt
{
	private Instantiate mInstantiateGameObj;

	public EquipType equipType => base.protoData.equipType;

	public PeSex sex => base.protoData.equipSex;

	public int equipPos => base.protoData.equipPos;

	public int replacePos => base.protoData.equipReplacePos;

	protected float durabilityFactor => base.protoData.durabilityFactor;

	public override void Init()
	{
		base.Init();
		mInstantiateGameObj = itemObj.GetCmpt<Instantiate>();
		if (mInstantiateGameObj == null)
		{
			Debug.LogError("item:" + itemObj.protoId + ", Equip need InstantiateGameObj");
		}
	}

	protected bool DurabilityExhaust()
	{
		return itemObj.GetCmpt<Durability>()?.value.IsCurrentMin() ?? false;
	}

	public bool AddBuff(SkEntity skEntity)
	{
		if (DurabilityExhaust())
		{
			return false;
		}
		Property cmpt = itemObj.GetCmpt<Property>();
		if (cmpt == null)
		{
			return false;
		}
		return null != cmpt.CreateSkBuff(skEntity);
	}

	public bool RemoveBuff(SkEntity skEntity)
	{
		return itemObj.GetCmpt<Property>()?.DestroySkBuff(skEntity) ?? false;
	}

	public bool AddMotionBuff(SkEntity skEntity)
	{
		return AddBuff(skEntity);
	}

	public bool RemoveMotionBuff(SkEntity skEntity)
	{
		return RemoveBuff(skEntity);
	}

	public GameObject CreateGameObj()
	{
		if (mInstantiateGameObj == null)
		{
			return null;
		}
		return mInstantiateGameObj.CreateViewGameObj(null);
	}

	public GameObject CreateLogicObj()
	{
		if (mInstantiateGameObj == null)
		{
			return null;
		}
		return mInstantiateGameObj.CreateLogicGameObj(null);
	}

	public virtual bool ExpendAttackDurability(SkEntity skEntity)
	{
		return false;
	}

	public virtual bool ExpendDefenceDurability(SkEntity skEntity, float hpChange)
	{
		return false;
	}

	protected bool ChangeDurability(SkEntity skEntity, float v)
	{
		Durability cmpt = itemObj.GetCmpt<Durability>();
		if (cmpt == null)
		{
			return false;
		}
		cmpt.value.Change(v);
		if (cmpt.value.IsCurrentMin())
		{
			RemoveBuff(skEntity);
		}
		return true;
	}

	public override string ProcessTooltip(string text)
	{
		string text2 = base.ProcessTooltip(text);
		return text2.Replace("$durabilityDec$", ((int)durabilityFactor).ToString());
	}
}
