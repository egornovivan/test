using Pathea;
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

	public virtual bool ExpendAttackDurability()
	{
		return false;
	}

	public virtual bool ExpendDefenceDurability(float hpChange)
	{
		return false;
	}

	protected bool ChangeDurability(float v)
	{
		return itemObj.GetCmpt<Durability>()?.ChangeValue(v) ?? false;
	}

	protected bool DurabilityExhaust()
	{
		return itemObj.GetCmpt<Durability>()?.floatValue.IsCurrentMin() ?? false;
	}

	public float GetDurability()
	{
		return itemObj.GetCmpt<Durability>()?.floatValue.current ?? (-1f);
	}
}
